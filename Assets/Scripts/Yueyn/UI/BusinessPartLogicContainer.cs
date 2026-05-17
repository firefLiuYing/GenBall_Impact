using Yueyn.Utils;

namespace Yueyn.UI
{
    /// <summary>
    /// BusinessPartLogic 容器
    /// 管理子页面逻辑的生命周期
    /// </summary>
    public abstract class BusinessPartLogicContainer : BusinessLogicBase
    {
        // ===== PartLogic 管理 =====

        private readonly SafeIterableList<BusinessPartLogic> _partLogics = new SafeIterableList<BusinessPartLogic>();

        // ===== 添加/移除 PartLogic =====

        /// <summary>
        /// 添加子页面逻辑
        /// </summary>
        protected void AddPartLogic(BusinessPartLogic partLogic)
        {
            if (_partLogics.Contains(partLogic))
                return;

            _partLogics.Add(partLogic);
            partLogic.OnCreate();
        }

        /// <summary>
        /// 移除子页面逻辑
        /// </summary>
        protected void RemovePartLogic(BusinessPartLogic partLogic)
        {
            if (!_partLogics.Contains(partLogic))
                return;

            partLogic.OnDestroy();
            _partLogics.Remove(partLogic);
        }

        /// <summary>
        /// 清空所有子页面逻辑
        /// </summary>
        protected void ClearPartLogics()
        {
            var snapshot = _partLogics.GetIterableSnapshot();
            foreach (var partLogic in snapshot)
            {
                partLogic.OnDestroy();
            }
            _partLogics.Clear();
        }

        // ===== 生命周期 =====

        public override void OnDestroy()
        {
            base.OnDestroy();
            ClearPartLogics();
        }
    }
}
