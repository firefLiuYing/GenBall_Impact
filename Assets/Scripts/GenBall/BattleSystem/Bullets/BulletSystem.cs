using System.Collections.Generic;
using GenBall.BattleSystem.Buff;
using JetBrains.Annotations;
using UnityEngine;
using Yueyn.Base.ReferencePool;

namespace GenBall.BattleSystem.Bullets
{
    public class BulletSystem : IBulletSystem
    {
        // ��Ҫ�ɵ������У������ӵ��������ӵ��������ӵ���������
        
        /// <summary>
        /// �����ӵ�ͳһ����
        /// </summary>
        /// <param name="info"></param>
        public void FireBullet([NotNull] BulletLaunchInfo info)
        {
            var bulletState=info.Model.Id.Create();
            bulletState.Init(info);
            var sourceBuffContainer = info.Source?.GetComponent<IBuffContainer>();
            if (sourceBuffContainer != null)
            {
                sourceBuffContainer.GetBuffs<ITriggerBeforeFireBullet>(out var beforeFireBuffs);
                foreach (var beforeFireBuff in beforeFireBuffs)
                {
                    beforeFireBuff.TriggerBeforeFireBullet(info);
                }
                beforeFireBuffs.ReleaseBuffList();
            }

            bulletState.GetBuffs<ITriggerBeforeBulletBeFired>(out var beforeBulletBeFiredBuffs);
            foreach (var beforeBulletBeFiredBuff in beforeBulletBeFiredBuffs)
            {
                beforeBulletBeFiredBuff.TriggerBeforeBulletBeFired(info);
            }
            beforeBulletBeFiredBuffs.ReleaseBuffList();
            // ʵ�ʷ���
            bulletState.Fire();
            if (sourceBuffContainer != null)
            {
                sourceBuffContainer.GetBuffs<ITriggerAfterFireBullet>(out var afterFireBulletBuffs);
                foreach (var afterFireBulletBuff in afterFireBulletBuffs)
                {
                    afterFireBulletBuff.TriggerAfterFireBullet(info);
                }
                afterFireBulletBuffs.ReleaseBuffList();
            }

            bulletState.GetBuffs<ITriggerAfterBulletBeFired>(out var afterBulletBeFiredBuffs);
            foreach (var afterBulletBeFiredBuff in afterBulletBeFiredBuffs)
            {
                afterBulletBeFiredBuff.TriggerAfterBulletBeFired(info);
            }
            afterBulletBeFiredBuffs.ReleaseBuffList();
            
            // ���մ�����Ϣ
            ReferencePool.Release(info);
        }

        public void RecycleBullet(BulletState bulletState)
        {
            Object.Destroy(bulletState.gameObject);
        }
        public void Init()
        {

        }

        public void UnInit()
        {

        }
    }
    
    public class BulletLaunchInfo:IReference
    {
        /// <summary>
        /// �����ӵ������壬����Ϊ��
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