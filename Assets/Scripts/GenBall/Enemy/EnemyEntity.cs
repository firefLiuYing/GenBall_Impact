using System;
using System.Collections.Generic;
using GenBall.BattleSystem;
using GenBall.Enemy.Fsm;
using JetBrains.Annotations;
using UnityEngine;

namespace GenBall.Enemy
{
    public class EnemyEntity : MonoBehaviour,IEnemy
    {
        private readonly List<Module> _moduleMap = new();
        private FsmModule _fsmModule;
        private Module GetModule(Type type)
        {
            foreach (var module in _moduleMap)
            {
                if (type.IsAssignableFrom(module.GetType()))
                {
                    return module;
                }
            }
            throw new Exception($"Module:{type} not found");
        }
        public TModule GetModule<TModule>() where TModule : Module =>(TModule)GetModule(typeof(TModule));
        public void OnAttacked(AttackInfo attackInfo)
        {
            _fsmModule?.OnAttacked(attackInfo);
        }

        public void EntityUpdate(float deltaTime)
        {
            foreach (var module in _moduleMap)
            {
                module.ModuleUpdate(deltaTime);
            }
        }

        public void EntityFixedUpdate(float fixedDeltaTime)
        {
            foreach (var module in _moduleMap)
            {
                module.ModuleFixedUpdate(fixedDeltaTime);
            }
        }

        public void OnRecycle()
        {
            foreach (var module in _moduleMap)
            {
                module.OnRecycle();
            }
            _moduleMap.Clear();
            gameObject.SetActive(false);
        }

        public void Initialize()
        {
            _moduleMap.Clear();
            var modules = GetComponents<Module>();
            foreach (var module in modules)
            {
                module.SetOwner(this);
                _moduleMap.Add(module);
            }
            foreach (var module in modules)
            {
                module.Initialize();
            }
            _fsmModule=GetModule<FsmModule>();
            
            gameObject.SetActive(true);
        }
    }
}