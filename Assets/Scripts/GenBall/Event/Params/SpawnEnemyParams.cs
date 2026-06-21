using UnityEngine;
using Yueyn.Event;

namespace GenBall.Event.Params
{
    [System.Serializable]
    [EventParamHint(6001)]
    public class SpawnEnemyParams : EventParameterBase
    {
        public string enemyType = "NormalOrbis";

        /// <summary>World position to spawn at. Populated at bake time from TriggerVolume.spawnPoint.</summary>
        public Vector3 spawnPosition;

        /// <summary>World rotation to spawn with. Populated at bake time from TriggerVolume.spawnPoint.</summary>
        public Quaternion spawnRotation = Quaternion.identity;

        public float patrolRadius = 5f;
        public float detectRadius = 10f;
        public int aiBehavior;

        public override void Dispatch(int eventId)
            => CEventRouter.Instance.FireNow(eventId, this);
    }
}
