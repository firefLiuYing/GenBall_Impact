namespace Yueyn.Utils
{
    /// <summary>
    /// 单例基类，用法为：class MySingleton:Singleton&lt;MySingleton&gt;
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T> where T :Singleton<T>, new()
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                // 线程安全的动态创建单例
                if (_instance == null)
                {
                    lock (typeof(T))
                    {
                        if (_instance == null)
                        {
                            _instance = new T();
                            _instance.Init();
                        }
                    }
                }
                return _instance;
            }
        }
        protected abstract void Init();
    }
}