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

        public GameObject LoadPrefab(string path)=>AssetDatabase.LoadAssetAtPath<GameObject>(path);

        public void OnRegister()
        {
            
        }

        public void OnUnregister()
        {
            
        }

        public void ComponentUpdate(float elapsedSeconds, float realElapseSeconds)
        {
            
        }

        public void ComponentFixedUpdate(float fixedDeltaTime)
        {
            
        }

        public void Shutdown()
        {
            
        }
    }
}