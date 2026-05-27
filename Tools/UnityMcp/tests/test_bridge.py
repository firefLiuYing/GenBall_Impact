"""Integration test: start MCP server + mock Unity TCP server, verify bridge."""

import asyncio
import json
import os
import subprocess
import sys

# Use a different port so tests don't conflict with a running Unity Editor
TEST_PORT = 19876


async def mock_unity_server(port: int = 9876):
    """Simulate Unity Editor as a TCP server responding to commands."""
    async def handle_client(reader: asyncio.StreamReader,
                            writer: asyncio.StreamWriter) -> None:
        print("[MockUnity] Client connected")
        while True:
            try:
                line = await reader.readline()
            except (ConnectionResetError, BrokenPipeError):
                break

            if not line:
                break

            msg = json.loads(line.decode("utf-8").strip())
            method = msg.get("method", "")
            req_id = msg.get("id", "")
            print(f"[MockUnity] Received: {method}")

            if method == "ping":
                response = {
                    "id": req_id,
                    "result": {
                        "status": "ok",
                        "unityVersion": "2022.3.fake",
                        "projectName": "GenBall_Impact",
                    }
                }
            elif method == "compile":
                response = {
                    "id": req_id,
                    "result": {
                        "status": "refresh_triggered",
                        "message": "AssetDatabase.Refresh() called.",
                    }
                }
            elif method == "list_hierarchy":
                response = {
                    "id": req_id,
                    "result": {
                        "hierarchy": {
                            "name": "MainHud",
                            "components": ["RectTransform", "Canvas", "CanvasScaler"],
                            "children": [
                                {
                                    "name": "TxtKills",
                                    "components": ["RectTransform", "CanvasRenderer", "Text"],
                                },
                                {
                                    "name": "TxtLevel",
                                    "components": ["RectTransform", "CanvasRenderer", "Text"],
                                },
                            ],
                        },
                        "totalObjects": 3,
                    }
                }
            else:
                response = {
                    "id": req_id,
                    "error": {"message": f"Unknown method: {method}"},
                }

            writer.write((json.dumps(response, ensure_ascii=False) + "\n").encode())
            await writer.drain()
            print(f"[MockUnity] Sent response for {method}")

        print("[MockUnity] Client disconnected")
        writer.close()
        await writer.wait_closed()

    server = await asyncio.start_server(
        handle_client, "localhost", port)
    print(f"[MockUnity] Listening on port {port}")
    return server


def _wl(proc, data: str):
    """Write a line to subprocess stdin."""
    proc.stdin.write((data + "\n").encode())
    return asyncio.ensure_future(proc.stdin.drain())


async def test_full_flow():
    """Full integration test."""
    print("=" * 50)
    print("Integration Test: MCP Server + Mock Unity TCP Server")
    print("=" * 50)

    # Start mock Unity TCP server first (so Python can connect to it)
    mock_server = await mock_unity_server(TEST_PORT)
    await asyncio.sleep(0.3)

    server_cmd = [sys.executable, "-m", "Tools.UnityMcp"]
    print(f"\n[Test] Starting MCP server: {' '.join(server_cmd)}")

    env = {**os.environ, "UNITY_MCP_PORT": str(TEST_PORT)}
    server_proc = await asyncio.create_subprocess_exec(
        *server_cmd,
        stdin=subprocess.PIPE,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        env=env,
    )

    # Background task to read stderr so the pipe doesn't fill up
    async def read_stderr():
        while True:
            line = await server_proc.stderr.readline()
            if not line:
                break
            print(f"[Server stderr] {line.decode('utf-8', errors='replace').strip()}")

    stderr_task = asyncio.create_task(read_stderr())
    await asyncio.sleep(1)

    # MCP initialize
    print("\n[Test] Sending MCP initialize...")
    await _wl(server_proc, json.dumps({
        "jsonrpc": "2.0", "id": 1, "method": "initialize",
        "params": {"protocolVersion": "2024-11-05", "capabilities": {},
                    "clientInfo": {"name": "test", "version": "1"}}
    }))

    line = await asyncio.wait_for(server_proc.stdout.readline(), timeout=10)
    print(f"[Test] Init: {line.decode().strip()[:80]}...")

    # Initialized notification
    await _wl(server_proc, json.dumps({
        "jsonrpc": "2.0", "id": 2, "method": "notifications/initialized", "params": {}}))

    # tools/list
    await _wl(server_proc, json.dumps({
        "jsonrpc": "2.0", "id": 3, "method": "tools/list", "params": {}}))
    line = await asyncio.wait_for(server_proc.stdout.readline(), timeout=10)
    tools = json.loads(line).get("result", {}).get("tools", [])
    tools_count = len(tools)
    print(f"[Test] Tools/list: {tools_count} tools")
    assert tools_count == 3, f"Expected 3 tools, got {tools_count}"

    # unity_ping
    await _wl(server_proc, json.dumps({
        "jsonrpc": "2.0", "id": 4, "method": "tools/call",
        "params": {"name": "unity_ping", "arguments": {}}}))
    line = await asyncio.wait_for(server_proc.stdout.readline(), timeout=10)
    ping_data = json.loads(line)
    ping_result = json.loads(ping_data["result"]["content"][0]["text"])
    print(f"[Test] unity_ping: {ping_result['status']}")

    # unity_compile
    await _wl(server_proc, json.dumps({
        "jsonrpc": "2.0", "id": 5, "method": "tools/call",
        "params": {"name": "unity_compile", "arguments": {}}}))
    line = await asyncio.wait_for(server_proc.stdout.readline(), timeout=10)
    compile_data = json.loads(line)
    compile_result = json.loads(compile_data["result"]["content"][0]["text"])
    print(f"[Test] unity_compile: {compile_result.get('status')}")

    await asyncio.sleep(0.2)

    # unity_list_prefab_hierarchy
    hierarchy_result = {"totalObjects": 0}
    await _wl(server_proc, json.dumps({
        "jsonrpc": "2.0", "id": 6, "method": "tools/call",
        "params": {"name": "unity_list_prefab_hierarchy",
                   "arguments": {"prefabPath": "Assets/Fake.prefab"}}}))
    line = await asyncio.wait_for(server_proc.stdout.readline(), timeout=10)
    hierarchy_data = json.loads(line)
    if "error" in hierarchy_data:
        print(f"[Test] hierarchy error: {hierarchy_data['error']}")
    else:
        hierarchy_result = json.loads(hierarchy_data["result"]["content"][0]["text"])
        total = hierarchy_result.get("totalObjects", 0)
        print(f"[Test] unity_list_prefab_hierarchy: {total} objects")

    # Results
    if (ping_result.get("status") == "ok" and
            compile_result.get("status") == "refresh_triggered" and
            hierarchy_result.get("totalObjects") == 3):
        print("\n[Test] PASS: All bridge relay tests passed!")
    else:
        print(f"\n[Test] FAIL: ping={ping_result.get('status')}, "
              f"compile={compile_result.get('status')}, "
              f"hierarchy_total={hierarchy_result.get('totalObjects', 0)}")

    # Cleanup
    server_proc.stdin.close()
    server_proc.terminate()
    stderr_task.cancel()
    mock_server.close()
    await mock_server.wait_closed()
    await server_proc.wait()
    print("[Test] Done")


if __name__ == "__main__":
    asyncio.run(test_full_flow())
