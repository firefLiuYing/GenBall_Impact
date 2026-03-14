using GenBall.BattleSystem.Command;
using UnityEngine;

namespace GenBall.BattleSystem.Character
{
    public class DefaultRotater : MonoBehaviour,IRotate
    {
        public void Rotate(RotateCommand command)
        {
            transform.rotation=command.Rotation;
        }
    }
}