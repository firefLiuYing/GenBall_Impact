using UnityEngine;

namespace GenBall.BattleSystem.Navigation
{
    public interface INavigator
    {
        public Vector3 CalculateVelocity(Vector3 desiredVelocity, Vector3 currentPosition);
        public bool HasValidPath { get; }
    }
}
