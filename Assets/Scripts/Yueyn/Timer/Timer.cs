using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Yueyn.Base.ReferencePool;

namespace Yueyn.Timer
{
    public static partial class Timer
    {
        private static readonly Dictionary<string, Event> ActiveEvents = new();
        private static readonly Dictionary<string, Event> PausedEvents = new();
        private static readonly Dictionary<string, Event> TempEvents = new();

        internal static void Update(float deltaTime)
        {
            TempEvents.Clear();
            TempEvents.AddRange(ActiveEvents);
            foreach (var pair in ActiveEvents)
            {
                pair.Value.Update(deltaTime);
                if (pair.Value.NeedTrigger)
                {
                    TriggerEvent(pair.Key,pair.Value);
                }
            }
        }

        public static void Subscribe(string eventName,Action callback,float countdownTime,bool countable=false,int repeatCount=1)
        {
            if (ActiveEvents.ContainsKey(eventName) || PausedEvents.ContainsKey(eventName))
            {
                throw new Exception($"Event {eventName} already exists!");
            }
            var timerEvent = ReferencePool.Acquire<Event>();
            timerEvent.SetInitArgs(callback, countdownTime, countable, repeatCount);
            ActiveEvents.Add(eventName, timerEvent);
        }

        public static void Pause(string eventName)
        {
            if (ActiveEvents.Remove(eventName, out var timerEvent))
            {
                PausedEvents.Add(eventName, timerEvent);
                return;
            }
            throw new Exception($"Event {eventName} is not active or does not exist!");
        }

        public static void Resume(string eventName)
        {
            if (PausedEvents.Remove(eventName, out var timerEvent))
            {
                ActiveEvents.Add(eventName, timerEvent);
                return;
            }
            throw new Exception($"Event {eventName} is not paused or does not exist!");
        }

        public static void Reset(string eventName)
        {
            if (ActiveEvents.TryGetValue(eventName, out var timerEvent))
            {
                timerEvent.Reset();
                return;
            }
            throw new Exception($"Event {eventName} is not active or does not exist!");
        }

        public static void Trigger(string eventName)
        {
            if (ActiveEvents.TryGetValue(eventName, out var timerEvent))
            {
                TriggerEvent(eventName, timerEvent);
                return;
            }
            throw new Exception($"Event {eventName} is not active or does not exist!");
        }

        private static void TriggerEvent(string eventName,Event timerEvent)
        {
            timerEvent.Trigger();
            if (!timerEvent.NeedRemove) return;
            ActiveEvents.Remove(eventName);
            ReferencePool.Release(timerEvent);
        }
    }
}