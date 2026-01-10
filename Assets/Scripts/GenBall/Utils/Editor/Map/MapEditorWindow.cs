using System;
using System.Collections.Generic;
using GenBall.Map;
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
        
        private GameObject _rootObject;
        private string _targetPath;
        private string _mapDisplayName;
        private string ConfigPath => _targetPath + "/Config";
        private string PrefabPath => _targetPath + "/Prefab";

        private MapConfig _curMapConfig;

        private readonly List<GameObject> _previewInstances = new();
        private Dictionary<string, int> _blockNameMap = new();
        private void OnGUI()
        {
            GUILayout.Label("地图解析工具", EditorStyles.boldLabel);
            _rootObject=(GameObject)EditorGUILayout.ObjectField("根物体（root）",_rootObject, typeof(GameObject), true);
            _targetPath=EditorGUILayout.TextField("生成路径", _targetPath);
            _mapDisplayName=EditorGUILayout.TextField("地图名称", _mapDisplayName);
            EditorGUILayout.Space();
            GUILayout.Label("生成路径预览：", EditorStyles.boldLabel);
            GUILayout.Label("配置文件生成路径："+ConfigPath);
            GUILayout.Label("预制体生成路径"+PrefabPath);

            if (GUILayout.Button("生成Config并导出预制体"))
            {
                if (_rootObject == null)
                {
                    Debug.LogWarning("请先选择要解析的根物体");
                }
                else if(string.IsNullOrEmpty(_targetPath))
                {
                    Debug.LogWarning("请配置生成路径");
                }
                else if (string.IsNullOrEmpty(_mapDisplayName))
                {
                    Debug.LogWarning("请配置地图在游戏内展示的名称");
                }
                else
                {
                    BakeMapConfig();
                }
            }
            
            EditorGUILayout.Space();
            _curMapConfig=(MapConfig)EditorGUILayout.ObjectField("MapConfig", _curMapConfig, typeof(MapConfig), true);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("在当前场景生成地图预览"))
            {
                if (_rootObject == null)
                {
                    Debug.LogWarning("请先选择生成的根物体");
                }
                else
                {
                    SpawnPreviewMap();
                }
            }

            if (GUILayout.Button("删除预览地图"))
            {
                DeletePreviewMap();
            }
            EditorGUILayout.EndHorizontal();
        }

        #region Bake MapConfig

        private void BakeMapConfig()
        {
            var blockAuthors = _rootObject.GetComponentsInChildren<MapBlockAuthoring>();
            _previewInstances.Clear();
            if (blockAuthors.Length <= 0)
            {
                Debug.LogWarning("没有检测到带有MapBlockAuthoring组件的子物体");
                return;
            }

            if (!AssetDatabase.IsValidFolder(_targetPath))
            {
                Debug.LogWarning($"找不到指定路径：{_targetPath}");
                return;
            }
            if (!AssetDatabase.IsValidFolder(ConfigPath))
            {
                AssetDatabase.CreateFolder(_targetPath, "Config");
            }

            if (!AssetDatabase.IsValidFolder(PrefabPath))
            {
                AssetDatabase.CreateFolder(_targetPath, "Prefab");
            }
            
            var mapConfig = ScriptableObject.CreateInstance<MapConfig>();
            mapConfig.sceneDisplayName=_mapDisplayName;
            mapConfig.sceneName=SceneManager.GetActiveScene().name;
            int indexCounter = 0;
            int savePointIndexCounter = 0;
            foreach (var blockAuthor in blockAuthors)
            {
                var block = new MapBlockConfig
                {
                    mapBlockIndex = indexCounter++,
                    multiBounds = new()
                };
                blockAuthor.AddMapBlock();
                foreach (var r in blockAuthor.GetComponentsInChildren<Renderer>())
                {
                    block.multiBounds.Add(r.bounds);
                }

                foreach (var savePointAuthor in blockAuthor.GetComponentsInChildren<SavePointAuthoring>())
                {
                    var savePointInfo=new SavePointInfo
                    {
                        index = savePointIndexCounter++,
                        mapBlockIndex = block.mapBlockIndex,
                        playerSpawnPosition = savePointAuthor.transform.position,
                        playerSpawnRotation = savePointAuthor.transform.rotation,
                        savePointName = savePointAuthor.SavePointName,
                    };
                    savePointAuthor.SavePointIndex=savePointInfo.index;
                    mapConfig.savePointInfos.Add(savePointInfo);
                }
                // 保存预制体
                var blockName = blockAuthor.gameObject.name;
                if (_blockNameMap.TryGetValue(blockName, out var value))
                {
                    blockName = $"{blockName}_{value+1}";
                }
                else
                {
                    _blockNameMap.Add(blockName,0);
                }

                _blockNameMap[blockAuthor.gameObject.name]++;
                blockAuthor.gameObject.name = blockName;
                string prefabPath = $"{PrefabPath}/{blockName}.prefab";
                PrefabUtility.SaveAsPrefabAsset(blockAuthor.gameObject, prefabPath);
                block.mapBlockPrefabPath = prefabPath;
                block.neighbors = new List<int>();
                mapConfig.mapBlockConfigs.Add(block);
                
                // 收集到实例列表里面
                _previewInstances.Add(blockAuthor.gameObject);
            }
            // 邻接节点分析
            for (int i = 0; i < mapConfig.mapBlockConfigs.Count; i++)
            {
                var a=mapConfig.mapBlockConfigs[i];
                for (int j = i + 1; j < mapConfig.mapBlockConfigs.Count; j++)
                {
                    var  b=mapConfig.mapBlockConfigs[j];
                    if (AreBlocksAdjacent(a, b))
                    {
                        a.neighbors.Add(b.mapBlockIndex);
                        b.neighbors.Add(b.mapBlockIndex);
                    }
                }
            }
            
            // 保存config
            string configPath = $"{ConfigPath}/MapConfig.asset";
            AssetDatabase.CreateAsset(mapConfig, configPath);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("解析完成", $"生成了{mapConfig.mapBlockConfigs.Count}个地块，MapConfig已保存到{configPath}",
                "Ok");
            _curMapConfig = mapConfig;
            
            SceneMapIndexProvider.RegisterMapConfig(mapConfig);
        }

        private bool AreBlocksAdjacent(MapBlockConfig a, MapBlockConfig b)
        {
            foreach (var ba in a.multiBounds)
            {
                foreach (var bb in b.multiBounds)
                {
                    if(ba.Intersects(bb)) return true;
                }
            }
            return false;
        }

        #endregion

        #region Preview Map

        private void SpawnPreviewMap()
        {
            if (_previewInstances.Count > 0)
            {
                DeletePreviewMap();
            }

            foreach (var block in _curMapConfig.mapBlockConfigs)
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(block.mapBlockPrefabPath);
                if (prefab != null)
                {
                    var instance=Instantiate(prefab, _rootObject.transform, true);
                    instance.name = prefab.name;
                    _previewInstances.Add(instance);
                }
            }
        }

        private void DeletePreviewMap()
        {
            foreach (var go in _previewInstances)
            {
                if (go != null)
                {
                    DestroyImmediate(go);
                }
            }
            _previewInstances.Clear();
        }

        #endregion
    }
}