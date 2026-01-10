using System;
using UnityEngine;

namespace GenBall.Map
{
    public class MapBlockAuthoring : MonoBehaviour
    {
        public void AddMapBlock()
        {
            if (!TryGetComponent<MapBlockBase>(out var block))
            {
                gameObject.AddComponent<DefaultMapBlock>();
            }
        }
    }
}