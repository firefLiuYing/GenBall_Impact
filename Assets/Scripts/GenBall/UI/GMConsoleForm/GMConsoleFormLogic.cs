using Yueyn.UI;

namespace GenBall.UI
{
    public class GMConsoleFormLogic : BusinessFormLogic
    {
        // ### GENERATED_BINDINGS_START ###
        public override string PrefabPath =>
            "Assets/AssetBundles/UI/GMConsoleForm/GMConsoleForm.prefab";

        public override UIFormType FormType => UIFormType.Popup;

        public GMConsoleFormView View { get; private set; }

        protected override void OnFormCreated()
        {
            base.OnFormCreated();
        }

        protected override void OnFormBound(UIFormScript form)
        {
            base.OnFormBound(form);
            View = form.GetComponentInChildren<GMConsoleFormView>();
        }

        protected override void OnFormUnbound(UIFormScript form)
        {
            View = null;
            base.OnFormUnbound(form);
        }

        public static GMConsoleFormLogic Open()
        {
            return BusinessLogicManager.Instance.CreateLogic<GMConsoleFormLogic>();
        }

        // ### GENERATED_BINDINGS_END ###

        // 在此处添加业务逻辑...
    }
}

