using System;
using UnityEngine;
using Yueyn.Event;
using Yueyn.Fsm;
using Yueyn.Main;
using Yueyn.Main.Entry;
using Yueyn.ObjectPool;

namespace GenBall
{
    public class GameEntry : MonoBehaviour
    {
        private void Awake()
        {
            RegisterModules();
        }

        private void Update()
        {
            Entry.Update(Time.deltaTime,Time.deltaTime);
        }

        public static T GetModule<T>() where T:IComponent => Entry.GetComponent<T>();
        private void RegisterModules()
        {
            Entry.Register(new EventManager());
            Entry.Register(new FsmManager());
            Entry.Register(new ObjectPoolManager());
        }
    }
}