using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Yueyn.Base.ReferencePool;

namespace GenBall.BattleSystem.Buff
{
    public abstract class BuffObj : IBuff,IReference
    {
        public BuffModel Model { get;private set; }
        public BuffId BuffId=>Model?.BuffId??BuffId.Default;
        public int Priority => Model?.Priority ?? 0;
        public bool CanMultiExist => Model?.CanMultiExist ?? false;
        public IReadOnlyList<string> Tags => Model?.Tags??Enumerable.Empty<string>().ToList();
        /// <summary>
        /// Buff施加者，可以为空
        /// </summary>
        public GameObject Caster{get;private set;}
        /// <summary>
        /// Buff携带者，不可为空
        /// </summary>
        public GameObject Carrier{get;private set;}
        protected int Stacks = 1;
        protected float TickTimer { get; private set; } = 0f;

        public static BuffObj Create([NotNull] AddBuffInfo addBuffInfo)
        {
            if (addBuffInfo.Model == null)
            {
                Debug.LogError("gzp 创建BuffObj失败：Model为null");
                return null;
            }
            var buffObj=(BuffObj)ReferencePool.Acquire(addBuffInfo.Model.BuffId.ToType());
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
        /// 当前Buff被添加时触发，如果此时存在同类型Buff，就不触发
        /// </summary>
        /// <param name="addBuffInfo"></param>
        public virtual void OnAdd(AddBuffInfo addBuffInfo){}
        /// <summary>
        /// 与当前同类型Buff被添加时触发
        /// 如果当前buff是可以同时存在多个的，那么就不支持叠层功能
        /// </summary>
        /// <param name="addBuffInfo"></param>
        public virtual void OnStack(AddBuffInfo addBuffInfo){}
        /// <summary>
        /// 当前Buff被移除时触发
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