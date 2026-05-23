"""WebSocket bridge: Python server that Unity Editor connects to."""

from __future__ import annotations

import asyncio
import json
import logging
from typing import Any, Dict, Optional

import websockets
from websockets.asyncio.server import ServerConnection

logger = logging.getLogger("unity-mcp.bridge")

DEFAULT_HOST = "localhost"
DEFAULT_PORT = 9876

# Protocol constants
CMD_TIMEOUT = 30.0  # seconds


class UnityBridge:
    """Manages a WebSocket server that Unity connects to as a client.

    The MCP server sends commands through this bridge to Unity,
    and receives responses.
    """

    def __init__(self, host: str = DEFAULT_HOST, port: int = DEFAULT_PORT):
        self.host = host
        self.port = port
        self._server: Optional[websockets.WebSocketServer] = None
        self._unity_connection: Optional[ServerConnection] = None
        self._pending_requests: Dict[str, asyncio.Future] = {}
        self._next_request_id = 0
        self._lock = asyncio.Lock()
        self._receive_task: Optional[asyncio.Task] = None

    @property
    def connected(self) -> bool:
        return self._unity_connection is not None

    async def start(self) -> None:
        """Start the WebSocket server and wait for Unity to connect."""
        self._server = await websockets.serve(
            self._handle_connection,
            self.host,
            self.port,
        )
        logger.info(f"WebSocket server listening on ws://{self.host}:{self.port}")

    async def stop(self) -> None:
        """Stop the WebSocket server."""
        if self._server:
            self._server.close()
            await self._server.wait_closed()
            self._server = None
        self._unity_connection = None
        logger.info("WebSocket server stopped")

    async def wait_for_connection(self, timeout: float = 0) -> bool:
        """Wait until Unity connects.

        Args:
            timeout: Max seconds to wait. 0 = wait forever.
        Returns:
            True if connected, False on timeout.
        """
        if self.connected:
            return True

        deadline = asyncio.get_event_loop().time() + timeout if timeout > 0 else float("inf")
        while not self.connected:
            if asyncio.get_event_loop().time() >= deadline:
                return False
            await asyncio.sleep(0.5)
        return True

    async def send_command(self, method: str, params: Optional[Dict] = None) -> Dict[str, Any]:
        """Send a command to Unity and wait for the response.

        Args:
            method: Command method name (e.g. "ping", "list_hierarchy").
            params: Optional parameters dict.

        Returns:
            Response dict with "result" or "error" key.

        Raises:
            RuntimeError: If Unity is not connected.
            asyncio.TimeoutError: If Unity doesn't respond within timeout.
        """
        if not self.connected:
            raise RuntimeError("Unity Editor is not connected")

        async with self._lock:
            self._next_request_id += 1
            request_id = str(self._next_request_id)

        request = {
            "id": request_id,
            "method": method,
            "params": params or {},
        }

        # Create a future for the response
        future: asyncio.Future = asyncio.get_event_loop().create_future()
        self._pending_requests[request_id] = future

        try:
            # Send the request
            await self._unity_connection.send(json.dumps(request))

            # Wait for response
            result = await asyncio.wait_for(future, timeout=CMD_TIMEOUT)
            return result
        finally:
            self._pending_requests.pop(request_id, None)

    async def _handle_connection(self, websocket: ServerConnection) -> None:
        """Handle a new WebSocket connection from Unity."""
        # If there's already a connection, reject the new one
        if self._unity_connection is not None:
            logger.warning("Rejecting new connection: Unity is already connected")
            await websocket.close(1000, "Already connected")
            return

        self._unity_connection = websocket
        logger.info("Unity Editor connected")

        try:
            await self._receive_loop(websocket)
        except websockets.exceptions.ConnectionClosed:
            logger.info("Unity Editor disconnected")
        finally:
            self._unity_connection = None
            self._receive_task = None

    async def _receive_loop(self, websocket: ServerConnection) -> None:
        """Receive messages from Unity."""
        async for message in websocket:
            try:
                data = json.loads(message)
                request_id = data.get("id", "")
                future = self._pending_requests.get(request_id)
                if future and not future.done():
                    future.set_result(data)
                else:
                    logger.warning(f"No pending request for response id={request_id}")
            except json.JSONDecodeError:
                logger.error(f"Invalid JSON from Unity: {message[:200]}")
