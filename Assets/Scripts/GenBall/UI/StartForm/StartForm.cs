using System.Collections.Generic;
using GenBall.Procedure;

namespace GenBall.UI
{
    public partial class StartForm : FormBase
    {
        private StartVm _vm;
        protected override void OnInit(object args = null)
        {
            base.OnInit(args);
            Bind();
        }

        protected override void OnOpen(object args = null)
        {
            base.OnOpen(args);
            _vm = GetVm<StartVm>();
            RegisterEvents();
            ResetForm();
            
            _vm.Init();
        }

        protected override void OnClose(object args = null)
        {
            UnRegisterEvents();
            base.OnClose(args);
        }

        private void ResetForm()
        {
            _autoRectWelcome.SA(true);
            _autoRectMenu.SA(false);
            _autoRectLoad.SA(false);
        }

        private void OnActivePageChanged(StartVm.Page page)
        {
            _autoRectLoad.SA(page==StartVm.Page.Load);
            _autoRectMenu.SA(page==StartVm.Page.Menu);
            _autoRectWelcome.SA(page==StartVm.Page.Welcome);
        }

        private void OnCanContinueLastGameChanged(bool canContinue)
        {
            _autoBtnContinue.SA(canContinue);
        }

        private void OnSaveSlotChanged(List<SaveSlotData> slots)
        {
            // todo gzp ÏÔÊ¾´æµµ
        }

        private void OnBackgroundClicked() => _vm.ChangePage(StartVm.Page.Menu);
        private void RegisterEvents()
        {
            _autoBtnBackground.onClick.AddListener(OnBackgroundClicked);
            _vm.CanContinueLastGame.Observe(OnCanContinueLastGameChanged);
            _vm.ActivePage.Observe(OnActivePageChanged);
            _vm.SaveSlots.Observe(OnSaveSlotChanged);
            
            _autoBtnNewGame.onClick.AddListener(_vm.StartNewGame);
            _autoBtnContinue.onClick.AddListener(_vm.ContinueLastGame);
        }

        private void UnRegisterEvents()
        {
            _autoBtnBackground.onClick.RemoveListener(OnBackgroundClicked);
            _vm.CanContinueLastGame.Unobserve(OnCanContinueLastGameChanged);
            _vm.ActivePage.Unobserve(OnActivePageChanged);
            _vm.SaveSlots.Unobserve(OnSaveSlotChanged);
            
            _autoBtnNewGame.onClick.RemoveListener(_vm.StartNewGame);
            _autoBtnContinue.onClick.RemoveListener(_vm.ContinueLastGame);
        }
    }
}