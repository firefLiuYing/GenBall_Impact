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
        private readonly SortedSet<BuffObj> _activeBuffs = new(new DefaultComparerBuff());
        private readonly List<BuffObj> _cachedBuffs = new();
        
        /// <summary>
        /// ��������Buff������������
        /// info�Ѿ����������Զ��ͷţ��������ֶ��ͷ�
        /// </summary>
        /// <param name="info"></param>
        public BuffObj AddBuff([NotNull] AddBuffInfo info)
        {
            if (info.Model == null)
            {
                Debug.LogError("gzp BuffModel����Ϊnull");
                ReferencePool.Release(info);
                return null;
            }
            if (info.Carrier == null)
            {
                Debug.LogError("gzp Buff����Ŀ�겻��Ϊnull");
                ReferencePool.Release(info);
                return null;
            }

            var buffContainer = info.Carrier.GetComponent<IBuffContainer>();
            buffContainer.GetBuffs(info.Model.BuffId, out var sameIdBuffs);
            if (sameIdBuffs.Count > 0&&!info.Model.CanMultiExist)
            {
                // �Ѿ����ڸ�Buff���Ҳ���ͬʱ���ڶ�����ߵ�������
                buffContainer.GetBuffs<ITriggerBeforeStackBuff>(out var beforeStackBuffs);
                foreach (var beforeStackBuff in beforeStackBuffs)
                {
                    beforeStackBuff.TriggerBeforeStackBuff(info);
                }
                beforeStackBuffs.ReleaseBuffList();
                
                // ʵ�ʽ��е���
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
            // ��ͬʱ���ڶ�������߻�δ����ͬ��Buff������������
            // ��������ǰ�ص���
            sameIdBuffs.ReleaseBuffList();
            buffContainer.GetBuffs<ITriggerBeforeAddBuff>(out var beforeAddBuffs);
            foreach (var beforeAddBuff in beforeAddBuffs)
            {
                beforeAddBuff.TriggerBeforeAddBuff(info);
            }
            beforeAddBuffs.ReleaseBuffList();
            // ʵ������
            var buffObj = BuffObj.Create(info);
            buffContainer.AddBuff(buffObj);
            _activeBuffs.Add(buffObj);
            buffObj.OnAdd(info);
            // �������Ӻ�ص���
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
        /// �Ƴ�Buffͳһ���̣���ʱδ�����Ƴ������еĻص��㣬����������������������չ
        /// ���Զ�����BuffObj
        /// </summary>
        /// <param name="buffObj"></param>
        public void RemoveBuff([NotNull] BuffObj buffObj)
        {
            var buffContainer=buffObj.Carrier.GetComponent<IBuffContainer>();
            buffContainer.RemoveBuff(buffObj);
            _activeBuffs.Remove(buffObj);
            ReferencePool.Release(buffObj);
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
        /// ���ӵ�Buff����
        /// </summary>
        public int AddStacks { get;private set; }
        public List<BuffParam> Parameters { get;private set; }

        public static AddBuffInfo Create(BuffId buffId, [NotNull] GameObject carrier,int addStacks=1,
            IEnumerable<BuffParam> param = null, GameObject caster = null)
        {
            var buffModel=ConfigProvider.GetOrCreateBuffModelConfig().GetBuffModel(buffId);
            if (buffModel == null)
            {
                Debug.LogError($"gzp δ�ҵ�BuffId��{buffId}��Ӧ��BuffModel����");
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