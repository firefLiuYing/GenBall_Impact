using System.Collections.Generic;
using UnityEngine;
using Yueyn.UI;

namespace GenBall.UI
{
    public class WheelPartLogic : BusinessPartLogic<WheelPartView>
    {
        public override string PrefabPath =>
            "Assets/AssetBundles/UI/WheelPart/WheelPart.prefab";

        private readonly WheelPartViewData _viewData = new();
        private readonly List<WheelSlotPartLogic> _slotLogics = new();

        private IReadOnlyList<object> _items;
        private int _selectedIndex = -1;

        public int SelectedIndex => _selectedIndex;

        public void SetItems(IReadOnlyList<object> items, float radius = 120f)
        {
            _items = items;
            ClearSlots();

            if (items == null || items.Count == 0) return;
            if (BoundView == null)
            {
                Debug.LogError("[WheelPartLogic] SetItems: BoundView is null");
                return;
            }

            _viewData.SlotCount = items.Count;
            _viewData.SlotRadius = radius;
            BoundView.SetViewData(_viewData);

            for (int i = 0; i < items.Count; i++)
            {
                var slotLogic = BusinessLogicManager.Instance.CreateLogic<WheelSlotPartLogic>(
                    p => p.ParentTransform = BoundView.RectSlotContainer);
                slotLogic.SetData(items[i]);
                AddPartLogic(slotLogic);
                _slotLogics.Add(slotLogic);

                var slotRect = slotLogic.BoundView?.GetComponent<RectTransform>();
                if (slotRect != null)
                    PositionSlot(slotRect, i, items.Count, radius);
            }

            BoundView.DrawSlotDividers(items.Count, radius);
        }

        public void UpdateCursorOffset(Vector2 offset)
        {
            if (_items == null || _items.Count == 0) return;

            int newIndex = CalculateSelectedIndex(offset, _items.Count);

            if (newIndex != _selectedIndex)
            {
                if (_selectedIndex >= 0 && _selectedIndex < _slotLogics.Count)
                    _slotLogics[_selectedIndex].SetHighlight(false);

                _selectedIndex = newIndex;

                if (_selectedIndex >= 0 && _selectedIndex < _slotLogics.Count)
                    _slotLogics[_selectedIndex].SetHighlight(true);
            }
        }

        private void ClearSlots()
        {
            ClearPartLogics();
            _slotLogics.Clear();
            _selectedIndex = -1;
        }

        private void PositionSlot(RectTransform slotRect, int index, int total, float radius)
        {
            float angleDeg = index * 360f / total - 90f;
            float angleRad = angleDeg * Mathf.Deg2Rad;
            slotRect.anchoredPosition = new Vector2(
                Mathf.Cos(angleRad) * radius,
                Mathf.Sin(angleRad) * radius);
        }

        private int CalculateSelectedIndex(Vector2 offset, int total)
        {
            const float deadZone = 40f;
            if (offset.magnitude < deadZone) return -1;

            float cursorAngle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            if (cursorAngle < 0f) cursorAngle += 360f;

            float slotAngleSpan = 360f / total;
            int bestIndex = 0;
            float bestDist = float.MaxValue;

            for (int i = 0; i < total; i++)
            {
                float slotAngle = i * slotAngleSpan - 90f;
                if (slotAngle < 0f) slotAngle += 360f;

                float dist = Mathf.Abs(cursorAngle - slotAngle);
                if (dist > 180f) dist = 360f - dist;

                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }
    }
}
