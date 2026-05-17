namespace Yueyn.Main
{
    /// <summary>
    /// 系统作用域，用于区分系统是否受暂停影响
    /// </summary>
    public enum SystemScope
    {
        /// <summary>
        /// 游戏逻辑和表现，暂停后不再更新
        /// </summary>
        Game,
        
        /// <summary>
        /// 系统逻辑，不受暂停影响（如UI、输入等）
        /// </summary>
        Framework
    }
}
