using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yueyn.UI;

namespace GenBall.UI.TestForm
{
    /// <summary>
    /// 测试Form的View层
    /// </summary>
    public class TestFormView : UIFormView
    {
        [Header("UI References")]
        [SerializeField] private Text titleText;
        [SerializeField] private Button closeButton;

        private TestFormLogic _logic;

        /// <summary>
        /// 设置Logic引用（由Logic在BindView时调用）
        /// </summary>
        public void SetLogic(TestFormLogic logic)
        {
            _logic = logic;
        }

        protected override void OnInit()
        {
            base.OnInit();
            Debug.Log("[TestFormView] OnInit");

            // 绑定按钮事件
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            Debug.Log("[TestFormView] OnOpen");
        }

        protected override void OnClose()
        {
            base.OnClose();
            Debug.Log("[TestFormView] OnClose");

            // 清理按钮事件
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            }
        }

        protected override void OnFocus()
        {
            base.OnFocus();
            Debug.Log("[TestFormView] OnFocus");
        }

        protected override void OnUnfocus()
        {
            base.OnUnfocus();
            Debug.Log("[TestFormView] OnUnfocus");
        }

        protected override void OnPause()
        {
            base.OnPause();
            Debug.Log("[TestFormView] OnPause");
        }

        protected override void OnResume()
        {
            base.OnResume();
            Debug.Log("[TestFormView] OnResume");
        }

        /// <summary>
        /// 设置标题文本
        /// </summary>
        public void SetTitle(string title)
        {
            if (titleText != null)
            {
                titleText.text = title;
            }
        }

        private void OnCloseButtonClicked()
        {
            Debug.Log("[TestFormView] OnCloseButtonClicked");
            _logic?.OnCloseButtonClicked();
        }
    }
}
