using UnityEngine;

namespace Yueyn.Utils
{
    /// <summary>
    /// Mono单例基类，用法为：class MySingleton:MonoSingleton&lt;MySingleton&gt;
    /// 该类型单例会在Awake时创建单例并初始化，如果已经存在单例，则会销毁当前对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                // 如果_instance为空，就输出一个警告，并返回null
                if (_instance != null) return _instance;
                Debug.LogWarning($"{typeof(T)} is not initialized");
                return null;
            }
        }
        // 在Awake时创建单例并初始化
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                Init();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        // 在Destroy时销毁单例
        protected virtual void OnDestroy()
        {
            if(_instance==this) _instance = null;
        }
        protected abstract void Init();
    }
}