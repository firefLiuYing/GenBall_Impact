using UnityEngine;

namespace GenBall.Enemy
{
    [CreateAssetMenu(menuName = "Enemy/EnemyConfig")]
    public class EnemyConfigSo : ScriptableObject
    {
        [Header("基础属性")] public int maxHealth = 100;
        [Header("击杀")] public int killPoints = 10;
    }
}
