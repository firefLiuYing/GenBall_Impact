using System;
using System.Collections.Generic;
using GenBall.Utils.Attributes.InspectorButton;
using UnityEngine;


namespace GenBall.Map
{
    [CreateAssetMenu(fileName = "MapConfig", menuName = "Map/MapConfig")]
    [Serializable]
    public class MapConfig : ScriptableObject
    {
        public List<MapBlockConfig> mapBlockConfigs=new();
    }

    [Serializable]
    public class MapBlockConfig 
    {
        public int mapBlockIndex;

        public List<int> neighbors;
        
        public string mapBlockPrefabPath;

        public string BlockName=> $"Block_{mapBlockIndex}";
    }
}