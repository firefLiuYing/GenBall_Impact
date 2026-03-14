using System;
using UnityEditor;
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

    public static class PlayerConfigProvider
    {
        private const string PlayerConfigSoPath = "Assets/AssetBundles/Config/PlayerConfig.asset";
        private static PlayerConfigSo _cachedConfig;
        public static PlayerConfigSo GetOrCreatePlayerConfigSo()
        {
            if(_cachedConfig!=null)  return _cachedConfig;
            var guids=AssetDatabase.FindAssets("t:PlayerConfigSO");
            if (guids.Length > 1)
            {
                Debug.LogError("gzp 发现多个PlayerConfigSO，请只保留一个");
                return null;
            }
            if (guids.Length == 1)
            {
                var path=AssetDatabase.GUIDToAssetPath(guids[0]);
                _cachedConfig= AssetDatabase.LoadAssetAtPath<PlayerConfigSo>(path);
                return _cachedConfig;
            }
            var config=ScriptableObject.CreateInstance<PlayerConfigSo>();
            AssetDatabase.CreateAsset(config,PlayerConfigSoPath);
            AssetDatabase.SaveAssets();
            Debug.Log("gzp 已自动创建PlayerConfigSO");
            _cachedConfig = config;
            return _cachedConfig;
        }
    }
}