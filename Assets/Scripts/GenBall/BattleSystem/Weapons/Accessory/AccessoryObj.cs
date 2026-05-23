using System.Collections.Generic;
using GenBall.BattleSystem.Buff;
using Yueyn.Base.ReferencePool;
using Yueyn.Main;

namespace GenBall.BattleSystem.Weapons.Accessory
{
    public class AccessoryObj : IReference
    {
        public AccessoryModel Model { get;private set; }

        private readonly Dictionary<BuffObj, AccessoryAddBuffInfo> _addBuffs = new();
        public void OnAdd(WeaponState weapon)
        {
            if (Model.addBuffs == null) return;
            foreach (var addBuff in Model.addBuffs)
            {
                var buffObj = SystemRepository.Instance.GetSystem<IBuffRegistry>().AddBuff(AddBuffInfo.Create(addBuff.buffId, weapon.gameObject, addBuff.stackCount));
                if(buffObj==null) return;
                _addBuffs.Add(buffObj, addBuff);
            }
        }

        public void OnRemove()
        {
            foreach (var buffObj in _addBuffs.Keys)
            {
                buffObj.OnUnstack(_addBuffs[buffObj].stackCount);
            }
            _addBuffs.Clear();
        }

        public static AccessoryObj Create(AccessoryModel model)
        {
            var obj=ReferencePool.Acquire<AccessoryObj>();
            obj.Model=model;
            return obj;
        }
        public void Clear()
        {
            _addBuffs.Clear();
        }
    }
}