using System.Collections.Generic;
using GenBall.BattleSystem;
using Yueyn.Main;
using Yueyn.ObjectPool;
using Yueyn.Utils;

namespace GenBall.Enemy
{
    public partial class EnemyCreator:IComponent
    {
        private readonly Dictionary<TypeNamePair, string> _enemyMap = new();
        private readonly List<IEnemy> _enemys = new();
        private readonly List<IEnemy> _tempEnemys = new();
        private IObjectPool<EnemyObject> _enemyPool;
        
        public void OnRegister()
        {
            _enemyPool = GameEntry.GetModule<ObjectPoolManager>().CreateSingleSpawnObjectPool<EnemyObject>();
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