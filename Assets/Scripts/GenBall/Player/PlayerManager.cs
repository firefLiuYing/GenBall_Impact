using System;
using GenBall.Utils.EntityCreator;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.Player
{
    public sealed class PlayerManager : MonoBehaviour,IComponent
    {
        public int Priority => 1000;
        private EntityCreator<Player> PlayerCreator => GameEntry.GetModule<EntityCreator<Player>>();
        public Player Player { get;private set; }
        [SerializeField] private Transform defaultPlayerSpawnPoint;
        
        public Transform DefaultPlayerSpawnPoint=>defaultPlayerSpawnPoint??transform;

        public void CreatePlayer()=>CreatePlayer(defaultPlayerSpawnPoint);

        public void CreatePlayer(Vector3 position, Quaternion rotation)
        {
            if (Player != null)
            {
                throw new Exception("当前场景已有Player");
            }
            var player = PlayerCreator.CreateEntity<Player>(position, rotation,DefaultPlayerSpawnPoint);
            player.Initialize();
            Player = player;
        }
        public void CreatePlayer(Transform spawnTransform)
        {
            if (Player != null)
            {
                throw new Exception("当前场景已有Player");
            }

            if (spawnTransform == null)
            {
                spawnTransform = transform;
            }
            var player = PlayerCreator.CreateEntity<Player>(spawnTransform.position, spawnTransform.rotation,DefaultPlayerSpawnPoint);
            player.Initialize();
            Player = player;
        }
        public void Init()
        {
            PlayerCreator.AddPrefab<Player>("Assets/AssetBundles/Common/Player/Prefab/Player.prefab");
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