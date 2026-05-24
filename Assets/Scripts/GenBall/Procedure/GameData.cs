using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GenBall.Procedure
{
    [Serializable]
    public class GameData
    {
        [SerializeField] private long createTime;
        [SerializeField] private long lastUpdateTime;
        [SerializeField] private long totalTime;

        [SerializeField] private List<DataBlock> dataBlocks = new();

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

        public IEnumerable<DataBlock> DataBlocks => dataBlocks;

        public GameData()
        {
            CreateTime = DateTime.Now;
            LastUpdateTime = DateTime.Now;
            TotalTime = new DateTime(0);
        }

        public string GetData(string key)
        {
            var block = dataBlocks.Find(b => b.Key == key);
            return block?.Value;
        }

        public void SetData(string key, string value)
        {
            var block = dataBlocks.Find(b => b.Key == key);
            if (block != null)
            {
                block.Value = value;
            }
            else
            {
                dataBlocks.Add(new DataBlock { Key = key, Value = value });
            }
        }

        public bool HasData(string key)
        {
            return dataBlocks.Exists(b => b.Key == key);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"CreateTime:{CreateTime:yyyy/MM/dd HH:mm:ss}");
            sb.Append($" LastUpdateTime:{LastUpdateTime:yyyy/MM/dd HH:mm:ss}");
            sb.Append($" TotalTime:{TotalTime:yyyy/MM/dd HH:mm:ss}");
            sb.Append($" DataBlocks:{dataBlocks.Count}");
            return sb.ToString();
        }

        [Serializable]
        public class DataBlock
        {
            public string Key;
            public string Value;
        }
    }
}
