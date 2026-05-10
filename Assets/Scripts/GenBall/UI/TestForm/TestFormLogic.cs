using UnityEngine;
using Yueyn.UI;

namespace GenBall.UI.TestForm
{
    /// <summary>
    /// 测试Form的Logic层
    /// 用于验证新UI框架的功能
    /// </summary>
    public class TestFormLogic : UIFormLogic
    {
        protected override string PrefabPath => "Assets/AssetBundles/UI/MainHud/Form/TestForm.prefab";

        private string _testData;

        internal override void BindView(UIFormScript form)
        {
            base.BindView(form);
            Debug.Log("[TestFormLogic] BindView called");

            // 将Logic引用传递给View
            if (View is TestFormView testView)
            {
                testView.SetLogic(this);
            }
        }

        public override void SetViewData(object param)
        {
            _testData = param?.ToString() ?? "No Data";
            Debug.Log($"[TestFormLogic] SetViewData: {_testData}");

            // 更新View显示
            if (View is TestFormView testView)
            {
                testView.SetTitle($"Test Form - {_testData}");
            }
        }

        public override void OnInit(object param)
        {
            Debug.Log("[TestFormLogic] OnInit");
        }

        public override void OnEnter()
        {
            Debug.Log("[TestFormLogic] OnEnter");
        }

        public override void OnExit()
        {
            Debug.Log("[TestFormLogic] OnExit");
        }

        public override void OnFocus()
        {
            Debug.Log("[TestFormLogic] OnFocus");
        }

        public override void OnUnfocus()
        {
            Debug.Log("[TestFormLogic] OnUnfocus");
        }

        public override void OnPause()
        {
            Debug.Log("[TestFormLogic] OnPause");
        }

        public override void OnResume()
        {
            Debug.Log("[TestFormLogic] OnResume");
        }

        /// <summary>
        /// 关闭按钮点击事件
        /// </summary>
        public void OnCloseButtonClicked()
        {
            Debug.Log("[TestFormLogic] Close button clicked");
            CloseForm();
        }
    }
}
