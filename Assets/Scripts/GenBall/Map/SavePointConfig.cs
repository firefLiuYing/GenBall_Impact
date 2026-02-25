using GenBall.Utils.Trigger;
using UnityEngine;

namespace GenBall.Map
{
    public class SavePointConfig : MonoBehaviour
    {
        [SerializeField] private Transform playerSpawnPoint; 
        [SerializeField,HideInInspector] private int index;
        [SerializeField] private string displayName;
        public Transform PlayerSpawnPoint => playerSpawnPoint ?? transform;
        public string DisplayName => displayName;

        public int Index
        {
            get => index;
            set => index = value;
        }

    }
}