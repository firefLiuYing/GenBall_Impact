using UnityEngine;
using Yueyn.Main;

namespace GenBall.GameCamera
{
    /// <summary>
    /// Virtual-follow model: the scene's MainCamera stays as-is and tracks
    /// the active target each frame.
    ///
    /// - Normal: MainCamera follows Player's camera transform (position + rotation).
    /// - Override: MainCamera follows a cutscene/death cam transform instead.
    /// - Weapon camera: separate rig on Player, renders weapon layer independently.
    /// </summary>
    public class CameraSystemDefault : ICameraSystem, ILateFrameUpdate
    {
        private const float DefaultFOV = 60f;
        private const float FOVLerpSpeed = 8f;

        // Shake state
        private float _shakeIntensity;
        private float _shakeEndTime;
        private float _shakeSeedX;
        private float _shakeSeedY;
        private const float ShakeFrequency = 30f;

        // Follow state
        private Transform _playerCamera;
        private Transform _overrideTarget;

        // FOV
        private float _targetFov;

        public Camera MainCamera { get; private set; }

        public float FOV
        {
            get => _targetFov;
            set => _targetFov = Mathf.Clamp(value, 30f, 120f);
        }

        public bool IsShaking => Time.time < _shakeEndTime;

        public SystemScope LateFrameUpdateScope => SystemScope.Game;

        private Transform ActiveTarget => _overrideTarget != null ? _overrideTarget : _playerCamera;

        public void Init()
        {
            MainCamera = Camera.main;
            if (MainCamera == null)
            {
                MainCamera = Object.FindAnyObjectByType<Camera>();
                if (MainCamera != null)
                    Debug.LogWarning("[CameraSystem] Camera.main is null, using fallback: " + MainCamera.name);
            }
            if (MainCamera == null)
                Debug.LogError("[CameraSystem] No Camera found in scene! Follow will not work.");

            _targetFov = MainCamera != null ? MainCamera.fieldOfView : DefaultFOV;
        }

        public void UnInit()
        {
            ClearOverrideTarget();
            MainCamera = null;
            _playerCamera = null;
        }

        /// <summary>
        /// Store the Player's camera transform as the default follow target.
        /// Does NOT reparent MainCamera — MainCamera stays independent and follows each frame.
        /// </summary>
        public void RegisterPlayerCamera(Transform cameraTransform)
        {
            _playerCamera = cameraTransform;
            Debug.Log($"[CameraSystem] Player camera registered: {cameraTransform.name}, instance={GetHashCode()}");
        }

        public void Shake(float intensity, float duration)
        {
            _shakeIntensity = intensity;
            _shakeEndTime = Time.time + duration;
            _shakeSeedX = Random.value * 100f;
            _shakeSeedY = Random.value * 100f;
        }

        public void SetOverrideTarget(Transform target)
        {
            _overrideTarget = target;
        }

        public void ClearOverrideTarget()
        {
            _overrideTarget = null;
        }

        public void LateFrameUpdate(float deltaTime)
        {
            // Re-acquire after scene changes (old MainCamera gets destroyed)
            if (MainCamera == null)
                MainCamera = Camera.main;

            if (MainCamera == null)
                return;

            var target = ActiveTarget;
            if (target != null)
            {
                MainCamera.transform.position = target.position;
                MainCamera.transform.rotation = target.rotation;
            }

            // FOV smoothing
            if (!Mathf.Approximately(MainCamera.fieldOfView, _targetFov))
            {
                MainCamera.fieldOfView = Mathf.Lerp(
                    MainCamera.fieldOfView, _targetFov, FOVLerpSpeed * deltaTime);
            }

            // Screen shake
            if (IsShaking)
            {
                float remaining = _shakeEndTime - Time.time;
                float totalDuration = remaining + deltaTime;
                float decay = remaining > 0f ? remaining / totalDuration : 0f;
                float currentIntensity = _shakeIntensity * Mathf.Clamp01(decay);

                float offsetX = (Mathf.PerlinNoise(Time.time * ShakeFrequency, _shakeSeedX) - 0.5f) * 2f * currentIntensity;
                float offsetY = (Mathf.PerlinNoise(Time.time * ShakeFrequency, _shakeSeedY) - 0.5f) * 2f * currentIntensity;

                MainCamera.transform.localPosition += new Vector3(offsetX, offsetY, 0f);
            }
        }
    }
}
