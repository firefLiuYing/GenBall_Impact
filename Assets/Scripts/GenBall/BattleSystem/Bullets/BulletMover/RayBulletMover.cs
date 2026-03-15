using System;
using GenBall.BattleSystem.Mover;
using GenBall.Procedure.Game;
using UnityEngine;

namespace GenBall.BattleSystem.Bullets.BulletMover
{
    public class RayBulletMover : MonoBehaviour, IBulletMover
    {
        private BulletState _bullet;
        private TransformMover _logicMover;
        [SerializeField] private Transform modelTransform;
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
            _logicMover.SetVelocity(_bullet.Model.Speed*_bullet.SpawnDirection);
        }

        #region äÖÈ¾²ã

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
            if((PauseManager.Instance.State&PauseState.LogicPaused)==PauseState.LogicPaused) return;
            
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