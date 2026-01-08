using GenBall.Utils.EntityCreator;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.Map
{
    public class MapModule : MonoBehaviour,IComponent
    {
        [SerializeField] private Transform mapRoot;
        private EntityCreator<IMapBlock> MapBlockCreator => GameEntry.GetModule<EntityCreator<IMapBlock>>();
        // [SerializeField][Tooltip("如果设定为n，就是指以player为中心的n*n方块")] private int preloadRadius;
        [SerializeField,Header("地图配置")] private MapConfig mapConfig;
        public void OnRegister()
        {
            if (mapRoot == null)
            {
                mapRoot = transform;
            }
        }

        
        public void OnUnregister()
        {
            
        }

        public void ComponentUpdate(float elapsedSeconds, float realElapseSeconds)
        {
            
        }

        public void ComponentFixedUpdate(float fixedDeltaTime)
        {
            
        }

        public void Shutdown()
        {
            
        }
    }
}