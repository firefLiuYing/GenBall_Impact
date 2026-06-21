using System;
using System.Collections.Generic;
using GenBall.Interact;
using GenBall.Map;
using GenBall.Utils.Trigger;
using UnityEngine;
using UnityEngine.Events;
using Yueyn.Event;
using Yueyn.Main;

namespace GenBall.Event
{
    /// <summary>
    /// Lightweight runtime-only component that deserializes baked SceneTriggerData
    /// and fires events when triggered. Created by SceneExecutorSystemDefault.SpawnTriggers().
    /// </summary>
    public class RuntimeEventTrigger : MonoBehaviour
    {
        private readonly List<(int eventId, EventParameterBase parameters)> _events = new();
        private TriggerMode _triggerMode;
        private TriggerBehavior _behavior;
        private int _maxFireCount;
        private float _cooldownSeconds;
        private int _listenerEventId;
        private float _radius;
        private int _layerMask;

        private Action _listenerDelegate;
        private int _fireCount;
        private float _lastFireTime = float.MinValue;

        public void Initialize(SceneTriggerData data)
        {
            _triggerMode = (TriggerMode)data.triggerMode;
            _behavior = (TriggerBehavior)data.triggerBehavior;
            _maxFireCount = data.maxFireCount;
            _cooldownSeconds = data.cooldownSeconds;
            _radius = data.radius;
            _layerMask = data.layerMask;
            _listenerEventId = data.listenerEventId;

            foreach (var evt in data.events)
            {
                EventParameterBase @params = null;
                if (!string.IsNullOrEmpty(evt.paramTypeName) && !string.IsNullOrEmpty(evt.serializedParams))
                {
                    var type = Type.GetType(evt.paramTypeName);
                    if (type != null && typeof(EventParameterBase).IsAssignableFrom(type))
                        @params = (EventParameterBase)JsonUtility.FromJson(evt.serializedParams, type);
                    else
                        Debug.LogWarning($"[RuntimeEventTrigger] Could not resolve parameter type: {evt.paramTypeName}");
                }
                _events.Add((evt.eventId, @params));
            }

            SetupTrigger();
        }

        private void SetupTrigger()
        {
            switch (_triggerMode)
            {
                case TriggerMode.Collision:
                    SetupCollisionTrigger();
                    break;
                case TriggerMode.Interact:
                    SetupInteractTrigger();
                    break;
                case TriggerMode.EventListener:
                    SetupEventListener();
                    break;
            }
        }

        private void SetupCollisionTrigger()
        {
            // Kinematic Rigidbody ensures OnTriggerEnter fires reliably and
            // prevents the collider from physically blocking the player.
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            var col = gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = _radius;

            var triggerObj = gameObject.AddComponent<TriggerObject>();
            triggerObj.targetLayerMask = _layerMask;
            triggerObj.onTriggerEnter = new UnityEvent();
            triggerObj.onTriggerEnter.AddListener(HandleTrigger);
        }

        private void SetupInteractTrigger()
        {
            var child = new GameObject("TriggerVolume");
            child.transform.SetParent(transform, false);
            child.layer = gameObject.layer;

            var col = child.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = _radius;

            var triggerObj = child.AddComponent<TriggerObject>();
            triggerObj.targetLayerMask = _layerMask;
            triggerObj.onTriggerEnter = new UnityEvent();
            triggerObj.onTriggerExit = new UnityEvent();

            var proxy = gameObject.AddComponent<RuntimeInteractProxy>();
            proxy.OnInteract += HandleTrigger;
        }

        private void SetupEventListener()
        {
            _listenerDelegate = HandleTrigger;
            CEventRouter.Instance.Subscribe(_listenerEventId, _listenerDelegate);
        }

        private void HandleTrigger()
        {
            // Cooldown check (applies to all behaviors)
            if (_cooldownSeconds > 0f && Time.time - _lastFireTime < _cooldownSeconds)
                return;

            // Once: already fired
            if (_behavior == TriggerBehavior.Once && _fireCount > 0)
                return;

            // Limited: reached max
            if (_behavior == TriggerBehavior.Limited && _fireCount >= _maxFireCount)
                return;

            _fireCount++;
            _lastFireTime = Time.time;

            foreach (var (eventId, @params) in _events)
            {
                if (@params != null)
                    @params.Dispatch(eventId);
                else
                    CEventRouter.Instance.FireNow(eventId);
            }

            // Disable self when exhausted
            var exhausted = (_behavior == TriggerBehavior.Once && _fireCount >= 1)
                || (_behavior == TriggerBehavior.Limited && _fireCount >= _maxFireCount);

            if (exhausted)
                gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_listenerDelegate != null)
            {
                CEventRouter.Instance.Unsubscribe(_listenerEventId, _listenerDelegate);
                _listenerDelegate = null;
            }
        }
    }
}
