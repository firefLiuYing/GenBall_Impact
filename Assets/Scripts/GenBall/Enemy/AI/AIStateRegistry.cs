using System;
using System.Collections.Generic;
using GenBall.Enemy.Controller;
using Yueyn.Fsm;

namespace GenBall.Enemy.AI
{
    public static class AIStateRegistry
    {
        private static readonly Dictionary<string, Type> StateTypes = new();

        static AIStateRegistry()
        {
            Register<AIIdleState>("Idle");
            Register<AIChaseState>("Chase");
            Register<AIAttackState>("Attack");
            Register<AIWanderState>("Wander");
        }

        public static void Register<T>(string name) where T : EnemyAIStateBase, new()
            => StateTypes[name] = typeof(T);

        public static Type GetStateType(string name)
            => StateTypes.GetValueOrDefault(name);

        public static FsmState<EnemyAIController> CreateState(AIStateConfig config)
        {
            var type = GetStateType(config.stateName);
            if (type == null) throw new Exception($"Unknown AI state: {config.stateName}");
            var state = (EnemyAIStateBase)Activator.CreateInstance(type);
            state.SetConfig(config);
            return state;
        }
    }
}
