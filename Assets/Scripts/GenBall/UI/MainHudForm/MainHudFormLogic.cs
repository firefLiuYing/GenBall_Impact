using Yueyn.UI;

namespace GenBall.UI
{
    public class MainHudFormLogic : BusinessFormLogic
    {
        // ### GENERATED_BINDINGS_START ###
        public override string PrefabPath =>
            "Assets/AssetBundles/UI/MainHudForm/MainHudForm.prefab";

        public override UIFormType FormType => UIFormType.Persistent;

        public MainHudFormView View { get; private set; }

        protected override void OnFormCreated()
        {
            base.OnFormCreated();
        }

        protected override void OnFormBound(UIFormScript form)
        {
            base.OnFormBound(form);
            View = form.GetComponentInChildren<MainHudFormView>();
        }

        protected override void OnFormUnbound(UIFormScript form)
        {
            View = null;
            base.OnFormUnbound(form);
        }

        public static MainHudFormLogic Open()
        {
            return BusinessLogicManager.Instance.CreateLogic<MainHudFormLogic>();
        }

        // ### GENERATED_BINDINGS_END ###

        // 在此处添加业务逻辑...
    }
}
