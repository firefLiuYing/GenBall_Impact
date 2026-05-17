namespace GenBall.Event
{
    /// <summary>
    /// 全局事件 ID 定义
    /// 每个模块使用不同的数值范围，避免冲突
    /// </summary>
    public static class GlobalEvents
    {
        /// <summary>
        /// 玩家相关事件 (1000-1999)
        /// </summary>
        public enum Player
        {
            HealthChanged = 1000,
            MaxHealthChanged = 1001,
            ArmorChanged = 1002,
            KillPointsChanged = 1003,
            DataPointsChanged = 1004,
            PositionChanged = 1005
        }

        /// <summary>
        /// 输入相关事件 (2000-2999)
        /// </summary>
        public enum Input
        {
            MoveInput = 2000,
            ViewInput = 2001,
            FireInput = 2002,
            JumpInput = 2003,
            DashInput = 2004,
            ReloadInput = 2005,
            UpgradeInput = 2006
        }

        /// <summary>
        /// 武器相关事件 (3000-3999)
        /// </summary>
        public enum Weapon
        {
            UnlockLevel = 3000,
            MagazineInfoChange = 3001,
            LevelChanged = 3002
        }

        /// <summary>
        /// 敌人相关事件 (4000-4999)
        /// </summary>
        public enum Enemy
        {
            Death = 4000,
            Spawned = 4001
        }

        /// <summary>
        /// 系统相关事件 (5000-5999)
        /// </summary>
        public enum System
        {
            Pause = 5000,
            Resume = 5001,
            GameStart = 5002,
            GameOver = 5003
        }
    }
}
