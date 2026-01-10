using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenBall.Utils.Singleton;
using UnityEngine;

namespace GenBall.Procedure.Game
{
    public class GameManager : ISingleton
    {
        public static GameManager Instance => SingletonManager.GetSingleton<GameManager>();
        private int _curSaveIndex;
        private readonly List<SaveSlotData> _cachedSaveSlotData = new();

        public GameData GameData { get; set; }

        public RunningMode Mode { get; set; }

        public int CurSaveIndex
        {
            get => _curSaveIndex;
            set=> _curSaveIndex = value;
        }

        /// <summary>
        /// 保存游戏
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SaveGame()
        {
            try
            {
                if ((Mode & RunningMode.SaveData) == 0) return true;
                return await InternalSaveGame();
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
                if(_curSaveIndex <0||GameData==null) return false;
                await Task.Delay(1);
                // todo gzp 模拟获取存档还有的数据
                
                // 最近一次游玩改成现在
                GameData.LastUpdateTime = DateTime.Now;
                Debug.Log($"保存存档信息：{GameData}");
                return await GameEntry.Save.SaveGameData(GameData, _curSaveIndex);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }
    }
    
    [Flags]
    public enum RunningMode
    {
        SaveData=1<<0,
        LoadData=1<<1,
    }
}