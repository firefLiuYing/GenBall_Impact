using System;
using System.Text;
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

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine("GameData");
            sb.AppendLine($"CreateTime: {CreateTime:yyyy/MM/dd HH:mm:ss}" );
            sb.AppendLine($"LastUpdateTime: {LastUpdateTime:yyyy/MM/dd HH:mm:ss}");
            sb.AppendLine($"TotalTime: {TotalTime:yyyy/MM/dd HH:mm:ss}");
            return sb.ToString();
        }
    }
}