using UnityEngine;

namespace GenBall.Map
{
    /// <summary>
    /// Marks an int field as a save point reference.
    /// Inspector shows a dropdown to select from baked save points.
    /// </summary>
    public class SavePointReferenceAttribute : PropertyAttribute
    {
        /// <summary>
        /// If true, shows save points from ALL scenes (dropdown only, no highlight).
        /// If false (default), shows only current scene's save points + PingObject support.
        /// </summary>
        public bool CrossScene;

        public SavePointReferenceAttribute() { }

        public SavePointReferenceAttribute(bool crossScene)
        {
            CrossScene = crossScene;
        }
    }
}
