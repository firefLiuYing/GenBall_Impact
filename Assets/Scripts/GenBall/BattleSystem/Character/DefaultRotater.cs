using GenBall.BattleSystem.Command;
using UnityEngine;

namespace GenBall.BattleSystem.Character
{
    public class DefaultRotater : MonoBehaviour,IRotate
    {
        public void Rotate(RotateCommand command)
        {
            var rotationEulerAngles = transform.rotation.eulerAngles;
            // unity 沟槽欧拉角小巧思处理
            if (rotationEulerAngles.x is > 180 and < 360)
            {
                rotationEulerAngles.x -= 360;
            }
            rotationEulerAngles.y += command.HorizontalAngle;
            rotationEulerAngles.x += command.VerticalAngle;
            rotationEulerAngles.x=Mathf.Clamp(rotationEulerAngles.x, -80, 80f);
            transform.rotation=Quaternion.Euler(rotationEulerAngles);;
        }
    }
}