using GenBall.Enemy.AI;
using UnityEngine;

namespace GenBall.Enemy
{
    /// <summary>
    /// Placed on enemy prefabs to provide config references at spawn time.
    /// Checked by EnemyId.Create() and SceneExecutorSystemDefault.LoadEnemyUnit().
    /// </summary>
    public class EnemyConfigReference : MonoBehaviour
    {
        [SerializeField] private EnemyConfigSo config;
        [SerializeField] private EnemyAIConfigSo aiConfig;

        public EnemyConfigSo Config => config;
        public EnemyAIConfigSo AiConfig => aiConfig;
    }
}
