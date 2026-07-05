using System.Collections.Generic;
using GenBall.BattleSystem.AbilityWeapon;
using GenBall.Event;
using UnityEngine;
using Yueyn.Event;
using Yueyn.Main;
using Yueyn.Resource;
using Yueyn.UI;

namespace GenBall.UI
{
    public class AbilityWheelFormLogic : BusinessFormLogic
    {
        // ### GENERATED_BINDINGS_START ###
        public override string PrefabPath =>
            "Assets/AssetBundles/UI/AbilityWheelForm/AbilityWheelForm.prefab";

        public override UIFormType FormType => UIFormType.Popup;

        public AbilityWheelFormView View { get; private set; }

        // ### GENERATED_BINDINGS_END ###

        private const string CancelIconPath = "Assets/AssetBundles/UI/AbilityWheelForm/Sprites/CancelIcon@4x.png";

        private WheelPartLogic _wheelPartLogic;
        private IAbilityWeaponSystem _weaponSystem;
        private IReadOnlyList<AbilityWeaponId> _weaponIds;
        private bool _isViewReady;

        public int SelectedIndex => _wheelPartLogic?.SelectedIndex ?? -1;

        protected override void OnFormCreated()
        {
            base.OnFormCreated();
            View = BoundForm.GetComponentInChildren<AbilityWheelFormView>();
        }

        protected override void OnFormBound(UIFormScript form)
        {
            base.OnFormBound(form);

            // 注：CursorOffsetChanged 是高频输入流（每帧），使用 C# event 避免 UIEventRouter 开销。
            // 这是 View→Logic 高频数据流的特殊情况，不同于按钮点击。
            View.CursorOffsetChanged += OnCursorOffset;

            CEventRouter.Instance.Subscribe((int)GlobalEventId.WheelConfirmed, OnWheelConfirmed);

            _weaponSystem = SystemRepository.Instance.GetSystem<IAbilityWeaponSystem>();
            _isViewReady = true;

            SetupWheel();
        }

        protected override void OnFormUnbound(UIFormScript form)
        {
            _isViewReady = false;

            View.CursorOffsetChanged -= OnCursorOffset;

            CEventRouter.Instance.Unsubscribe((int)GlobalEventId.WheelConfirmed, OnWheelConfirmed);

            if (_wheelPartLogic != null)
            {
                _wheelPartLogic.OnDestroy();
                _wheelPartLogic = null;
            }

            View = null;
            base.OnFormUnbound(form);
        }

        protected override void OnFormDestroying()
        {
            // 兜底取消订阅
            View.CursorOffsetChanged -= OnCursorOffset;
            CEventRouter.Instance.Unsubscribe((int)GlobalEventId.WheelConfirmed, OnWheelConfirmed);
            base.OnFormDestroying();
        }

        public static AbilityWheelFormLogic Open()
        {
            return BusinessLogicManager.Instance.CreateLogic<AbilityWheelFormLogic>();
        }

        /// <summary>
        /// 重置光标偏移（轮盘打开时调用）
        /// </summary>
        public void ResetCursor()
        {
            View.ResetCursor();
        }

        // ---- Event Handlers ----

        private void OnWheelConfirmed()
        {
            if (!_isViewReady) return;

            int selectedIndex = SelectedIndex;
            // selectedIndex < _weaponIds.Count = valid weapon
            // selectedIndex >= _weaponIds.Count = cancel slot
            // selectedIndex == -1 = dead zone
            if (selectedIndex >= 0 && selectedIndex < _weaponIds.Count)
            {
                _weaponSystem.ActivateWeapon(_weaponIds[selectedIndex]);
            }

            CloseForm();
        }

        private void SetupWheel()
        {
            var weaponIds = _weaponSystem.AvailableWeaponIds;
            if (weaponIds == null || weaponIds.Count == 0) return;

            _weaponIds = weaponIds;

            if (_wheelPartLogic == null)
            {
                _wheelPartLogic = BusinessLogicManager.Instance.CreateLogic<WheelPartLogic>(
                    p => p.ParentTransform = View.RectPanel);
                AddPartLogic(_wheelPartLogic);
            }

            // Build items: weapon slots + cancel slot
            var items = new List<object>(weaponIds.Count + 1);
            foreach (var id in weaponIds)
            {
                var config = _weaponSystem.GetWeaponConfig(id);
                Sprite icon = null;
                if (!string.IsNullOrEmpty(config.IconResourcePath))
                    icon = CResourceManager.Instance.LoadSync<Sprite>(config.IconResourcePath);

                items.Add(new AbilityWheelSlotData
                {
                    WeaponId = id,
                    DisplayName = config.DisplayName,
                    Icon = icon,
                });
            }

            var cancelIcon = CResourceManager.Instance.LoadSync<Sprite>(CancelIconPath);
            items.Add(new AbilityWheelSlotData
            {
                IsCancel = true,
                DisplayName = "取消",
                Icon = cancelIcon,
            });

            _wheelPartLogic.SetItems(items);

            // Reset virtual cursor to center
            View.ResetCursor();
        }

        private void OnCursorOffset(UnityEngine.Vector2 offset)
        {
            _wheelPartLogic?.UpdateCursorOffset(offset);
        }
    }
}
