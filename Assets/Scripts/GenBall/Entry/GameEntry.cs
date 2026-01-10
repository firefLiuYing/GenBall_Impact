using System;
using GenBall.BattleSystem.Bullets;
using GenBall.BattleSystem.Weapons;
using GenBall.Enemy;
using GenBall.Procedure.Execute;
using GenBall.Utils.EntityCreator;
using UnityEngine;
using Yueyn.Event;
using Yueyn.Fsm;
using Yueyn.Main;
using Yueyn.Main.Entry;
using Yueyn.ObjectPool;
using Yueyn.Resource;

namespace GenBall
{
    public partial class GameEntry : MonoBehaviour
    {
        private static Entry _entry;
        private void Awake()
        {
            _entry = new Entry();
            RegisterModules();
            
            RegisterEntityPrefabs();
        }

        private void Start()
        {
            _entry.Initialize();
        }

        private void Update()
        {
            _entry.Update(Time.deltaTime,Time.deltaTime);
        }

        private void FixedUpdate()
        {
            _entry.FixedUpdate(Time.deltaTime);
        }
        public static T GetModule<T>() where T:IComponent => _entry.GetComponent<T>();
    }
}