using System;
using GenBall.BattleSystem.Bullets;
using GenBall.BattleSystem.Weapons;
using UnityEngine;
using Yueyn.Event;
using Yueyn.Fsm;
using Yueyn.Main;
using Yueyn.Main.Entry;
using Yueyn.ObjectPool;
using Yueyn.Resource;

namespace GenBall
{
    public class GameEntry : MonoBehaviour
    {
        private void Awake()
        {
            RegisterModules();
            // todo 暂时这么写，后续改成流程控制里面可以控制1，按Alt键显示鼠标
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
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
            Entry.Register(new ResourceManager());
            Entry.Register(new WeaponCreator());
            Entry.Register(new BulletCreator());
        }
    }
}