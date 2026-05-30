using System.Collections.Generic;
using GenBall.BattleSystem.Buff;
using GenBall.Framework.Config;
using JetBrains.Annotations;
using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.Main;

namespace GenBall.BattleSystem.Bullets
{
    /// <summary>
    /// Bullet system implementation. Manages bullet creation, pooling, and lifecycle.
    /// BulletInstance is pooled via internal Stack. BulletVisual is GameObject-pooled via a secondary pool.
    /// </summary>
    public class BulletSystem : IBulletSystem
    {
        private const int PoolCapacity = 200;
        private Stack<BulletInstance> _bulletPool;
        private readonly Dictionary<string, GameObject> _visualPrefabs = new();
        private readonly Queue<BulletVisual> _visualPool = new();

        public void Init()
        {
            _bulletPool = new Stack<BulletInstance>(PoolCapacity);
            Debug.Log("[BulletSystem] Initialized bullet pool");
        }

        public void UnInit()
        {
            _bulletPool?.Clear();
            _bulletPool = null;
            _visualPrefabs.Clear();
            _visualPool.Clear();
        }

        // ======== New API ========

        public void FireBullet(BulletFireParams fireParams)
        {
            // Look up bullet config
            var configProvider = SystemRepository.Instance.GetSystem<IConfigProvider>();
            var configCollection = configProvider?.GetConfig<BulletConfigCollection>();
            var config = configCollection?.Get(fireParams.ConfigId);

            if (config == null)
            {
                Debug.LogError($"[BulletSystem] BulletConfig '{fireParams.ConfigId}' not found");
                return;
            }

            // Get BulletInstance from pool
            BulletInstance bullet;
            if (_bulletPool.Count > 0)
                bullet = _bulletPool.Pop();
            else
                bullet = new BulletInstance();

            // Get or create BulletVisual
            var visual = GetVisual(config.VisualPrefab, fireParams.VisualOrigin);
            if (visual == null)
            {
                Debug.LogError($"[BulletSystem] Failed to get BulletVisual for config '{fireParams.ConfigId}'");
                ReturnToPool(bullet);
                return;
            }

            // Initialize bullet with return-to-pool callback
            bullet.Init(fireParams, config, visual, ReturnToPool);

            // Fire buff callbacks on source
            var sourceBuffContainer = fireParams.Source?.GetComponent<IBuffContainer>();
            if (sourceBuffContainer != null)
            {
                sourceBuffContainer.GetBuffs<ITriggerBeforeFireBullet>(out var beforeFireBuffs);
                foreach (var buff in beforeFireBuffs)
                {
                    buff.TriggerBeforeFireBullet(null); // Legacy compat — BulletLaunchInfo may be null
                }
                beforeFireBuffs.ReleaseBuffList();

                sourceBuffContainer.GetBuffs<ITriggerAfterFireBullet>(out var afterFireBuffs);
                foreach (var buff in afterFireBuffs)
                {
                    buff.TriggerAfterFireBullet(null);
                }
                afterFireBuffs.ReleaseBuffList();
            }

            // Bullet buffs (bullet itself implements IBuffContainer? For now, skip — BulletInstance is not IBuffContainer.
            // If needed later, add IBuffContainer to BulletInstance.)
        }

        public void RecycleBullet(int bulletId)
        {
            // Bullets self-recycle via BulletInstance.Recycle() which calls the return-to-pool callback.
            // This method is provided for external systems that need to force-recycle a bullet.
            // For now, the primary recycling path is bullet self-recycling in LogicUpdate.
        }

        private void ReturnToPool(BulletInstance bullet)
        {
            if (bullet != null && _bulletPool != null && _bulletPool.Count < PoolCapacity)
            {
                _bulletPool.Push(bullet);
            }
        }

        // ======== Legacy API (backward compat) ========

        public void FireBullet([NotNull] BulletLaunchInfo info)
        {
            if (info == null) return;

            var bulletState = info.Model.Id.Create();
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

            ReferencePool.Release(info);
        }

        public void RecycleBullet(BulletState bulletState)
        {
            if (bulletState != null)
                Object.Destroy(bulletState.gameObject);
        }

        // ======== Visual pooling ========

        private BulletVisual GetVisual(GameObject visualPrefab, Vector3 position)
        {
            if (visualPrefab == null)
            {
                // No visual prefab configured — create an empty visual
                return CreateEmptyVisual(position);
            }

            // Try to get from pool
            if (_visualPool.Count > 0)
            {
                var visual = _visualPool.Dequeue();
                if (visual != null)
                {
                    visual.transform.position = position;
                    visual.OnPoolSpawn();
                    return visual;
                }
            }

            // Instantiate new
            var go = Object.Instantiate(visualPrefab, position, Quaternion.identity);
            var bulletVisual = go.GetComponent<BulletVisual>();
            if (bulletVisual == null)
            {
                bulletVisual = go.AddComponent<BulletVisual>();
            }
            return bulletVisual;
        }

        private BulletVisual CreateEmptyVisual(Vector3 position)
        {
            var go = new GameObject("BulletVisual_Empty");
            go.transform.position = position;
            var visual = go.AddComponent<BulletVisual>();
            return visual;
        }

        /// <summary>
        /// Return a BulletVisual to the pool. Called by BulletInstance when recycling.
        /// </summary>
        public void ReturnVisual(BulletVisual visual)
        {
            if (visual != null)
            {
                visual.OnRecycle();
                _visualPool.Enqueue(visual);
            }
        }
    }

    // ======== Legacy BulletLaunchInfo (kept for backward compat) ========

    public class BulletLaunchInfo : IReference
    {
        public GameObject Source;
        public BulletModel Model;
        public Vector3 LogicSpawnPoint;
        public Vector3 RendererSpawnPoint;
        public Vector3 SpawnDirection;

        public static BulletLaunchInfo Create(BulletModel model, Vector3 logicSpawnPoint, Vector3 rendererSpawnPoint, Vector3 spawnDirection, GameObject source = null)
        {
            var info = ReferencePool.Acquire<BulletLaunchInfo>();
            info.Source = source;
            info.Model = model;
            info.LogicSpawnPoint = logicSpawnPoint;
            info.RendererSpawnPoint = rendererSpawnPoint;
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
