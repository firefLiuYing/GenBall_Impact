using System;
using System.Collections.Generic;
using GenBall.BattleSystem.Framework;
using GenBall.BattleSystem.Framework.AI;
using Yueyn.Fsm;

namespace GenBall.Enemy.AI
{
    public static class AIStateRegistry
    {
        private static readonly Dictionary<string, Type> StateTypes = new();

        static AIStateRegistry()
        {
            Register<AIDecisionIdleState>("Idle");
            Register<AIDecisionChaseState>("Chase");
            Register<AIDecisionAttackState>("Attack");
            Register<AIDecisionWanderState>("Wander");
        }

        public static void Register<T>(string name) where T : EnemyDecisionStateBase, new()
            => StateTypes[name] = typeof(T);

        public static Type GetStateType(string name)
            => StateTypes.GetValueOrDefault(name);

        public static FsmState<EnemyDecisionLayer> CreateState(AIStateConfig config)
        {
            var type = GetStateType(config.stateName);
            if (type == null) throw new Exception($"Unknown AI state: {config.stateName}");
            var state = (EnemyDecisionStateBase)Activator.CreateInstance(type);
            state.SetConfig(config);
            return state;
        }
    }
}
