using System;
using UnityEngine;

namespace Yueyn.UI
{
    /// <summary>
    /// 挂到 Part prefab 上（与 PartViewBase 同级），实现 ICellView。
    /// 桥接 CellViewList 的 ICellView 生命周期 → PartLogic 生命周期。
    /// 不负责 PartLogic 的销毁——销毁由 PartLogicCellViewListLogic.RemovePartLogic 处理。
    /// </summary>
    [RequireComponent(typeof(PartViewBase))]
    public class PartLogicCellAdapter : MonoBehaviour, ICellView
    {
        /// <summary>适配器创建的 PartLogic 实例（OnCreate 后可用）</summary>
        public BusinessPartLogic CreatedLogic { get; private set; }

        void ICellView.OnCreate()
        {
            var view = GetComponent<PartViewBase>();
            if (view == null)
            {
                Debug.LogWarning($"[PartLogicCellAdapter] No PartViewBase on {gameObject.name}");
                return;
            }

            var logicType = PartLogicTypeRegistry.Resolve(view.GetType());
            if (logicType == null)
            {
                Debug.LogWarning($"[PartLogicCellAdapter] No PartLogic registered for {view.GetType().Name}");
                return;
            }

            CreatedLogic = (BusinessPartLogic)Activator.CreateInstance(logicType);
            CreatedLogic.ParentTransform = transform;
            CreatedLogic.OnCreate();
        }

        void ICellView.OnRefresh(int index, object data)
        {
            if (CreatedLogic is ICellViewDataReceiver receiver)
                receiver.ReceiveData(index, data);
        }

        void ICellView.OnRemove()
        {
            // 不调用 CreatedLogic.OnDestroy()。PartLogicCellViewListLogic 负责。
            CreatedLogic = null;
        }
    }
}
