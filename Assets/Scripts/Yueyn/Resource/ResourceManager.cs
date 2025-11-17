using System;
using UnityEditor;
using UnityEngine;
using Yueyn.Main;

namespace Yueyn.Resource
{
    public class ResourceManager:IComponent
    {
        public void LoadPrefab(string path, Action<object> callback)
        {
            GameObject prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            callback?.Invoke(prefab);
        }

        public object LoadPrefab(string path)=>AssetDatabase.LoadAssetAtPath<GameObject>(path);

        public void OnRegister()
        {
            
        }

        public void OnUnregister()
        {
            
        }

        public void Update(float elapsedSeconds, float realElapseSeconds)
        {
            
        }

        public void FixedUpdate(float fixedDeltaTime)
        {
            
        }

        public void Shutdown()
        {
            
        }
    }
}