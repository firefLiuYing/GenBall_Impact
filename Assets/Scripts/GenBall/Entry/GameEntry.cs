using System;
using GenBall.BattleSystem.Bullets;
using GenBall.BattleSystem.Weapons;
using GenBall.Enemy;
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
        private void Awake()
        {
            RegisterModules();
            RegisterEntityPrefabs();
            // todo gzp 暂时这么写，后续改成流程控制里面可以控制1，按Alt键显示鼠标
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
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