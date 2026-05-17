using UnityEngine;
using Yueyn.Event;
using Yueyn.UI;

namespace Yueyn.Main
{
    /// <summary>
    /// 框架基类
    /// </summary>
    public class FrameworkBase : MonoBehaviour
    {
        protected SystemRepository SystemRep;

        protected CEventRouter EventRouter;
        protected UIManager UIManager;
        private void Awake()
        {
            SystemRep = SystemRepository.Instance;
            DontDestroyOnLoad(this);
            
            EventRouter=CEventRouter.Instance;
            #if UNITY_EDITOR
            Yueyn.Resource.CResourceManager.Instance.SetHelper(new Yueyn.Resource.ResourceHelperEditor());
            #else
            Yueyn.Resource.CResourceManager.Instance.SetHelper(new Yueyn.Resource.ResourceHelperAssetBundle());
            #endif

            // Yueyn.Pool.CPoolManager.Instance.SetHelper(new Yueyn.Pool.PoolHelperDefault());
            UIManager=UIManager.Instance;
            DoInit();
        }

        private void Start()
        {
            DoStart();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            DoFrameUpdate();
            SystemUpdaterManager.Instance.FrameUpdate(deltaTime);

            // 更新对象池
            Yueyn.Pool.CPoolManager.Instance.Update(deltaTime, Time.realtimeSinceStartup);
        }

        private void FixedUpdate()
        {
            float deltaTime = Time.fixedDeltaTime;
            DoLogicUpdate();
            SystemUpdaterManager.Instance.LogicUpdate(deltaTime);
        }
        
        private void LateUpdate()
        {
            float deltaTime = Time.deltaTime;
            DoLateFrameUpdate();
            SystemUpdaterManager.Instance.LateFrameUpdate(deltaTime);
        }

        protected virtual void DoInit() { }
        protected virtual void DoStart() { }
        protected virtual void DoFrameUpdate() { }
        protected virtual void DoLogicUpdate() { }
        protected virtual void DoLateFrameUpdate() { }
    }
}