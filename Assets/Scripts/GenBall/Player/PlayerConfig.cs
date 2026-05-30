using UnityEngine;

namespace GenBall.Player
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "GenBall/PlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Movement")]
        public float speed = 5f;
        public float verticalSensitivity = 0.1f;
        public float horizontalSensitivity = 0.1f;

        [Header("Jump")]
        public float shortPressJumpHeight = 2f;
        public float longPressJumpMaxHeight = 4f;
        public float longPressMaxTime = 1f;
        public float shortPressJustifyTime = 0.25f;
        public float gravityAcceleration = 9.8f;
        public float maxDropVelocity = 20f;
        public float coyoteTime = 0.1f;
        public float jumpInputBufferTime = 0.1f;

        [Header("Dash")]
        public float invincibleTime = 0.15f;
        public float endingTime = 0.1f;
        public float dashSpeed = 10f;
        public float dashCountdownTime = 0.5f;

        [Header("Interact")]
        public float sightDetectRadius = 0.5f;
        public float sightDetectDistance = 5f;
        public LayerMask interactableLayer;
    }
}

