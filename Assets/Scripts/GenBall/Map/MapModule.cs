using System.Collections.Generic;
using System.Linq;
using GenBall.Event.Generated;
using GenBall.Procedure;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yueyn.Main;
using Yueyn.Resource;

namespace GenBall.Map
{
    public class MapModule : MonoBehaviour,IComponent
    {
        public int Priority => 1000;
        [SerializeField] private Transform mapRoot;
        private MapConfig _mapConfig;
        [SerializeField,Header("加载参数"),Tooltip("进入一个关卡时，当加载周围关卡块层数，当为0时加载所有层数")] private int loadLayerCount;
        private readonly Dictionary<int, bool> _blockActiveTable = new();

        private readonly Dictionary<int, string> _blockPrefabPaths = new();
        private readonly Dictionary<int, MapBlockConfig> _blockMap = new();
        private int _curMapBlockIndex;
        public void Init()
        {
            if (mapRoot == null)
            {
                mapRoot = transform;
            }
            var sceneName=SceneManager.GetActiveScene().name;
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
            foreach (var blockConfig in _blockMap.Values)
            {
                if (blockConfig.InBlock(playerTransform.position))
                {
                    EnterMapBlock(blockConfig.mapBlockIndex);
                    break;
                }
            }
        }
        private void EnterMapBlock(int mapBlockIndex)
        {
            if (_curMapBlockIndex == mapBlockIndex)
            {
                return;
            }
            if (!_blockMap.ContainsKey(mapBlockIndex)&&_curMapBlockIndex!=-1)
            {
                Debug.LogError($"gzp mapBlockIndex:{mapBlockIndex}不合法");
                return;
            }
            _curMapBlockIndex = mapBlockIndex;
            LoadBlocks(mapBlockIndex,loadLayerCount);
            Debug.Log($"gzp 当前关卡块：{mapBlockIndex}");
        }

        private void LoadMapBlock(int index)
        {
            if (!_blockActiveTable[index])
            {
                var path = _blockPrefabPaths[index];
                var prefab = CResourceManager.Instance.LoadSync<GameObject>(path);
                var block = Object.Instantiate(prefab, mapRoot);
                var mapBlock = block.GetComponent<IMapBlock>();
                mapBlock.SetIndex(index);
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
            _cachedBlockNeighborIndexList.Add(blockIndex);
            for (int i = 0; i < layerCount; i++)
            {
                _tempBlockNeighborIndexList.Clear();
                foreach (var blockIndexInCached in _cachedBlockNeighborIndexList)
                {
                    _tempBlockNeighborIndexList.AddRange(_blockMap[blockIndexInCached].neighbors);
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
