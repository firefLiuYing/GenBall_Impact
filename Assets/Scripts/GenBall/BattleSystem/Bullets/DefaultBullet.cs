using GenBall.BattleSystem.Weapons;
using UnityEngine;

namespace GenBall.BattleSystem.Bullets
{
    public class DefaultBullet : MonoBehaviour, IBullet
    {
        public IWeapon Source { get;private set; }
        
        [SerializeField]private float bulletSpeed;
        [SerializeField]private LayerMask targetLayer;
        [SerializeField] private float lifeTime;
        private BulletCreator BulletCreator => GameEntry.GetModule<BulletCreator>();

        private Vector3 _spawnPoint;
        private Vector3 _direction;
        private Vector3 _logicSource;
        private Vector3 _logicTarget;
        private Vector3 _controlPoint;
        private float _process;
        private float _predictDistance;
        private float _curLifeTime;
        
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
            
            gameObject.SetActive(true);
        }

        public void OnRecycle()
        {
            _fired=false;
            _process = 0f;
            _predictDistance = 0f;
            _curLifeTime = 0f;
        }

        public void BulletUpdate(float deltaTime)
        {
            if(!_fired) return;
            if(_hit)  return;
            
            // 视觉表现
            _process+=deltaTime*bulletSpeed;
            float process=_process/_predictDistance;
            if (process <= 1f)
            {
                transform.position = Bezier(process, _spawnPoint, _controlPoint, _logicTarget);
            }
            else
            {
                transform.position = _logicSource + _process * _direction;
            }
        }

        public void BulletFixedUpdate(float fixedDeltaTime)
        {
            // 逻辑命中判断
            _curLifeTime+=fixedDeltaTime;
            if (_curLifeTime >= lifeTime)
            {
                BulletCreator.RecycleBullet(gameObject);
                return;
            }
            var ray=new Ray(_logicSource+_process*_direction,_direction);
            Physics.Raycast(ray,out var hitInfo,bulletSpeed*fixedDeltaTime,targetLayer);
            if (hitInfo.collider == null) return;
            _hit=true;
            var attackables = hitInfo.collider.GetComponentsInParent<IAttackable>();
            var attackInfo = new AttackInfo
            {
                Attacker = Source.Owner,
            };
            foreach (var attackable in attackables)
            {
                attackable.OnAttacked(attackInfo);
            }
            BulletCreator.RecycleBullet(gameObject);
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
    }
}