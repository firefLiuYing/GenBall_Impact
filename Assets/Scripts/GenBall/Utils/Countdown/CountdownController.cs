using System;
using System.Collections.Generic;
using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.Base.Variable;

namespace GenBall.Utils.Countdown
{
    public partial class CountdownController
    {
        private readonly Dictionary<string, CountdownEvent> _countdownEvents = new();

        public bool HasCountdownEvent(string name)=> _countdownEvents.ContainsKey(name);

        public bool HasCountdownCompleted(string name)
        {
            if (_countdownEvents.TryGetValue(name, out var countdownEvent))
            {
                return countdownEvent.Compeleted;
            }
            throw new Exception("Countdown event not found");
        }

        public void Start(string countdownName)
        {
            if (!_countdownEvents.TryGetValue(countdownName, out var countdownEvent)) throw new Exception("Countdown event not found");
            countdownEvent.Start();
        }

        public void Pause(string countdownName)
        {
            if (!_countdownEvents.TryGetValue(countdownName, out var countdownEvent)) throw new Exception("Countdown event not found");
            countdownEvent.Pause();
        }

        public void Resume(string countdownName)
        {
            if (!_countdownEvents.TryGetValue(countdownName, out var countdownEvent))  throw new Exception("Countdown event not found");
            countdownEvent.Resume();
        }
        public void Update(float deltaTime)
        {
            foreach (var countdownEvent in _countdownEvents.Values)
            {
                countdownEvent.Update(deltaTime);
            }
        }

        public void AddCountdownEvent(string name,float countdownTime,Action<float> updateCallback=null,Action completeCallback=null)
        {
            if (_countdownEvents.ContainsKey(name))
            {
                throw new Exception("Countdown Event has already been registered");
            }
            var countdown=CountdownEvent.Create(countdownTime, updateCallback, completeCallback);
            _countdownEvents.Add(name,countdown);
        }

        public bool RemoveCountdownEvent(string name)
        {
            if (_countdownEvents.Remove(name,out CountdownEvent countdown))
            {
                ReferencePool.Release(countdown);
            }
            return false;
        }

        public void RemoveAllCountdownEvents()
        {
            var countdownEvents=new List<CountdownEvent>();
            countdownEvents.AddRange(_countdownEvents.Values);
            _countdownEvents.Clear();
            foreach (var countdownEvent in countdownEvents)
            {
                ReferencePool.Release(countdownEvent);
            }
            countdownEvents.Clear();
        }
    }
}