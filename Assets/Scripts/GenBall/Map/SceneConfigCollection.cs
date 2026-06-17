using System;
using System.Collections.Generic;
using UnityEngine;

namespace GenBall.Map
{
    /// <summary>
    /// Global scene configuration collection.
    /// One asset per project, stored at Assets/Resources/Configs/SceneConfigCollection.asset.
    /// Loaded at runtime via IConfigProvider.GetConfig&lt;SceneConfigCollection&gt;().
    /// </summary>
    public class SceneConfigCollection : ScriptableObject
    {
        public List<SceneConfigEntry> scenes = new();
    }

    /// <summary>
    /// Per-scene configuration entry containing all placeable data for one scene.
    /// </summary>
    [Serializable]
    public class SceneConfigEntry
    {
        public string sceneName;
        public string displayName;
        public List<SavePointData> savePoints = new();
        public List<EnemySpawnData> enemySpawns = new();
        public List<SceneTriggerData> triggers = new();
        public List<MechanismData> mechanisms = new();
    }

    /// <summary>
    /// Baked data for a save point / checkpoint.
    /// </summary>
    [Serializable]
    public class SavePointData
    {
        public int id;
        public string displayName;
        public Vector3 position;
        public Quaternion rotation;
    }

    /// <summary>
    /// Baked data for an enemy spawn point.
    /// </summary>
    [Serializable]
    public class EnemySpawnData
    {
        public int id;
        public string enemyType;
        public Vector3 position;
        public Quaternion rotation;
        /// <summary>Patrol wander radius in meters. 0 = stationary.</summary>
        public float patrolRadius;
        /// <summary>Player detection radius for entering combat state.</summary>
        public float detectRadius;
        /// <summary>AI behavior mode: Melee (=0) or Ranged (=1).</summary>
        public int aiBehavior;
    }

    /// <summary>
    /// Baked data for a scene event trigger.
    /// </summary>
    [Serializable]
    public class SceneTriggerData
    {
        public int id;
        public string triggerName;
        public string eventName;
        public Vector3 position;
        public float radius;
        public TriggerActivationType activationType;
    }

    /// <summary>
    /// How a scene trigger activates.
    /// </summary>
    public enum TriggerActivationType
    {
        OnEnter = 0,
        OnExit = 1,
        OnInteract = 2,
    }

    /// <summary>
    /// Baked data for a mechanism / interactable object.
    /// </summary>
    [Serializable]
    public class MechanismData
    {
        public int id;
        public string mechanismName;
        public string mechanismType;
        public Vector3 position;
        public Quaternion rotation;
        /// <summary>Optional JSON string for type-specific configuration.</summary>
        public string customDataJson;
    }
}
