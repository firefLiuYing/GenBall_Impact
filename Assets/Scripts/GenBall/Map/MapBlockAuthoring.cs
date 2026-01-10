using System;
using UnityEngine;

namespace GenBall.Map
{
    public class MapBlockAuthoring : MonoBehaviour
    {
        public Bounds bounds;
        
        public void AddMapBlock()
        {
            if (!TryGetComponent<MapBlockBase>(out var block))
            {
                gameObject.AddComponent<DefaultMapBlock>();
            }
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(bounds.center, bounds.size);    
        }
        
        #endif
    }
}