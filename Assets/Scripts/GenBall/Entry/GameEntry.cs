using System;
using UnityEngine;
using Yueyn.Main;
using Yueyn.Main.Entry;

namespace GenBall
{
    [Obsolete]
    public partial class GameEntry : MonoBehaviour
    {
        private static Entry _entry;
        private void Awake()
        {
            _entry = new Entry();
            RegisterModules();
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