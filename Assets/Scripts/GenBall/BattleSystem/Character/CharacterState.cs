using System;
using System.Collections.Generic;
using System.Linq;
using GenBall.BattleSystem.Buff;
using GenBall.BattleSystem.Command;
using GenBall.Procedure.Game;
using GenBall.Utils.EntityCreator;
using UnityEngine;
using Yueyn.Base.ReferencePool;

namespace GenBall.BattleSystem.Character
{
    public class CharacterState : MonoBehaviour,IDamageable,IBuffContainer,IEntity
    {
        private readonly List<ICharacterInitializer> _initializers=new();
        private readonly List<ICharacterController>  _controllers=new();
        private IMove _move;
        private IRotate _rotate;
        [SerializeField,Tooltip("角色基础属性配置")] private CharacterStatsModel characterStatsModel;
        public CharacterStats Stats;
        private void Awake()
        {
            var initializers = GetComponentsInChildren<ICharacterInitializer>();
            _initializers.AddRange(initializers);
            _initializers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            var controllers = GetComponentsInChildren<ICharacterController>();
            _controllers.AddRange(controllers);
            _controllers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            TryGetComponent(out _move);
            TryGetComponent(out _rotate);
        }

        private void Initialize()
        {
            Stats=CharacterStats.Create(characterStatsModel);
            foreach (var initializer in _initializers)
            {
                initializer.Initialize(this);
            }
            foreach (var controller in _controllers)
            {
                controller.Initialize(this);
            }
            Health=MaxHealth;
        }

        public bool CanMove { get; set; }
        public bool CanRotate { get; set; }
        public bool CanJump { get; set; }
        public void HandleCommand(ICommand command)
        {
            if(IsPause) return;
            switch (command)
            {
                case MoveCommand moveCommand:
                    _move?.Move(CanMove ? moveCommand : new MoveCommand(Vector2.zero));
                    break;
                case RotateCommand rotateCommand:
                    if(CanRotate) _rotate?.Rotate(rotateCommand);
                    break;
                default:
                    break;
            }
        }

        #region Health

        public int Health
        {
            get => _health;
            private set
            {
                _health=value;
                OnHealthChange?.Invoke(_health);
            }
        }

        private int _health;
        public Action<int> OnHealthChange;
        public int MaxHealth => Stats.MaxHealth.CurrentValue;
        public bool IsDead { get; private set;}
        public void Die(DeathInfo deathInfo)
        {
            IsDead=true;
            // todo gzp 后续死亡逻辑
        }

        #endregion

        #region Damage

        public void TakeDamage(DamageInfo damageInfo)
        {
            // 伤害结算
            Health-=damageInfo.Damage.GetValue();
            if (Health <= 0)
            {
                // 死了，走死亡流程
                DeathSystem.Instance.ApplyDeath(DeathInfo.Create(gameObject,new List<string>()
                {
                    DeathTag.HealthEmpty,
                },damageInfo.Attacker));
            }
            // todo gzp 后续补充
        }

        #endregion
        
        #region Buff
        public IReadOnlyList<IBuff> Buffs => _buffs.ToList();
        private readonly SortedSet<IBuff> _buffs = new(new DefaultComparerBuff());
        public void AddBuff(IBuff buff)=>_buffs.Add(buff);

        public void RemoveBuff(Buff.IBuff buff)=>_buffs.Remove(buff);
        #endregion

        #region Entity

        public void OnSpawn()
        {
            Initialize();
        }
        
        public void EntityUpdate(float deltaTime)
        {
            
        }

        public void EntityFixedUpdate(float fixedDeltaTime)
        {
            if(IsPause) return;
            foreach (var controller in _controllers)
            {
                controller.Tick(fixedDeltaTime);
            }
        }

        public void OnRecycle()
        {
            ReferencePool.Release(Stats);
            Stats = null;
        }

        #endregion
        
        private bool IsPause=>(PauseManager.Instance.State&PauseState.LogicPaused)==PauseState.LogicPaused;
    }
    
}