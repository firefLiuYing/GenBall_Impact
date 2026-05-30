using UnityEngine;

namespace GenBall.Enemy
{
    [CreateAssetMenu(menuName = "Enemy/Config")]
    public class EnemyConfigSo : ScriptableObject
    {
        [Header("基础属性")]
        public int maxHealth = 100;
        public int killPoints = 10;

        [Header("移动")]
        public float moveSpeed = 3f;
        public float patrolSpeed = 1.5f;

        [Header("跳跃")]
        public float jumpForce = 8f;
        public float jumpInterval = 0.5f;
        [Range(0f, 90f)]
        public float jumpElevation = 45f;

        [Header("攻击 - 冲锋")]
        public int attackDamage = 15;
        public float dashSpeed = 12f;
        public float preparationTime = 0.3f;
        public float chargingTime = 0.2f;
        public float flyHeightOffset = 1.5f;
        public float dashHeightOffset = 1f;
        public float reboundForce = 12f;

        [Header("检测")]
        public float detectRange = 10f;
        public float hateRange = 15f;
        public float attackRange = 2f;

        [Header("重量等级")]
        public int weightLevel;

        [Header("物理")]
        public float gravityAcceleration = 10f;
        public float maxDropVelocity = 15f;

        [Header("攻击 - 飞行打磨")]
        public float flyAccelTime = 0.5f;

        [Header("攻击 - 反弹")]
        [Range(0.2f, 2f)]
        public float reboundUpwardRatio = 1.2f;

        [Header("攻击 - 撞墙")]
        [Range(0.1f, 1f)]
        public float wallBounceMultiplier = 0.4f;

        [Header("挤压拉伸")]
        public float squashRatio = 0.7f;
        public float stretchRatio = 1.3f;
        public float squashStretchRecoverySpeed = 6f;

        [Header("跳跃变化")]
        [Range(0f, 0.3f)] public float jumpIntervalVariation = 0.15f;
        [Range(0f, 0.2f)] public float jumpForceVariation = 0.1f;
        [Range(0f, 10f)] public float jumpElevationVariation = 5f;
    }
}
