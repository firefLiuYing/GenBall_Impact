using GenBall.Procedure.Execute;
using Yueyn.Main;
using Yueyn.UI;

namespace GenBall.UI
{
    public class StartFormLogic : BusinessFormLogic
    {
        // ### GENERATED_BINDINGS_START ###
        public override string PrefabPath =>
            "Assets/AssetBundles/UI/StartForm/StartForm.prefab";

        public override UIFormType FormType => UIFormType.Popup;

        public StartFormView View { get; private set; }

        // ### GENERATED_BINDINGS_END ###

        protected override void OnFormCreated()
        {
            base.OnFormCreated();
        }

        protected override void OnFormBound(UIFormScript form)
        {
            base.OnFormBound(form);
            View = form.GetComponentInChildren<StartFormView>();

            // 订阅 UI 事件（View 按钮点击 → UIEventBus → Logic）
            Yueyn.UI.UIManager.Instance.UIEventRouter.Subscribe(UIEventKey.StartForm_NewGame, OnNewGame);
            Yueyn.UI.UIManager.Instance.UIEventRouter.Subscribe(UIEventKey.StartForm_Continue, OnContinue);
            Yueyn.UI.UIManager.Instance.UIEventRouter.Subscribe(UIEventKey.StartForm_LoadGame, OnLoadGame);
            Yueyn.UI.UIManager.Instance.UIEventRouter.Subscribe(UIEventKey.StartForm_CloseRequest, OnCloseRequest);
        }

        protected override void OnFormUnbound(UIFormScript form)
        {
            // 取消 UI 事件订阅
            Yueyn.UI.UIManager.Instance.UIEventRouter.Unsubscribe(UIEventKey.StartForm_NewGame, OnNewGame);
            Yueyn.UI.UIManager.Instance.UIEventRouter.Unsubscribe(UIEventKey.StartForm_Continue, OnContinue);
            Yueyn.UI.UIManager.Instance.UIEventRouter.Unsubscribe(UIEventKey.StartForm_LoadGame, OnLoadGame);
            Yueyn.UI.UIManager.Instance.UIEventRouter.Unsubscribe(UIEventKey.StartForm_CloseRequest, OnCloseRequest);

            View = null;
            base.OnFormUnbound(form);
        }

        public static StartFormLogic Open()
        {
            return BusinessLogicManager.Instance.CreateLogic<StartFormLogic>();
        }

        private void OnNewGame()
        {
            var launchSystem = SystemRepository.Instance.GetSystem<ILaunchSystem>();
            launchSystem.StartNewGame();
        }

        private void OnContinue()
        {
            var launchSystem = SystemRepository.Instance.GetSystem<ILaunchSystem>();
            launchSystem.ContinueLastGame();
        }

        private void OnLoadGame()
        {
            var launchSystem = SystemRepository.Instance.GetSystem<ILaunchSystem>();
            // TODO: 存档选择 UI 完成后传入实际 index
            launchSystem.LoadGame(0);
        }

        private void OnCloseRequest()
        {
            CloseForm();
        }
    }
}
