using UnityEngine;

namespace Yueyn.Utils
{
    /// <summary>
    /// 协程运行器（框架工具类）
    /// 用于在非 MonoBehaviour 类中运行协程
    /// </summary>
    public class CoroutineRunner : MonoSingleton<CoroutineRunner>
    {
        protected override void Init()
        {
            Debug.Log("[CoroutineRunner] Initialized");
        }
    }
}
