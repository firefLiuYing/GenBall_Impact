using System;
using System.Collections.Generic;
using GenBall.Enemy.Controller;
using UnityEngine;
using Yueyn.Fsm;

namespace GenBall.Enemy.AI
{
    [CreateAssetMenu(menuName = "Enemy/AIConfig")]
    public class EnemyAIConfigSo : ScriptableObject
    {
        [SerializeField] private string startStateName = "Idle";
        [SerializeField] private List<AIStateConfig> stateConfigs;

        public Type StartStateType => AIStateRegistry.GetStateType(startStateName);

        public List<FsmState<EnemyAIController>> CreateStates()
        {
            var states = new List<FsmState<EnemyAIController>>();
            foreach (var config in stateConfigs)
            {
                var state = AIStateRegistry.CreateState(config);
                states.Add(state);
            }
            return states;
        }
    }
}
