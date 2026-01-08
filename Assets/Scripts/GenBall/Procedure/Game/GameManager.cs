using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenBall.Utils.EntityCreator;
using GenBall.Utils.Singleton;
using JetBrains.Annotations;
using UnityEngine;

namespace GenBall.Procedure.Game
{
    public class GameManager : ISingleton
    {
        public static GameManager Instance => SingletonManager.GetSingleton<GameManager>();
        private int _curSaveIndex;
        private readonly List<SaveSlotData> _cachedSaveSlotData = new();
        private GameData _gameData;

        /// <summary>
        /// 获取所有存档的基本信息
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<SaveSlotData>> GetSaveSlotDatas()
        {
            try
            {
                return await GameEntry.Save.GetSaveSlotDatas();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return Enumerable.Empty<SaveSlotData>();
            }
        }
        
        /// <summary>
        /// 开始新的游戏
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartNewGame()
        {
            try
            {
                var saveIndex = await GameEntry.Save.CreateNewSave();
                return await InternalStartGame(saveIndex);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        /// <summary>
        /// 继续上次游玩
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ContinueLastGame()
        {
            try
            {
                return await InternalContinueLastGame();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        /// <summary>
        /// 开始指定id的存档
        /// </summary>
        /// <param name="saveIndex"></param>
        /// <returns></returns>
        public async Task<bool> LoadGame(int saveIndex)
        {
            try
            {
                return await InternalStartGame(saveIndex);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        /// <summary>
        /// 保存游戏
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SaveGame()
        {
            try
            {
                return await InternalSaveGame();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }
        

        private async Task<bool> InternalContinueLastGame()
        {
            try
            {
                // _saveSlotInfo=await GameEntry.Save.GetSaveSlotInfo();
                var saveSlotDatas = await GameEntry.Save.GetSaveSlotDatas();
                _cachedSaveSlotData.Clear();
                _cachedSaveSlotData.AddRange(saveSlotDatas);
                var saveIndex= _cachedSaveSlotData.OrderByDescending(slot=>slot.LastUpdateTime).First().saveIndex;
                return await InternalStartGame(saveIndex);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        private async Task<bool> InternalStartGame(int saveIndex)
        {
            try
            {   
                var gameData = await GameEntry.Save.LoadGameData(saveIndex);
                _curSaveIndex = saveIndex;
                if (await InternalStartGame(gameData))
                {
                    _curSaveIndex=saveIndex;
                    return true;
                }
                _curSaveIndex = -1;
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }
        private async Task<bool> InternalStartGame(GameData gameData)
        {
            try
            {
                await Task.Delay(1);
                // todo gzp 模拟通过游戏数据开始游戏
                Debug.Log($"读取到存档信息：{gameData}");
                Debug.Log("开始游戏");
                
                // 隐藏鼠标
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                // 加载地图
                // 创建玩家
                GameEntry.Player.CreatePlayer();
                _gameData = gameData;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        private async Task<bool> InternalSaveGame()
        {
            try
            {
                if(_curSaveIndex <0||_gameData==null) return false;
                await Task.Delay(1);
                // todo gzp 模拟获取存档还有的数据
                
                // 最近一次游玩改成现在
                _gameData.LastUpdateTime = DateTime.Now;
                Debug.Log($"保存存档信息：{_gameData}");
                return await GameEntry.Save.SaveGameData(_gameData, _curSaveIndex);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }
    }
}