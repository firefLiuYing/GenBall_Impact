using Yueyn.UI;

namespace GenBall.UI
{
    public class LoadingFormLogic : BusinessFormLogic
    {
        // ### GENERATED_BINDINGS_START ###
        public override string PrefabPath =>
            "Assets/AssetBundles/UI/LoadingForm/LoadingForm.prefab";

        public override UIFormType FormType => UIFormType.Popup;

        public LoadingFormView View { get; private set; }

        // ### GENERATED_BINDINGS_END ###

        protected override void OnFormCreated()
        {
            base.OnFormCreated();
        }

        protected override void OnFormBound(UIFormScript form)
        {
            base.OnFormBound(form);
            View = form.GetComponentInChildren<LoadingFormView>();

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

        public static LoadingFormLogic Open()
        {
            return BusinessLogicManager.Instance.CreateLogic<LoadingFormLogic>();
        }

        /// <summary>
        /// 设置加载进度 (0-1)
        /// </summary>
        public void SetProgress(float progress)
        {
            if (View != null)
            {
                View.SetViewData(new LoadingFormViewData { Progress = progress });
            }
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
