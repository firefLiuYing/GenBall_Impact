using System;
using System.Collections.Generic;
using System.Linq;
using GenBall.Map;
using GenBall.Map.EnemyUnitConfig;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GenBall.Utils.Editor.Map
{
    public class MapEditorWindow : EditorWindow
    {
        [MenuItem("Tools/Map/地图编辑器")]
        public static void OpenWindow()
        {
            GetWindow<MapEditorWindow>("地图编辑器");
        }

        private void OnGUI()
        {
            GUILayout.Label("地图解析工具", EditorStyles.boldLabel);
            if (GUILayout.Button("解析存档点信息"))
            {
                AnalysisSavePoint();
            }

            if (GUILayout.Button("解析敌人配置信息"))
            {
                AnalysisEnemyUnit();
            }

            if (GUILayout.Button("一键解析地图信息"))
            {
                AnalysisSavePoint();
                AnalysisEnemyUnit();
            }
        }

    #region Bake SavePointConfig

    private readonly List<SavePointConfig> _cachedSavePoints = new List<SavePointConfig>();
    private void AnalysisSavePoint()
    {
        var scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        _cachedSavePoints.Clear();
        SceneConfig sceneConfig=null;
        foreach (var root in roots)
        {
            _cachedSavePoints.AddRange(root.GetComponentsInChildren<SavePointConfig>());
            if (sceneConfig == null)
            {
                sceneConfig= root.GetComponentInChildren<SceneConfig>();
            }
        }

        var mapModel = ConfigProvider.GetOrCreateMapConfig();
        var sceneModel = mapModel.scenes.FirstOrDefault(s => s.sceneName == scene.name);
        if (sceneModel == null)
        {
            sceneModel = new SceneModel();
            mapModel.scenes.Add(sceneModel);
        }

        sceneModel.displayName = sceneConfig != null ? sceneConfig.DisplayName : "请输入文本";
        sceneModel.sceneName=scene.name;
        sceneModel.savePoints.Clear();
        for (var i = 0; i < _cachedSavePoints.Count; i++)
        {
            _cachedSavePoints[i].Index = i;
            var savePointModel = new SavePointModel
            {
                id = i,
                displayName = _cachedSavePoints[i].DisplayName,
                spawnPosition = _cachedSavePoints[i].PlayerSpawnPoint.position,
                spawnRotation = _cachedSavePoints[i].PlayerSpawnPoint.rotation
            };
            sceneModel.savePoints.Add(savePointModel);
        }
        
        EditorUtility.SetDirty(mapModel);
        AssetDatabase.SaveAssets();
    }

    private readonly List<EnemyUnitConfigBase> _cachedEnemyUnitConfigs = new();
    private void AnalysisEnemyUnit()
    {
        var scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        _cachedEnemyUnitConfigs.Clear();
        SceneConfig sceneConfig=null;
        foreach (var root in roots)
        {
            _cachedEnemyUnitConfigs.AddRange(root.GetComponentsInChildren<EnemyUnitConfigBase>());
            if (sceneConfig == null)
            {
                sceneConfig= root.GetComponentInChildren<SceneConfig>();
            }
        }
        var mapModel = ConfigProvider.GetOrCreateMapConfig();
        var sceneModel = mapModel.scenes.FirstOrDefault(s => s.sceneName == scene.name);
        if (sceneModel == null)
        {
            sceneModel = new SceneModel();
            mapModel.scenes.Add(sceneModel);
        }
        sceneModel.displayName = sceneConfig != null ? sceneConfig.DisplayName : "请输入文本";
        sceneModel.sceneName=scene.name;
        sceneModel.enemyUnits.Clear();

        for (var i = 0; i < _cachedEnemyUnitConfigs.Count; i++)
        {
            _cachedEnemyUnitConfigs[i].Index = i;
            var enemyUnitModel = new EnemyUnitModel()
            {
                id = i,
                enemyType = _cachedEnemyUnitConfigs[i].TypeName,
                spawnPosition = _cachedEnemyUnitConfigs[i].transform.position,
                spawnRotation = _cachedEnemyUnitConfigs[i].transform.rotation,
            };
            sceneModel.enemyUnits.Add(enemyUnitModel);
        }
        EditorUtility.SetDirty(mapModel);
        AssetDatabase.SaveAssets();
    }
    


    #endregion
    
    }
}