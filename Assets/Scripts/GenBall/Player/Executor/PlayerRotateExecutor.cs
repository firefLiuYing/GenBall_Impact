using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Mover;
using UnityEngine;

namespace GenBall.Player.Executor
{
    /// <summary>
    /// Execute layer: applies rotation from RotateCommand.
    /// Splits rotation to match FPS convention:
    /// - Horizontal (Y axis) rotates the Player body via RigidbodyMover (pause-safe).
    /// - Vertical (X axis) tilts only the camera arm (MainCameraTransform).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerRotateExecutor : MonoBehaviour, IRotate
    {
        [SerializeField, Tooltip("The camera arm transform (MainCameraTransform). Only this tilts vertically.")]
        private Transform cameraTransform;

        private Rigidbody _rigidbody;
        private RigidbodyMover _mover;
        private float _horizontalSensitivity;
        private float _verticalSensitivity;
        private float _verticalAngle;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _mover = GetComponent<RigidbodyMover>();
            if (cameraTransform == null)
                cameraTransform = transform.Find("MainCameraTransform");
        }

        public void Init(float horizontalSensitivity, float verticalSensitivity)
        {
            _horizontalSensitivity = horizontalSensitivity;
            _verticalSensitivity = verticalSensitivity;
        }

        public void Rotate(RotateCommand command)
        {
            // Horizontal — rotate body around world Y via RigidbodyMover (pause-safe)
            float yaw = command.HorizontalAngle * _horizontalSensitivity;
            if (_mover != null)
            {
                _mover.SetRotation(_rigidbody.rotation * Quaternion.Euler(0f, yaw, 0f));
            }
            else if (_rigidbody != null && !_rigidbody.isKinematic)
            {
                _rigidbody.MoveRotation(_rigidbody.rotation * Quaternion.Euler(0f, yaw, 0f));
            }
            else
            {
                transform.Rotate(0f, yaw, 0f, Space.World);
            }

            // Vertical — tilt camera arm only, clamped, negated for standard FPS
            _verticalAngle -= command.VerticalAngle * _verticalSensitivity;
            _verticalAngle = Mathf.Clamp(_verticalAngle, -80f, 80f);

            if (cameraTransform != null)
                cameraTransform.localRotation = Quaternion.Euler(_verticalAngle, 0f, 0f);
        }
    }
}
