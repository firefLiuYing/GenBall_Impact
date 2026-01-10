using System.Collections.Generic;
using System.Linq;
using GenBall.Event.Generated;
using GenBall.Procedure;
using GenBall.Utils.EntityCreator;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yueyn.Main;

namespace GenBall.Map
{
    public class MapModule : MonoBehaviour,IComponent
    {
        public int Priority => 1000;
        [SerializeField] private Transform mapRoot;
        private EntityCreator<IMapBlock> MapBlockCreator => GameEntry.GetModule<EntityCreator<IMapBlock>>();
        private MapConfig _mapConfig;
        [SerializeField,Header("加载层数"),Tooltip("加载一个地块时，额外加载周围地块的层数，等于0时代表不额外加载")] private int loadLayerCount;
        private readonly Dictionary<int, bool> _blockActiveTable = new();

        private readonly Dictionary<int, MapBlockConfig> _blockMap = new();
        private int _curMapBlockIndex;
        public void Init()
        {
            if (mapRoot == null)
            {
                mapRoot = transform;
            }
            var sceneName=SceneManager.GetActiveScene().name;
            _mapConfig=SceneMapIndexProvider.GetMapConfig(sceneName);
            if (_mapConfig == null)
            {
                Debug.LogError("gzp 请添加地图配置");
                return;
            }
            if (_mapConfig.mapBlockConfigs.Count == 0)
            {
                Debug.LogError("gzp 地图配置至少要有一个地图块");
                return;
            }

            if (_mapConfig.mapBlockConfigs.Count == 0)
            {
                Debug.LogError("gzp 地图配置至少要有一个存档点");
                return;
            }
        
            foreach (var blockConfig in _mapConfig.mapBlockConfigs)
            {
                _blockActiveTable[blockConfig.mapBlockIndex] = false;
                _blockMap.Add(blockConfig.mapBlockIndex, blockConfig);
                MapBlockCreator.AddPrefab<MapBlockBase>(blockConfig.BlockName,blockConfig.mapBlockPrefabPath);
            }
            
            _curMapBlockIndex = -1;
        }

        public void LoadSavePointAround(int savePointIndex)
        {
            var savePointInfo=GetSavePointInfo(savePointIndex);
            if (savePointInfo == null)
            {
                Debug.LogError($"gzp savePointIndex:{savePointIndex}不存在");
                return;
            }
            var blockIndex=savePointInfo.mapBlockIndex;
            LoadBlocks(blockIndex,loadLayerCount);
        }

        public SavePointInfo GetSavePointInfo(int savePointIndex)
        {
            var savePointInfo=_mapConfig.savePointInfos.FirstOrDefault(s => s.index==savePointIndex);
            return savePointInfo;
        }
        private void OnPlayerPositionChanged(Transform playerTransform)
        {
            // todo gzp 补充判定进入哪个地块的逻辑
        }
        private void EnterMapBlock(int mapBlockIndex)
        {
            if (_curMapBlockIndex == mapBlockIndex)
            {
                // Debug.LogError($"gzp 进入的地图块和当前地图块index相同：_curMapBlockIndex:{mapBlockIndex}");
                return;
            }
        
            if (_curMapBlockIndex != -1)
            {
                GameEntry.Event.FireMapExit(_curMapBlockIndex);
            }
            if (!_blockMap.ContainsKey(mapBlockIndex)&&_curMapBlockIndex!=-1)
            {
                Debug.LogError($"gzp mapBlockIndex:{mapBlockIndex}不合法");
                return;
            }
            _curMapBlockIndex = mapBlockIndex;
            LoadBlocks(mapBlockIndex,3);
            Debug.Log($"gzp 进入地块：{mapBlockIndex}");
            GameEntry.Event.FireMapEnter(mapBlockIndex);
        }
        
        private void LoadMapBlock(int index)
        {
            if (!_blockActiveTable[index])
            {
                var block= MapBlockCreator.CreateEntity<MapBlockBase>($"Block_{index}");
                block.transform.SetParent(mapRoot.transform, true);
                block.SetIndex(index);
                _blockActiveTable[index] = true;
            }
        }
        
        private readonly List<int> _cachedBlockNeighborIndexList = new List<int>();
        private readonly List<int> _tempBlockNeighborIndexList = new List<int>();
        private void LoadBlocks(int blockIndex, int layerCount)
        {
            LoadMapBlock(blockIndex);
            _cachedBlockNeighborIndexList.Clear();
            _tempBlockNeighborIndexList.Clear();
            _cachedBlockNeighborIndexList.AddRange(_blockMap[blockIndex].neighbors);
            for (int i = 0; i < layerCount; i++)
            {
                _tempBlockNeighborIndexList.Clear();
                foreach (var neighbor in _cachedBlockNeighborIndexList)
                {
                    _tempBlockNeighborIndexList.AddRange(_blockMap[neighbor].neighbors);
                }
                _cachedBlockNeighborIndexList.Clear();
                _cachedBlockNeighborIndexList.AddRange(_tempBlockNeighborIndexList);
                foreach (var index in _cachedBlockNeighborIndexList)
                {
                    LoadMapBlock(index);
                }
            }
        }

        public void OnUnregister()
        {
            
        }

        public void ComponentUpdate(float elapsedSeconds, float realElapseSeconds)
        {
            
        }

        public void ComponentFixedUpdate(float fixedDeltaTime)
        {
            
        }

        public void Shutdown()
        {
            
        }
    }
}