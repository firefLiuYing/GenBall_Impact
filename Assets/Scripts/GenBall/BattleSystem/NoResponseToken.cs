using System.Runtime.InteropServices;
using Yueyn.Base.ReferencePool;

namespace GenBall.BattleSystem
{
    /// <summary>
    /// 在交互中表示没有反应，占位符
    /// </summary>
    ///
    [StructLayout(LayoutKind.Auto)]
    public struct NoResponseToken :IInteractToken
    {
        public static NoResponseToken Create(IInteractable source)
        {
            var token =new NoResponseToken
            {
                Source = source
            };
            return token;
        }

        public IInteractable Source { get; private set; }
    }
}