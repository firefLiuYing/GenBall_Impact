"""Standalone CLI to trigger Unity compilation via TCP.

Usage:
    py compile_cli.py              # trigger compile, wait, print results
    py compile_cli.py --no-wait    # trigger only, don't poll
    py compile_cli.py --timeout 60 # custom timeout

Connects directly to Unity's TCP server on port 9876.
Reads Temp/UnityMcpCompileState.json directly for results across
domain reloads (phase-based: refresh_pending → compiling → done | no_changes).
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
READ_TIMEOUT = 30.0
RECONNECT_DELAY = 4.0

# Paths relative to project root
PROJECT_ROOT = Path(__file__).resolve().parent.parent.parent
STATE_FILE = PROJECT_ROOT / "Temp" / "UnityMcpCompileState.json"


def read_state_file() -> Optional[dict]:
    """Read the compile state file. Returns parsed dict or None."""
    try:
        if not STATE_FILE.exists():
            return None
        with open(STATE_FILE, "r", encoding="utf-8") as f:
            return json.load(f)
    except (json.JSONDecodeError, OSError):
        return None


def send_recv(sock: socket.socket, method: str,
              params: Optional[dict] = None) -> dict:
    """Send a JSON command and read one JSON response line.

    Raises:
        ConnectionError: socket died or returned garbage
        socket.timeout: no response within READ_TIMEOUT
        json.JSONDecodeError: unparseable response
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

    text = line.decode("utf-8-sig").strip()
    if not text:
        raise ConnectionError("Empty response")
    return json.loads(text)


def connect(quiet: bool = False) -> socket.socket:
    """Connect to Unity's TCP server with exponential backoff."""
    for attempt in range(10):
        try:
            sock = socket.create_connection((HOST, PORT), timeout=3.0)
            if not quiet:
                print(f"  Connected to Unity.")
            return sock
        except (ConnectionRefusedError, OSError, socket.timeout):
            delay = min(1.0 * (2 ** attempt), 30.0)
            if not quiet:
                print(f"  Connection attempt {attempt + 1}/10, "
                      f"retry in {delay:.0f}s...")
            time.sleep(delay)
    print("ERROR: Could not connect to Unity Editor.")
    sys.exit(2)


def safe_send_recv(sock: socket.socket, method: str,
                   params: Optional[dict] = None):
    """send_recv with reconnection on failure.

    Returns (sock, response). If compilation finished during disconnect,
    returns (None, state_file_result) instead.
    """
    try:
        return sock, send_recv(sock, method, params)
    except (ConnectionError, socket.timeout, json.JSONDecodeError) as e:
        print(f"  (connection lost: {e})")

        # Check state file — compilation may have finished during disconnect
        state = read_state_file()
        if state and state.get("phase") in ("done", "no_changes"):
            print("  (state file has terminal result)")
            return None, {"result": build_result_from_state(state)}

        try:
            sock.close()
        except Exception:
            pass
        print(f"  reconnecting (waiting {RECONNECT_DELAY}s)...")
        time.sleep(RECONNECT_DELAY)
        new_sock = connect(quiet=True)
        return new_sock, send_recv(new_sock, method, params)


def build_result_from_state(state: dict) -> dict:
    """Convert a state file dict to a compile_status-like response."""
    errors = state.get("errors", [])
    warnings = state.get("warnings", [])
    is_no_changes = state.get("phase") == "no_changes"
    return {
        "isCompiling": False,
        "compileFinished": True,
        "noChanges": is_no_changes,
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
        pass


def main():
    parser = argparse.ArgumentParser(
        description="Trigger Unity compilation")
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

        # ── Handle already_compiling ──
        if status == "already_compiling":
            print("Unity already compiling, waiting for it to finish...")
            elapsed = 0.0
            while elapsed < args.timeout:
                time.sleep(2.0)
                elapsed += 2.0

                state = read_state_file()
                if state and state.get("phase") in ("done", "no_changes"):
                    # Previous compile done, now trigger ours
                    sock, resp = safe_send_recv(sock, "compile")
                    result = resp.get("result", {})
                    if result.get("status") == "compilation_started":
                        break
                    continue

                print(f"  waiting... ({elapsed:.0f}s)")
            else:
                print("ERROR: Timed out waiting for "
                      "in-progress compilation")
                sys.exit(2)

        # ── Wait for our compilation ──
        if (status == "compilation_started"
                or result.get("status") == "compilation_started"):
            if args.no_wait:
                print("Compilation triggered, exiting (--no-wait).")
                sys.exit(0)

            # 2. Poll state file until terminal phase
            time.sleep(1.0)
            elapsed = 1.0
            while elapsed < args.timeout:
                state = read_state_file()
                if state:
                    phase = state.get("phase", "")
                    if phase == "done":
                        break  # got results
                    elif phase == "no_changes":
                        print("\nNo script changes detected — "
                              "compilation skipped.\n")
                        sys.exit(0)
                    errors = len(state.get("errors", []))
                    warnings = len(state.get("warnings", []))
                    print(f"  Compiling... errors={errors} "
                          f"warnings={warnings} "
                          f"phase={phase} ({elapsed:.0f}s)")

                time.sleep(2.0)
                elapsed += 2.0
            else:
                # Final check on timeout
                state = read_state_file()
                if state and state.get("phase") != "done":
                    print(f"\nERROR: Compilation timed out "
                          f"(phase={state.get('phase', '?')})")
                    sys.exit(2)
                elif not state:
                    print("\nERROR: Compilation timed out "
                          "(no state file)")
                    sys.exit(2)

            # 3. Collect final results from state file
            state = read_state_file()
            if state is None:
                print("\nWARNING: No state file at completion")
                sys.exit(0)

            errors = state.get("errors", [])
            warnings = state.get("warnings", [])
            error_count = len(errors)
            warning_count = len(warnings)

            print(f"\nCompilation complete -- {error_count} errors, "
                  f"{warning_count} warnings\n")
            for e in errors:
                print(f"  {e.get('file', '')}"
                      f"({e.get('line', '')},"
                      f"{e.get('column', '')}): "
                      f"{e.get('message', '')}")

            # Clean up
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
