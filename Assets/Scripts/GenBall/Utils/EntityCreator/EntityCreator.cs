using System;
using System.Collections.Generic;
using GenBall.BattleSystem.Bullets;
using UnityEngine;
using Yueyn.Main;
using Yueyn.ObjectPool;
using Yueyn.Resource;
using Yueyn.Utils;
using Object = UnityEngine.Object;

namespace GenBall.Utils.EntityCreator
{
    public partial class EntityCreator<TEntityInterface>:IComponent where TEntityInterface:IEntity
    {
        private readonly Dictionary<TypeNamePair, string> _prefabMap = new();
        private readonly List<TEntityInterface> _prefabs = new();
        private readonly List<TEntityInterface> _tempPrefabs = new();
        private readonly List<TEntityInterface> _fixedTempPrefabs = new();
        private IObjectPool<EntityObject> _entityPool;
        private ResourceManager ResourceManager => GameEntry.GetModule<ResourceManager>();

        public void RecycleEntity(GameObject entity)
        {
            _entityPool.Despawn(entity);
            _prefabs.Remove(entity.GetComponent<TEntityInterface>());
        }
        public TEntity CreateEntity<TEntity>() where TEntity : TEntityInterface => (TEntity)CreateEntity(new TypeNamePair(typeof(TEntity)));
        public TEntity CreateEntity<TEntity>(string name)where TEntity:TEntityInterface =>(TEntity)CreateEntity(new TypeNamePair(typeof(TEntity), name));
        
        private TEntityInterface CreateEntity(TypeNamePair typeNamePair)
        {
            var entityObject = _entityPool.Spawn($"{typeNamePair}");
            TEntityInterface entityInterface;
            if (entityObject != null)
            {
                entityInterface = ((GameObject)entityObject.Target).GetComponent<TEntityInterface>();
                _prefabs.Add(entityInterface);
                return entityInterface;
            }

            if (!_prefabMap.TryGetValue(typeNamePair, out var prefabPath))
            {
                throw new Exception($"Entity Prefab {typeNamePair} is not registered");
            }
            var prefab=(GameObject)ResourceManager.LoadPrefab(prefabPath);
            var go = Object.Instantiate(prefab);
            entityInterface = go.GetComponent<TEntityInterface>();
            if (entityInterface == null) throw new Exception($"Entity Prefab not found Component {typeof(TEntityInterface).FullName}");
            _prefabs.Add(entityInterface);
            entityObject = EntityObject.Create($"{typeNamePair}", go);
            _entityPool.Register(entityObject,true);
            return entityInterface;
        }
        public void AddPrefab<TEntity>(string prefabPath) where TEntity : class, TEntityInterface => AddPrefab(new TypeNamePair(typeof(TEntity)),prefabPath);
        public void AddPrefab<TEntity>(string name,string prefabPath) where TEntity : class, TEntityInterface =>AddPrefab(new TypeNamePair(typeof(TEntity), name), prefabPath);
        private void AddPrefab(TypeNamePair typeNamePair, string prefabPath)
        {
            if (!typeof(TEntityInterface).IsAssignableFrom(typeNamePair.Type))
            {
                throw new Exception($"{typeNamePair.Type} is not assignable from {typeof(TEntityInterface)}");
            }

            if (!_prefabMap.TryAdd(typeNamePair, prefabPath))
            {
                throw new Exception($"{typeNamePair.Type} is already registered");
            }
        }
        public void OnRegister()
        {
            _entityPool = GameEntry.GetModule<ObjectPoolManager>().CreateSingleSpawnObjectPool<EntityObject>($"{typeof(TEntityInterface).FullName}");
        }

        public void OnUnregister()
        {
            
        }

        public void Update(float elapsedSeconds, float realElapseSeconds)
        {
            _tempPrefabs.Clear();
            _tempPrefabs.AddRange(_prefabs);
            foreach (var prefab in _tempPrefabs)
            {
                prefab.EntityUpdate(elapsedSeconds);
            }
        }

        public void FixedUpdate(float fixedDeltaTime)
        {
            _fixedTempPrefabs.Clear();
            _fixedTempPrefabs.AddRange(_prefabs);
            foreach (var prefab in _fixedTempPrefabs)
            {
                prefab.EntityFixedUpdate(fixedDeltaTime);
            }
        }

        public void Shutdown()
        {
            
        }
    }
}