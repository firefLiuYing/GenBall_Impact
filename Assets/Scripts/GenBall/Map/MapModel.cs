using System;
using System.Collections.Generic;
using UnityEngine;

namespace GenBall.Map
{
    /// <summary>
    /// 储存已解锁的场景
    /// </summary>
    [Serializable]
    public class MapModel:ScriptableObject
    {
        public List<SceneModel>  scenes = new List<SceneModel>();
    }
    /// <summary>
    /// 储存持久化场景数据
    /// </summary>
    [Serializable]
    public class SceneModel
    {
        public string sceneName;
        public string displayName;
        public List<SavePointModel> savePoints = new();
        public List<EnemyUnitModel> enemyUnits = new();
    }

    /// <summary>
    /// 存档点信息
    /// </summary>
    [Serializable]
    public class SavePointModel
    {
        public int id;
        public string displayName;
        public Vector3 spawnPosition;
        public Quaternion spawnRotation;
    }

    /// <summary>
    /// 地图敌人个体信息
    /// </summary>
    [Serializable]
    public class EnemyUnitModel
    {
        public int id;
        public string enemyType;
        public Vector3 spawnPosition;
        public Quaternion spawnRotation;
    }
}