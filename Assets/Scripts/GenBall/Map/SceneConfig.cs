using UnityEngine;

namespace GenBall.Map
{
    /// <summary>
    /// Optional per-scene metadata component.
    /// Place on a GameObject in the scene to set a custom display name for baking.
    /// </summary>
    public class SceneConfig : MonoBehaviour
    {
        [SerializeField] private string displayName = "Scene Name";
        public string DisplayName => displayName;
    }
}