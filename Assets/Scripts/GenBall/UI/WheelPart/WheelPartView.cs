using System.Collections.Generic;
using GenBall.Utils.CodeGenerator.UI;
using UnityEngine;
using UnityEngine.UI;
using Yueyn.UI;

namespace GenBall.UI
{
    public class WheelPartView : PartViewBase<WheelPartViewData>
    {
        // ### GENERATED_BINDINGS_START ###
        private UiViewBinding _binding;

        public Image ImgBg { get; private set; }
        public RectTransform RectSlotContainer { get; private set; }

        private void BindControls()
        {
            _binding = GetComponent<UiViewBinding>();
            ImgBg             = _binding.GetBinding<Image>("ImgBg");
            RectSlotContainer = _binding.GetBinding<RectTransform>("RectSlotContainer");
        }

        // ### GENERATED_BINDINGS_END ###

        public RectTransform WheelRectTransform { get; private set; }

        private RectTransform _dividerContainer;
        private readonly List<GameObject> _dividerObjects = new();

        protected override void DoBusinessStart()
        {
            base.DoBusinessStart();
            BindControls();
            WheelRectTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        /// 动态绘制轮盘槽位分隔线
        /// </summary>
        public void DrawSlotDividers(int slotCount, float radius)
        {
            ClearDividers();

            if (_dividerContainer == null)
            {
                var go = new GameObject("DividerContainer", typeof(RectTransform));
                go.transform.SetParent(transform, false);
                _dividerContainer = go.GetComponent<RectTransform>();
                _dividerContainer.anchorMin = new Vector2(0.5f, 0.5f);
                _dividerContainer.anchorMax = new Vector2(0.5f, 0.5f);
                _dividerContainer.sizeDelta = Vector2.zero;
                _dividerContainer.SetAsFirstSibling(); // behind slots
            }

            for (int i = 0; i < slotCount; i++)
            {
                float angle = i * 360f / slotCount;
                var divider = new GameObject("Divider", typeof(RectTransform));
                divider.transform.SetParent(_dividerContainer, false);

                var rt = divider.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.sizeDelta = new Vector2(2f, radius);
                rt.localRotation = Quaternion.Euler(0, 0, angle);

                var img = divider.AddComponent<Image>();
                img.color = new Color(1f, 1f, 1f, 0.25f);
                img.raycastTarget = false;

                _dividerObjects.Add(divider);
            }
        }

        private void ClearDividers()
        {
            foreach (var d in _dividerObjects)
            {
                if (d != null)
                    Destroy(d);
            }
            _dividerObjects.Clear();
        }

        protected override void RefreshView()
        {
        }
    }
}
