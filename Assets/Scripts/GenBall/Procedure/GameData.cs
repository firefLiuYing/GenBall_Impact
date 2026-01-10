using System;
using System.Text;
using GenBall.Map;
using GenBall.Player;
using UnityEngine;

namespace GenBall.Procedure
{
    [Serializable]
    public class GameData
    {
        
        [SerializeField] private long createTime;
        [SerializeField] private long lastUpdateTime;
        [SerializeField] private long totalTime;
        
        public PlayerSaveData  playerSaveData;
        public MapSaveData mapSaveSaveData; 

        public GameData()
        {
            CreateTime = DateTime.Now;
            LastUpdateTime = DateTime.Now;
            TotalTime = new  DateTime(0);
            playerSaveData = new PlayerSaveData()
            {
                lastSavePointIndex = 0,
                lastSceneName = "",
            };
        }
        public DateTime CreateTime
        {
            get => new DateTime(createTime);
            set => createTime = value.Ticks;
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