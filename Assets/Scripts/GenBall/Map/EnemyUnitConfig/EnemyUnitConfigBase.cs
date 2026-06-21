using System;
using GenBall.Map;
using UnityEngine;

namespace GenBall.Map.EnemyUnitConfig
{
    public abstract class EnemyUnitConfigBase : MonoBehaviour, IScenePlaceable
    {
        [SerializeField, HideInInspector] private int id = -1;
        [SerializeField] private float patrolRadius = 5f;
        [SerializeField] private float detectRadius = 10f;
        [SerializeField] private int aiBehavior = 0;
        public abstract string TypeName { get; }

        public int Index { get => id; set => id = value; }
        public float PatrolRadius => patrolRadius;
        public float DetectRadius => detectRadius;
        public int AiBehavior => aiBehavior;

        int IScenePlaceable.Id
        {
            get => id;
            set => id = value;
        }

        string IScenePlaceable.DisplayLabel => $"{TypeName}_{Index}";

        string IScenePlaceable.Category => "Enemy";

        Transform IScenePlaceable.Anchor => transform;

        bool IScenePlaceable.IsDynamic => true;

        object IScenePlaceable.BakeToConfigData() => new EnemySpawnData
        {
            id = id,
            enemyType = TypeName,
            position = transform.position,
            rotation = transform.rotation,
            patrolRadius = patrolRadius,
            detectRadius = detectRadius,
            aiBehavior = aiBehavior,
        };

        string IScenePlaceable.Validate() => OnValidateConfig();

        protected virtual string OnValidateConfig() => null;
    }
}