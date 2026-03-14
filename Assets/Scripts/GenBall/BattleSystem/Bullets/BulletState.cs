using System.Collections.Generic;
using System.Linq;
using GenBall.BattleSystem.Buff;
using GenBall.Utils.EntityCreator;
using UnityEngine;

namespace GenBall.BattleSystem.Bullets
{
    public class BulletState : MonoBehaviour, IBuffContainer,IEntity
    {
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