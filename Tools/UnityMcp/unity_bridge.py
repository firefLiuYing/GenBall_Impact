"""TCP bridge: Python client that connects to Unity Editor's TCP server."""

from __future__ import annotations

import asyncio
import json
import logging
from typing import Any, Dict, Optional

logger = logging.getLogger("unity-mcp.bridge")

DEFAULT_HOST = "localhost"
DEFAULT_PORT = 9876

CMD_TIMEOUT = 30.0       # seconds for regular commands
HEARTBEAT_INTERVAL = 30.0  # seconds — Unity ReceiveTimeout is 60s
PING_TIMEOUT = 5.0       # quick timeout for heartbeat pings

# Reconnect backoff
RECONNECT_BASE_DELAY = 1.0   # seconds
RECONNECT_MAX_DELAY = 30.0   # seconds
RECONNECT_MAX_ATTEMPTS = 10


class UnityBridge:
    """Manages a TCP connection to the Unity Editor.

    The MCP server sends commands through this bridge to Unity,
    and receives responses. The Python side is the TCP client;
    Unity Editor is the TCP server.
    """

    def __init__(self, host: str = DEFAULT_HOST, port: int = DEFAULT_PORT):
        self.host = host
        self.port = port
        self._reader: Optional[asyncio.StreamReader] = None
        self._writer: Optional[asyncio.StreamWriter] = None
        self._pending_requests: Dict[str, asyncio.Future] = {}
        self._next_request_id = 0
        self._write_lock = asyncio.Lock()
        self._receive_task: Optional[asyncio.Task] = None
        self._heartbeat_task: Optional[asyncio.Task] = None
        self._heartbeat_paused = False

    @property
    def connected(self) -> bool:
        return self._writer is not None

    # ── Heartbeat control ──────────────────────────────────────────

    def pause_heartbeat(self) -> None:
        """Pause heartbeats — call during compile polling to avoid
        unnecessary TCP traffic while Unity may be reloading."""
        self._heartbeat_paused = True
        logger.debug("Heartbeat paused")

    def resume_heartbeat(self) -> None:
        """Resume heartbeats after compile completes."""
        self._heartbeat_paused = False
        logger.debug("Heartbeat resumed")

    # ── Connection lifecycle ───────────────────────────────────────

    async def start(self) -> None:
        """Connect to Unity Editor's TCP server with exponential backoff."""
        for attempt in range(RECONNECT_MAX_ATTEMPTS):
            if await self.connect():
                return
            delay = min(
                RECONNECT_BASE_DELAY * (2 ** attempt),
                RECONNECT_MAX_DELAY)
            logger.info(
                f"Connection attempt {attempt + 1}/{RECONNECT_MAX_ATTEMPTS} "
                f"failed, retrying in {delay:.0f}s...")
            await asyncio.sleep(delay)
        logger.error(
            f"Failed to connect to Unity Editor after "
            f"{RECONNECT_MAX_ATTEMPTS} attempts")

    async def stop(self) -> None:
        """Disconnect from Unity Editor."""
        self._cancel_heartbeat()

        if self._receive_task and not self._receive_task.done():
            self._receive_task.cancel()
            try:
                await self._receive_task
            except asyncio.CancelledError:
                pass
            self._receive_task = None

        if self._writer:
            try:
                self._writer.close()
                await self._writer.wait_closed()
            except Exception:
                pass
            self._writer = None
            self._reader = None
        logger.info("Disconnected from Unity Editor")

    async def connect(self) -> bool:
        """Initiate a TCP connection to Unity Editor.

        Returns:
            True if connected successfully.
        """
        if self.connected:
            return True

        try:
            self._reader, self._writer = await asyncio.open_connection(
                self.host, self.port)
            logger.info(
                f"Connected to Unity Editor at {self.host}:{self.port}")
            self._receive_task = asyncio.create_task(self._receive_loop())
            self._heartbeat_task = asyncio.create_task(self._heartbeat_loop())
            # Warmup: wait briefly for Unity's ReceiveLoop thread to start
            # so the first command doesn't time out.
            await asyncio.sleep(1.0)
            return True
        except (ConnectionRefusedError, OSError) as e:
            logger.debug(f"Connection failed: {e}")
            return False

    # ── Command send ───────────────────────────────────────────────

    async def send_command(
        self, method: str, params: Optional[Dict] = None
    ) -> Dict[str, Any]:
        """Send a command to Unity and wait for the response.

        Args:
            method: Command method name (e.g. "ping", "compile").
            params: Optional parameters dict.

        Returns:
            Response dict with "result" or "error" key.

        Raises:
            RuntimeError: If Unity is not connected.
            asyncio.TimeoutError: If Unity doesn't respond within timeout.
        """
        if not self.connected:
            logger.info("Not connected, attempting reconnect...")
            if not await self.connect():
                raise RuntimeError("Unity Editor is not connected")

        # Allocate a unique request id.
        self._next_request_id += 1
        request_id = str(self._next_request_id)

        request = {
            "id": request_id,
            "method": method,
            "params": params or {},
        }

        future: asyncio.Future = asyncio.get_event_loop().create_future()
        self._pending_requests[request_id] = future

        try:
            # Write under lock so concurrent sends (heartbeat + command)
            # don't interleave their JSON lines.
            async with self._write_lock:
                line = json.dumps(request, ensure_ascii=False,
                                  separators=(',', ':')) + "\n"
                self._writer.write(line.encode("utf-8"))
                await self._writer.drain()

            result = await asyncio.wait_for(future, timeout=CMD_TIMEOUT)
            return result
        except ConnectionError as e:
            raise RuntimeError(str(e)) from None
        finally:
            self._pending_requests.pop(request_id, None)

    # ── Heartbeat ──────────────────────────────────────────────────

    async def _heartbeat_loop(self) -> None:
        """Periodically ping Unity to keep the TCP connection alive.

        Unity's ReceiveTimeout is 60s — we ping every 30s to prevent
        the connection from being dropped due to inactivity.

        Heartbeats are paused during compile polling to avoid
        unnecessary traffic during domain reloads.
        """
        while True:
            await asyncio.sleep(HEARTBEAT_INTERVAL)

            if self._heartbeat_paused:
                logger.debug("Heartbeat skipped (paused)")
                continue

            if not self.connected:
                logger.debug("Heartbeat skipped: not connected")
                continue

            try:
                await asyncio.wait_for(
                    self.send_command("ping"), timeout=PING_TIMEOUT)
                logger.debug("Heartbeat OK")
            except asyncio.TimeoutError:
                logger.warning(
                    "Heartbeat ping timed out, connection may be stale")
            except (ConnectionError, RuntimeError) as e:
                logger.warning(f"Heartbeat ping failed: {e}")
            except asyncio.CancelledError:
                break
            except Exception as e:
                logger.error(f"Heartbeat unexpected error: {e}")

    def _cancel_heartbeat(self) -> None:
        """Cancel the heartbeat task if running."""
        if self._heartbeat_task and not self._heartbeat_task.done():
            self._heartbeat_task.cancel()
        self._heartbeat_task = None

    # ── Receive loop ───────────────────────────────────────────────

    async def _receive_loop(self) -> None:
        """Read newline-delimited JSON lines from Unity."""
        try:
            while self._reader is not None:
                try:
                    line = await self._reader.readline()
                except (ConnectionResetError, BrokenPipeError, OSError):
                    break

                if not line:
                    # EOF — connection closed
                    break

                line_str = line.decode("utf-8").strip()
                if not line_str:
                    continue

                try:
                    data = json.loads(line_str)
                except json.JSONDecodeError:
                    logger.error(
                        f"Invalid JSON from Unity: {line_str[:200]}")
                    continue

                request_id = data.get("id", "")
                future = self._pending_requests.get(request_id)
                if future and not future.done():
                    future.set_result(data)
                else:
                    logger.debug(
                        f"No pending request for response id={request_id}")
        except asyncio.CancelledError:
            pass
        except Exception as e:
            logger.error(f"Receive loop error: {e}")
        finally:
            self._reader = None
            self._writer = None
            self._cancel_heartbeat()
            # Cancel all pending futures so callers don't hang until timeout
            for req_id, future in list(self._pending_requests.items()):
                if not future.done():
                    future.set_exception(
                        ConnectionError(
                            f"Unity disconnected (request {req_id})"))
            self._pending_requests.clear()
            logger.info("Unity Editor disconnected")
