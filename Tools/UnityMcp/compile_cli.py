"""Standalone CLI to trigger Unity compilation via TCP.

Usage:
    py compile_cli.py              # trigger compile, wait, print results
    py compile_cli.py --no-wait    # trigger only, don't poll
    py compile_cli.py --timeout 60 # custom timeout

Bypasses MCP entirely — connects directly to Unity's TCP server on port 9876.
Reads Temp/UnityMcpCompileState.json directly for results across domain reloads.
Exit code 0 = success (0 errors), 1 = errors found, 2 = connection failed.
"""

import argparse
import json
import os
import socket
import sys
import time
from pathlib import Path
from typing import Optional

HOST = "localhost"
PORT = 9876
READ_TIMEOUT = 30.0  # generous timeout — Unity main thread may be busy
RECONNECT_DELAY = 4.0  # wait after domain reload before reconnecting

# Paths relative to project root
PROJECT_ROOT = Path(__file__).resolve().parent.parent.parent
STATE_FILE = PROJECT_ROOT / "Temp" / "UnityMcpCompileState.json"


def read_state_file() -> Optional[dict]:
    """Read the compile state file directly. Returns parsed dict or None."""
    try:
        if not STATE_FILE.exists():
            return None
        with open(STATE_FILE, "r", encoding="utf-8") as f:
            return json.load(f)
    except (json.JSONDecodeError, OSError):
        return None


def send_recv(sock: socket.socket, method: str, params: Optional[dict] = None) -> dict:
    """Send a JSON command and read one JSON response line.

    Raises:
        ConnectionError: socket died or returned garbage
        socket.timeout: no response within READ_TIMEOUT
        json.JSONDecodeError: unparseable response (stale connection after reload)
    """
    request = json.dumps({
        "id": "cli",
        "method": method,
        "params": params or {},
    }, ensure_ascii=False)
    try:
        sock.sendall((request + "\n").encode("utf-8"))
    except OSError as e:
        raise ConnectionError(f"Send failed: {e}")

    sock.settimeout(READ_TIMEOUT)
    line = b""
    while not line.endswith(b"\n"):
        try:
            chunk = sock.recv(1)
        except (ConnectionResetError, BrokenPipeError, OSError) as e:
            raise ConnectionError(f"Recv failed: {e}")
        if not chunk:
            raise ConnectionError("Unity closed connection")
        line += chunk

    # Strip BOM and whitespace; handle garbage from stale connections
    text = line.decode("utf-8-sig").strip()
    if not text:
        raise ConnectionError("Empty response")
    return json.loads(text)


def connect(quiet: bool = False) -> socket.socket:
    """Connect to Unity's TCP server with retries."""
    for attempt in range(10):
        try:
            sock = socket.create_connection((HOST, PORT), timeout=3.0)
            if not quiet:
                print(f"  Connected to Unity.")
            return sock
        except (ConnectionRefusedError, OSError, socket.timeout):
            delay = 1.0 * (attempt + 1)
            if not quiet:
                print(f"  Connection attempt {attempt + 1}/10, retry in {delay:.0f}s...")
            time.sleep(delay)
    print("ERROR: Could not connect to Unity Editor.")
    sys.exit(2)


def safe_send_recv(sock: socket.socket, method: str, params: Optional[dict] = None):
    """send_recv with reconnection on failure. Returns (sock, response).

    Before reconnecting, checks the state file — if compilation finished during
    the disconnection, returns the file-based result directly.
    """
    try:
        return sock, send_recv(sock, method, params)
    except (ConnectionError, socket.timeout, json.JSONDecodeError) as e:
        print(f"  (connection lost: {e})")

        # Check state file before reconnecting — compilation may have finished
        state = read_state_file()
        if state and state.get("state") == "done":
            print("  (state file reports compile done)")
            return None, {"result": build_status_from_file(state)}

        try:
            sock.close()
        except Exception:
            pass
        print(f"  reconnecting (waiting {RECONNECT_DELAY}s for Unity to restart)...")
        time.sleep(RECONNECT_DELAY)
        new_sock = connect(quiet=True)
        return new_sock, send_recv(new_sock, method, params)


def build_status_from_file(state: dict) -> dict:
    """Convert a state file dict to a compile_status response dict."""
    errors = state.get("errors", [])
    warnings = state.get("warnings", [])
    return {
        "isCompiling": False,
        "compileRequested": True,
        "compileFinished": True,
        "errorCount": len(errors),
        "warningCount": len(warnings),
        "errors": errors,
        "warnings": warnings,
    }


def cleanup_state(sock: socket.socket) -> None:
    """Tell Unity to clean up the compile state file."""
    try:
        send_recv(sock, "cleanup_compile_state")
    except Exception:
        pass  # best-effort


def main():
    parser = argparse.ArgumentParser(description="Trigger Unity compilation")
    parser.add_argument("--no-wait", action="store_true",
                        help="Trigger only, don't wait for result")
    parser.add_argument("--timeout", type=float, default=120.0,
                        help="Max wait time in seconds (default: 120)")
    args = parser.parse_args()

    sock = connect()
    try:
        # 1. Trigger compilation
        resp = send_recv(sock, "compile")
        result = resp.get("result", {})

        if "error" in resp:
            print(f"ERROR: {resp['error']}")
            sys.exit(2)

        status = result.get("status", "")
        print(f"[compile] {status}")

        if status == "already_compiling":
            print("Unity already compiling, waiting for it to finish...")
            elapsed = 0.0
            while elapsed < args.timeout:
                time.sleep(2.0)
                elapsed += 2.0

                # Check state file first
                state = read_state_file()
                if state and state.get("state") == "done":
                    cs = build_status_from_file(state)
                    if not cs.get("isCompiling") and cs.get("compileFinished"):
                        sock, resp = safe_send_recv(sock, "compile")
                        result = resp.get("result", {})
                        if result.get("status") == "compilation_started":
                            break
                        continue

                sock, resp = safe_send_recv(sock, "compile_status")
                cs = resp.get("result", {})
                if not cs.get("isCompiling"):
                    # Previous compile done, now trigger ours
                    sock, resp = safe_send_recv(sock, "compile")
                    result = resp.get("result", {})
                    if result.get("status") == "compilation_started":
                        break
                print(f"  waiting... ({elapsed:.0f}s)")
            else:
                print("ERROR: Timed out waiting for in-progress compilation")
                sys.exit(2)

        if status == "compilation_started" or result.get("status") == "compilation_started":
            if args.no_wait:
                print("Compilation triggered, exiting (--no-wait).")
                sys.exit(0)

            # 2. Poll until done
            time.sleep(1.0)
            elapsed = 1.0
            while elapsed < args.timeout:
                # Check state file first — may have results even if TCP is down
                state = read_state_file()
                if state and state.get("state") == "done":
                    cs = build_status_from_file(state)
                    if not cs.get("isCompiling") and cs.get("compileFinished"):
                        break  # got results from file

                sock, resp = safe_send_recv(sock, "compile_status")
                cs = resp.get("result", {})

                if not cs.get("isCompiling") and cs.get("compileFinished"):
                    break  # got results from TCP

                print(f"  Compiling... errors={cs.get('errorCount',0)} warnings={cs.get('warningCount',0)} ({elapsed:.0f}s)")
                time.sleep(2.0)
                elapsed += 2.0
            else:
                # Final check of state file on timeout
                state = read_state_file()
                if state and state.get("state") == "done":
                    cs = build_status_from_file(state)
                else:
                    print("\nERROR: Compilation timed out")
                    sys.exit(2)

            error_count = cs.get("errorCount", 0)
            warning_count = cs.get("warningCount", 0)
            errors = cs.get("errors", [])
            warnings = cs.get("warnings", [])

            print(f"\nCompilation complete -- {error_count} errors, {warning_count} warnings\n")
            for e in errors:
                print(f"  {e.get('file','')}({e.get('line','')},{e.get('column','')}): {e.get('message','')}")

            # Clean up state file on Unity side
            cleanup_state(sock)

            if error_count > 0:
                sys.exit(1)
            else:
                print("OK")
                sys.exit(0)
        elif status not in ("already_compiling", "compilation_started"):
            print(f"Unexpected status: {status}")
            print(json.dumps(resp, indent=2))
            sys.exit(2)
    finally:
        try:
            sock.close()
        except Exception:
            pass


if __name__ == "__main__":
    main()
