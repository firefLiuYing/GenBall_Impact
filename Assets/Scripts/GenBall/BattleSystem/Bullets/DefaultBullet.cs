using GenBall.BattleSystem.Weapons;
using GenBall.Framework.Entity;
using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.Main;

namespace GenBall.BattleSystem.Bullets
{
    public class DefaultBullet : MonoBehaviour, IBullet, IEntityFrameUpdate, IEntityLogicUpdate
    {
        public IWeapon Source { get;private set; }

        [SerializeField]private float bulletSpeed;
        [SerializeField]private LayerMask targetLayer;
        [SerializeField] private float lifeTime;
        private Vector3 _spawnPoint;
        private Vector3 _direction;
        private Vector3 _logicSource;
        private Vector3 _logicTarget;
        private Vector3 _controlPoint;
        private float _process;
        private float _predictDistance;
        private float _curLifeTime;
        private bool _needBezier;

        private bool _fired = false;
        private bool _hit=false;
        private bool _registered = false;
        public void Fire(IWeapon source, Vector3 spawnPoint,Vector3 direction)
        {
            if(_fired) return;
            Source = source;
            _spawnPoint = spawnPoint;
            _direction = direction;
            _logicSource = Camera.main.transform.position;
            Physics.Raycast(_logicSource,_direction,out var hitInfo);
            if (hitInfo.collider != null)
            {
                _logicTarget = hitInfo.point;
            }
            else
            {
                _logicTarget=_logicSource+_direction*10000;
            }
            _fired=true;
            _hit = false;
            _process = 0f;
            _predictDistance=Vector3.Distance(_logicSource, _logicTarget);
            _controlPoint=GetControlPosition(_logicSource, _spawnPoint, _logicTarget);
            _needBezier=NeedBezier(_logicSource,_spawnPoint,_logicTarget);

            gameObject.SetActive(true);

            if (!_registered)
            {
                var entitySystem = SystemRepository.Instance.GetSystem<IEntityUpdateSystem>();
                entitySystem.AddFrameUpdate(this);
                entitySystem.AddLogicUpdate(this);
                _registered = true;
            }
        }

        public void LogicUpdate(float deltaTime)
        {
            _curLifeTime+=deltaTime;
            if (_curLifeTime >= lifeTime)
            {
                Object.Destroy(gameObject);
                return;
            }
            var ray=new Ray(_logicSource+_process*_direction,_direction);
            Physics.Raycast(ray,out var hitInfo,bulletSpeed*deltaTime,targetLayer);
            if (hitInfo.collider == null) return;
            _hit=true;
            var attackables = hitInfo.collider.GetComponentsInParent<IDamageable>();
            foreach (var attackable in attackables)
            {
                var attackInfo = AttackInfo.Create(Source.Owner, Source.Stats.Damage, _direction.normalized, Source.Stats.ImpactForce);
                Source.Attack(attackable,attackInfo);
            }
            Object.Destroy(gameObject);
        }

        public void FrameUpdate(float deltaTime)
        {
            if(!_fired) return;
            if(_hit)  return;

            _process+=deltaTime*bulletSpeed;
            float process=_process/_predictDistance;
            if (process <= 1f&&_needBezier)
            {
                transform.position = Bezier(process, _spawnPoint, _controlPoint, _logicTarget);
            }
            else if(process<=1f)
            {
                transform.position = Vector3.Lerp(_spawnPoint, _logicTarget, process);
            }
            else
            {
                transform.position = _logicSource + _process * _direction;
            }
        }

        private void OnDestroy()
        {
            var entitySystem = SystemRepository.Instance.GetSystem<IEntityUpdateSystem>();
            entitySystem?.RemoveFrameUpdate(this);
            entitySystem?.RemoveLogicUpdate(this);
        }

        private static Vector3 Bezier(float t, Vector3 p0, Vector3 p1, Vector3 p2)
            =>(1-t)*(1-t)*p0+2*(1-t)*t*p1+t*t*p2;
        private Vector3 GetControlPosition(Vector3 logicSource,Vector3 spawnPos,Vector3 logicTarget)
        {
            Vector3 distanceLine=logicTarget - logicSource;
            Vector3 offsetLine=spawnPos - logicSource;
            float alpha=Vector3.Angle(distanceLine,offsetLine);
            float distance=distanceLine.magnitude;
            float offset = offsetLine.magnitude;

            float delta = offset * Mathf.Cos(alpha*Mathf.Deg2Rad);
            distance-=2*delta;
            return spawnPos + distance * distanceLine.normalized;
        }

        private bool NeedBezier(Vector3 logicSource, Vector3 spawnPos, Vector3 logicTarget)
        {
            Vector3 distanceLine=logicTarget - logicSource;
            Vector3 offsetLine=spawnPos - logicSource;
            float alpha=Vector3.Angle(distanceLine,offsetLine);
            float distance=distanceLine.magnitude;
            float offset = offsetLine.magnitude;

            float delta = offset * Mathf.Cos(alpha*Mathf.Deg2Rad);
            distance-=2*delta;
            return distance > 0;
        }
    }
}
