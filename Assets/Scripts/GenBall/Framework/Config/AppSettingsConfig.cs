using GenBall.Procedure.Game;
using UnityEngine;

namespace GenBall.Framework.Config
{
    /// <summary>
    /// 应用设置配置，替代各系统 MonoBehavior 上的 [SerializeField] 字段
    /// 各字段在迁移对应系统时填充默认值
    /// </summary>
    [CreateAssetMenu(fileName = "AppSettingsConfig", menuName = "GenBall/AppSettingsConfig")]
    public class AppSettingsConfig : ScriptableObject
    {
        [Header("存档")]
        public int maxSaveCount = 6;

        [Header("启动")]
        public string startSceneName = "Prologue";
        public RunningMode runningMode = RunningMode.SaveData | RunningMode.LoadData;

        [Header("UI")]
        public int orderInterval = 100;

        [Header("地图")]
        public int loadLayerCount = 1;

        [Header("Player")]
        public Vector3 defaultPlayerSpawnPosition = Vector3.zero;
        public Vector3 defaultPlayerSpawnRotation = Vector3.zero;

        [Header("Debug")]
        public bool devMode = false;
    }
}
