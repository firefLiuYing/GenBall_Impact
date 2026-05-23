using UnityEngine;
using Yueyn.Main;

namespace GenBall.Player
{
    public interface IPlayerSystem : ISystem
    {
        GameObject Player { get; }
        void CreatePlayer();
        void CreatePlayer(Vector3 position, Quaternion rotation);
    }
}
