using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Weapons;
using GenBall.BattleSystem.Weapons.Accessory;
using GenBall.Procedure.Game;
using UnityEngine;
using UnityEngine.InputSystem;
using Yueyn.Base.ReferencePool;
using Yueyn.Main;

namespace GenBall.Player.Controller
{
    public class WeaponController : CharacterControllerBase
    {
        [SerializeField] private Transform weaponSpawnPoint;
        private CharacterState  _player;
        private WeaponState _currentWeapon;
        private IEvolutionSystem _evolution;
        public override void Initialize(CharacterState characterState)
        {
            _player = characterState;
            _evolution = SystemRepository.Instance.GetSystem<IEvolutionSystem>();
            Equip(1);
        }

        public void OnFireInputChange(InputAction.CallbackContext context)
        {
            if(SystemRepository.Instance.GetSystem<IPauseSystem>().IsPaused) return;
            var buttonState = context.phase switch
            {
                InputActionPhase.Started=>ButtonState.Down,
                InputActionPhase.Canceled=>ButtonState.Up,
                InputActionPhase.Performed=>ButtonState.Hold,
                _=>ButtonState.None
            };
            _currentWeapon?.Trigger(buttonState);
        }

        /// <summary>
        /// Trigger fire without InputSystem context. Used by PlayerAttackExecutor.
        /// </summary>
        public void Fire(ButtonState state)
        {
            _currentWeapon?.Trigger(state);
        }

        public void OnReloadInputChange(InputAction.CallbackContext context)
        {
            if(SystemRepository.Instance.GetSystem<IPauseSystem>().IsPaused) return;
            var buttonState = context.phase switch
            {
                InputActionPhase.Started=>ButtonState.Down,
                InputActionPhase.Canceled=>ButtonState.Up,
                InputActionPhase.Performed=>ButtonState.Hold,
                _=>ButtonState.None
            };
            _currentWeapon?.Reload(buttonState);
        }

        public void OnEvolutionInputChange(InputAction.CallbackContext context)
        {
            if(SystemRepository.Instance.GetSystem<IPauseSystem>().IsPaused) return;
            if(context.phase!=InputActionPhase.Started) return;
            if(!_evolution.CanEvolve) return;
            Equip(_evolution.CurrentEvolutionLevel+1);
        }

        private void Equip(int level)
        {
            var equipInfo = _evolution.GetEquipInfo(level);
            if(equipInfo==null) return;
            _evolution.CurrentEvolutionLevel = level;
            Equip(equipInfo);
        }
        private void Equip(EquipInfo info)
        {
            if (_currentWeapon != null)
            {
                _currentWeapon.OnUnequip();
                Object.Destroy(_currentWeapon.gameObject);
                _currentWeapon=null;
            }
            _currentWeapon=info.WeaponId.Create();
            if(_currentWeapon==null) return;
            _currentWeapon.transform.SetParent(weaponSpawnPoint,false);
            _currentWeapon.gameObject.SetActive(true);
            _currentWeapon.Init(_player);
            if(info.Accessories==null) return;
            foreach (var accessory in info.Accessories)
            {
                #if UNITY_EDITOR
                var model= AccessoryModelConfigProvider.GetOrCreateConfig().GetModel(accessory);
                #else
                var model=new AccessoryModel();
                #endif
                var obj = AccessoryObj.Create(model);
                _currentWeapon.AddAccessory(obj);
            }
        }
        public override void Tick(float deltaTime)
        {
            
        }
    }
}