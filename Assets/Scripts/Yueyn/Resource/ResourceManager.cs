using System;
using UnityEditor;
using UnityEngine;
using Yueyn.Main;

namespace Yueyn.Resource
{
    public class ResourceManager:MonoBehaviour,IComponent
    {
        public int Priority => 0;
        public void LoadPrefab(string path, Action<object> callback)
        {
            #if UNITY_EDITOR
            GameObject prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            callback?.Invoke(prefab);
            #endif
        }
#if UNITY_EDITOR
        public GameObject LoadPrefab(string path)=>AssetDatabase.LoadAssetAtPath<GameObject>(path);
#endif
        public void Init()
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