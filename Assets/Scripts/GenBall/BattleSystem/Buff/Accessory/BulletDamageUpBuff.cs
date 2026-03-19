using GenBall.BattleSystem.Weapons;

namespace GenBall.BattleSystem.Buff.Accessory
{
    public class BulletDamageUpBuff : BuffObj
    {
        private WeaponState _weapon;
        public override void OnAdd(AddBuffInfo addBuffInfo)
        {
            base.OnAdd(addBuffInfo);
            _weapon = addBuffInfo.Carrier.GetComponent<WeaponState>();
            _weapon.Stats.Damage.AddMultipleZone("Åä¼þ",0.25f);
        }

        public override void OnRemove()
        {
            base.OnRemove();
            _weapon.Stats.Damage.AddMultipleZone("Åä¼þ",-0.25f);
        }
    }
}