using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenBall.Map;
using GenBall.Procedure.Execute;
using GenBall.Utils.EntityCreator;
using GenBall.Utils.Singleton;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GenBall.Procedure.Game
{
    public class GameManager : ISingleton
    {
        public static GameManager Instance => SingletonManager.GetSingleton<GameManager>();
        private int _curSaveIndex;
        private readonly List<SaveSlotData> _cachedSaveSlotData = new();
        private GameData _gameData;
        public GameData GameData => _gameData;
        public ExecuteComponent.PlayMode Mode { get; set; }

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
        public void StartNewGame()
        {
            InternalStartNewGame();
        }

        /// <summary>
        /// 继续上次游玩
        /// </summary>
        /// <returns></returns>
        public void ContinueLastGame()
        {
            InternalContinueLastGame();
        }

        /// <summary>
        /// 开始指定id的存档
        /// </summary>
        /// <param name="saveIndex"></param>
        /// <returns></returns>
        public void LoadGame(int saveIndex)
        {
            InternalStartGame(saveIndex);
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
        private async void InternalStartNewGame()
        {
            try
            {
                var saveIndex = await GameEntry.Save.CreateNewSave();
                InternalStartGame(saveIndex);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        private async void InternalContinueLastGame()
        {
            try
            {
                // _saveSlotInfo=await GameEntry.Save.GetSaveSlotInfo();
                var saveSlotDatas = await GameEntry.Save.GetSaveSlotDatas();
                _cachedSaveSlotData.Clear();
                _cachedSaveSlotData.AddRange(saveSlotDatas);
                var saveIndex= _cachedSaveSlotData.OrderByDescending(slot=>slot.LastUpdateTime).First().saveIndex;
                InternalStartGame(saveIndex);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        private async void InternalStartGame(int saveIndex)
        {
            try
            {   
                var gameData = await GameEntry.Save.LoadGameData(saveIndex);
                _curSaveIndex = saveIndex;
                InternalStartGame(gameData);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
        private void InternalStartGame(GameData gameData)
        {
            _gameData = gameData;
            GameEntry.Execute.StartGame(gameData);
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