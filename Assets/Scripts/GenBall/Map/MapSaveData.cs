using System;
using System.Collections.Generic;

namespace GenBall.Map
{
    [Serializable]
    public class MapSaveData
    {
        public List<SceneSaveData> unlockedScenes = new();
    }

    [Serializable]
    public class SceneSaveData
    {
        public string sceneName;
        public List<int> unlockedSavePoints=new();
    }
}