using System;
using System.Collections.Generic;
using System.Linq;
using GenBall.Procedure;
using GenBall.Procedure.Game;
using UnityEngine;
using Yueyn.Base.Variable;

namespace GenBall.UI
{
    public class StartVm : VmBase
    {
        public readonly Variable<bool> CanContinueLastGame;
        public readonly Variable<List<SaveSlotData>> SaveSlots;
        public readonly Variable<Page> ActivePage;
        
        public async void Init()
        {
            try
            {
                ActivePage.PostValue(Page.Welcome);
                var saveSlotDatas = await GameEntry.Save.GetSaveSlotDatas();
                var slots=saveSlotDatas.ToList();
                Debug.Log($"gzp 获取到已有存档数量为：{slots.Count}");
                CanContinueLastGame.PostValue(slots.Count>0);
                SaveSlots.PostValue(slots);
            }
            catch (Exception e)
            {
                Debug.LogError($"gzp 启动界面初始化失败：{e.Message}");
            }
        }

        public void ChangePage(Page page)
        {
            ActivePage.PostValue(page);
        }

        public void ContinueLastGame()
        {
            GameEntry.Execute.ContinueLastGame();
            CloseStartForm();
        }

        public void StartNewGame()
        {
            GameEntry.Execute.StartNewGame();
            CloseStartForm();
        }

        public void LoadGame(int saveIndex)
        {
            GameEntry.Execute.LoadGame(saveIndex);
            CloseStartForm();
        }

        private void CloseStartForm()
        {
            GameEntry.UI.CloseForm<StartForm>();
        }
        public enum Page
        {
            None,Welcome,Menu,Load
        }
        
        
        public StartVm()
        {
            CanContinueLastGame=Variable<bool>.Create();
            SaveSlots=Variable<List<SaveSlotData>>.Create();
            ActivePage=Variable<Page>.Create();
            AddDispose(CanContinueLastGame);
            AddDispose(SaveSlots);
            AddDispose(ActivePage);
        }
    }
}