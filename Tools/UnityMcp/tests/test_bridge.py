"""Integration test: start MCP server, connect mock Unity client, verify bridge."""

import asyncio
import json
import subprocess
import sys

import websockets


async def mock_unity_client(ws_url: str):
    """Simulate Unity Editor connecting and responding to commands."""
    print("[MockUnity] Connecting...")
    async with websockets.connect(ws_url) as ws:
        print("[MockUnity] Connected")

        async for msg_text in ws:
            msg = json.loads(msg_text)
            method = msg.get("method", "")
            req_id = msg.get("id", "")
            print(f"[MockUnity] Received: {method}")

            if method == "ping":
                response = {
                    "id": req_id,
                    "result": {"status": "ok", "unityVersion": "2022.3.fake",
                               "projectName": "GenBall_Impact"}
                }
            elif method == "list_hierarchy":
                response = {
                    "id": req_id,
                    "result": {
                        "hierarchy": {
                            "name": "MainHud",
                            "components": ["RectTransform", "Canvas", "CanvasScaler"],
                            "children": [
                                {"name": "TxtKills", "components": ["RectTransform", "CanvasRenderer", "Text"]},
                                {"name": "TxtLevel", "components": ["RectTransform", "CanvasRenderer", "Text"]},
                            ]
                        },
                        "totalObjects": 3
                    }
                }
            else:
                response = {"id": req_id, "error": {"message": f"Unknown method: {method}"}}

            await ws.send(json.dumps(response))
            print(f"[MockUnity] Sent response for {method}")


def _wl(proc, data: str):
    """Write a line to subprocess stdin."""
    proc.stdin.write((data + "\n").encode())
    return asyncio.ensure_future(proc.stdin.drain())


async def test_full_flow():
    """Full integration test."""
    print("=" * 50)
    print("Integration Test: MCP Server + Mock Unity Bridge")
    print("=" * 50)

    server_cmd = [sys.executable, "-m", "Tools.UnityMcp"]
    print(f"\n[Test] Starting server: {' '.join(server_cmd)}")

    server_proc = await asyncio.create_subprocess_exec(
        *server_cmd,
        stdin=subprocess.PIPE,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
    )
    await asyncio.sleep(1)

    # Connect mock Unity
    ws_task = asyncio.create_task(mock_unity_client("ws://localhost:9876"))
    await asyncio.sleep(0.5)

    # MCP initialize
    print("\n[Test] Sending MCP initialize...")
    await _wl(server_proc, json.dumps({
        "jsonrpc": "2.0", "id": 1, "method": "initialize",
        "params": {"protocolVersion": "2024-11-05", "capabilities": {},
                    "clientInfo": {"name": "test", "version": "1"}}
    }))

    line = await asyncio.wait_for(server_proc.stdout.readline(), timeout=5)
    print(f"[Test] Init: {line.decode().strip()[:80]}...")

    # Initialized notification
    await _wl(server_proc, json.dumps({
        "jsonrpc": "2.0", "id": 2, "method": "notifications/initialized", "params": {}}))

    # tools/list
    await _wl(server_proc, json.dumps({"jsonrpc":"2.0","id":3,"method":"tools/list","params":{}}))
    line = await asyncio.wait_for(server_proc.stdout.readline(), timeout=5)
    tools_count = len(json.loads(line).get("result", {}).get("tools", []))
    print(f"[Test] Tools/list: {tools_count} tools")

    # unity_ping (MockUnity is connected!)
    await _wl(server_proc, json.dumps({
        "jsonrpc":"2.0","id":4,"method":"tools/call",
        "params":{"name":"unity_ping","arguments":{}}}))
    line = await asyncio.wait_for(server_proc.stdout.readline(), timeout=5)
    ping_data = json.loads(line)
    ping_result = json.loads(ping_data["result"]["content"][0]["text"])
    print(f"[Test] unity_ping: {ping_result['status']}")

    # unity_list_prefab_hierarchy
    await _wl(server_proc, json.dumps({
        "jsonrpc":"2.0","id":5,"method":"tools/call",
        "params":{"name":"unity_list_prefab_hierarchy",
                   "arguments":{"prefabPath":"Assets/Fake.prefab"}}}))
    line = await asyncio.wait_for(server_proc.stdout.readline(), timeout=5)
    hierarchy_data = json.loads(line)
    hierarchy_result = json.loads(hierarchy_data["result"]["content"][0]["text"])
    total = hierarchy_result.get("totalObjects", 0)
    print(f"[Test] unity_list_prefab_hierarchy: {total} objects")

    # Results
    if ping_result["status"] == "ok":
        print("\n[Test] PASS: Bridge relay works correctly!")
    else:
        print(f"\n[Test] FAIL: Expected 'ok', got '{ping_result['status']}'")

    # Cleanup
    server_proc.stdin.close()
    server_proc.terminate()
    ws_task.cancel()
    await server_proc.wait()
    print("[Test] Done")


if __name__ == "__main__":
    asyncio.run(test_full_flow())
