using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Yueyn.Base.ReferencePool;

namespace GenBall.BattleSystem.Buff
{
    // BuffSystem is now migrated to IBuffRegistry + IBuffTickSystem.
    // Keep AddBuffInfo data class for backward compatibility.

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
            #if UNITY_EDITOR
            var buffModel=ConfigProvider.GetOrCreateBuffModelConfig().GetBuffModel(buffId);
            #else
            var buffModel=new BuffModel();
            #endif
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
