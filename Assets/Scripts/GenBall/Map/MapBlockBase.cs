using System;
using System.Collections.Generic;
using GenBall.BattleSystem;
using GenBall.Procedure;
using UnityEngine;
using Yueyn.Base.EventPool;
using Yueyn.Event;

namespace GenBall.Map
{
    public abstract class MapBlockBase : MonoBehaviour, IMapBlock
    {
        public void SetIndex(int index)
        {
            gameObject.SetActive(true);
        }
        public void EntityUpdate(float deltaTime)
        {
            
        }

        public void EntityFixedUpdate(float fixedDeltaTime)
        {
            
        }

        public void OnRecycle()
        {
            
        }

    }
}