using System.Collections.Generic;
using GenBall.Procedure.Game;
using GenBall.Utils.Singleton;
using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.Main;

namespace GenBall.BattleSystem.Timeline
{
    public partial class TimelineSystem:MonoBehaviour,IComponent
    {
        private readonly List<TimelineObj> _timelineObjs = new();
        private TimelineModelConfig _config;

        public void AddTimeline(AddTimelineInfo info)
        {
            var model=_config.GetModel(info.TimelineId);
            if (model == null)
            {
                Debug.LogError($"gzp 找不对{info.TimelineId}对应TimelineModel配置");
                return;
            }
            var obj=TimelineObj.Create(model,info.Target,info.TimeScale);
            obj.Start();
            _timelineObjs.Add(obj);
        }

        public void RemoveTimeline(TimelineObj timelineObj)
        {
            _timelineObjs.Remove(timelineObj);
            ReferencePool.Release(timelineObj);
        }
        private readonly List<TimelineObj> _cachedTimelineObjs = new();
        public void ComponentFixedUpdate(float fixedDeltaTime)
        {
            if((PauseManager.Instance.State&PauseState.LogicPaused)==PauseState.LogicPaused) return;
            _cachedTimelineObjs.Clear();
            _cachedTimelineObjs.AddRange(_timelineObjs);
            foreach (var timelineObj in _cachedTimelineObjs)
            {
                timelineObj.Tick(fixedDeltaTime);
            }
        }
        public int Priority => 1000;
        public void Init()
        {
            #if UNITY_EDITOR
            _config=ConfigProvider.GetOrCreateConfig();
            #else
            _config=null;
            #endif
        }

        public void OnUnregister()
        {
            
        }

        public void ComponentUpdate(float elapsedSeconds, float realElapseSeconds)
        {
            
        }

        public void Shutdown()
        {
            
        }
    }

    public struct AddTimelineInfo
    {
        public string TimelineId;
        public float TimeScale;
        public GameObject Target;

        public AddTimelineInfo(string timelineId, float timeScale=1, GameObject target=null)
        {
            TimelineId = timelineId;
            TimeScale = timeScale;
            Target = target;
        }
    }
}