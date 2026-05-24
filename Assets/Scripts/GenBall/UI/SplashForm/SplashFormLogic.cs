using Yueyn.UI;

namespace GenBall.UI
{
    public class SplashFormLogic : BusinessFormLogic
    {
        // ### GENERATED_BINDINGS_START ###
        public override string PrefabPath =>
            "Assets/AssetBundles/UI/SplashForm/SplashForm.prefab";

        public override UIFormType FormType => UIFormType.Popup;

        public SplashFormView View { get; private set; }

        protected override void OnFormCreated()
        {
            base.OnFormCreated();
        }

        protected override void OnFormBound(UIFormScript form)
        {
            base.OnFormBound(form);
            View = form.GetComponentInChildren<SplashFormView>();

            // 订阅 UI 事件
            Yueyn.UI.UIManager.Instance.UIEventRouter.Subscribe<float>(
                UIEventKey.SplashForm_ProgressUpdate, OnProgressUpdate);
            Yueyn.UI.UIManager.Instance.UIEventRouter.Subscribe(
                UIEventKey.SplashForm_CloseRequest, OnCloseRequest);

            // 通知 UI 事件总线：SplashForm 已打开
            Yueyn.UI.UIManager.Instance.UIEventRouter.FireNow(UIEventKey.SplashForm_Opened);
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
                UIEventKey.SplashForm_ProgressUpdate, OnProgressUpdate);
            Yueyn.UI.UIManager.Instance.UIEventRouter.Unsubscribe(
                UIEventKey.SplashForm_CloseRequest, OnCloseRequest);

            // 通知 UI 事件总线：SplashForm 即将关闭
            Yueyn.UI.UIManager.Instance.UIEventRouter.FireNow(UIEventKey.SplashForm_Closed);

            base.OnFormDestroying();
        }

        public static SplashFormLogic Open()
        {
            return BusinessLogicManager.Instance.CreateLogic<SplashFormLogic>();
        }

        // ### GENERATED_BINDINGS_END ###

        /// <summary>
        /// 设置加载进度 (0-1)
        /// </summary>
        public void SetProgress(float progress)
        {
            if (View != null)
            {
                View.SetViewData(new SplashFormViewData { Progress = progress });
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
