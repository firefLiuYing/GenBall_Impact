using UnityEngine;

namespace GenBall.BattleSystem.Navigation
{
    public class DirectNavigator : MonoBehaviour, INavigator
    {
        public Vector3 CalculateVelocity(Vector3 desiredVelocity, Vector3 currentPosition)
            => desiredVelocity;

        public bool HasValidPath => true;
    }
}
