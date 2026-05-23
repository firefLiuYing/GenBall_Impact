using Yueyn.Main;

namespace GenBall.Framework.Config
{
    /// <summary>
    /// 统一配置提供接口，所有系统配置通过此接口获取
    /// </summary>
    public interface IConfigProvider : ISystem
    {
        T GetConfig<T>() where T : class;
    }
}
