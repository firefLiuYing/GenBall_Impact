using System;
using System.Collections.Generic;
using UnityEngine;

namespace GenBall.Procedure
{
    [Serializable]
    public class SaveSlotDataListJson
    {
        public List<SaveSlotData> slots;
    }

    [Serializable]
    public struct SaveSlotData
    {
        public int saveIndex;
        public bool isEmpty;
        [SerializeField] private long lastUpdateTime;
        [SerializeField] private long totalTime;
        [SerializeField] private long createTime;

        public DateTime LastUpdateTime
        {
            get => new DateTime(lastUpdateTime);
            set => lastUpdateTime = value.Ticks;
        }

        public DateTime CreateTime
        {
            get => new DateTime(createTime);
            set => createTime = value.Ticks;
        }

        public DateTime TotalTime
        {
            get => new DateTime(totalTime);
            set => totalTime = value.Ticks;
        }
    }
}
