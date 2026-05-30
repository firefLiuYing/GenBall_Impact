using GenBall.BattleSystem.Framework;

namespace GenBall.BattleSystem.Weapons.Components.Spread
{
    public interface ISpreadProvider
    {
        float GetSpreadAngle(bool isMoving);
    }

    public class SpreadComponent : ISpreadProvider
    {
        private readonly StatComponent _stats;

        private const string StatSpreadBase = "SpreadBase";
        private const string StatSpreadMoving = "SpreadMoving";

        public SpreadComponent(StatComponent stats) { _stats = stats; }

        public float GetSpreadAngle(bool isMoving)
        {
            if (isMoving) return _stats?.GetValue(StatSpreadMoving) ?? 5f;
            return _stats?.GetValue(StatSpreadBase) ?? 0f;
        }
    }
}
