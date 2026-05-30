using GenBall.Utils.CodeGenerator.UI;
using UnityEngine;
using UnityEngine.UI;
using Yueyn.UI;

namespace GenBall.UI
{
    public class StartFormView : UIBusinessFormBase<StartFormViewData>
    {
        // ### GENERATED_BINDINGS_START ###
        private UiViewBinding _binding;

        public Text TxtTitle { get; private set; }
        public Button BtnNewGame { get; private set; }
        public Button BtnContinue { get; private set; }
        public Button BtnLoadGame { get; private set; }

        private void BindControls()
        {
            _binding = GetComponent<UiViewBinding>();
            TxtTitle    = _binding.GetBinding<Text>("TxtTitle");
            BtnNewGame  = _binding.GetBinding<Button>("BtnNewGame");
            BtnContinue = _binding.GetBinding<Button>("BtnContinue");
            BtnLoadGame = _binding.GetBinding<Button>("BtnLoadGame");
        }

        // ### GENERATED_BINDINGS_END ###

        protected override void DoBusinessStart()
        {
            base.DoBusinessStart();
            BindControls();
            BindButtonEvents();
        }

        /// <summary>
        /// 按钮绑定 UI 事件（View → UIEventBus → Logic）
        /// </summary>
        private void BindButtonEvents()
        {
            if (BtnNewGame != null)
                BtnNewGame.onClick.AddListener(() =>
                    Yueyn.UI.UIManager.Instance.UIEventRouter.FireNow((int)UIEventKey.StartForm_NewGame));

            if (BtnContinue != null)
                BtnContinue.onClick.AddListener(() =>
                    Yueyn.UI.UIManager.Instance.UIEventRouter.FireNow((int)UIEventKey.StartForm_Continue));

            if (BtnLoadGame != null)
                BtnLoadGame.onClick.AddListener(() =>
                    Yueyn.UI.UIManager.Instance.UIEventRouter.FireNow((int)UIEventKey.StartForm_LoadGame));
        }

        protected override void RefreshView()
        {
            if (ViewData == null) return;
            BtnContinue.interactable = ViewData.CanContinue;
        }
    }
}
