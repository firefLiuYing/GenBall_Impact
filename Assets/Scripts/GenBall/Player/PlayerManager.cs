using System;
using GenBall.BattleSystem.Character;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.Player
{
    public sealed class PlayerManager : MonoBehaviour,IComponent
    {
        public int Priority => 1000;
        public GameObject Player => SystemRepository.Instance.GetSystem<IPlayerSystem>().Player;
        [SerializeField] private Transform defaultPlayerSpawnPoint;

        public Transform DefaultPlayerSpawnPoint=>defaultPlayerSpawnPoint??transform;

        public void CreatePlayer()
        {
            var sys = SystemRepository.Instance.GetSystem<IPlayerSystem>();
            var pos = defaultPlayerSpawnPoint != null ? defaultPlayerSpawnPoint.position : Vector3.zero;
            var rot = defaultPlayerSpawnPoint != null ? defaultPlayerSpawnPoint.rotation : Quaternion.identity;
            sys.CreatePlayer(pos, rot);
        }

        public void CreatePlayer(Vector3 position, Quaternion rotation)
        {
            SystemRepository.Instance.GetSystem<IPlayerSystem>().CreatePlayer(position, rotation);
        }
        public void CreatePlayer(Transform spawnTransform)
        {
            if (spawnTransform == null)
                spawnTransform = transform;
            SystemRepository.Instance.GetSystem<IPlayerSystem>()
                .CreatePlayer(spawnTransform.position, spawnTransform.rotation);
        }
        public void Init()
        {
        }

        public void OnUnregister()
        {
            
        }

        public void ComponentUpdate(float elapsedSeconds, float realElapseSeconds)
        {
            
        }

        public void ComponentFixedUpdate(float fixedDeltaTime)
        {
            
        }

        public void Shutdown()
        {
            
        }
    }
}