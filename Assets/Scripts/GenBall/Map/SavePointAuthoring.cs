using GenBall.Procedure;
using UnityEngine;

namespace GenBall.Map
{
    [RequireComponent(typeof(SavePoint))]
    public class SavePointAuthoring:MonoBehaviour
    {
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private int index;
        [SerializeField] private string savePointName;

        public int SavePointIndex
        {
            get=> index;
            set => index = value;
        }

        public Transform PlayerSpawnPoint => playerSpawnPoint ?? transform;
        
        public string SavePointName => string.IsNullOrEmpty(savePointName)?$"SavePoint_{index}":savePointName;
    }
}