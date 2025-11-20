using System;
using GenBall.Utils.EntityCreator;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.Player
{
    public sealed class PlayerManager : MonoBehaviour,IComponent
    {
        private EntityCreator<Player> PlayerCreator => GameEntry.GetModule<EntityCreator<Player>>();
        public Player Player { get;private set; }
        [SerializeField] private Transform defaultPlayerSpawnPoint;

        public void CreatePlayer()=>CreatePlayer(defaultPlayerSpawnPoint);
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
            var player = PlayerCreator.CreateEntity<Player>(spawnTransform);
            player.Initialize();
            Player = player;
        }
        public void OnRegister()
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