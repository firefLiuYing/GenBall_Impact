using GenBall.Map;
using UnityEditor;
using UnityEngine;

namespace GenBall.Utils.Editor.Map
{
    [CustomEditor(typeof(MapConfig))]
    public class MapConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            if (GUILayout.Button("打开地图节点编辑器"))
            {
                var targetMapConfig = (MapConfig)target;
                if (targetMapConfig != null)
                {
                    MapBlockGraphWindow.ShowWindow(targetMapConfig);
                }
            }
        }
    }
}