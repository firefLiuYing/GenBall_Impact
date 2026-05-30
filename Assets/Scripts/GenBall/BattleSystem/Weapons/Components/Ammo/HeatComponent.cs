using GenBall.BattleSystem.Framework;
using GenBall.Framework.Entity;

namespace GenBall.BattleSystem.Weapons.Components.Ammo
{
    /// <summary>
    /// Heat-based ammo. Each shot generates heat; heat cools over time in LogicUpdate.
    /// Stats: MaxHeat, CurrentHeat, HeatCoolRate, HeatPerShot — all in StatComponent.
    /// </summary>
    public class HeatComponent : IAmmoSystem, IEntityLogicUpdate
    {
        private readonly BattleEntity _weapon;

        private const string StatMaxHeat = "MaxHeat";
        private const string StatCurrentHeat = "CurrentHeat";
        private const string StatHeatCoolRate = "HeatCoolRate";
        private const string StatHeatPerShot = "HeatPerShot";

        public HeatComponent(BattleEntity weapon) { _weapon = weapon; }

        private StatComponent Stats => _weapon?.Get<StatComponent>();
        private float Heat
        {
            get => Stats?.GetValue(StatCurrentHeat) ?? 0f;
            set => Stats?.SetBase(StatCurrentHeat, value);
        }
        private float MaxHeat => Stats?.GetValue(StatMaxHeat) ?? 100f;
        private float PerShot => Stats?.GetValue(StatHeatPerShot) ?? 10f;
        private float CoolRate => Stats?.GetValue(StatHeatCoolRate) ?? 20f;

        public bool CanFire => Heat + PerShot <= MaxHeat;
        public void AddHeat() { Heat += PerShot; }

        public void LogicUpdate(float deltaTime)
        {
            float h = Heat - CoolRate * deltaTime;
            Heat = h < 0f ? 0f : h;
        }

        public AmmoDisplayInfo GetDisplayInfo()
        {
            return new AmmoDisplayInfo
            {
                Type = AmmoDisplayType.Heat,
                NormalizedValue = MaxHeat > 0f ? Heat / MaxHeat : 0f
            };
        }
    }
}
