using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Weapons;
using GenBall.BattleSystem.Weapons.Accessory;
using GenBall.Procedure.Game;
using GenBall.Utils.EntityCreator;
using UnityEngine;
using UnityEngine.InputSystem;
using Yueyn.Base.ReferencePool;

namespace GenBall.Player.Controller
{
    public class WeaponController : CharacterControllerBase
    {
        [SerializeField] private Transform weaponSpawnPoint;
        private CharacterState  _player;
        private WeaponState _currentWeapon;
        public override void Initialize(CharacterState characterState)
        {
            _player = characterState;
            Equip(new EquipInfo
            {
                WeaponId = WeaponId.Pistol,
            });
        }

        public void OnFireInputChange(InputAction.CallbackContext context)
        {
            if((PauseManager.Instance.State&PauseState.LogicPaused)==PauseState.LogicPaused) return;
            var buttonState = context.phase switch
            {
                InputActionPhase.Started=>ButtonState.Down,
                InputActionPhase.Canceled=>ButtonState.Up,
                InputActionPhase.Performed=>ButtonState.Hold,
                _=>ButtonState.None
            };
            _currentWeapon?.Trigger(buttonState);
        }

        public void OnReloadInputChange(InputAction.CallbackContext context)
        {
            if((PauseManager.Instance.State&PauseState.LogicPaused)==PauseState.LogicPaused) return;
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
            if((PauseManager.Instance.State&PauseState.LogicPaused)==PauseState.LogicPaused) return;
            if(context.phase!=InputActionPhase.Started) return;
            
        }

        private void Equip(EquipInfo info)
        {
            if (_currentWeapon != null)
            {
                _currentWeapon.OnUnequip();
                GameEntry.GetModule<EntityCreator<WeaponState>>().RecycleEntity(_currentWeapon.gameObject);
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
                var model= AccessoryModelConfigProvider.GetOrCreateConfig().GetModel(accessory);
                var obj = AccessoryObj.Create(model);
                _currentWeapon.AddAccessory(obj);
            }
        }
        public override void Tick(float deltaTime)
        {
            
        }
    }
}