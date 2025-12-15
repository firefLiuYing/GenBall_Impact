using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GenBall.Utils.Attributes.InspectorButton
{
    [CustomEditor(typeof(object),true,isFallback = true)]
    [CanEditMultipleObjects]
    public class InspectorButtonEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            foreach (var targetObject in targets)
            {
                DrawButton(targetObject);
            }
        }

        private void DrawButton(object targetObject)
        {
            if(targetObject == null) return;
            
            var methods=targetObject.GetType().GetMethods(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<InspectorButtonAttribute>();
                if (attr != null)
                {
                    if (GUILayout.Button(attr.Name ?? method.Name))
                    {
                        method.Invoke(targetObject, null);

                        // 标记脏，以便保存
                        if (targetObject is ScriptableObject so)
                            EditorUtility.SetDirty(so);
                        else if (targetObject is MonoBehaviour mb)
                            EditorUtility.SetDirty(mb);
                    }
                }
            }
        }
    }
}