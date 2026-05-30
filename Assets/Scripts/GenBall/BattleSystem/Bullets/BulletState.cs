using System;
using System.Collections.Generic;
using System.Linq;
using GenBall.BattleSystem.Buff;
using GenBall.BattleSystem.Bullets.BulletController;
using GenBall.Framework.Entity;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.BattleSystem.Bullets
{
    [System.Obsolete("Replaced by BulletInstance + BulletVisual. Will be removed in Phase E cleanup.")]
    public class BulletState : MonoBehaviour, IBuffContainer,IEntityLogicUpdate
    {
        public BulletModel Model{get;private set;}
        public GameObject Source{get;private set;}
        public Vector3 LogicSpawnPoint{get;private set;}
        public Vector3 RendererSpawnPoint{get;private set;}
        public Vector3 SpawnDirection{get;private set;}
        private IBulletController _controller;
        public void Init(BulletLaunchInfo info)
        {
            Model = info.Model;
            Source = info.Source;
            LogicSpawnPoint = info.LogicSpawnPoint;
            RendererSpawnPoint = info.RendererSpawnPoint;
            SpawnDirection = info.SpawnDirection;
            _controller.Init(this);
            SystemRepository.Instance.GetSystem<IEntityUpdateSystem>().AddLogicUpdate(this);
        }

        public void Fire()
        {
            _controller.Fire();
        }

        private void Awake()
        {
            _controller = GetComponent<IBulletController>();
        }

        #region Buff

        public IReadOnlyList<IBuff> Buffs=>_buffs.ToList();
        private readonly SortedSet<IBuff> _buffs = new(new DefaultComparerBuff());
        public void AddBuff(IBuff buff)=>_buffs.Add(buff);

        public void RemoveBuff(IBuff buff)=>_buffs.Remove(buff);

        #endregion
        
        public void LogicUpdate(float deltaTime)
        {
            _controller.Tick(deltaTime);
        }

        private void OnDestroy()
        {
            SystemRepository.Instance.GetSystem<IEntityUpdateSystem>()?.RemoveLogicUpdate(this);
        }
    }
}