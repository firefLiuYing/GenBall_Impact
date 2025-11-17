using System;
using System.Collections.Generic;
using UnityEngine;
using Yueyn.Main;
using Yueyn.ObjectPool;
using Yueyn.Resource;
using Yueyn.Utils;
using Object = UnityEngine.Object;

namespace GenBall.BattleSystem.Weapons
{
    public partial class WeaponCreator:IComponent
    {
        private IObjectPool<WeaponObject> _weaponPool;
        private readonly Dictionary<TypeNamePair, string> _weaponMap = new();
        private ResourceManager ResourceManager => GameEntry.GetModule<ResourceManager>();

        /// <summary>
        /// ªÿ ’Œ‰∆˜GameObject
        /// </summary>
        /// <param name="weapon">Œ‰∆˜GameObject</param>
        public void RecycleWeapon(GameObject weapon) => _weaponPool.Despawn(weapon);
        public TWeapon CreateWeapon<TWeapon>(Transform spawnPoint) where TWeapon : IWeapon => (TWeapon)CreateWeapon(new TypeNamePair(typeof(TWeapon)),spawnPoint);
        public TWeapon CreateWeapon<TWeapon>(string name,Transform spawnPoint) where TWeapon : IWeapon => (TWeapon)CreateWeapon(new TypeNamePair(typeof(TWeapon), name), spawnPoint);
        private IWeapon CreateWeapon(TypeNamePair typeNamePair,Transform spawnPoint)
        {
            var weaponObject=_weaponPool.Spawn($"{typeNamePair}");
            if (weaponObject != null)
            {
                return (IWeapon)weaponObject.Target;
            }
            if (!_weaponMap.TryGetValue(typeNamePair, out var prefabPath))
            {
                throw new Exception($"Weapon Prefab {typeNamePair} is not registered");
            }
            var prefab=(GameObject)ResourceManager.LoadPrefab(prefabPath);
            var go=Object.Instantiate(prefab,spawnPoint);
            var weapon = go.GetComponent<IWeapon>();
            if (weapon == null) throw new Exception($"Weapon Prefab {typeNamePair} not found Component IWeapon");
            weaponObject=WeaponObject.Create($"{typeNamePair}",go);
            _weaponPool.Register(weaponObject,true);
            return weapon;
        }

        private void AddWeaponPrefab<TWeapon>(string prefabPath) where TWeapon : IWeapon => AddWeaponPrefab(new TypeNamePair(typeof(TWeapon)), prefabPath);
        private void AddWeaponPrefab<TWeapon>(string name,string prefabPath) where TWeapon:IWeapon =>AddWeaponPrefab(new TypeNamePair(typeof(TWeapon),name),prefabPath);

        private void AddWeaponPrefab(TypeNamePair typeNamePair, string prefabPath)
        {
            if (!typeof(IWeapon).IsAssignableFrom(typeNamePair.Type))
            {
                throw new Exception($"Weapon  Prefab {typeNamePair} is not a Weapon type");
            }

            if (!_weaponMap.TryAdd(typeNamePair, prefabPath))
            {
                throw new Exception($"Weapon Prefab {typeNamePair} is not a bullet type");
            }
        }
        public void OnRegister()
        {
            _weaponPool = GameEntry.GetModule<ObjectPoolManager>().CreateSingleSpawnObjectPool<WeaponObject>();
            RegisterWeapons();
        }

        public void OnUnregister()
        {
            
        }

        public void Update(float elapsedSeconds, float realElapseSeconds)
        {
            
        }

        public void FixedUpdate(float fixedDeltaTime)
        {
            
        }

        public void Shutdown()
        {
            
        }
    }
}