using System.Threading.Tasks;
using GenBall.Procedure;
using GenBall.Procedure.Execute;
using GenBall.Procedure.Game;
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

        private readonly StartFormViewData _viewData = new();

        protected override void OnFormCreated()
        {
            base.OnFormCreated();
            View = BoundForm.GetComponentInChildren<StartFormView>();
        }

        protected override void OnFormBound(UIFormScript form)
        {
            base.OnFormBound(form);

            // 订阅 UI 事件（View 按钮点击 → UIEventBus → Logic）
            Yueyn.UI.UIManager.Instance.UIEventRouter.Subscribe((int)UIEventKey.StartForm_NewGame, OnNewGame);
            Yueyn.UI.UIManager.Instance.UIEventRouter.Subscribe((int)UIEventKey.StartForm_Continue, OnContinue);
            Yueyn.UI.UIManager.Instance.UIEventRouter.Subscribe((int)UIEventKey.StartForm_LoadGame, OnLoadGame);
            Yueyn.UI.UIManager.Instance.UIEventRouter.Subscribe((int)UIEventKey.StartForm_CloseRequest, OnCloseRequest);

            _ = RefreshCanContinue();
        }

        protected override void OnFormUnbound(UIFormScript form)
        {
            // 取消 UI 事件订阅
            Yueyn.UI.UIManager.Instance.UIEventRouter.Unsubscribe((int)UIEventKey.StartForm_NewGame, OnNewGame);
            Yueyn.UI.UIManager.Instance.UIEventRouter.Unsubscribe((int)UIEventKey.StartForm_Continue, OnContinue);
            Yueyn.UI.UIManager.Instance.UIEventRouter.Unsubscribe((int)UIEventKey.StartForm_LoadGame, OnLoadGame);
            Yueyn.UI.UIManager.Instance.UIEventRouter.Unsubscribe((int)UIEventKey.StartForm_CloseRequest, OnCloseRequest);

            View = null;
            base.OnFormUnbound(form);
        }

        protected override void OnFormDestroying()
        {
            // 兜底取消订阅
            Yueyn.UI.UIManager.Instance.UIEventRouter.Unsubscribe((int)UIEventKey.StartForm_NewGame, OnNewGame);
            Yueyn.UI.UIManager.Instance.UIEventRouter.Unsubscribe((int)UIEventKey.StartForm_Continue, OnContinue);
            Yueyn.UI.UIManager.Instance.UIEventRouter.Unsubscribe((int)UIEventKey.StartForm_LoadGame, OnLoadGame);
            Yueyn.UI.UIManager.Instance.UIEventRouter.Unsubscribe((int)UIEventKey.StartForm_CloseRequest, OnCloseRequest);
            base.OnFormDestroying();
        }

        public static StartFormLogic Open()
        {
            return BusinessLogicManager.Instance.CreateLogic<StartFormLogic>();
        }

        private async void OnNewGame()
        {
            var gameStartSystem = SystemRepository.Instance.GetSystem<IGameStartSystem>();
            var context = await gameStartSystem.PrepareStartAsync(
                new GameStartRequest { Type = GameStartType.NewGame });

            var launchSystem = SystemRepository.Instance.GetSystem<ILaunchSystem>();
            launchSystem.StartGameWithContext(context);
        }

        private async void OnContinue()
        {
            var gameStartSystem = SystemRepository.Instance.GetSystem<IGameStartSystem>();
            var context = await gameStartSystem.PrepareStartAsync(
                new GameStartRequest { Type = GameStartType.Continue });

            var launchSystem = SystemRepository.Instance.GetSystem<ILaunchSystem>();
            launchSystem.StartGameWithContext(context);
        }

        private void OnLoadGame()
        {
            CloseForm();
            SaveSlotFormLogic.Open(saveIndex =>
            {
                _ = StartLoadGame(saveIndex);
            });
        }

        private async Task StartLoadGame(int saveIndex)
        {
            var gameStartSystem = SystemRepository.Instance.GetSystem<IGameStartSystem>();
            var context = await gameStartSystem.PrepareStartAsync(
                new GameStartRequest { Type = GameStartType.LoadGame, SaveIndex = saveIndex });

            var launchSystem = SystemRepository.Instance.GetSystem<ILaunchSystem>();
            launchSystem.StartGameWithContext(context);
        }

        private async Task RefreshCanContinue()
        {
            var saveService = SystemRepository.Instance.GetSystem<ISaveService>();
            if (saveService == null) return;

            var slots = await saveService.GetSaveSlotDatas();
            bool canContinue = false;
            foreach (var slot in slots)
            {
                if (!slot.isEmpty) { canContinue = true; break; }
            }

            _viewData.CanContinue = canContinue;
            View?.SetViewData(_viewData);
        }

        private void OnCloseRequest()
        {
            CloseForm();
        }
    }
}
