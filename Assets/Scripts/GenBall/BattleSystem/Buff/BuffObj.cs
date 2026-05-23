using System.Collections.Generic;
using System.Linq;
using GenBall.Framework.Config;
using JetBrains.Annotations;
using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.Main;

namespace GenBall.BattleSystem.Buff
{
    public abstract class BuffObj : IBuff,IReference
    {
        public BuffModel Model { get;private set; }
        public string BuffId => Model?.BuffId ?? string.Empty;
        public int Priority => Model?.Priority ?? 0;
        public bool CanMultiExist => Model?.CanMultiExist ?? false;
        public IReadOnlyList<string> Tags => Model?.Tags??Enumerable.Empty<string>().ToList();
        /// <summary>
        /// Buffʩ���ߣ�����Ϊ��
        /// </summary>
        public GameObject Caster{get;private set;}
        /// <summary>
        /// BuffЯ���ߣ�����Ϊ��
        /// </summary>
        public GameObject Carrier{get;private set;}
        protected int Stacks = 1;
        protected float TickTimer { get; private set; } = 0f;

        public static BuffObj Create([NotNull] AddBuffInfo addBuffInfo)
        {
            if (addBuffInfo.Model == null)
            {
                Debug.LogError("gzp ����BuffObjʧ�ܣ�ModelΪnull");
                return null;
            }
            var buffType = SystemRepository.Instance.GetSystem<IConfigProvider>()?.GetConfig<BuffModelConfig>()?.GetBuffType(addBuffInfo.Model.BuffId);
            if (buffType == null)
            {
                Debug.LogError($"gzp 创建BuffObj失败: Type not found for BuffId={addBuffInfo.Model.BuffId}");
                return null;
            }
            var buffObj=(BuffObj)ReferencePool.Acquire(buffType);
            buffObj.Model = addBuffInfo.Model;
            buffObj.Carrier = addBuffInfo.Carrier;
            buffObj.Caster = addBuffInfo.Caster;
            buffObj.TickTimer = 0f;
            return buffObj;
        }
        
        public void Tick(float deltaTime)
        {
            TickTimer += deltaTime; 
            OnUpdate(deltaTime);
        }
        protected virtual void OnUpdate(float deltaTime){}
        /// <summary>
        /// ��ǰBuff������ʱ�����������ʱ����ͬ����Buff���Ͳ�����
        /// </summary>
        /// <param name="addBuffInfo"></param>
        public virtual void OnAdd(AddBuffInfo addBuffInfo){}
        /// <summary>
        /// �뵱ǰͬ����Buff������ʱ����
        /// �����ǰbuff�ǿ���ͬʱ���ڶ���ģ���ô�Ͳ�֧�ֵ��㹦��
        /// </summary>
        /// <param name="addBuffInfo"></param>
        public virtual void OnStack(AddBuffInfo addBuffInfo){}

        /// <summary>
        /// �ⲿ���ٲ���������ͳһ���̣�Ĭ��ʵ��Ϊ����Stack�������������0һ���¾��Զ��Ƴ��������Ҫ��д����ؿ��Ǽ���0���µ����
        /// </summary>
        /// <param name="unStackCount"></param>
        public virtual void OnUnstack(int unStackCount)
        {
            Stacks-=unStackCount;
            if (Stacks <= 0)
            {
                SystemRepository.Instance.GetSystem<IBuffRegistry>()?.RemoveBuff(this);
            }
        }
        /// <summary>
        /// ��ǰBuff���Ƴ�ʱ����
        /// </summary>
        public virtual void OnRemove(){}
        public virtual void Clear()
        {
            Model = null;
            Caster = null;
            Carrier = null;
            Stacks = 0;
            TickTimer = 0f;
        }
    }
}