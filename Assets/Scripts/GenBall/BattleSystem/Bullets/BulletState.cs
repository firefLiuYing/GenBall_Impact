using System;
using System.Collections.Generic;
using System.Linq;
using GenBall.BattleSystem.Buff;
using GenBall.BattleSystem.Bullets.BulletMover;
using GenBall.Utils.EntityCreator;
using UnityEngine;

namespace GenBall.BattleSystem.Bullets
{
    public class BulletState : MonoBehaviour, IBuffContainer,IEntity
    {
        public BulletModel Model{get;private set;}
        public GameObject Source{get;private set;}
        public Vector3 LogicSpawnPoint{get;private set;}
        public Vector3 RendererSpawnPoint{get;private set;}
        public Vector3 SpawnDirection{get;private set;}
        private IBulletMover _mover;
        public void Init(BulletLaunchInfo info)
        {
            Model = info.Model;
            Source = info.Source;
            LogicSpawnPoint = info.LogicSpawnPoint;
            RendererSpawnPoint = info.RendererSpawnPoint;
            SpawnDirection = info.SpawnDirection;
            _mover.Init(this);
        }

        public void Fire()
        {
            _mover.Fire();
        }

        private void Awake()
        {
            _mover = GetComponent<IBulletMover>();
        }

        #region Buff

        public IReadOnlyList<IBuff> Buffs=>_buffs.ToList();
        private readonly SortedSet<IBuff> _buffs = new(new DefaultComparerBuff());
        public void AddBuff(IBuff buff)=>_buffs.Add(buff);

        public void RemoveBuff(IBuff buff)=>_buffs.Remove(buff);

        #endregion
        
        #region Entity
        public void EntityUpdate(float deltaTime)
        {
            
        }

        public void EntityFixedUpdate(float fixedDeltaTime)
        {
            _mover.Tick(fixedDeltaTime);
        }

        public void OnRecycle()
        {
            
        }

        public void OnSpawn()
        {
            
        }
        #endregion
    }
}