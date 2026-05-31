using System.Collections.Generic;
using GenBall.BattleSystem.Framework;
using Yueyn.Main;

namespace GenBall.AbilityWeapon
{
    public interface IAbilityWeaponSystem : ISystem
    {
        void BindPlayer(BattleEntity playerEntity, object weaponAttackExecutor, object visibilityExecutor);

        bool IsAnyActive { get; }
        AbilityWeaponId? ActiveWeaponId { get; }

        void ActivateWeapon(AbilityWeaponId weaponId);
        void DeactivateWeapon();

        float GetCooldownRemaining(AbilityWeaponId weaponId);
        IReadOnlyList<AbilityWeaponId> AvailableWeaponIds { get; }

        IAbilityWeaponConfig GetWeaponConfig(AbilityWeaponId weaponId);
    }
}
