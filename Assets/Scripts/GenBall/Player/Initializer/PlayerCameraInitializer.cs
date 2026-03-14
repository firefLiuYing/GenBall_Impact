using System;
using GenBall.BattleSystem.Character;
using UnityEngine;

namespace GenBall.Player.Initializer
{
    public class PlayerCameraInitializer : CharacterInitializerBase
    {
        [SerializeField] private Transform mainCameraTransform;
        private void SetCamera()
        {
            if (mainCameraTransform == null)
            {
                throw new Exception("Main Camera Transform √ª≈‰÷√");
            }
            var camTrans=Camera.main.transform;
            camTrans.SetParent(mainCameraTransform,false);
            camTrans.localPosition = Vector3.zero;
            camTrans.localRotation = Quaternion.identity;
        }
        public override void Initialize(CharacterState characterState)
        {
            SetCamera();
        }
    }
}