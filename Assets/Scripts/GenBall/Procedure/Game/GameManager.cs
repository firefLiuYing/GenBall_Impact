using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenBall.Map;
using GenBall.Procedure;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.Procedure.Game
{
    public class GameManager : IGameManagerSystem
    {
        private int _curSaveIndex;

        public GameData GameData { get; set; }

        public RunningMode Mode { get; set; }

        public int CurSaveIndex
        {
            get => _curSaveIndex;
            set=> _curSaveIndex = value;
        }

        public void Init() { }
        public void UnInit() { }

        /// <summary>
        /// ������Ϸ
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
                // todo gzp ģ���ȡ�浵���е�����
                
                // ���һ������ĳ�����
                GameData.LastUpdateTime = DateTime.Now;
                Debug.Log($"����浵��Ϣ��{GameData}");
                var saveService = SystemRepository.Instance.GetSystem<ISaveService>();
                return await saveService.SaveGameData(GameData, _curSaveIndex);
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