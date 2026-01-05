using System;
using UnityEngine;

namespace GenBall.Enemy.Attack
{
    [RequireComponent(typeof(Collider))]
    public class AttackCollider : Module
    {
        private Collider _collider;
        private Action<Player.Player> _findCallback;
        
        private bool _triggered = false;
        public override void Initialize()
        {
            _collider=GetComponent<Collider>();
            
            _collider.isTrigger=true;
            _collider.enabled = false;
        }
        public void SetFindCallback(Action<Player.Player> callback)=>_findCallback=callback;
        // todo gzp 改成可以选择判定模式的

        public void StartDetect()
        {
            _collider.enabled = true;
            _triggered = false;
        }

        public void StopDetect()
        {
            _collider.enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if(_triggered) return;
            var target=other.GetComponentInParent<Player.Player>();
            if(target==null)  return;
            _triggered = true;
            _findCallback?.Invoke(target);
        }

        public override void OnRecycle()
        {
            
        }

    }
}