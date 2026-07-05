using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GenBall.Player;
using GenBall.Procedure;
using UnityEngine;
using Yueyn.Main;
using Yueyn.UI;

namespace GenBall.UI
{
    public class SaveSlotFormLogic : BusinessFormLogic
    {
        // ### GENERATED_BINDINGS_START ###
        public override string PrefabPath =>
            "Assets/AssetBundles/UI/SaveSlotForm/SaveSlotForm.prefab";

        public override UIFormType FormType => UIFormType.Popup;

        public SaveSlotFormView View { get; private set; }

        // ### GENERATED_BINDINGS_END ###

        private Action<int> _slotSelectedCallback;

        /// <summary>Open the save slot selection form.</summary>
        public static SaveSlotFormLogic Open(Action<int> onSlotSelected)
        {
            var logic = BusinessLogicManager.Instance.CreateLogic<SaveSlotFormLogic>();
            logic._slotSelectedCallback = onSlotSelected;
            return logic;
        }

        protected override void OnFormCreated()
        {
            base.OnFormCreated();
            View = BoundForm.GetComponentInChildren<SaveSlotFormView>();
        }

        protected override void OnFormBound(UIFormScript form)
        {
            base.OnFormBound(form);

            Yueyn.UI.UIManager.Instance.UIEventRouter.Subscribe<int>(
                (int)UIEventKey.SaveSlotForm_SlotSelected, OnSlotSelected);
            Yueyn.UI.UIManager.Instance.UIEventRouter.Subscribe(
                (int)UIEventKey.SaveSlotForm_Back, OnBack);

            _ = LoadSlotDataAsync();
        }

        protected override void OnFormUnbound(UIFormScript form)
        {
            Yueyn.UI.UIManager.Instance.UIEventRouter.Unsubscribe<int>(
                (int)UIEventKey.SaveSlotForm_SlotSelected, OnSlotSelected);
            Yueyn.UI.UIManager.Instance.UIEventRouter.Unsubscribe(
                (int)UIEventKey.SaveSlotForm_Back, OnBack);

            View = null;
            base.OnFormUnbound(form);
        }

        protected override void OnFormDestroying()
        {
            // 兜底取消订阅
            Yueyn.UI.UIManager.Instance.UIEventRouter.Unsubscribe<int>(
                (int)UIEventKey.SaveSlotForm_SlotSelected, OnSlotSelected);
            Yueyn.UI.UIManager.Instance.UIEventRouter.Unsubscribe(
                (int)UIEventKey.SaveSlotForm_Back, OnBack);
            base.OnFormDestroying();
        }

        private async Task LoadSlotDataAsync()
        {
            var saveService = SystemRepository.Instance.GetSystem<ISaveService>();
            if (saveService == null) return;

            var slots = await saveService.GetSaveSlotDatas();
            if (View == null) return;

            var slotInfos = new List<SaveSlotItemInfo>();
            foreach (var slot in slots)
            {
                var info = new SaveSlotItemInfo
                {
                    SaveIndex = slot.saveIndex,
                    IsEmpty = slot.isEmpty,
                    CreateTimeText = slot.CreateTime.ToString("yyyy/MM/dd HH:mm"),
                    PlayTimeText = FormatTimeSpan(new TimeSpan(slot.TotalTime.Ticks)),
                };

                if (!slot.isEmpty)
                    info.SceneName = await LoadSceneNameAsync(saveService, slot.saveIndex);

                slotInfos.Add(info);
            }

            View.SetViewData(new SaveSlotFormViewData { Slots = slotInfos });
        }

        private static async Task<string> LoadSceneNameAsync(ISaveService saveService, int saveIndex)
        {
            try
            {
                var gameData = await saveService.LoadGameData(saveIndex);
                if (gameData == null) return "";

                var playerJson = gameData.GetData("Player");
                if (string.IsNullOrEmpty(playerJson)) return "";

                var playerData = JsonUtility.FromJson<PlayerSaveData>(playerJson);
                return playerData?.lastSceneName ?? "";
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSlotFormLogic] Failed to load scene name for slot {saveIndex}: {e.Message}");
                return "";
            }
        }

        private static string FormatTimeSpan(TimeSpan time)
        {
            if (time.TotalHours >= 1)
                return $"{(int)time.TotalHours}:{time.Minutes:D2}:{time.Seconds:D2}";
            return $"{time.Minutes:D2}:{time.Seconds:D2}";
        }

        private void OnSlotSelected(int saveIndex)
        {
            _slotSelectedCallback?.Invoke(saveIndex);
            CloseForm();
        }

        private void OnBack()
        {
            CloseForm();
        }
    }
}

