using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.Procedure
{
    /// <summary>
    /// 用户设置独立存储，不依赖存档槽位。
    /// 使用 JsonUtility 序列化到 user_settings.json。
    /// </summary>
    public class UserSettingsStorage : IUserSettingsStorage
    {
        public UserSettings Settings { get; private set; }

        public void Init()
        {
            Settings = LoadSync();
        }

        public void UnInit()
        {
        }

        public async Task SaveAsync()
        {
            var filePath = GetFilePath();
            try
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonUtility.ToJson(Settings, true);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[UserSettingsStorage] Failed to save user settings: {e.Message}");
            }
        }

        public void ApplyToRuntime()
        {
            // Placeholder: future wiring to audio mixer / input system.
            // Example:
            //   AudioMixer.SetFloat("MasterVolume", LinearToDb(Settings.masterVolume));
            //   InputSystem.LookSensitivity = new Vector2(Settings.horizontalSensitivity, Settings.verticalSensitivity);
        }

        private UserSettings LoadSync()
        {
            var filePath = GetFilePath();
            try
            {
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    var settings = JsonUtility.FromJson<UserSettings>(json);
                    if (settings != null)
                    {
                        return settings;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[UserSettingsStorage] Failed to load user settings, using defaults: {e.Message}");
            }

            return new UserSettings();
        }

        public static string GetFilePath()
        {
#if UNITY_EDITOR
            return Path.Combine(Application.dataPath, "SaveGames", "user_settings.json");
#else
            return Path.Combine(Application.persistentDataPath, "user_settings.json");
#endif
        }
    }
}
