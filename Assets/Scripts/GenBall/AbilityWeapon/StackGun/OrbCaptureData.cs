using UnityEngine;

namespace GenBall.AbilityWeapon.StackGun
{
    /// <summary>
    /// Data for an Orbis absorbed into the StackGun magazine.
    /// The Orbis is captured alive (GameObject disabled, HP preserved).
    /// </summary>
    public struct OrbCaptureData
    {
        public GameObject OrbGameObject;
        public int RemainingHp;
    }
}
