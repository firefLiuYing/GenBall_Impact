using System;
using GenBall.BattleSystem.Bullets;
using GenBall.Player;
using GenBall.Procedure.Game;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.BattleSystem.Weapons
{
    [RequireComponent(typeof(NormalReloadController))]
    public class NormalTriggerController : MonoBehaviour, IWeaponTriggerController
    {
        [SerializeField] private Transform bulletSpawnPoint;
        [SerializeField] private FireMode fireMode=FireMode.Automatic;
        [SerializeField] private BulletModel bulletModel;
        
        private WeaponState _weapon;
        private NormalReloadController _reload;
        private float FireInterval=>_weapon.Stats.FireInterval.CurrentValue;
        public void Init(WeaponState weapon)
        {
            _weapon = weapon;
        }
        public void Trigger(ButtonState buttonState)
        {
            _fireButtonState=buttonState;
            if (buttonState == ButtonState.Down)
            {
                _fireButtonDownThisFrame=true;
            }
        }

        private void Update()
        {
            var ps = SystemRepository.Instance.GetSystem<IPauseSystem>();
            if(ps != null && ps.IsLogicPaused) return;
            _fireColdTime+=Time.deltaTime;
            if (CanFireCurFrame())
            {
                _fireColdTime=0;
                _fireButtonDownThisFrame=false;
                HandleFireBullet();
            }
        }

        private float _fireColdTime;
        private bool _fireButtonDownThisFrame=false;
        private ButtonState _fireButtonState;
        private bool CanFireCurFrame()
        {
            // TODO B-3: replace with proper CanAttack check (e.g. HitReactionComponent.IsStunned)
            // if(_weapon.PlayerGo...)
            if(_fireColdTime<FireInterval) return false;
            if (!_reload.HaveBullets)
            {
                if ((fireMode & FireMode.AutoReload) == FireMode.AutoReload)
                {
                    _reload.AutoReload();
                }
                return false;
            }
            if ((fireMode & FireMode.Automatic) == FireMode.Automatic)
            {
                return _fireButtonState is ButtonState.Down or ButtonState.Hold;
            }
            return _fireButtonDownThisFrame;
        }
        private void HandleFireBullet()
        {
            _reload.CostBullet(1);
            bulletModel.Damage=_weapon.Stats.Damage.GetValue();
            SystemRepository.Instance.GetSystem<IBulletSystem>().FireBullet(BulletLaunchInfo.Create(
                bulletModel,
                Camera.main.transform.position,
                bulletSpawnPoint.position,
                Camera.main.transform.forward,
                _weapon.gameObject));
        }

        private void Awake()
        {
            _reload = GetComponent<NormalReloadController>();
        }

        [Flags]
        private enum FireMode
        {
            None = 1<<0,
            Automatic = 1 << 1,
            AutoReload = 1 << 2,
        }
    }
}