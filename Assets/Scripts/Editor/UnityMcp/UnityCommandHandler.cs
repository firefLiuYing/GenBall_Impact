using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Yueyn.Editor.UnityMcp
{
    public static class UnityCommandHandler
    {
        public static CmdResult Execute(string method, Dictionary<string, string> args)
        {
            return method switch
            {
                "ping" => Ping(),
                "list_hierarchy" => ListHierarchy(args),
                _ => CmdResult.Err($"Unknown method: {method}"),
            };
        }

        private static CmdResult Ping()
        {
            return CmdResult.Ok(new Dictionary<string, object>
            {
                ["status"] = "ok",
                ["unityVersion"] = Application.unityVersion,
                ["projectName"] = Application.productName,
            });
        }

        private static CmdResult ListHierarchy(Dictionary<string, string> args)
        {
            args.TryGetValue("prefabPath", out var prefabPath);
            if (string.IsNullOrEmpty(prefabPath))
                return CmdResult.Err("Missing parameter: prefabPath");

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
                return CmdResult.Err($"Prefab not found at: {prefabPath}");

            return CmdResult.Ok(new Dictionary<string, object>
            {
                ["prefabPath"] = prefabPath,
                ["hierarchy"] = BuildTree(prefab.transform),
                ["totalObjects"] = CountAll(prefab.transform),
            });
        }

        private static Dictionary<string, object> BuildTree(Transform t)
        {
            var node = new Dictionary<string, object>
            {
                ["name"] = t.gameObject.name,
                ["active"] = t.gameObject.activeSelf,
            };

            var comps = t.GetComponents<Component>();
            var names = new List<string>();
            foreach (var c in comps)
                if (c != null) names.Add(c.GetType().Name);
            node["components"] = names;

            if (t.childCount > 0)
            {
                var children = new List<Dictionary<string, object>>();
                for (int i = 0; i < t.childCount; i++)
                    children.Add(BuildTree(t.GetChild(i)));
                node["children"] = children;
            }

            return node;
        }

        private static int CountAll(Transform t)
        {
            int n = 1;
            for (int i = 0; i < t.childCount; i++)
                n += CountAll(t.GetChild(i));
            return n;
        }
    }

    public class CmdResult
    {
        public bool Success;
        public object Data;
        public string ErrMsg;

        public static CmdResult Ok(object data) => new() { Success = true, Data = data };
        public static CmdResult Err(string msg) => new() { Success = false, ErrMsg = msg };
    }
}
