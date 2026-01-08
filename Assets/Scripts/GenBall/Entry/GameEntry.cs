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
        private ExecuteProcedure _procedure;
        private void Awake()
        {
            RegisterModules();
            RegisterEntityPrefabs();
            _procedure = new ExecuteProcedure();
            _procedure.Init();
            _procedure.Start();
        }

        private void OnDestroy()
        {
            _procedure.Stop();
        }

        private void Update()
        {
            Entry.Update(Time.deltaTime,Time.deltaTime);
        }

        private void FixedUpdate()
        {
            Entry.FixedUpdate(Time.deltaTime);
        }
        public static T GetModule<T>() where T:IComponent => Entry.GetComponent<T>();
    }
}