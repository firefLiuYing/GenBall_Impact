using System;
using System.Collections.Generic;
using GenBall.Utils.CodeGenerator.UI;
using UnityEngine;
using UnityEngine.UI;
using Yueyn.UI;

namespace GenBall.UI
{
    public class SaveSlotFormView : UIBusinessFormBase<SaveSlotFormViewData>
    {
        // ### GENERATED_BINDINGS_START ###
        private UiViewBinding _binding;

        public Text TxtTitle { get; private set; }
        public RectTransform RectSlotContainer { get; private set; }
        public Button BtnBack { get; private set; }

        private void BindControls()
        {
            _binding = GetComponent<UiViewBinding>();
            TxtTitle          = _binding.GetBinding<Text>("TxtTitle");
            RectSlotContainer = _binding.GetBinding<RectTransform>("RectSlotContainer");
            BtnBack           = _binding.GetBinding<Button>("BtnBack");
        }

        // ### GENERATED_BINDINGS_END ###

        /// <summary>Fired when a non-empty slot is clicked. Parameter is save index.</summary>
        public event Action<int> SlotClicked;

        private readonly List<GameObject> _slotEntries = new();

        protected override void DoBusinessStart()
        {
            base.DoBusinessStart();
            BindControls();
            BindButtonEvents();
        }

        private void BindButtonEvents()
        {
            if (BtnBack != null)
                BtnBack.onClick.AddListener(() =>
                    Yueyn.UI.UIManager.Instance.UIEventRouter.FireNow((int)UIEventKey.SaveSlotForm_Back));
        }

        protected override void RefreshView()
        {
            if (ViewData == null || RectSlotContainer == null) return;

            ClearSlotEntries();

            if (ViewData.Slots == null || ViewData.Slots.Count == 0) return;

            for (int i = 0; i < ViewData.Slots.Count; i++)
            {
                var entry = CreateSlotEntry(ViewData.Slots[i], i);
                _slotEntries.Add(entry);
            }
        }

        private void ClearSlotEntries()
        {
            foreach (var entry in _slotEntries)
            {
                if (entry != null) Destroy(entry);
            }
            _slotEntries.Clear();
        }

        private GameObject CreateSlotEntry(SaveSlotItemInfo info, int index)
        {
            var entryGo = new GameObject($"Slot_{info.SaveIndex}", typeof(RectTransform));
            entryGo.transform.SetParent(RectSlotContainer, false);
            var entryRt = entryGo.GetComponent<RectTransform>();
            entryRt.sizeDelta = new Vector2(0, 80);
            entryRt.anchorMin = new Vector2(0, 1);
            entryRt.anchorMax = new Vector2(1, 1);
            entryRt.pivot = new Vector2(0.5f, 1);
            entryRt.anchoredPosition = new Vector2(0, -index * 90);

            // Background
            var bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(entryRt, false);
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            bgGo.GetComponent<Image>().color = info.IsEmpty
                ? new Color(0.15f, 0.15f, 0.15f, 0.5f)
                : new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Index Text
            var indexGo = new GameObject("TxtIndex", typeof(RectTransform), typeof(Text));
            indexGo.transform.SetParent(entryRt, false);
            var indexRt = indexGo.GetComponent<RectTransform>();
            indexRt.anchorMin = new Vector2(0, 0);
            indexRt.anchorMax = new Vector2(0.25f, 1);
            indexRt.sizeDelta = Vector2.zero;
            indexRt.anchoredPosition = new Vector2(10, 0);
            var indexTxt = indexGo.GetComponent<Text>();
            indexTxt.text = info.IsEmpty ? $"存档 {info.SaveIndex + 1}（空槽位）" : $"存档 {info.SaveIndex + 1}";
            indexTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            indexTxt.fontSize = info.IsEmpty ? 20 : 24;
            indexTxt.color = info.IsEmpty ? Color.gray : Color.white;
            indexTxt.alignment = TextAnchor.MiddleLeft;
            indexTxt.raycastTarget = false;

            // Info Text (scene + play time)
            var infoGo = new GameObject("TxtInfo", typeof(RectTransform), typeof(Text));
            infoGo.transform.SetParent(entryRt, false);
            var infoRt = infoGo.GetComponent<RectTransform>();
            infoRt.anchorMin = new Vector2(0.25f, 0.45f);
            infoRt.anchorMax = new Vector2(0.75f, 1);
            infoRt.sizeDelta = Vector2.zero;
            var infoTxt = infoGo.GetComponent<Text>();
            if (info.IsEmpty)
            {
                infoTxt.text = "空槽位";
                infoTxt.color = Color.gray;
            }
            else
            {
                var scene = string.IsNullOrEmpty(info.SceneName) ? "未知场景" : info.SceneName;
                infoTxt.text = $"{scene}  |  游戏时间: {info.PlayTimeText}";
                infoTxt.color = new Color(0.8f, 0.8f, 0.8f);
            }
            infoTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            infoTxt.fontSize = 16;
            infoTxt.alignment = TextAnchor.MiddleLeft;
            infoTxt.raycastTarget = false;

            // CreateTime Text
            var timeGo = new GameObject("TxtCreateTime", typeof(RectTransform), typeof(Text));
            timeGo.transform.SetParent(entryRt, false);
            var timeRt = timeGo.GetComponent<RectTransform>();
            timeRt.anchorMin = new Vector2(0.25f, 0);
            timeRt.anchorMax = new Vector2(0.75f, 0.45f);
            timeRt.sizeDelta = Vector2.zero;
            var timeTxt = timeGo.GetComponent<Text>();
            timeTxt.text = info.IsEmpty ? "" : $"创建时间: {info.CreateTimeText}";
            timeTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            timeTxt.fontSize = 14;
            timeTxt.color = new Color(0.6f, 0.6f, 0.6f);
            timeTxt.alignment = TextAnchor.MiddleLeft;
            timeTxt.raycastTarget = false;

            // Invisible button covering the whole entry
            var btnGo = new GameObject("BtnSlot", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(entryRt, false);
            var btnRt = btnGo.GetComponent<RectTransform>();
            btnRt.anchorMin = Vector2.zero;
            btnRt.anchorMax = Vector2.one;
            btnRt.sizeDelta = Vector2.zero;
            btnGo.GetComponent<Image>().color = new Color(1, 1, 1, 0);
            var btn = btnGo.GetComponent<Button>();
            if (!info.IsEmpty)
            {
                var saveIndex = info.SaveIndex;
                btn.onClick.AddListener(() => SlotClicked?.Invoke(saveIndex));
            }
            else
            {
                btn.interactable = false;
            }

            return entryGo;
        }

        protected override void DoBusinessClose()
        {
            ClearSlotEntries();
            base.DoBusinessClose();
        }
    }
}

