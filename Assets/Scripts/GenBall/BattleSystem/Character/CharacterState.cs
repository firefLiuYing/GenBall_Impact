using System;
using System.Collections.Generic;
using System.Linq;
using GenBall.BattleSystem.Buff;
using GenBall.BattleSystem.Command;
using GenBall.Framework.Entity;
using GenBall.Procedure.Game;
using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.Main;

namespace GenBall.BattleSystem.Character
{
    public class CharacterState : MonoBehaviour,IDamageable,IBuffContainer,IEntityLogicUpdate
    {
        private readonly List<ICharacterInitializer> _initializers=new();
        private readonly List<ICharacterController>  _controllers=new();
        private IMove _move;
        private IRotate _rotate;
        private IAttack _attack;
        private IFaceDirection _faceDirection;
        [SerializeField,Tooltip("基本属性")] private CharacterStatsModel characterStatsModel;
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
            TryGetComponent(out _attack);
            TryGetComponent(out _faceDirection);
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
            SystemRepository.Instance.GetSystem<IEntityUpdateSystem>().AddLogicUpdate(this);
        }

        public bool CanMove { get; set; }
        public bool CanRotate { get; set; }
        public bool CanJump { get; set; }
        public bool CanAttack{get;set;}
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
                case AttackCommand attackCommand:
                    if(CanAttack) _attack?.Attack(attackCommand);
                    break;
                case FaceDirectionCommand faceCommand:
                    if(CanRotate) _faceDirection?.Face(faceCommand);
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
            // todo gzp ���������߼�
        }

        #endregion

        #region Damage

        public void TakeDamage(DamageInfo damageInfo)
        {
            // �˺�����
            Health-=damageInfo.Damage.GetValue();
            if (Health <= 0)
            {
                // ���ˣ�����������
                SystemRepository.Instance.GetSystem<IDeathSystem>().ApplyDeath(DeathInfo.Create(gameObject,new List<string>()
                {
                    DeathTag.HealthEmpty,
                },damageInfo.Attacker));
            }
            // todo gzp ��������
        }

        #endregion
        
        #region Buff
        public IReadOnlyList<IBuff> Buffs => _buffs.ToList();
        private readonly SortedSet<IBuff> _buffs = new(new DefaultComparerBuff());
        public void AddBuff(IBuff buff)=>_buffs.Add(buff);

        public void RemoveBuff(Buff.IBuff buff)=>_buffs.Remove(buff);
        #endregion

        public void LogicUpdate(float deltaTime)
        {
            if(IsPause) return;
            foreach (var controller in _controllers)
            {
                controller.Tick(deltaTime);
            }
        }

        private void OnDestroy()
        {
            SystemRepository.Instance.GetSystem<IEntityUpdateSystem>()?.RemoveLogicUpdate(this);
            if (Stats != null)
            {
                ReferencePool.Release(Stats);
                Stats = null;
            }
        }
        
        private bool IsPause=>SystemRepository.Instance.GetSystem<IPauseSystem>().IsPaused;
    }
    
}