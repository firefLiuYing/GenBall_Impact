using GenBall.BattleSystem.Command;
using UnityEngine;

namespace GenBall.Player
{
    public class PlayerRotater : MonoBehaviour, IRotate
    {
        [SerializeField] private Transform mainCamera;
        public void Rotate(RotateCommand command)
        {
            // 模型绕垂直中线转
            // 相机绕水平中线转
            var playerRotationEuler=transform.rotation.eulerAngles;
            var cameraRotationEuler = mainCamera.localRotation.eulerAngles;

            // Unity 沟槽的欧拉角小巧思处理
            if (cameraRotationEuler.x is > 180 and < 360)
            {
                cameraRotationEuler.x -= 360;
            }

            playerRotationEuler.y += command.HorizontalAngle;
            cameraRotationEuler.x+=command.VerticalAngle;
            cameraRotationEuler.x=Mathf.Clamp(cameraRotationEuler.x, -80, 80f);
            
            transform.rotation=Quaternion.Euler(playerRotationEuler);
            mainCamera.localRotation=Quaternion.Euler(cameraRotationEuler);
        }
    }
}