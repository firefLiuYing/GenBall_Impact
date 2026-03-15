using System.Collections.Generic;
using System.Runtime.InteropServices;
using GenBall.BattleSystem.Buff;
using GenBall.Utils.EntityCreator;
using JetBrains.Annotations;
using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.Main;

namespace GenBall.BattleSystem.Bullets
{
    public class BulletSystem:MonoBehaviour,IComponent
    {
        public int Priority => 1000;
        // 需要干的事情有，生成子弹，销毁子弹，管理子弹生命周期
        
        /// <summary>
        /// 发射子弹统一方法
        /// </summary>
        /// <param name="info"></param>
        public void FireBullet([NotNull] BulletLaunchInfo info)
        {
            var bulletState=info.Model.Id.Create();
            bulletState.Init(info);
            var sourceBuffContainer = info.Source?.GetComponent<IBuffContainer>();
            if (sourceBuffContainer != null)
            {
                var beforeFireBuffs = sourceBuffContainer.GetBuffs<ITriggerBeforeFireBullet>();
                foreach (var beforeFireBuff in beforeFireBuffs)
                {
                    beforeFireBuff.TriggerBeforeFireBullet(info);
                }
                beforeFireBuffs.Clear();
            }

            var beforeBulletBeFiredBuffs = bulletState.GetBuffs<ITriggerBeforeBulletBeFired>();
            foreach (var beforeBulletBeFiredBuff in beforeBulletBeFiredBuffs)
            {
                beforeBulletBeFiredBuff.TriggerBeforeBulletBeFired(info);
            }
            beforeBulletBeFiredBuffs.Clear();
            // 实际发射
            bulletState.Fire();
            if (sourceBuffContainer != null)
            {
                var afterFireBulletBuffs = sourceBuffContainer.GetBuffs<ITriggerAfterFireBullet>();
                foreach (var afterFireBulletBuff in afterFireBulletBuffs)
                {
                    afterFireBulletBuff.TriggerAfterFireBullet(info);
                }
                afterFireBulletBuffs.Clear();
            }

            var afterBulletBeFiredBuffs = bulletState.GetBuffs<ITriggerAfterBulletBeFired>();
            foreach (var afterBulletBeFiredBuff in afterBulletBeFiredBuffs)
            {
                afterBulletBeFiredBuff.TriggerAfterBulletBeFired(info);
            }
            afterBulletBeFiredBuffs.Clear();
            
            // 回收创建信息
            ReferencePool.Release(info);
        }

        public void RecycleBullet(BulletState bulletState)
        {
            GameEntry.GetModule<EntityCreator<BulletState>>().RecycleEntity(bulletState.gameObject);
        }
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
    
    public class BulletLaunchInfo:IReference
    {
        /// <summary>
        /// 发射子弹的物体，可以为空
        /// </summary>
        public GameObject Source;

        public BulletModel Model;
        public Vector3 LogicSpawnPoint;
        public Vector3 RendererSpawnPoint;
        public Vector3 SpawnDirection;

        public static BulletLaunchInfo Create(BulletModel model, Vector3 logicSpawnPoint,Vector3 rendererSpawnPoint, Vector3 spawnDirection,GameObject source=null)
        {
            var info=ReferencePool.Acquire<BulletLaunchInfo>();
            info.Source = source;
            info.Model = model;
            info.LogicSpawnPoint = logicSpawnPoint;
            info.RendererSpawnPoint=rendererSpawnPoint;
            info.SpawnDirection = spawnDirection;
            return info;
        }
        public void Clear()
        {
            Source = null;
            LogicSpawnPoint = Vector3.zero;
            RendererSpawnPoint = Vector3.zero;
            SpawnDirection = Vector3.zero;
        }
    }
}