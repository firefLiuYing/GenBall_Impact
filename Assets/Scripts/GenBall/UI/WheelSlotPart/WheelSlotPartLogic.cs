using GenBall.AbilityWeapon;
using Yueyn.Main;
using Yueyn.UI;

namespace GenBall.UI
{
    public class WheelSlotPartLogic : BusinessPartLogic<WheelSlotPartView>
    {
        public override string PrefabPath =>
            "Assets/AssetBundles/UI/WheelSlotPart/WheelSlotPart.prefab";

        private readonly WheelSlotPartViewData _viewData = new();
        private WheelSlotPartView _view;
        private AbilityWeaponId _weaponId;
        private bool _isCancel;

        public AbilityWeaponId WeaponId => _weaponId;
        public bool IsCancel => _isCancel;

        protected override void OnViewBound(PartViewBase view)
        {
            base.OnViewBound(view);
            _view = BoundView;
        }

        public void SetData(object data)
        {
            var slotData = (AbilityWheelSlotData)data;

            _viewData.DisplayName = slotData.DisplayName;
            _viewData.Icon = slotData.Icon;

            if (slotData.IsCancel)
            {
                _isCancel = true;
                _viewData.IsCancelSlot = true;
                _viewData.IsHighlighted = false;
                _viewData.ShowCooldown = false;
            }
            else
            {
                _isCancel = false;
                _viewData.IsCancelSlot = false;
                _weaponId = slotData.WeaponId!.Value;
                var system = SystemRepository.Instance.GetSystem<IAbilityWeaponSystem>();
                UpdateCooldown(system.GetCooldownRemaining(_weaponId));
            }

            _view.SetViewData(_viewData);
        }

        public void SetHighlight(bool highlighted)
        {
            _viewData.IsHighlighted = highlighted;
            _view.SetViewData(_viewData);
        }

        public void UpdateCooldown(float remaining)
        {
            if (remaining <= 0f)
            {
                _viewData.ShowCooldown = false;
                _viewData.CooldownText = string.Empty;
            }
            else
            {
                _viewData.ShowCooldown = true;
                _viewData.CooldownText = $"{remaining:F1}s";
            }
        }
    }
}
