using System.Collections.Generic;
using GenBall.BattleSystem.Mover;
using GenBall.BattleSystem.Weapons;
using GenBall.Procedure.Game;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.BattleSystem.Bullets.BulletController
{
    [System.Obsolete("Replaced by RayDetection + DealDamageBehavior. Will be removed in Phase E cleanup.")]
    public class RayBulletController : MonoBehaviour, IBulletController
    {
        private BulletState _bullet;
        private TransformMover _logicMover;
        [SerializeField] private Transform modelTransform;
        [SerializeField] private LayerMask targetLayer;
        public void Init(BulletState bulletState)
        {
            _bullet = bulletState;
        }

        public void Fire()
        {
            InitRendererArgs();
            transform.position = _bullet.LogicSpawnPoint;
            modelTransform.position = _bullet.RendererSpawnPoint;
            transform.rotation = Quaternion.LookRotation(_bullet.SpawnDirection);
            gameObject.SetActive(true);
            _logicMover.SetVelocity(_bullet.Model.Speed*_bullet.SpawnDirection);
        }

        public void Tick(float deltaTime)
        {
            if (_flyTime > 2f)
            {
                SystemRepository.Instance.GetSystem<IBulletSystem>().RecycleBullet(_bullet);
            }
            _logicMover.SetVelocity(_bullet.Model.Speed*_bullet.SpawnDirection);
            _logicMover.Tick(deltaTime);
            var ray=new Ray(transform.position,_bullet.SpawnDirection);
            Physics.Raycast(ray,out var hitInfo,_bullet.Model.Speed*deltaTime,targetLayer);
            if (hitInfo.collider == null) return;
            _flying = false;
            SystemRepository.Instance.GetSystem<IDamageSystem>().ApplyDamage(DamageInfo.Create(hitInfo.collider.gameObject,_bullet.Model.Damage,new List<string>()
            {
                "Bullet"
            },_bullet.SpawnDirection,0,_bullet.Source.GetComponent<WeaponState>().PlayerGo));

            SystemRepository.Instance.GetSystem<IBulletSystem>().RecycleBullet(_bullet);
        }

        #region ��Ⱦ��

        private bool _flying = false;
        private float _predictTime;
        private float _flyTime;
        private bool _needBezier;
        private Vector3 _rendererSpawnPoint;
        private Vector3 _rendererControlPoint;
        private Vector3 _rendererTargetPoint;

        private void InitRendererArgs()
        {
            _flying = true;
            _flyTime = 0;
            _rendererSpawnPoint=_bullet.RendererSpawnPoint;
            Physics.Raycast(_bullet.LogicSpawnPoint,_bullet.SpawnDirection,out var hitInfo);
            if (hitInfo.collider != null)
            {
                _rendererTargetPoint = hitInfo.point;
            }
            else
            {
                _rendererTargetPoint = _bullet.LogicSpawnPoint + 1000f * _bullet.SpawnDirection;
            }
            _predictTime = Vector3.Distance(_rendererTargetPoint, _bullet.LogicSpawnPoint) /(_bullet.Model.Speed+0.001f);
            Vector3 distanceLine=_rendererTargetPoint - _bullet.LogicSpawnPoint;
            Vector3 offsetLine=_rendererSpawnPoint - _bullet.LogicSpawnPoint;
            float alpha=Vector3.Angle(distanceLine,offsetLine);
            float distance=distanceLine.magnitude;
            float offset = offsetLine.magnitude;
        
            float delta = offset * Mathf.Cos(alpha*Mathf.Deg2Rad);
            distance-=2*delta;
            _rendererControlPoint= _rendererSpawnPoint + distance * distanceLine.normalized;
            _needBezier = distance > 0f;
        }
        private void Update()
        {
            if(!_flying) return;
            var ps = SystemRepository.Instance.GetSystem<IPauseSystem>();
            if(ps != null && ps.IsLogicPaused) return;
            
            _flyTime += Time.deltaTime;
            if (_flyTime < _predictTime&&_needBezier)
            {
                modelTransform.position=Bezier(_flyTime/_predictTime,_rendererSpawnPoint,_rendererControlPoint,_rendererTargetPoint);
            }
            else if (_flyTime < _predictTime)
            {
                modelTransform.position=Vector3.Lerp(_rendererSpawnPoint,_rendererTargetPoint,_flyTime / _predictTime);
            }
            else
            {
                modelTransform.localPosition=Vector3.zero;
            }
        }

        #endregion
        

        private void Awake()
        {
            _logicMover = GetComponent<TransformMover>();
        }
        
        private static Vector3 Bezier(float t, Vector3 p0, Vector3 p1, Vector3 p2)
            =>(1-t)*(1-t)*p0+2*(1-t)*t*p1+t*t*p2;
    }
}