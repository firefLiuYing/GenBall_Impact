using System;
using UnityEditor;
using UnityEngine;

namespace GenBall.Player
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Player/PlayerConfig")]
    [Serializable]
    [System.Obsolete("Migrated to AppSettingsConfig")]
    public partial class PlayerConfigSo : ScriptableObject
    {
        [Header("�ƶ��ٶ�")]
        public float speed;
        [Header("�ӽ�ת��������")]
        public float verticalSensitivity;
        public float horizontalSensitivity;

        [Header("��Ծ")] 
        public float shortPressJumpHeight;
        public float longPressJumpMaxHeight;
        public float longPressMaxTime;
        public float shortPressJustifyTime;
        public float gravityAcceleration;
        public float maxDropVelocity;
        public float coyoteTime;
        public float jumpInputBufferTime;
        
        [Header("���")]
        public float invincibleTime;
        public float endingTime;
        public float dashSpeed;
        public float dashCountdownTime;

    }

    public static class PlayerConfigProvider
    {
        private const string PlayerConfigSoPath = "Assets/AssetBundles/Config/PlayerConfig.asset";
        private static PlayerConfigSo _cachedConfig;
        #if UNITY_EDITOR
        public static PlayerConfigSo GetOrCreatePlayerConfigSo()
        {
            
            if(_cachedConfig!=null)  return _cachedConfig;
            var guids=AssetDatabase.FindAssets("t:PlayerConfigSO");
            if (guids.Length > 1)
            {
                Debug.LogError("gzp ���ֶ��PlayerConfigSO����ֻ����һ��");
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
            Debug.Log("gzp ���Զ�����PlayerConfigSO");
            _cachedConfig = config;
            return _cachedConfig;
        }
        #endif
    }
}