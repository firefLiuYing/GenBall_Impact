using System;
using System.Collections.Generic;
using UnityEngine;

namespace GenBall.Enemy.AI
{
    [Serializable]
    public class AIStateConfig
    {
        public string stateName;
        public float moveSpeed;
        public int attackId;
        public float duration;
        public List<AITransitionConfig> transitions;
    }

    [Serializable]
    public class AITransitionConfig
    {
        public string targetStateName;
        public string conditionType;
        public float conditionValue;
    }
}
