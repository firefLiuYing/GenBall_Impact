using UnityEngine;

namespace GenBall.Map
{
    /// <summary>
    /// Implemented by MonoBehaviours that represent editor-placeable scene entities.
    /// The scene editor discovers all implementors automatically via reflection.
    /// </summary>
    public interface IScenePlaceable
    {
        /// <summary>Unique ID within the category for this scene. Set by the baking pipeline.</summary>
        int Id { get; set; }

        /// <summary>Display label shown in the editor tree and Scene View gizmo.</summary>
        string DisplayLabel { get; }

        /// <summary>Category key matching the PlaceableCategory attribute value.</summary>
        string Category { get; }

        /// <summary>Transform representing the placement position and rotation.</summary>
        Transform Anchor { get; }

        /// <summary>
        /// If true: baked to config and GameObject disabled (SetActive(false)).
        /// For dynamically spawned entities like enemies.
        /// If false: stays active in scene. For static objects like save points.
        /// </summary>
        bool IsDynamic { get; }

        /// <summary>Produces the serializable config data object for baking.</summary>
        object BakeToConfigData();

        /// <summary>Validates the configuration. Returns null if valid, error message otherwise.</summary>
        string Validate();
    }
}
