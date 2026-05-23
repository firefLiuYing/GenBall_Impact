using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.Main;

namespace GenBall.BattleSystem.Buff
{
    public class BuffRegistry : IBuffRegistry
    {
        private readonly SortedSet<BuffObj> _activeBuffs = new(new DefaultComparerBuff());
        private readonly List<BuffObj> _cachedBuffs = new();

        public IReadOnlyCollection<BuffObj> ActiveBuffs => _activeBuffs;

        public void Init() { }

        public void UnInit()
        {
            _activeBuffs.Clear();
            _cachedBuffs.Clear();
        }

        /// <summary>
        /// 添加Buff统一流程
        /// info已经使用完会自动释放，无需手动释放
        /// </summary>
        /// <param name="info"></param>
        public BuffObj AddBuff([NotNull] AddBuffInfo info)
        {
            if (info.Model == null)
            {
                Debug.LogError("gzp BuffModel不能为null");
                ReferencePool.Release(info);
                return null;
            }
            if (info.Carrier == null)
            {
                Debug.LogError("gzp Buff添加目标不能为null");
                ReferencePool.Release(info);
                return null;
            }

            var buffContainer = info.Carrier.GetComponent<IBuffContainer>();
            buffContainer.GetBuffs(info.Model.BuffId, out var sameIdBuffs);
            if (sameIdBuffs.Count > 0&&!info.Model.CanMultiExist)
            {
                // 已经存在该Buff且不能同时存在多个，走叠加逻辑
                buffContainer.GetBuffs<ITriggerBeforeStackBuff>(out var beforeStackBuffs);
                foreach (var beforeStackBuff in beforeStackBuffs)
                {
                    beforeStackBuff.TriggerBeforeStackBuff(info);
                }
                beforeStackBuffs.ReleaseBuffList();

                // 实际进行叠加
                var sameIdBuff = sameIdBuffs.First();
                sameIdBuffs.ReleaseBuffList();
                sameIdBuff?.OnStack(info);

                buffContainer.GetBuffs<ITriggerAfterStackBuff>(out var afterStackBuffs);
                foreach (var afterStackBuff in afterStackBuffs)
                {
                    afterStackBuff.TriggerAfterStackBuff(info);
                }
                afterStackBuffs.ReleaseBuffList();
                ReferencePool.Release(info);
                return sameIdBuff;
            }
            // 可同时存在多个或者尚未有同ID的Buff，走新增逻辑
            // 添加前回调
            sameIdBuffs.ReleaseBuffList();
            buffContainer.GetBuffs<ITriggerBeforeAddBuff>(out var beforeAddBuffs);
            foreach (var beforeAddBuff in beforeAddBuffs)
            {
                beforeAddBuff.TriggerBeforeAddBuff(info);
            }
            beforeAddBuffs.ReleaseBuffList();
            // 实际添加
            var buffObj = BuffObj.Create(info);
            buffContainer.AddBuff(buffObj);
            _activeBuffs.Add(buffObj);
            buffObj.OnAdd(info);
            // 添加后回调
            buffContainer.GetBuffs<ITriggerAfterAddBuff>(out var afterAddBuffs);
            foreach (var afterAddBuff in afterAddBuffs)
            {
                afterAddBuff.TriggerAfterAddBuff(info);
            }
            afterAddBuffs.ReleaseBuffList();
            ReferencePool.Release(info);

            return buffObj;
        }

        /// <summary>
        /// 移除Buff统一流程，暂时未加移除流程中的回调点，后续可以继续扩展
        /// 会自动回收BuffObj
        /// </summary>
        /// <param name="buffObj"></param>
        public void RemoveBuff([NotNull] BuffObj buffObj)
        {
            var buffContainer=buffObj.Carrier.GetComponent<IBuffContainer>();
            buffContainer.RemoveBuff(buffObj);
            _activeBuffs.Remove(buffObj);
            ReferencePool.Release(buffObj);
        }
    }
}
