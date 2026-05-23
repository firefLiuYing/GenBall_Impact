"""Minimal MCP Server over stdio, bridging to Unity via WebSocket.

Implements the Model Context Protocol (MCP) without external SDK dependencies.
Just JSON-RPC 2.0 over stdin/stdout.
"""

from __future__ import annotations

import asyncio
import json
import logging
import sys
from typing import Any, Dict, List, Optional

from .unity_bridge import UnityBridge

logger = logging.getLogger("unity-mcp")
logger.setLevel(logging.DEBUG)
handler = logging.StreamHandler(sys.stderr)
handler.setFormatter(logging.Formatter("[%(name)s] %(levelname)s: %(message)s"))
logger.addHandler(handler)

# ─── MCP Constants ────────────────────────────────────────────────────
PROTOCOL_VERSION = "2024-11-05"
SERVER_NAME = "unity-mcp"
SERVER_VERSION = "0.1.0"

# ─── Tool Definitions ─────────────────────────────────────────────────

TOOLS = [
    {
        "name": "unity_ping",
        "description": "Check if Unity Editor is connected and responding. Returns status.",
        "inputSchema": {
            "type": "object",
            "properties": {},
            "required": [],
        },
    },
    {
        "name": "unity_list_prefab_hierarchy",
        "description": (
            "Get the GameObject hierarchy of a Unity prefab. "
            "Returns the tree of child GameObjects with their names."
        ),
        "inputSchema": {
            "type": "object",
            "properties": {
                "prefabPath": {
                    "type": "string",
                    "description": "Project-relative path to the .prefab file, "
                    "e.g. 'Assets/AssetBundles/UI/MainHud/Form/MainHud.prefab'",
                }
            },
            "required": ["prefabPath"],
        },
    },
]


class McpServer:
    """Minimal MCP server that handles the protocol over stdio."""

    def __init__(self, bridge: UnityBridge):
        self.bridge = bridge
        self._initialized = False
        self._client_info: Dict = {}

    async def run(self) -> None:
        """Run the MCP server on stdin/stdout."""
        # Start the WebSocket bridge for Unity
        await self.bridge.start()

        logger.info("MCP server ready (stdio)")

        # Use run_in_executor for stdio to avoid Windows asyncio pipe issues
        loop = asyncio.get_event_loop()

        while True:
            try:
                # Read line from stdin (blocking, run in thread pool)
                line_bytes = await loop.run_in_executor(None, sys.stdin.readline)
                if not line_bytes:
                    logger.info("stdin closed, shutting down")
                    break

                line_str = line_bytes.strip()
                if not line_str:
                    continue

                request = json.loads(line_str)
                response = await self._handle_request(request)

                if response is not None:
                    resp_str = json.dumps(response) + "\n"
                    await loop.run_in_executor(None, sys.stdout.write, resp_str)
                    await loop.run_in_executor(None, sys.stdout.flush)
            except json.JSONDecodeError as e:
                logger.error(f"Invalid JSON: {e}")
                error_resp = json.dumps({
                    "jsonrpc": "2.0",
                    "id": None,
                    "error": {"code": -32700, "message": f"Parse error: {e}"},
                }) + "\n"
                await loop.run_in_executor(None, sys.stdout.write, error_resp)
                await loop.run_in_executor(None, sys.stdout.flush)
            except Exception as e:
                logger.error(f"Unexpected error: {e}")

        await self.bridge.stop()

    async def _handle_request(self, request: Dict) -> Optional[Dict]:
        """Handle a single JSON-RPC request."""
        req_id = request.get("id")
        method = request.get("method", "")
        params = request.get("params", {})

        logger.debug(f"← MCP: {method} id={req_id}")

        try:
            if method == "initialize":
                return self._handle_initialize(req_id, params)
            elif method == "notifications/initialized":
                self._initialized = True
                logger.info("MCP initialized")
                return None  # No response for notifications
            elif method == "tools/list":
                return self._handle_tools_list(req_id)
            elif method == "tools/call":
                return await self._handle_tool_call(req_id, params)
            elif method == "ping":
                return {"jsonrpc": "2.0", "id": req_id, "result": {}}
            else:
                return {
                    "jsonrpc": "2.0",
                    "id": req_id,
                    "error": {"code": -32601, "message": f"Method not found: {method}"},
                }
        except Exception as e:
            logger.error(f"Error handling {method}: {e}")
            return {
                "jsonrpc": "2.0",
                "id": req_id,
                "error": {"code": -32603, "message": str(e)},
            }

    def _handle_initialize(self, req_id: Any, params: Dict) -> Dict:
        self._client_info = params.get("clientInfo", {})
        return {
            "jsonrpc": "2.0",
            "id": req_id,
            "result": {
                "protocolVersion": PROTOCOL_VERSION,
                "capabilities": {
                    "tools": {},
                },
                "serverInfo": {
                    "name": SERVER_NAME,
                    "version": SERVER_VERSION,
                },
            },
        }

    def _handle_tools_list(self, req_id: Any) -> Dict:
        return {
            "jsonrpc": "2.0",
            "id": req_id,
            "result": {"tools": TOOLS},
        }

    async def _handle_tool_call(self, req_id: Any, params: Dict) -> Dict:
        tool_name = params.get("name", "")
        arguments = params.get("arguments", {})

        if tool_name == "unity_ping":
            result = await self._tool_ping()
        elif tool_name == "unity_list_prefab_hierarchy":
            result = await self._tool_list_hierarchy(**arguments)
        else:
            return {
                "jsonrpc": "2.0",
                "id": req_id,
                "error": {"code": -32602, "message": f"Unknown tool: {tool_name}"},
            }

        return {
            "jsonrpc": "2.0",
            "id": req_id,
            "result": {
                "content": [
                    {
                        "type": "text",
                        "text": json.dumps(result, ensure_ascii=False, indent=2),
                    }
                ]
            },
        }

    # ─── Tool Implementations ────────────────────────────────────────

    async def _tool_ping(self) -> Dict:
        """Check if Unity is connected."""
        if not self.bridge.connected:
            return {"status": "disconnected", "message": "Unity Editor is not connected"}
        try:
            response = await self.bridge.send_command("ping")
            return {
                "status": "ok",
                "unity_status": response.get("result", {}).get("status", "unknown"),
            }
        except RuntimeError as e:
            return {"status": "disconnected", "message": str(e)}

    async def _tool_list_hierarchy(self, prefab_path: str) -> Dict:
        """Get prefab GameObject hierarchy from Unity."""
        if not self.bridge.connected:
            return {"error": "Unity Editor is not connected"}

        try:
            response = await self.bridge.send_command("list_hierarchy", {
                "prefabPath": prefab_path,
            })

            if "error" in response:
                return {"error": response["error"]}

            result = response.get("result", {})
            return {
                "prefabPath": prefab_path,
                "hierarchy": result.get("hierarchy", {}),
                "totalObjects": result.get("totalObjects", 0),
            }
        except RuntimeError as e:
            return {"error": str(e)}
        except asyncio.TimeoutError:
            return {"error": "Unity did not respond within timeout"}


async def main() -> None:
    """Entry point."""
    bridge = UnityBridge()
    server = McpServer(bridge)
    await server.run()


if __name__ == "__main__":
    asyncio.run(main())
