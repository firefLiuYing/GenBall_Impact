using GenBall.UI;
using UnityEngine;
using Yueyn.UI;

namespace GenBall.GM
{
    public class GMConsoleFormLogic : BusinessFormLogic
    {
        // ### GENERATED_BINDINGS_START ###
        public override string PrefabPath =>
            "Assets/AssetBundles/UI/GMConsoleForm/GMConsoleForm.prefab";

        public override UIFormType FormType => UIFormType.Popup;

        public GMConsoleFormView View { get; private set; }

        // ### GENERATED_BINDINGS_END ###

        protected override void OnFormCreated()
        {
            base.OnFormCreated();
        }

        protected override void OnFormBound(UIFormScript form)
        {
            base.OnFormBound(form);
            View = form.GetComponentInChildren<GMConsoleFormView>();

            Yueyn.UI.UIManager.Instance.UIEventRouter.Subscribe(
                UIEventKey.GMConsole_Submit, OnSubmit);
            Yueyn.UI.UIManager.Instance.UIEventRouter.Subscribe(
                UIEventKey.GMConsole_Close, OnClose);
        }

        protected override void OnFormUnbound(UIFormScript form)
        {
            Yueyn.UI.UIManager.Instance.UIEventRouter.Unsubscribe(
                UIEventKey.GMConsole_Submit, OnSubmit);
            Yueyn.UI.UIManager.Instance.UIEventRouter.Unsubscribe(
                UIEventKey.GMConsole_Close, OnClose);

            View = null;
            base.OnFormUnbound(form);

            // Sync state: notify the system that console has been closed
            _gmSystem?.NotifyConsoleClosed();
        }

        private IGMCommandSystem _gmSystem;

        public void InitWithSystem(IGMCommandSystem system)
        {
            _gmSystem = system;
        }

        private void OnSubmit()
        {
            if (_gmSystem == null || View == null)
                return;

            var input = View.InputCmd.text;
            if (string.IsNullOrWhiteSpace(input))
                return;

            var output = _gmSystem.ExecuteCommand(input);
            View.SetViewData(new GMConsoleFormViewData
            {
                OutputText = output,
                InputText = ""
            });
        }

        private void OnClose()
        {
            CloseForm();
        }
    }
}
