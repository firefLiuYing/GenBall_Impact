namespace GenBall.Procedure
{
    /// <summary>
    /// 存档数据提供者接口。
    /// 每个需要持久化的子系统实现此接口，独立管理自身数据的序列化和反序列化。
    /// GameData 使用键值对模型，DataKey 作为唯一键，CollectSaveData/ApplySaveData 处理 JSON 序列化。
    /// 部署在 GenBall.Procedure 业务层，后续迁移至 Yueyn 框架层。
    /// </summary>
    public interface ISaveDataProvider
    {
        /// <summary>在 GameData.dataBlocks 中的唯一标识键</summary>
        string DataKey { get; }

        /// <summary>收集当前运行时状态并序列化为 JSON 字符串</summary>
        string CollectSaveData();

        /// <summary>从 JSON 字符串反序列化并应用到当前运行时状态</summary>
        void ApplySaveData(string json);
    }
}
