using System.Collections.Generic;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.Framework.TimeScale
{
    /// <summary>
    /// Stub implementation of ITimeScaleSystem.
    /// Logs all requests, does NOT modify Time.timeScale yet.
    /// </summary>
    public class TimeScaleSystemDefault : ITimeScaleSystem
    {
        private readonly List<ScaleRequest> _requests = new();
        private int _nextId;

        public float EffectiveScale
        {
            get
            {
                if (_requests.Count == 0) return 1f;
                float minScale = 1f;
                foreach (var req in _requests)
                {
                    if (req.TargetScale < minScale)
                        minScale = req.TargetScale;
                }
                return minScale;
            }
        }

        public object Request(object source, float targetScale, int priority = 0)
        {
            var handle = new ScaleHandle { Id = _nextId++ };
            _requests.Add(new ScaleRequest
            {
                Handle = handle,
                Source = source,
                TargetScale = targetScale,
                Priority = priority,
            });
            Debug.Log($"[TimeScale] Request: source={source}, scale={targetScale}, priority={priority}, effective={EffectiveScale}");
            return handle;
        }

        public void ReleaseRequest(object handle)
        {
            var h = handle as ScaleHandle;
            if (h == null) return;
            _requests.RemoveAll(r => r.Handle.Id == h.Id);
            Debug.Log($"[TimeScale] Released: handle={h.Id}, effective={EffectiveScale}, remaining={_requests.Count}");
        }

        public void Init() { }
        public void UnInit() { _requests.Clear(); }

        private class ScaleRequest
        {
            public ScaleHandle Handle;
            public object Source;
            public float TargetScale;
            public int Priority;
        }

        private class ScaleHandle
        {
            public int Id;
        }
    }
}
