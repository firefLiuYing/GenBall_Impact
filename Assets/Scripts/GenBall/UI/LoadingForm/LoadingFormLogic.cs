using Yueyn.UI;

namespace GenBall.UI
{
    public class LoadingFormLogic : BusinessFormLogic
    {
        // ### GENERATED_BINDINGS_START ###
        public override string PrefabPath =>
            "Assets/AssetBundles/UI/LoadingForm/LoadingForm.prefab";

        public override UIFormType FormType => UIFormType.Transition;

        public LoadingFormView View { get; private set; }

        // ### GENERATED_BINDINGS_END ###

        protected override void OnFormCreated()
        {
            base.OnFormCreated();
            View = BoundForm.GetComponentInChildren<LoadingFormView>();
        }

        protected override void OnFormBound(UIFormScript form)
        {
            base.OnFormBound(form);

            // 订阅 UI 事件
            Yueyn.UI.UIManager.Instance.UIEventRouter.Subscribe<float>(
                (int)UIEventKey.LoadingForm_ProgressUpdate, OnProgressUpdate);
            Yueyn.UI.UIManager.Instance.UIEventRouter.Subscribe(
                (int)UIEventKey.LoadingForm_CloseRequest, OnCloseRequest);

            // 通知 UI 事件总线：LoadingForm 已打开
            Yueyn.UI.UIManager.Instance.UIEventRouter.FireNow((int)UIEventKey.LoadingForm_Opened);
        }

        protected override void OnFormUnbound(UIFormScript form)
        {
            Yueyn.UI.UIManager.Instance.UIEventRouter.Unsubscribe<float>(
                (int)UIEventKey.LoadingForm_ProgressUpdate, OnProgressUpdate);
            Yueyn.UI.UIManager.Instance.UIEventRouter.Unsubscribe(
                (int)UIEventKey.LoadingForm_CloseRequest, OnCloseRequest);

            View = null;
            base.OnFormUnbound(form);
        }

        protected override void OnFormDestroying()
        {
            // 取消所有订阅
            Yueyn.UI.UIManager.Instance.UIEventRouter.Unsubscribe<float>(
                (int)UIEventKey.LoadingForm_ProgressUpdate, OnProgressUpdate);
            Yueyn.UI.UIManager.Instance.UIEventRouter.Unsubscribe(
                (int)UIEventKey.LoadingForm_CloseRequest, OnCloseRequest);

            // 通知 UI 事件总线：LoadingForm 即将关闭
            Yueyn.UI.UIManager.Instance.UIEventRouter.FireNow((int)UIEventKey.LoadingForm_Closed);

            base.OnFormDestroying();
        }

        private readonly LoadingFormViewData _viewData = new();

        public static LoadingFormLogic Open()
        {
            return BusinessLogicManager.Instance.CreateLogic<LoadingFormLogic>();
        }

        /// <summary>
        /// 设置加载进度 (0-1)
        /// </summary>
        public void SetProgress(float progress)
        {
            _viewData.Progress = progress;
            View?.SetViewData(_viewData);
        }

        private void OnProgressUpdate(float progress)
        {
            SetProgress(progress);
        }

        private void OnCloseRequest()
        {
            CloseForm();
        }
    }
}
