using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Yueyn.UI
{
    /// <summary>
    /// 扩展 ScrollRect，区分拖拽和点击。
    /// 拖拽距离超过阈值后标记 IsDragging=true。
    /// ScrollRect 内的 Button 可读取此属性忽略误触。
    /// </summary>
    public class DragAwareScrollRect : ScrollRect
    {
        [SerializeField] private float _dragThreshold = 5f;

        /// <summary>当前是否正在拖拽（超过阈值）</summary>
        public bool IsDragging { get; private set; }

        private Vector2 _pointerDownPos;
        private bool _isPointerDown;

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            IsDragging = false;
            _pointerDownPos = eventData.position;
            _isPointerDown = true;
        }

        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);
            if (!_isPointerDown) return;
            if (Vector2.Distance(_pointerDownPos, eventData.position) >= _dragThreshold)
                IsDragging = true;
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            IsDragging = false;
            _isPointerDown = false;
        }
    }
}
