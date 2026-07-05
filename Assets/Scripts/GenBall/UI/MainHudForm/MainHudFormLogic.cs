using GenBall.BattleSystem.Weapons.Components.Ammo;
using GenBall.Event;
using Yueyn.Event;
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

        // ### GENERATED_BINDINGS_END ###

        private readonly MainHudFormViewData _viewData = new();

        protected override void OnFormCreated()
        {
            base.OnFormCreated();
            View = BoundForm.GetComponentInChildren<MainHudFormView>();
        }

        protected override void OnFormBound(UIFormScript form)
        {
            base.OnFormBound(form);

            CEventRouter.Instance.Subscribe<int>((int)GlobalEventId.HealthChanged, OnHealthChanged);
            CEventRouter.Instance.Subscribe<int>((int)GlobalEventId.ArmorChanged, OnArmorChanged);
            CEventRouter.Instance.Subscribe<int>((int)GlobalEventId.MaxHealthChanged, OnMaxHealthChanged);
            CEventRouter.Instance.Subscribe<int>((int)GlobalEventId.KillPointsChanged, OnKillPointsChanged);
            CEventRouter.Instance.Subscribe<int>((int)GlobalEventId.LevelChanged, OnWeaponLevelChanged);
            CEventRouter.Instance.Subscribe<AmmoDisplayInfo>((int)GlobalEventId.MagazineInfoChange, OnMagazineInfoChanged);
        }

        protected override void OnFormUnbound(UIFormScript form)
        {
            CEventRouter.Instance.Unsubscribe<int>((int)GlobalEventId.HealthChanged, OnHealthChanged);
            CEventRouter.Instance.Unsubscribe<int>((int)GlobalEventId.ArmorChanged, OnArmorChanged);
            CEventRouter.Instance.Unsubscribe<int>((int)GlobalEventId.MaxHealthChanged, OnMaxHealthChanged);
            CEventRouter.Instance.Unsubscribe<int>((int)GlobalEventId.KillPointsChanged, OnKillPointsChanged);
            CEventRouter.Instance.Unsubscribe<int>((int)GlobalEventId.LevelChanged, OnWeaponLevelChanged);
            CEventRouter.Instance.Unsubscribe<AmmoDisplayInfo>((int)GlobalEventId.MagazineInfoChange, OnMagazineInfoChanged);

            View = null;
            base.OnFormUnbound(form);
        }

        protected override void OnFormDestroying()
        {
            // 兜底取消订阅
            CEventRouter.Instance.Unsubscribe<int>((int)GlobalEventId.HealthChanged, OnHealthChanged);
            CEventRouter.Instance.Unsubscribe<int>((int)GlobalEventId.ArmorChanged, OnArmorChanged);
            CEventRouter.Instance.Unsubscribe<int>((int)GlobalEventId.MaxHealthChanged, OnMaxHealthChanged);
            CEventRouter.Instance.Unsubscribe<int>((int)GlobalEventId.KillPointsChanged, OnKillPointsChanged);
            CEventRouter.Instance.Unsubscribe<int>((int)GlobalEventId.LevelChanged, OnWeaponLevelChanged);
            CEventRouter.Instance.Unsubscribe<AmmoDisplayInfo>((int)GlobalEventId.MagazineInfoChange, OnMagazineInfoChanged);
            base.OnFormDestroying();
        }

        public static MainHudFormLogic Open()
        {
            return BusinessLogicManager.Instance.CreateLogic<MainHudFormLogic>();
        }

        private void OnHealthChanged(int health)
        {
            _viewData.Health = health;
            RefreshDisplay();
        }

        private void OnArmorChanged(int armor)
        {
            _viewData.Armor = armor;
            RefreshDisplay();
        }

        private void OnMaxHealthChanged(int maxHealth)
        {
            _viewData.MaxHealth = maxHealth;
            RefreshDisplay();
        }

        private void OnKillPointsChanged(int killPoints)
        {
            _viewData.KillPoints = killPoints;
            RefreshDisplay();
        }

        private void OnWeaponLevelChanged(int level)
        {
            _viewData.WeaponLevel = level;
            RefreshDisplay();
        }

        private void OnMagazineInfoChanged(AmmoDisplayInfo info)
        {
            _viewData.AmmoCount = info.CurrentValue;
            _viewData.AmmoCapacity = info.MaxValue;
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            View?.SetViewData(_viewData);
        }
    }
}
