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

        protected override void OnFormCreated()
        {
            base.OnFormCreated();
        }

        protected override void OnFormBound(UIFormScript form)
        {
            base.OnFormBound(form);
            View = form.GetComponentInChildren<MainHudFormView>();

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

        public static MainHudFormLogic Open()
        {
            return BusinessLogicManager.Instance.CreateLogic<MainHudFormLogic>();
        }

        private readonly MainHudFormViewData _viewData = new MainHudFormViewData();

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
