using System.Collections.Generic;
using UnityEngine;
using Yueyn.Base.ReferencePool;

namespace GenBall.BattleSystem.Timeline
{
    public class TimelineObj:IReference
    {
        public TimelineModel Model;
        public float TimeElapsed{get;private set;}
        public GameObject Target{get;private set;}
        public float TimeScale=1;
        private int _currentSegmentIndex = 0;
        private TimelineSegmentObj _currentSegment;
        public void Start()
        {
            NextSegment();
        }
        public void Tick(float deltaTime)
        {
            TimeElapsed += deltaTime*TimeScale;
            if (_currentSegment != null)
            {
                _currentSegment.Tick(deltaTime);
                if (TimeElapsed > _currentSegment.Model.endTime)
                {
                    NextSegment();
                }
            }
        }

        private void NextSegment()
        {
            if (_currentSegment != null)
            {
                ReferencePool.Release(_currentSegment);
                _currentSegment = null;
            }
            _currentSegmentIndex++;
            if (_currentSegmentIndex >= Model.segments.Count)
            {
                GameEntry.Timeline.RemoveTimeline(this);
                return;
            }
            _currentSegment = TimelineSegmentObj.Create(Model.segments[_currentSegmentIndex],this);
            _currentSegment.Start();
        }
        public static TimelineObj Create(TimelineModel model,GameObject target=null,float timeScale=1)
        {
            var obj=ReferencePool.Acquire<TimelineObj>();
            obj.Model = model;
            obj.Target = target;
            obj.TimeElapsed = 0f;
            obj.TimeScale = timeScale;
            obj._currentSegmentIndex = -1;
            obj._currentSegment = null;
            return obj;
        }
        public void Clear()
        {
            Model = null;
            Target=null;
            TimeElapsed = 0;
            TimeScale = 1;
            _currentSegmentIndex = 0;
            if (_currentSegment != null)
            {
                ReferencePool.Release(_currentSegment);
                _currentSegment = null;
            }
        }
        
    }

    public abstract class TimelineSegmentObj:IReference
    {
        public TimelineObj Timeline{get;private set;}

        public TimelineSegment Model{get;private set;}

        public static TimelineSegmentObj Create(TimelineSegment model,TimelineObj timelineObj)
        {
            var obj = (TimelineSegmentObj)ReferencePool.Acquire(model.segmentId.ToType());
            obj.Model=model;
            obj.Timeline = timelineObj;
            return obj;
        }
        public abstract void Start();
        public abstract void Tick(float deltaTime);
        public virtual void Clear()
        {
            Model=null;
            Timeline = null;
        }
    }
}