using System;
using System.Collections.Generic;
using System.Linq;
using GenBall.Utils.Attributes.InspectorButton;
using UnityEngine;


namespace GenBall.Map
{
    [CreateAssetMenu(fileName = "MapConfig", menuName = "Map/MapConfig")]
    [Serializable]
    public class MapConfig : ScriptableObject
    {
        public string sceneName;
        public string sceneDisplayName;
        public List<MapBlockConfig> mapBlockConfigs=new();
        
        public List<SavePointInfo>  savePointInfos = new List<SavePointInfo>();
    }

    [Serializable]
    public class MapBlockConfig 
    {
        public int mapBlockIndex;

        public List<int> neighbors;
        
        public List<Bounds> multiBounds;
        
        public string mapBlockPrefabPath;

        public string BlockName=> $"Block_{mapBlockIndex}";
    }
    
    [Serializable]
    public class SavePointInfo
    {
        public string savePointName;
        public int index;
        public int mapBlockIndex;
        public Vector3 playerSpawnPosition;
        public Quaternion playerSpawnRotation;
    }
}