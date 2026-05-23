using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GenBall.Utils.CodeGenerator.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GenBall.Utils.CodeGenerator.UI.Editor
{
    /// <summary>
    /// Editor-only prefab scanner that traverses all child GameObjects
    /// and matches them against the UiBindingConfig prefix mappings.
    /// </summary>
    public static class UiPrefabScanner
    {
        [Serializable]
        public class ScanResult
        {
            public string formName;
            public string prefabPath;
            public string prefabFullPath;
            public List<BindingInfo> bindings = new List<BindingInfo>();
            public List<string> warnings = new List<string>();
        }

        [Serializable]
        public class BindingInfo
        {
            public string gameObjectName;
            public string componentType;
            public string fullTypeName;
            public string propertyName;
            public string childPath;
            public bool included = true; // for UI checkbox toggling
            public Component targetComponent;
        }

        /// <summary>
        /// Scan a loaded prefab for UI bindings.
        /// </summary>
        /// <param name="prefab">The loaded prefab GameObject (from AssetDatabase.LoadAssetAtPath).</param>
        /// <param name="config">The binding configuration.</param>
        /// <param name="prefabPath">Project-relative path to the prefab asset.</param>
        public static ScanResult Scan(GameObject prefab, UiBindingConfig config, string prefabPath)
        {
            var result = new ScanResult
            {
                prefabPath = prefabPath,
                prefabFullPath = Path.GetFullPath(prefabPath),
                formName = Path.GetFileNameWithoutExtension(prefabPath)
            };

            if (prefab == null)
            {
                result.warnings.Add("Prefab is null.");
                return result;
            }

            if (config == null || config.prefixMappings.Count == 0)
            {
                result.warnings.Add("UiBindingConfig is null or has no prefix mappings.");
                return result;
            }

            var allTransforms = prefab.GetComponentsInChildren<Transform>(includeInactive: true);

            // -- Part boundary: find child UiViewBinding roots and skip their subtrees --
            var partSubtrees = new HashSet<Transform>();
            foreach (var t in allTransforms)
            {
                if (t == prefab.transform) continue;
                var go = t.gameObject;
                if (go.TryGetComponent<UiViewBinding>(out _))
                {
                    foreach (var sub in go.GetComponentsInChildren<Transform>(includeInactive: true))
                        partSubtrees.Add(sub);
                }
            }

            var dedupNames = new Dictionary<string, int>();

            foreach (var t in allTransforms)
            {
                // Skip the root GameObject itself
                if (t == prefab.transform)
                    continue;

                // Skip GameObjects belonging to a Part
                if (partSubtrees.Contains(t))
                    continue;

                var go = t.gameObject;
                var mapping = config.Match(go.name);
                if (mapping == null)
                    continue;

                // Verify the expected component exists
                var compType = FindComponentType(mapping.fullName);
                if (compType == null)
                {
                    result.warnings.Add(
                        $"Unknown component type '{mapping.fullName}' for prefix '{mapping.prefix}'.");
                    continue;
                }

                var comp = go.GetComponent(compType);
                if (comp == null)
                {
                    result.warnings.Add(
                        $"'{go.name}' matches prefix '{mapping.prefix}' but has no {mapping.componentType} component.");
                    continue;
                }

                // Derive property name
                var propName = SanitizePropertyName(go.name);

                // Handle duplicates
                if (dedupNames.ContainsKey(propName))
                {
                    dedupNames[propName]++;
                    propName = $"{propName}_{dedupNames[propName]}";
                }
                else
                {
                    dedupNames[propName] = 1;
                }

                // Build child path
                var childPath = AnimationUtility.CalculateTransformPath(t, prefab.transform);

                var info = new BindingInfo
                {
                    gameObjectName = go.name,
                    componentType = mapping.componentType,
                    fullTypeName = mapping.fullName,
                    propertyName = propName,
                    childPath = childPath,
                    targetComponent = comp,
                    included = true
                };
                result.bindings.Add(info);
            }

            // Check for non-prefixed GameObjects with UI components
            var allBindableTypes = new HashSet<string>();
            foreach (var m in config.prefixMappings)
                allBindableTypes.Add(m.fullName);

            foreach (var t in allTransforms)
            {
                if (t == prefab.transform) continue;
                var go = t.gameObject;

                // Skip already-matched
                if (config.Match(go.name) != null) continue;

                // Check for UI components
                var uiComps = new List<string>();
                foreach (var typeName in allBindableTypes)
                {
                    var type = FindComponentType(typeName);
                    if (type != null && go.GetComponent(type) != null)
                        uiComps.Add(type.Name);
                }

                if (uiComps.Count > 0)
                {
                    // Skip Unity default names
                    var name = go.name;
                    if (name.StartsWith("Text") || name.StartsWith("Image") ||
                        name.StartsWith("Button") || name.StartsWith("RawImage") ||
                        name.StartsWith("GameObject") || name.StartsWith("Canvas"))
                        continue;

                    result.warnings.Add(
                        $"'{name}' has UI components ({string.Join(", ", uiComps)}) but no binding prefix.");
                }
            }

            return result;
        }

        private static Type FindComponentType(string fullName)
        {
            // Try direct type lookup first
            var type = Type.GetType(fullName + ", UnityEngine.UI");
            if (type != null) return type;

            type = Type.GetType(fullName + ", UnityEngine");
            if (type != null) return type;

            // Fallback: search all loaded assemblies
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(fullName);
                if (type != null) return type;
            }

            return null;
        }

        private static string SanitizePropertyName(string name)
        {
            // Replace invalid C# identifier characters with underscore
            var chars = name.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]) && chars[i] != '_')
                    chars[i] = '_';
            }

            var result = new string(chars);
            // Ensure it doesn't start with a digit
            if (result.Length > 0 && char.IsDigit(result[0]))
                result = "_" + result;

            return result;
        }
    }
}
