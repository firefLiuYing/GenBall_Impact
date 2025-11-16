using System;
using UnityEngine;

namespace GenBall.Player
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Player/PlayerConfig")]
    [Serializable]
    public partial class PlayerConfigSo : ScriptableObject
    {
        [Header("移动速度")]
        public float speed;
        [Header("视角转动灵敏度")]
        public float verticalSensitivity;
        public float horizontalSensitivity;

        [Header("跳跃")] 
        public float shortPressJumpHeight;
        public float longPressJumpMaxHeight;
        public float longPressMaxTime;
        public float shortPressJustifyTime;
        public float gravityAcceleration;
        public float maxDropVelocity;
        public float coyoteTime;
        public float jumpInputBufferTime;
        
        [Header("冲刺")]
        public float invincibleTime;
        public float endingTime;
        public float dashSpeed;
        public float dashCountdownTime;

    }
}