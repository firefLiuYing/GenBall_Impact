using System;
using UnityEngine;

namespace GenBall.Map.EnemyUnitConfig
{
    public abstract class EnemyUnitConfigBase:MonoBehaviour
    {
        [SerializeField,HideInInspector] private int id;
        public abstract string TypeName { get; }

        public int Index{get=>id; set=>id=value;}
        private void Awake()
        {
            gameObject.SetActive(false);
        }
    }
}