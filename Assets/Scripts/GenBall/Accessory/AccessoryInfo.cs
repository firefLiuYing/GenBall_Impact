using System.Collections.Generic;
using GenBall.Player;
using JetBrains.Annotations;
using Yueyn.Base.ReferencePool;
using Yueyn.Event;

namespace GenBall.Accessory
{
    public class AccessoryInfo
    {
        private int _level;

        public int Level
        {
            get=>_level;
            set
            {
                var e=ValueChangeEventArgs<int>.Create(value,"Level");
                GameEntry.GetModule<EventManager>().Fire(this,e);
                _level = value;
            }
        }

        public readonly Dictionary<int, LevelConfig> LevelConfigMap = new()
        {
            [1]=new LevelConfig
            {
                Level=1,
            },
            [2]=new LevelConfig
            {
                Level=2,
            },
            [3]=new LevelConfig
            {
                Level=3,
            },
            [4]=new LevelConfig
            {
                Level=4,
            }
        };
        
    }
}