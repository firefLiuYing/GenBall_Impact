using UnityEngine;

namespace Yueyn.Main
{
    /// <summary>
    /// 入口基类，封装一些初始化逻辑，全局只需要一个入口
    /// </summary>
    public class FrameworkBase:MonoBehaviour
    {
        protected SystemRepository SystemRep;
        private void Awake()
        {
            SystemRep = SystemRepository.Instance;
            DontDestroyOnLoad(this);
            DoInit();
        }

        private void Start()
        {
            DoStart();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            SystemRep.RenderUpdate(deltaTime);
        }

        private void FixedUpdate()
        {
            float deltaTime = Time.fixedDeltaTime;
            SystemRep.LogicUpdate(deltaTime);
        }

        protected virtual void DoInit(){}
        protected virtual void DoStart(){}
    }
}