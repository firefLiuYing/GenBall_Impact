using UnityEngine;

namespace GenBall.Map
{
    public abstract class MapBlockBase : MonoBehaviour, IMapBlock
    {
        public void SetIndex(int index)
        {
            gameObject.SetActive(true);
        }
    }
}