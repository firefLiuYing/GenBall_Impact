using System;
using System.Collections.Generic;
using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.Main;
using Yueyn.ObjectPool;
using Yueyn.Resource;
using Yueyn.Utils;
using Object = UnityEngine.Object;

namespace GenBall.BattleSystem.Bullets
{
    public partial class BulletCreator : IComponent
    {
        private readonly Dictionary<TypeNamePair, string> _bulletMap = new();
        private IObjectPool<BulletObject> _bulletPool;
        private ResourceManager ResourceManager => GameEntry.GetModule<ResourceManager>();

        #region RecycleBullet

        /// <summary>
        /// 回收子弹GameObject
        /// </summary>
        /// <param name="bullet">子弹实例的GameObject</param>
        public void RecycleBullet(GameObject bullet) => _bulletPool.Despawn(bullet);

        #endregion
        
        #region CreateBullet

        public TBullet CreateBullet<TBullet>() where TBullet : IBullet => (TBullet)CreateBullet(new TypeNamePair(typeof(TBullet)));
        public TBullet CreateBullet<TBullet>(string name) where TBullet : IBullet => (TBullet)CreateBullet(new TypeNamePair(typeof(TBullet), name));
        public IBullet CreateBullet(Type bulletType) =>CreateBullet(new TypeNamePair(bulletType));
        public IBullet CreateBullet(Type bulletType,string name)=>CreateBullet(new TypeNamePair(bulletType, name));
        private IBullet CreateBullet(TypeNamePair typeNamePair)
        {
            var bulletObject = _bulletPool.Spawn($"{typeNamePair}");
            if (bulletObject != null)
            {
                return (IBullet)bulletObject.Target;
            }

            if (!_bulletMap.TryGetValue(typeNamePair, out var prefabPath))
            {
                throw new Exception($"Bullet Prefab {typeNamePair} is not registered");
            }
            var prefab=(GameObject)ResourceManager.LoadPrefab(prefabPath);
            var go = Object.Instantiate(prefab);
            var bullet = go.GetComponent<IBullet>();
            if (bullet == null) throw new Exception("Bullet Prefab not found Component IBullet");
            bulletObject = BulletObject.Create($"{typeNamePair}", go);
            _bulletPool.Register(bulletObject,true);
            return bullet;
        }
        #endregion

        #region AddBulletPrefab

        private void AddBulletPrefab<TBullet>(string prefabPath) where TBullet : IBullet =>AddBulletPrefab(new TypeNamePair(typeof(TBullet)),prefabPath);
        private void AddBulletPrefab(Type bulletType,string prefabPath)=>AddBulletPrefab(new TypeNamePair(bulletType), prefabPath);
        private void AddBulletPrefab<TBullet>(string name,string prefab) where TBullet:IBullet =>AddBulletPrefab(new TypeNamePair(typeof(TBullet),name), prefab);
        private void AddBulletPrefab(Type bulletType, string name, string prefabPath)=>AddBulletPrefab(new TypeNamePair(bulletType, name), prefabPath);
        private void AddBulletPrefab(TypeNamePair typeNamePair, string prefabPath)
        {
            if (!typeof(IBullet).IsAssignableFrom(typeNamePair.Type))
            {
                throw new Exception($"Bullet Prefab {typeNamePair} is not a bullet type");
            }
            if (!_bulletMap.TryAdd(typeNamePair, prefabPath))
            {
                throw new Exception($"Bullet Prefab {typeNamePair} is already registered");
            }
        }
        #endregion
        
        public void OnRegister()
        {
            _bulletPool = GameEntry.GetModule<ObjectPoolManager>().CreateSingleSpawnObjectPool<BulletObject>();
            RegisterBullets();
        }

        public void OnUnregister()
        {
            
        }

        public void Update(float elapsedSeconds, float realElapseSeconds)
        {
            
        }

        public void Shutdown()
        {
            
        }
    }
}