using System;
using UnityEngine;

namespace GenBall.Procedure
{
    [Serializable]
    public class GameData
    {
        public DateTime CreateTime
        {
            get => new DateTime(createTime);
            set
            {
                // Debug.Log(value.Ticks);
                createTime = value.Ticks;
                Debug.Log("CreateTime: " + createTime);
            }
        }

        public DateTime LastUpdateTime
        {
            get => new DateTime(lastUpdateTime);
            set => lastUpdateTime = value.Ticks;
        }

        public DateTime TotalTime
        {
            get => new DateTime(totalTime);
            set => totalTime = value.Ticks;
        }
        
        [SerializeField] private long createTime;
        [SerializeField] private long lastUpdateTime;
        [SerializeField] private long totalTime;
    }
}