using Yueyn.Main;

namespace GenBall.BattleSystem.Weapons.Accessory
{
    public interface IEvolutionSystem : ISystem
    {
        int MaxEvolutionLevel { get; }
        int CurrentEvolutionLevel { get; set; }
        int KillPoints { get; }
        bool CanEvolve { get; }
        EquipInfo GetEquipInfo(int level);
    }
}
