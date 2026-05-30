using System;
using System.Collections.Generic;
using GenBall.Framework.Config;
using GenBall.Framework.Entity;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.BattleSystem.Bullets
{
    /// <summary>
    /// Pure C# bullet entity. No MonoBehaviour inheritance.
    /// Logic and visual are separated — BulletVisual handles rendering.
    /// Pooled via BulletSystem's internal Stack pool.
    /// </summary>
    public class BulletInstance : IEntityLogicUpdate, IEntityFrameUpdate
    {
        // ── Identity ──
        public int Id { get; private set; }
        private static int _nextId = 1;

        // ── Runtime state ──
        public Vector3 Position;
        public Vector3 Velocity;
        public Vector3 Direction;
        public float ElapsedTime;

        // ── Config & params ──
        public BulletConfigEntry Config { get; private set; }
        public GameObject Source { get; private set; }
        public int FinalDamage { get; private set; }
        public float FinalSpeed { get; private set; }
        public float FinalRadius { get; private set; }

        // ── Behavior layers ──
        public IDetectionStrategy Detection { get; private set; }
        public IHitBehavior[] HitBehaviors { get; private set; }
        public IMovementModifier[] MovementModifiers { get; private set; }

        // ── State tracking ──
        public int CurrentPenetrationCount;
        public int CurrentBounceCount;
        public HashSet<int> HitTargetIds { get; private set; }

        // ── Visual binding ──
        public BulletVisual Visual { get; private set; }

        // ── Pooling ──
        private bool _isRecycled;
        private Action<BulletInstance> _returnToPool;

        // Default target mask — hits everything except IgnoreRaycast
        private static readonly LayerMask DefaultTargetMask = ~(1 << LayerMask.NameToLayer("Ignore Raycast"));

        public BulletInstance()
        {
            HitTargetIds = new HashSet<int>();
        }

        /// <summary>
        /// Initialize the bullet instance after getting from pool.
        /// Called by BulletSystem.FireBullet().
        /// </summary>
        public void Init(BulletFireParams fireParams, BulletConfigEntry config, BulletVisual visual, Action<BulletInstance> returnToPool)
        {
            Id = _nextId++;
            Config = config;
            Source = fireParams.Source;
            Position = fireParams.LogicOrigin;
            Direction = fireParams.Direction;
            Velocity = fireParams.Direction * fireParams.FinalSpeed;
            ElapsedTime = 0f;

            // Apply runtime overrides
            FinalDamage = fireParams.FinalDamage;
            FinalSpeed = fireParams.FinalSpeed;
            FinalRadius = fireParams.FinalRadius;

            // Reset state
            CurrentPenetrationCount = 0;
            CurrentBounceCount = 0;
            HitTargetIds.Clear();

            // Pool callback
            _returnToPool = returnToPool;

            // Bind visual
            Visual = visual;
            if (visual != null)
            {
                visual.Init(fireParams.VisualOrigin, Position + Direction * config.MaxLifetime * FinalSpeed);
            }

            // Assemble detection strategy
            Detection = config.DetectionMode switch
            {
                DetectionMode.SphereCast => new SphereCastDetection(),
                _ => new RayDetection()
            };

            // Assemble hit behavior chain
            HitBehaviors = BuildHitBehaviors(config, fireParams);

            // Assemble movement modifiers
            MovementModifiers = BuildMovementModifiers(config);

            // Register for logic & frame updates (logic for physics, frame for visual)
            var entitySystem = SystemRepository.Instance.GetSystem<IEntityUpdateSystem>();
            entitySystem?.AddLogicUpdate(this);
            entitySystem?.AddFrameUpdate(this);

            _isRecycled = false;
        }

        /// <summary>
        /// Logic update tick. Called by EntityUpdateSystem each FixedUpdate.
        /// </summary>
        public void LogicUpdate(float deltaTime)
        {
            if (_isRecycled) return;

            ElapsedTime += deltaTime;

            // 1. Apply movement modifiers (gravity, etc.)
            if (MovementModifiers != null)
            {
                foreach (var modifier in MovementModifiers)
                {
                    modifier.Apply(this, deltaTime);
                }
            }

            // 2. Update position
            Position += Velocity * deltaTime;

            // 3. Detection
            var hit = Detection?.Detect(Position, Direction, FinalRadius, FinalSpeed,
                deltaTime, DefaultTargetMask, HitTargetIds);

            if (hit != null)
            {
                var hitResult = hit.Value;
                HitTargetIds.Add(hitResult.TargetId);

                // 4. Execute hit behavior chain
                if (HitBehaviors != null)
                {
                    foreach (var behavior in HitBehaviors)
                    {
                        bool shouldContinue = behavior.OnHit(this, hitResult);
                        if (!shouldContinue)
                        {
                            Recycle();
                            return;
                        }
                    }
                }
            }

            // 5. Lifetime check
            if (ElapsedTime >= Config.MaxLifetime)
            {
                Recycle();
            }
        }

        /// <summary>
        /// Frame update tick. Drives visual position. Called by EntityUpdateSystem each Update.
        /// Syncs BulletVisual's GameObject transform to the current logic position.
        /// </summary>
        public void FrameUpdate(float deltaTime)
        {
            if (_isRecycled || Visual == null || Config == null) return;

            float progress = Config.MaxLifetime > 0f ? ElapsedTime / Config.MaxLifetime : 0f;
            Visual.UpdateVisual(Position, progress);
        }

        /// <summary>
        /// Recycle this bullet back to the pool.
        /// </summary>
        public void Recycle()
        {
            if (_isRecycled) return;
            _isRecycled = true;

            // Unregister from entity update system
            var entitySystem = SystemRepository.Instance.GetSystem<IEntityUpdateSystem>();
            entitySystem?.RemoveLogicUpdate(this);
            entitySystem?.RemoveFrameUpdate(this);

            // Recycle visual
            if (Visual != null)
            {
                Visual.OnRecycle();
                Visual = null;
            }

            // Clear references
            Source = null;
            Config = null;
            Detection = null;
            HitBehaviors = null;
            MovementModifiers = null;
            HitTargetIds.Clear();

            // Return to pool
            _returnToPool?.Invoke(this);
            _returnToPool = null;
        }

        // ======== Private helpers ========

        private static IHitBehavior[] BuildHitBehaviors(BulletConfigEntry config, BulletFireParams fireParams)
        {
            if (config.HitBehaviors == null || config.HitBehaviors.Length == 0)
            {
                return new IHitBehavior[] { new DealDamageBehavior() };
            }

            var behaviors = new List<IHitBehavior>();
            foreach (var def in config.HitBehaviors)
            {
                switch (def.Type)
                {
                    case HitBehaviorType.DealDamage:
                        behaviors.Add(new DealDamageBehavior());
                        break;
                    case HitBehaviorType.Penetrate:
                        int maxPen = def.Count + fireParams.ExtraPenetrations;
                        if (maxPen > 0)
                            behaviors.Add(new PenetrateBehavior(maxPen));
                        break;
                    case HitBehaviorType.Bounce:
                        int maxBounce = def.Count + fireParams.ExtraBounces;
                        if (maxBounce > 0)
                            behaviors.Add(new BounceBehavior(maxBounce));
                        break;
                    case HitBehaviorType.AOEDamage:
                        behaviors.Add(new AOEDamageBehavior(def.Count, (int)def.Value, DefaultTargetMask));
                        break;
                }
            }
            return behaviors.ToArray();
        }

        private static IMovementModifier[] BuildMovementModifiers(BulletConfigEntry config)
        {
            if (config.MovementModifiers == null || config.MovementModifiers.Length == 0)
            {
                return new IMovementModifier[0];
            }

            var modifiers = new List<IMovementModifier>();
            foreach (var def in config.MovementModifiers)
            {
                switch (def.Type)
                {
                    case MovementModifierType.Gravity:
                        modifiers.Add(new GravityModifier(def.Value));
                        break;
                }
            }
            return modifiers.ToArray();
        }
    }
}
