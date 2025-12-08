using GenBall.BattleSystem.Weapons;
using GenBall.Utils.EntityCreator;
using UnityEngine;

namespace GenBall.BattleSystem.Bullets
{
    public class DefaultBullet : MonoBehaviour, IBullet
    {
        public IWeapon Source { get;private set; }
        
        [SerializeField]private float bulletSpeed;
        [SerializeField]private LayerMask targetLayer;
        [SerializeField] private float lifeTime;
        // private BulletCreator BulletCreator => GameEntry.GetModule<BulletCreator>();

        private EntityCreator<IBullet> BulletCreator => GameEntry.GetModule<EntityCreator<IBullet>>();
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
                // 没检测到目标点就视作10000米后命中
                _logicSource=_logicSource+_direction*10000;
            }
            _fired=true;
            _hit = false;
            _process = 0f;
            _predictDistance=Vector3.Distance(_logicSource, _logicTarget);
            _controlPoint=GetControlPosition(_logicSource, _spawnPoint, _logicTarget);
            _needBezier=NeedBezier(_logicSource,_spawnPoint,_logicTarget);
            
            gameObject.SetActive(true);
        }

        public void OnRecycle()
        {
            _fired=false;
            _process = 0f;
            _predictDistance = 0f;
            _curLifeTime = 0f;
        }

        public void EntityUpdate(float deltaTime)
        {
            if(!_fired) return;
            if(_hit)  return;
            
            // 视觉表现
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

        public void EntityFixedUpdate(float fixedDeltaTime)
        {
            // 逻辑命中判断
            _curLifeTime+=fixedDeltaTime;
            if (_curLifeTime >= lifeTime)
            {
                // BulletCreator.RecycleBullet(gameObject);
                BulletCreator.RecycleEntity(gameObject);
                return;
            }
            var ray=new Ray(_logicSource+_process*_direction,_direction);
            Physics.Raycast(ray,out var hitInfo,bulletSpeed*fixedDeltaTime,targetLayer);
            if (hitInfo.collider == null) return;
            _hit=true;
            // var interactables = hitInfo.collider.GetComponentsInParent<IInteractable>();
            // todo gzp 等攻击参数补充完整后记得来完善
            // var attackInfo = new AttackArgs
            // {
            //     
            // };
            // var attackToken = DefaultAttackToken.Create(Source.Owner, attackInfo);
            // foreach (var interactable in interactables)
            // {
            //     // attackable.OnAttacked(attackInfo);
            //     // interactable.Handle(attackToken,out var attackResponses);
            //     for (int i = 0; i < attackResponses.Length; i++)
            //     {
            //         Source.Owner.Handle(attackResponses[i],out _);
            //     }
            // }
            // BulletCreator.RecycleBullet(gameObject);
            BulletCreator.RecycleEntity(gameObject);
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