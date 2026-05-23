using System;
using System.Collections.Generic;
using UnityEngine;

namespace GenBall.Utils.CodeGenerator.UI
{
    /// <summary>
    /// Binding component placed on the prefab root (alongside UIFormScript).
    /// Scans child GameObjects for UI controls matching prefix conventions
    /// and generates the paired View.cs + Logic.cs files.
    ///
    /// Workflow: Attach to prefab → Configure settings → Scan → Generate.
    /// </summary>
    [AddComponentMenu("UI/UiViewBinding")]
    [DisallowMultipleComponent]
    public class UiViewBinding : MonoBehaviour
    {
        [Header("Config")]
        [Tooltip("Binding prefix configuration. Auto-detected from project if left empty.")]
        public UiBindingConfig bindingConfig;

        [Header("Type")]
        [Tooltip("Form = top-level UI page; Part = reusable sub-component (like hearts, skill icons).")]
        public ViewType viewType = ViewType.Form;

        [Header("Settings")]
        [Tooltip("Form name (without suffix). Defaults to prefab name.")]
        public string formName = "";

        [Tooltip("UI form type (ignored when ViewType = Part).")]
        public FormTypeEnum formType = FormTypeEnum.Popup;

        [Tooltip("C# namespace for generated code.")]
        public string namespaceName = "GenBall.UI";

        [Header("Output")]
        [Tooltip("Output directory for generated files. Leave empty for default path.")]
        public string outputPath = "";

        [Tooltip("Type of code to generate.")]
        public GenerateTarget generateTarget = GenerateTarget.Both;

        [Header("Scan Results")]
        [SerializeField]
        public List<BindingEntry> bindings = new List<BindingEntry>();

        // -- Accessors --
        public List<BindingEntry> GetBindings() => bindings;

        public Component GetBinding(string propertyName)
        {
            foreach (var e in bindings)
                if (e.propertyName == propertyName)
                    return e.component;
            return null;
        }

        public T GetBinding<T>(string propertyName) where T : Component
            => GetBinding(propertyName) as T;

        public enum ViewType { Form, Part }
        public enum FormTypeEnum { Persistent, Popup, Transition }

        public enum GenerateTarget { Both, ViewOnly, LogicOnly }

        [Serializable]
        public class BindingEntry
        {
            public string gameObjectName;
            public string componentType;
            public string fullTypeName;
            public string propertyName;
            public string childPath;
            public Component component;
            public bool included = true;
        }
    }
}
