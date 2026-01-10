using System;
using System.Collections.Generic;
using UnityEngine;

namespace GenBall.Map
{
    public class SceneMapIndex : ScriptableObject
    {
        public List<MapConfigChoose> mapConfigChooses = new();
    }

    [Serializable]
    public class MapConfigChoose
    {
        public MapConfig mapConfig;
        public bool selected;
    }
}