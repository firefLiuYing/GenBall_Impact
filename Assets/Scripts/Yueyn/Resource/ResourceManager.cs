using System;
using UnityEditor;
using UnityEngine;

namespace Yueyn.Resource
{
    public class ResourceManager
    {
        public void LoadPrefab(string path, Action<object> callback)
        {
            GameObject prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            callback?.Invoke(prefab);
        }
    }
}