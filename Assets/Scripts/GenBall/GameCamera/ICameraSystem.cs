using UnityEngine;
using Yueyn.Main;

namespace GenBall.GameCamera
{
    /// <summary>
    /// Global camera authority. One canonical MainCamera renders the world;
    /// its position/rotation follow the active target (Player camera or override).
    ///
    /// The Player's first-person weapon camera is a separate rig that renders
    /// weapon/hand layers only — Unity manages it via culling mask, not this system.
    ///
    /// Registered in FrameworkDefault. Access via
    /// SystemRepository.Instance.GetSystem&lt;ICameraSystem&gt;().
    /// </summary>
    public interface ICameraSystem : ISystem
    {
        /// <summary>Canonical world camera. Replaces Camera.main lookups.</summary>
        Camera MainCamera { get; }

        /// <summary>Target FOV for the world camera. Smoothed in FrameUpdate.</summary>
        float FOV { get; set; }

        /// <summary>True while a shake effect is active.</summary>
        bool IsShaking { get; }

        /// <summary>Trigger a screen shake.</summary>
        void Shake(float intensity, float duration);

        /// <summary>
        /// Register the Player's camera transform as the normal follow target.
        /// Called by PlayerEntityFactory after assembly.
        /// </summary>
        void RegisterPlayerCamera(Transform playerCameraTransform);

        /// <summary>
        /// Override the follow target (cutscene, death cam).
        /// MainCamera detaches from Player and follows this target instead.
        /// </summary>
        void SetOverrideTarget(Transform target);

        /// <summary>Restore follow target to the Player camera.</summary>
        void ClearOverrideTarget();
    }
}
