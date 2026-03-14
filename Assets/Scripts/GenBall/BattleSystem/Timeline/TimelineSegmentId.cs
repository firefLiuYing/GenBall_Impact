using System;
using GenBall.BattleSystem.Timeline.Player;

namespace GenBall.BattleSystem.Timeline
{
    public enum TimelineSegmentId
    {
        PlayerDash
    }

    public static class TimelineSegmentIdExtension
    {
        public static Type ToType(this TimelineSegmentId id)
        {
            return id switch
            {
                TimelineSegmentId.PlayerDash=>typeof(DashTimeline),
                _ => typeof(TimelineSegmentObj),
            };
        }
    }
}