using GenBall.BattleSystem.Bullets;
using GenBall.BattleSystem.Framework;
using GenBall.BattleSystem.Weapons.Components;
using GenBall.BattleSystem.Weapons.Components.Ammo;
using GenBall.BattleSystem.Weapons.Components.Spread;
using GenBall.BattleSystem.Weapons.Components.Trigger;
using UnityEngine;
using Yueyn.Resource;

namespace GenBall.BattleSystem.Weapons.Factory
{
    /// <summary>
    /// Assembles a BattleEntity on a weapon GameObject by reading its WeaponAssembly component.
    /// Does NOT load prefabs — the caller provides the instantiated GameObject.
    /// </summary>
    public static class WeaponEntityFactory
    {
        private const string PrefabPath = "Assets/AssetBundles/Common/Weapon/";

        /// <summary>Load weapon prefab by WeaponId, instantiate it, and assemble.</summary>
        public static BattleEntity Create(WeaponId weaponId, Transform parent)
        {
            var path = $"{PrefabPath}{weaponId}.prefab";
            var prefab = CResourceManager.Instance.LoadSync<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError($"[WeaponEntityFactory] Failed to load prefab: {path}");
                return null;
            }

            var go = Object.Instantiate(prefab, parent, false);
            go.name = $"Weapon_{weaponId}";
            return Assemble(go);
        }

        /// <summary>The default starting weapon.</summary>
        public static BattleEntity CreateDefault(Transform parent)
        {
            return Create(WeaponId.Pistol, parent);
        }

        /// <summary>
        /// Assemble a BattleEntity onto an already-instantiated weapon GameObject.
        /// Reads WeaponAssembly from the GO to determine component composition.
        /// </summary>
        public static BattleEntity Assemble(GameObject go)
        {
            var assembly = go.GetComponent<WeaponAssembly>();
            if (assembly == null)
            {
                Debug.LogError($"[WeaponEntityFactory] {go.name} has no WeaponAssembly component.");
                return null;
            }

            var entity = go.GetComponent<BattleEntity>();
            if (entity == null)
                entity = go.AddComponent<BattleEntity>();

            // ── StatComponent ──
            var stats = new StatComponent(entity);
            stats.GetOrCreate("Damage", assembly.Damage);
            stats.GetOrCreate("FireInterval", assembly.FireInterval);
            PopulateAmmoStats(stats, assembly);
            PopulateSpreadStats(stats, assembly);

            // Bullet stats — base values from WeaponAssembly, modifiable by buffs at fire time
            stats.GetOrCreate("BulletSpeed", assembly.BulletSpeed);
            stats.GetOrCreate("BulletRadius", assembly.BulletRadius);
            stats.GetOrCreate("BulletSpeedMultiplier", 1f);
            stats.GetOrCreate("ExtraPenetrations", 0f);
            stats.GetOrCreate("ExtraBounces", 0f);
            entity.RegisterComponent(stats);

            // ── BuffContainer (future accessory buffs) ──
            entity.RegisterComponent(new BuffContainerComponent());

            // ── EventDispatcher ──
            entity.RegisterComponent(new EventDispatcherComponent(entity));

            // ── Ammo ──
            switch (assembly.AmmoType)
            {
                case AmmoArchetype.Magazine:
                    var mag = new WeaponMagazineExecutor(entity);
                    entity.RegisterComponent(mag);
                    entity.RegisterComponentAs<IAmmoSystem>(mag);
                    break;
                case AmmoArchetype.Heat:
                    var heat = new HeatComponent(entity);
                    entity.RegisterComponent(heat);
                    entity.RegisterComponentAs<IAmmoSystem>(heat);
                    break;
                case AmmoArchetype.Infinite:
                    entity.RegisterComponent<IAmmoSystem>(new InfiniteAmmoComponent());
                    break;
            }

            // ── Trigger behavior ──
            ITriggerBehavior behavior = assembly.Archetype switch
            {
                WeaponArchetype.SemiAutoPistol => new SemiAutoTriggerBehavior(),
                WeaponArchetype.FullAutoRifle => new FullAutoTriggerBehavior(),
                WeaponArchetype.Shotgun => new ShotgunTriggerBehavior(
                    assembly.ShotgunPelletCount, assembly.ShotgunSpreadAngle),
                WeaponArchetype.ChargeLaser => new ChargeTriggerBehavior(
                    assembly.ChargeMaxTime, assembly.ChargeMaxDamageMult),
                _ => new SemiAutoTriggerBehavior()
            };

            // ── WeaponFireDecision ──
            entity.RegisterComponent<IWeaponTrigger>(new WeaponFireDecision(entity, behavior));

            // ── WeaponFireExecutor ──
            var spawnPoint = assembly.BulletSpawnPoint != null
                ? assembly.BulletSpawnPoint
                : go.transform;
            entity.RegisterComponent(new WeaponFireExecutor(entity, spawnPoint, assembly.BulletConfigId));

            // ── Optional SpreadComponent ──
            if (assembly.SpreadBase > 0f || assembly.SpreadMoving > 0f)
                entity.RegisterComponent(new SpreadComponent(stats));

            go.SetActive(true);
            return entity;
        }

        private static void PopulateAmmoStats(StatComponent stats, WeaponAssembly assembly)
        {
            switch (assembly.AmmoType)
            {
                case AmmoArchetype.Magazine:
                    stats.GetOrCreate("MagazineCapacity", assembly.MagazineCapacity);
                    stats.GetOrCreate("AmmoCount", assembly.MagazineCapacity);
                    stats.GetOrCreate("ReloadTime", assembly.ReloadTime);
                    break;
                case AmmoArchetype.Heat:
                    stats.GetOrCreate("MaxHeat", assembly.MaxHeat);
                    stats.GetOrCreate("CurrentHeat", 0f);
                    stats.GetOrCreate("HeatCoolRate", assembly.HeatCoolRate);
                    stats.GetOrCreate("HeatPerShot", assembly.HeatPerShot);
                    break;
            }
        }

        private static void PopulateSpreadStats(StatComponent stats, WeaponAssembly assembly)
        {
            if (assembly.SpreadBase > 0f || assembly.SpreadMoving > 0f)
            {
                stats.GetOrCreate("SpreadBase", assembly.SpreadBase);
                stats.GetOrCreate("SpreadMoving", assembly.SpreadMoving);
            }
        }

    }
}
