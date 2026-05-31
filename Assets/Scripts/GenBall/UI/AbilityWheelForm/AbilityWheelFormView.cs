using System;
using GenBall.Utils.CodeGenerator.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using Yueyn.UI;

namespace GenBall.UI
{
    public class AbilityWheelFormView : UIBusinessFormBase<AbilityWheelFormViewData>
    {
        // ### GENERATED_BINDINGS_START ###
        private UiViewBinding _binding;

        public RectTransform RectPanel { get; private set; }

        private void BindControls()
        {
            _binding = GetComponent<UiViewBinding>();
            RectPanel = _binding.GetBinding<RectTransform>("RectPanel");
        }

        // ### GENERATED_BINDINGS_END ###

        public event Action<Vector2> CursorOffsetChanged;

        private WheelPartView _wheelPartView;
        private RectTransform _wheelRectTransform;
        private Vector2 _virtualCursor;

        protected override void DoBusinessStart()
        {
            base.DoBusinessStart();
            BindControls();
        }

        /// <summary>
        /// 重置虚拟光标到轮盘中心
        /// </summary>
        public void ResetCursor()
        {
            _virtualCursor = Vector2.zero;
        }

        private void Update()
        {
            if (_wheelPartView == null)
            {
                _wheelPartView = GetComponentInChildren<WheelPartView>();
                if (_wheelPartView != null)
                    _wheelRectTransform = _wheelPartView.GetComponent<RectTransform>();
            }
            if (_wheelRectTransform == null) return;

            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            _virtualCursor += mouseDelta;
            _virtualCursor = Vector2.ClampMagnitude(_virtualCursor, 300f);

            CursorOffsetChanged?.Invoke(_virtualCursor);
        }

        protected override void RefreshView()
        {
            if (ViewData == null) return;
            gameObject.SetActive(ViewData.IsVisible);
        }
    }
}
