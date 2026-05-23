using System;
using System.Collections.Generic;
using UnityEngine;

namespace GenBall.Utils.CodeGenerator.UI
{
    /// <summary>
    /// Prefix-to-component binding configuration.
    /// Shared between Python CLI and Unity Editor.
    /// Create via: Assets > Create > UI > Binding Configuration
    /// </summary>
    [CreateAssetMenu(fileName = "UiBindingConfig", menuName = "UI/Binding Configuration")]
    public class UiBindingConfig : ScriptableObject
    {
        public List<PrefixMapping> prefixMappings = new List<PrefixMapping>();

        [Header("Generation Settings — Form")]
        public string viewBaseClass = "Yueyn.UI.UIBusinessFormBase";
        public string logicBaseClass = "Yueyn.UI.BusinessFormLogic";

        [Header("Generation Settings — Part")]
        [Tooltip("Base class for Part View. Default: MonoBehaviour.")]
        public string partViewBaseClass = "UnityEngine.MonoBehaviour";
        [Tooltip("Base class for Part Logic. Leave empty if Part doesn't need Logic.")]
        public string partLogicBaseClass = "";

        [Header("Output")]
        public string defaultNamespace = "GenBall.UI";
        public string outputBasePath = "Assets/Scripts/GenBall/UI";

        /// <summary>
        /// Find the matching PrefixMapping for a GameObject name.
        /// Uses longest-prefix-match.
        /// </summary>
        public PrefixMapping Match(string gameObjectName)
        {
            PrefixMapping best = null;
            int bestLen = -1;

            foreach (var mapping in prefixMappings)
            {
                if (gameObjectName.StartsWith(mapping.prefix, StringComparison.Ordinal))
                {
                    if (mapping.prefix.Length > bestLen)
                    {
                        bestLen = mapping.prefix.Length;
                        best = mapping;
                    }
                }
            }

            return best;
        }

        public string ExportToJson()
        {
            // Simple manual JSON serialization to avoid Newtonsoft dependency
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("  \"version\": \"2.0\",");
            sb.AppendLine("  \"prefixMappings\": [");
            for (int i = 0; i < prefixMappings.Count; i++)
            {
                var m = prefixMappings[i];
                sb.AppendLine("    {");
                sb.AppendLine($"      \"prefix\": \"{m.prefix}\",");
                sb.AppendLine($"      \"componentType\": \"{m.componentType}\",");
                sb.AppendLine($"      \"fullName\": \"{m.fullName}\",");
                sb.AppendLine($"      \"usingNamespace\": \"{m.usingNamespace}\",");
                sb.AppendLine($"      \"category\": \"{m.category}\",");
                sb.AppendLine($"      \"priority\": {m.priority}");
                sb.Append("    }");
                if (i < prefixMappings.Count - 1) sb.Append(",");
                sb.AppendLine();
            }
            sb.AppendLine("  ],");
            sb.AppendLine("  \"generationSettings\": {");
            sb.AppendLine($"    \"viewBaseClass\": \"{viewBaseClass}\",");
            sb.AppendLine($"    \"logicBaseClass\": \"{logicBaseClass}\",");
            sb.AppendLine($"    \"defaultNamespace\": \"{defaultNamespace}\",");
            sb.AppendLine($"    \"outputBasePath\": \"{outputBasePath}\"");
            sb.AppendLine("  }");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }

    [Serializable]
    public class PrefixMapping
    {
        [Tooltip("Prefix to match against GameObject names. E.g. 'Btn' matches 'BtnStart'.")]
        public string prefix;

        [Tooltip("Short C# type name. E.g. 'Button', 'Text'.")]
        public string componentType;

        [Tooltip("Fully qualified C# type. E.g. 'UnityEngine.UI.Button'.")]
        public string fullName;

        [Tooltip("Namespace for the 'using' directive. E.g. 'UnityEngine.UI'.")]
        public string usingNamespace;

        [Tooltip("Category for UI display purposes.")]
        public string category;

        [Tooltip("Priority for conflict resolution (lower = higher priority).")]
        public int priority;
    }
}
