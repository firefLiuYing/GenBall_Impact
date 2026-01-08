using System;
using GenBall.Map;
using UnityEditor;
using UnityEngine.UIElements;

namespace GenBall.Utils.Editor.Map
{
    public class MapBlockGraphWindow : EditorWindow
    {
        private MapConfig _mapConfig;
        public static void ShowWindow(MapConfig config)
        {
            var window = GetWindow<MapBlockGraphWindow>("µØÍ¼½Úµã±à¼­Æ÷");
            // window._mapConfig = config;
            window.ConstructMapBlockGraphView(config);
        }


        private MapBlockGraphView _graphView;
        private void ConstructMapBlockGraphView(MapConfig mapConfig)
        {
            _mapConfig = mapConfig;
            _graphView = new MapBlockGraphView(mapConfig);
            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }
    }
}