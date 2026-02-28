using System.Collections.Generic;
using System.Linq;
using GenBall.Procedure.Game;
using JetBrains.Annotations;
using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.Main;

namespace GenBall.BattleSystem.Buff
{
    public class BuffSystem:MonoBehaviour,IComponent
    {
        private readonly SortedSet<BuffObj> _activeBuffs = new();
        private readonly List<BuffObj> _cachedBuffs = new();
        
        /// <summary>
        /// 所有添加Buff务必走这个方法
        /// info已经在流程中自动释放，无需再手动释放
        /// </summary>
        /// <param name="info"></param>
        public void AddBuff([NotNull] AddBuffInfo info)
        {
            if (info.Model == null)
            {
                Debug.LogError("gzp BuffModel不能为null");
                ReferencePool.Release(info);
                return;
            }
            if (info.Carrier == null)
            {
                Debug.LogError("gzp Buff添加目标不能为null");
                ReferencePool.Release(info);
                return;
            }

            var buffContainer = info.Carrier.GetComponent<IBuffContainer>();
            var sameIdBuffs = buffContainer.GetBuffs(info.Model.BuffId);
            if (sameIdBuffs.Count > 0&&!info.Model.CanMultiExist)
            {
                // 已经存在该Buff，且不可同时存在多个，走叠层流程
                var beforeStackBuffs = buffContainer.GetBuffs<ITriggerBeforeStackBuff>();
                foreach (var beforeStackBuff in beforeStackBuffs)
                {
                    beforeStackBuff.TriggerBeforeStackBuff(info);
                }
                beforeStackBuffs.Clear();
                
                // 实际进行叠层
                foreach (var sameIdBuff in sameIdBuffs)
                {
                    sameIdBuff.OnStack(info);
                }

                var afterStackBuffs = buffContainer.GetBuffs<ITriggerAfterStackBuff>();
                foreach (var afterStackBuff in afterStackBuffs)
                {
                    afterStackBuff.TriggerAfterStackBuff(info);
                }
                afterStackBuffs.Clear();
                ReferencePool.Release(info);
                return;
            }
            // 可同时存在多个，或者还未存在同名Buff，走添加流程
            // 触发添加前回调点
            var beforeAddBuffs = buffContainer.GetBuffs<ITriggerBeforeAddBuff>();
            foreach (var beforeAddBuff in beforeAddBuffs)
            {
                beforeAddBuff.TriggerBeforeAddBuff(info);
            }
            beforeAddBuffs.Clear();
            // 实际添加
            var buffObj = BuffObj.Create(info);
            buffContainer.AddBuff(buffObj);
            _activeBuffs.Add(buffObj);
            // 触发添加后回调点
            var afterAddBuffs = buffContainer.GetBuffs<ITriggerAfterAddBuff>();
            foreach (var afterAddBuff in afterAddBuffs)
            {
                afterAddBuff.TriggerAfterAddBuff(info);
            }
            afterAddBuffs.Clear();
            ReferencePool.Release(info);
        }

        /// <summary>
        /// 移除Buff统一流程，暂时未考虑移除过程中的回调点，后续如果有相关需求再来拓展
        /// </summary>
        /// <param name="buffObj"></param>
        public void RemoveBuff([NotNull] BuffObj buffObj)
        {
            var buffContainer=buffObj.Carrier.GetComponent<IBuffContainer>();
            buffContainer.RemoveBuff(buffObj);
            _activeBuffs.Remove(buffObj);
        }
        public void ComponentFixedUpdate(float fixedDeltaTime)
        {
            if((PauseManager.Instance.State&PauseState.LogicPaused)==PauseState.LogicPaused) return;
            _cachedBuffs.Clear();
            _cachedBuffs.AddRange(_activeBuffs);
            foreach (var buffObj in _cachedBuffs)
            {
                buffObj.Tick(fixedDeltaTime);
            }
        }
        public int Priority => 1000;
        public void Init()
        {
        }

        public void OnUnregister()
        {
        }

        public void ComponentUpdate(float elapsedSeconds, float realElapseSeconds)
        {
        }


        public void Shutdown()
        {
        }
    }

    public class AddBuffInfo:IReference
    {
        public BuffModel Model { get; private set; }
        public GameObject Caster { get; private set; }
        public GameObject Carrier { get; private set; }
        /// <summary>
        /// 添加的Buff层数
        /// </summary>
        public int AddStacks { get;private set; }
        public List<BuffParam> Parameters { get;private set; }

        public static AddBuffInfo Create(BuffId buffId, [NotNull] GameObject carrier,int addStacks=1,
            IEnumerable<BuffParam> param = null, GameObject caster = null)
        {
            var buffModel=ConfigProvider.GetOrCreateBuffModelConfig().GetBuffModel(buffId);
            if (buffModel == null)
            {
                Debug.LogError($"gzp 未找到BuffId：{buffId}对应的BuffModel配置");
                return null;
            }
            var info=ReferencePool.Acquire<AddBuffInfo>();
            info.Model = buffModel;
            info.Caster = caster;
            info.Carrier = carrier;
            info.Parameters = param?.ToList();
            info.AddStacks = addStacks;
            return info;
        }
        public void Clear()
        {
            Model = null;
            Caster = null;
            Carrier = null;
            Parameters?.Clear();
            Parameters = null;
            AddStacks = 0;
        }
    }
}