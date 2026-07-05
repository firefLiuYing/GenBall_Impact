"""Minimal MCP Server over stdio, bridging to Unity via TCP."""

from __future__ import annotations

import asyncio
import json
import logging
import os
import sys
from typing import Any, Dict, List, Optional

from .unity_bridge import UnityBridge

logger = logging.getLogger("unity-mcp")
logger.setLevel(logging.DEBUG)
_handler = logging.StreamHandler(sys.stderr)
_handler.setFormatter(logging.Formatter("[%(name)s] %(levelname)s: %(message)s"))
logger.addHandler(_handler)

# ─── MCP Constants ────────────────────────────────────────────────────
PROTOCOL_VERSION = "2024-11-05"
SERVER_NAME = "unity-mcp"
SERVER_VERSION = "0.1.0"


# ═══════════════════════════════════════════════════════════════════════
#  Tool Registry — decorator-based, extensible
# ═══════════════════════════════════════════════════════════════════════

class _ToolDef:
    """Internal: registered tool metadata + handler."""
    def __init__(self, name: str, description: str, input_schema: dict, handler):
        self.name = name
        self.description = description
        self.input_schema = input_schema
        self.handler = handler


class ToolRegistry:
    """Registry for MCP tools with decorator-based registration.

    Usage::

        @ToolRegistry.register("unity_ping", "Check if Unity...", {...})
        async def tool_ping(bridge, arguments):
            ...

    Adding a new tool only requires **one** async handler with the
    decorator — no changes to dispatch logic, TOOLS list, or any
    other infrastructure.
    """

    _tools: Dict[str, _ToolDef] = {}

    @classmethod
    def register(cls, name: str, description: str, input_schema: dict):
        """Decorator: register an async handler for a named MCP tool.

        Handler signature::

            async def handler(bridge: UnityBridge, arguments: dict) -> dict
        """
        def decorator(func):
            cls._tools[name] = _ToolDef(
                name=name,
                description=description,
                input_schema=input_schema,
                handler=func,
            )
            return func
        return decorator

    @classmethod
    def get_tool_definitions(cls) -> List[Dict]:
        """Return the TOOLS list for MCP ``tools/list`` response."""
        return [
            {
                "name": t.name,
                "description": t.description,
                "inputSchema": t.input_schema,
            }
            for t in cls._tools.values()
        ]

    @classmethod
    async def execute(
        cls, bridge: UnityBridge, name: str, arguments: dict
    ) -> dict:
        """Execute a registered tool.  Raises ValueError if unknown."""
        tool = cls._tools.get(name)
        if tool is None:
            raise ValueError(f"Unknown tool: {name}")
        return await tool.handler(bridge, arguments)


# ═══════════════════════════════════════════════════════════════════════
#  Tool implementations
# ═══════════════════════════════════════════════════════════════════════

@ToolRegistry.register(
    "unity_ping",
    "Check if Unity Editor is connected and responding. Returns status.",
    {"type": "object", "properties": {}, "required": []},
)
async def _tool_ping(bridge: UnityBridge, _arguments: dict) -> dict:
    """Check if Unity is connected."""
    if not bridge.connected and not await bridge.connect():
        return {
            "status": "disconnected",
            "message": "Unity Editor is not connected",
        }
    try:
        response = await bridge.send_command("ping")
        return {
            "status": "ok",
            "unity_status": response.get("result", {}).get("status", "unknown"),
        }
    except RuntimeError as e:
        return {"status": "disconnected", "message": str(e)}


@ToolRegistry.register(
    "unity_compile",
    (
        "Trigger script compilation in Unity and wait for the result. "
        "Returns compilation status including all errors and warnings "
        "(file, line, column, message). May take up to 120s. "
        "Set fullRebuild=true to force a full recompilation of all "
        "assemblies (default: false = incremental)."
    ),
    {
        "type": "object",
        "properties": {
            "fullRebuild": {
                "type": "boolean",
                "description": (
                    "If true, force a full recompilation of all assemblies "
                    "(touches a sentinel + AssetDatabase.Refresh ForceUpdate). "
                    "Default false uses incremental compilation which is "
                    "faster and avoids unnecessary domain reloads."
                ),
            }
        },
        "required": [],
    },
)
async def _tool_compile(bridge: UnityBridge, _arguments: dict) -> dict:
    """Trigger Unity script compilation and wait for result.

    Uses ``Temp/UnityMcpCompileState.json`` as the single source of
    truth.  State file survives domain reloads when TCP is
    disconnected.  Phases: ``compiling → done | no_changes``.

    The initial ``compile`` command is fire-and-forget — we don't wait
    for its TCP response because the domain reload may kill the
    connection before the response arrives.  Instead we poll the state
    file exclusively.
    """
    COMPILE_TIMEOUT = 120.0
    POLL_INTERVAL = 2.0
    full_rebuild = bool(arguments.get("fullRebuild", False))

    bridge.pause_heartbeat()

    try:
        # Build compile params (fullRebuild flag).
        compile_params = {"fullRebuild": "true" if full_rebuild else "false"}

        # 1. Trigger compilation (fire-and-forget with short timeout).
        triggered = False
        try:
            if not bridge.connected:
                await bridge.connect()
            response = await bridge.send_command(
                "compile", compile_params)
            result = response.get("result", {})
            status = result.get("status", "")
            if status in ("compilation_started", "already_compiling"):
                triggered = True
                logger.debug(f"Compile trigger ACK: {status}")
            else:
                logger.debug(f"Compile trigger response: {status}")
        except (RuntimeError, asyncio.TimeoutError) as e:
            logger.debug(f"Compile trigger TCP error (expected): {e}")

        # 2. Wait briefly, then check state file to confirm trigger.
        await asyncio.sleep(1.0)

        if not triggered:
            state = _read_compile_state()
            if state and state.get("phase") in (
                "compiling", "done", "no_changes"):
                logger.debug(
                    "State file confirms trigger: phase=%s", state.get("phase"))
                triggered = True

        if not triggered:
            logger.debug("Retrying compile trigger via TCP...")
            try:
                if not bridge.connected:
                    await bridge.connect()
                response = await bridge.send_command(
                    "compile", compile_params)
                result = response.get("result", {})
                if result.get("status") in (
                    "compilation_started", "already_compiling"):
                    triggered = True
            except Exception:
                pass

        if not triggered:
            return {
                "status": "trigger_failed",
                "message": (
                    "Could not trigger compilation. "
                    "Is Unity Editor responsive?"
                ),
            }

        # 3. Poll state file until terminal phase.
        elapsed = 0.0
        while elapsed < COMPILE_TIMEOUT:
            state = _read_compile_state()
            if state is None:
                logger.debug(
                    "State file not yet created (elapsed=%.0fs)", elapsed)
                await asyncio.sleep(POLL_INTERVAL)
                elapsed += POLL_INTERVAL
                continue

            phase = state.get("phase", "")

            if phase == "done":
                logger.debug("State file reports compile done")
                break
            elif phase == "no_changes":
                logger.debug("State file reports no script changes")
                break

            logger.debug(
                "Compiling... phase=%s errors=%d elapsed=%.0fs",
                phase, len(state.get("errors", [])), elapsed)
            await asyncio.sleep(POLL_INTERVAL)
            elapsed += POLL_INTERVAL
        else:
            return {
                "status": "compilation_timeout",
                "message": (
                    f"Compilation did not finish within {COMPILE_TIMEOUT}s"
                ),
            }

        # 4. Collect results from state file.
        state = _read_compile_state()
        if state is None:
            return {
                "status": "compilation_complete",
                "errorCount": 0,
                "warningCount": 0,
                "errors": [],
                "warnings": [],
            }

        phase = state.get("phase", "")
        errors = state.get("errors", [])
        warnings = state.get("warnings", [])

        if phase == "no_changes":
            return {
                "status": "no_changes",
                "message": (
                    "No script changes detected. Compilation skipped."
                ),
                "errorCount": 0,
                "warningCount": 0,
                "errors": [],
                "warnings": [],
            }

        # Clean up state file on Unity side (best-effort).
        try:
            if not bridge.connected:
                await bridge.connect()
            await asyncio.wait_for(
                bridge.send_command("cleanup_compile_state"),
                timeout=5.0)
        except Exception:
            pass

        return {
            "status": "compilation_complete",
            "errorCount": len(errors),
            "warningCount": len(warnings),
            "errors": errors,
            "warnings": warnings,
        }

    except Exception as e:
        logger.error(f"Compile error: {e}")
        return {"error": str(e)}
    finally:
        bridge.resume_heartbeat()


@ToolRegistry.register(
    "unity_import_asset",
    "Explicitly import a single asset file. Use this after creating new "
    ".cs files to force Unity to detect and compile them.",
    {
        "type": "object",
        "properties": {
            "path": {
                "type": "string",
                "description": (
                    "Project-relative path to the asset, "
                    "e.g. 'Assets/Scripts/Yueyn/Fsm/Editor/SimpleFsmTests.cs'"
                ),
            }
        },
        "required": ["path"],
    },
)
async def _tool_import_asset(bridge: UnityBridge, arguments: dict) -> dict:
    """Explicitly import an asset in Unity."""
    if not bridge.connected and not await bridge.connect():
        return {"error": "Unity Editor is not connected"}

    asset_path = arguments.get("path", "")

    try:
        response = await bridge.send_command("import_asset", {
            "path": asset_path,
        })
        if "error" in response:
            return {"error": response["error"]}
        result = response.get("result", {})
        return {
            "status": result.get("status", "unknown"),
            "path": result.get("path", asset_path),
        }
    except RuntimeError as e:
        return {"error": str(e)}
    except asyncio.TimeoutError:
        return {"error": "Unity did not respond within timeout"}


@ToolRegistry.register(
    "unity_list_prefab_hierarchy",
    (
        "Get the GameObject hierarchy of a Unity prefab. "
        "Returns the tree of child GameObjects with their names."
    ),
    {
        "type": "object",
        "properties": {
            "prefabPath": {
                "type": "string",
                "description": (
                    "Project-relative path to the .prefab file, "
                    "e.g. 'Assets/AssetBundles/UI/MainHud/Form/MainHud.prefab'"
                ),
            }
        },
        "required": ["prefabPath"],
    },
)
async def _tool_list_hierarchy(bridge: UnityBridge, arguments: dict) -> dict:
    """Get prefab GameObject hierarchy from Unity."""
    if not bridge.connected and not await bridge.connect():
        return {"error": "Unity Editor is not connected"}

    prefab_path = arguments.get("prefabPath", "")

    try:
        response = await bridge.send_command("list_hierarchy", {
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


# ─── Helpers ──────────────────────────────────────────────────────────

def _read_compile_state() -> Optional[Dict]:
    """Read Unity compile state file from the project Temp folder."""
    try:
        # Project root is 2 levels up from Tools/UnityMcp/server.py
        project_root = os.path.normpath(
            os.path.join(os.path.dirname(__file__), "..", ".."))
        state_path = os.path.join(
            project_root, "Temp", "UnityMcpCompileState.json")
        if not os.path.exists(state_path):
            return None
        with open(state_path, "r", encoding="utf-8") as f:
            return json.load(f)
    except (json.JSONDecodeError, OSError):
        return None


# ═══════════════════════════════════════════════════════════════════════
#  MCP Server
# ═══════════════════════════════════════════════════════════════════════

class McpServer:
    """Minimal MCP server that handles the protocol over stdio."""

    def __init__(self, bridge: UnityBridge):
        self.bridge = bridge
        self._initialized = False
        self._client_info: Dict = {}

    async def run(self) -> None:
        """Run the MCP server on stdin/stdout."""
        await self.bridge.start()

        logger.info("MCP server ready (stdio)")

        loop = asyncio.get_event_loop()

        while True:
            try:
                line_bytes = await loop.run_in_executor(
                    None, sys.stdin.readline)
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
                    await loop.run_in_executor(
                        None, sys.stdout.write, resp_str)
                    await loop.run_in_executor(None, sys.stdout.flush)
            except json.JSONDecodeError as e:
                logger.error(f"Invalid JSON: {e}")
                error_resp = json.dumps({
                    "jsonrpc": "2.0",
                    "id": None,
                    "error": {
                        "code": -32700,
                        "message": f"Parse error: {e}",
                    },
                }) + "\n"
                await loop.run_in_executor(
                    None, sys.stdout.write, error_resp)
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
                    "error": {
                        "code": -32601,
                        "message": f"Method not found: {method}",
                    },
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
                "capabilities": {"tools": {}},
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
            "result": {"tools": ToolRegistry.get_tool_definitions()},
        }

    async def _handle_tool_call(self, req_id: Any, params: Dict) -> Dict:
        tool_name = params.get("name", "")
        arguments = params.get("arguments", {})

        try:
            result = await ToolRegistry.execute(
                self.bridge, tool_name, arguments)
        except ValueError as e:
            return {
                "jsonrpc": "2.0",
                "id": req_id,
                "error": {"code": -32602, "message": str(e)},
            }

        return {
            "jsonrpc": "2.0",
            "id": req_id,
            "result": {
                "content": [
                    {
                        "type": "text",
                        "text": json.dumps(
                            result, ensure_ascii=False, indent=2),
                    }
                ]
            },
        }


async def main() -> None:
    """Entry point. Reads UNITY_MCP_PORT env var for port override."""
    port = int(os.environ.get("UNITY_MCP_PORT", "9876"))
    bridge = UnityBridge(port=port)
    server = McpServer(bridge)
    await server.run()


if __name__ == "__main__":
    asyncio.run(main())
