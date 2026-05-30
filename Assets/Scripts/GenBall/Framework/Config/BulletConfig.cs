using System;
using System.Collections.Generic;
using UnityEngine;
using GenBall.BattleSystem.Bullets;

namespace GenBall.Framework.Config
{
    /// <summary>
    /// Collection of all bullet configs. Loaded by AppConfigManager as a ScriptableObject from Resources/Configs/.
    /// </summary>
    [CreateAssetMenu(fileName = "BulletConfigCollection", menuName = "GenBall/BulletConfigCollection")]
    public class BulletConfigCollection : ScriptableObject
    {
        public List<BulletConfigEntry> Configs = new List<BulletConfigEntry>();

        private Dictionary<BulletId, BulletConfigEntry> _lookup;

        public void Init()
        {
            _lookup = new Dictionary<BulletId, BulletConfigEntry>();
            foreach (var config in Configs)
            {
                if (config.Id == BulletId.None)
                {
                    Debug.LogWarning("[BulletConfigCollection] Skipping entry with None Id");
                    continue;
                }
                if (_lookup.ContainsKey(config.Id))
                {
                    Debug.LogWarning($"[BulletConfigCollection] Duplicate bullet config Id: {config.Id}");
                    continue;
                }
                _lookup[config.Id] = config;
            }
            Debug.Log($"[BulletConfigCollection] Initialized with {_lookup.Count} bullet configs");
        }

        public BulletConfigEntry Get(BulletId id)
        {
            if (_lookup == null) Init();
            _lookup.TryGetValue(id, out var entry);
            return entry;
        }

        public bool TryGet(BulletId id, out BulletConfigEntry entry)
        {
            if (_lookup == null) Init();
            return _lookup.TryGetValue(id, out entry);
        }
    }
    /// <summary>
    /// Detection mode for bullet hit detection.
    /// </summary>
    public enum DetectionMode
    {
        Ray,
        SphereCast
    }

    /// <summary>
    /// Hit behavior type for the behavior chain.
    /// </summary>
    public enum HitBehaviorType
    {
        DealDamage,
        Penetrate,
        Bounce,
        AOEDamage
    }

    /// <summary>
    /// Movement modifier type.
    /// </summary>
    public enum MovementModifierType
    {
        Gravity
    }

    /// <summary>
    /// Defines a hit behavior in the chain. Ordered execution.
    /// </summary>
    [Serializable]
    public struct HitBehaviorDef
    {
        [Tooltip("行为类型：DealDamage=造成伤害并销毁，Penetrate=穿透，Bounce=反弹，AOEDamage=范围伤害")]
        public HitBehaviorType Type;
        [Tooltip("通用计数参数。Penetrate=最大穿透次数，Bounce=最大反弹次数，AOEDamage=AOE半径")]
        public int Count;
        [Tooltip("通用数值参数。AOEDamage=AOE伤害值。其他类型暂未使用")]
        public float Value;
    }

    /// <summary>
    /// Defines a movement modifier.
    /// </summary>
    [Serializable]
    public struct MovementModifierDef
    {
        [Tooltip("修饰器类型。Gravity=重力加速度")]
        public MovementModifierType Type;
        [Tooltip("修饰器数值。Gravity=重力加速度大小（如10）")]
        public float Value;
    }

    /// <summary>
    /// Entry in the BulletConfigCollection. Defines all parameters for a bullet type.
    /// </summary>
    [Serializable]
    public class BulletConfigEntry
    {
        [Header("Identity")]
        [Tooltip("唯一标识，对应 WeaponAssembly 上的 BulletConfigId")]
        public BulletId Id = BulletId.RayBullet;

        [Header("Detection")]
        [Tooltip("碰撞检测方式：Ray = 射线检测（快速子弹），SphereCast = 球体扫掠（大型弹丸）")]
        public DetectionMode DetectionMode = DetectionMode.Ray;

        [Header("Tuning")]
        [Tooltip("子弹视觉预制体（包含 BulletVisual 组件）")]
        public GameObject VisualPrefab;
        [Tooltip("视觉表现收敛到逻辑位置的时间（秒）。FPS下枪口和准心不在同一点，此参数控制贝塞尔插值速度")]
        public float VisualBlendTime = 0.15f;
        [Tooltip("子弹最大存活时间（秒），超时自动回收")]
        public float MaxLifetime = 3f;

        [Header("Hit Behavior Chain (ordered)")]
        [Tooltip("命中行为链，按数组顺序执行。DealDamage=命中造成伤害后销毁，Penetrate=穿透N次，Bounce=反弹N次，AOEDamage=命中点范围伤害")]
        public HitBehaviorDef[] HitBehaviors = new HitBehaviorDef[]
        {
            new HitBehaviorDef { Type = HitBehaviorType.DealDamage, Count = 0, Value = 0f }
        };

        [Header("Movement Modifiers")]
        [Tooltip("移动修饰器列表。Gravity=每帧施加向下的重力加速度")]
        public MovementModifierDef[] MovementModifiers = new MovementModifierDef[0];
    }

    
}
