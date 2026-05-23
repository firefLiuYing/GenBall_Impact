using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GenBall.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Yueyn.Utils;

namespace GenBall.Utils.CodeGenerator.UI 
{
    [Obsolete("Use UiBindingWindow instead. Window > UI Code Generator.")]
    [CustomEditor(typeof(UiBindTool))]
    public class UiBindToolInspector : UnityEditor.Editor
    {
        private enum UiType
        {
            Undefined,
            Button,
            Image,
            Text,
            RectTransform,
        }
        private const string DefaultScriptsPath = "Assets/Scripts/GenBall/UI/";
        
        private readonly Dictionary<string, RectTransform> _rectMap = new();
        private readonly Dictionary<string, Button> _buttonMap = new();
        private readonly Dictionary<string, Image> _imageMap = new();
        private readonly Dictionary<string,Text> _textMap = new();
        // private readonly List<ItemBase> _items = new();
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GUILayout.Label($"Ŀ���ļ�����·���� {GetFordPath()}");
            GUILayout.Space(20);
            GUILayout.Label("�Զ��󶨹���ƥ�䵽��Ӧǰ׺�Ҽ�⵽��Ӧ����ͻ��");
            GUILayout.Label("ǰ׺�����Ӧ��ϵ��");
            GUILayout.Label("AutoBtn\t\t=>Button");
            GUILayout.Label("AutoTxt\t\t=>Text");
            GUILayout.Label("AutoImg\t\t=>Image");
            GUILayout.Label("AutoRect\t=>RectTransform");
            if (GUILayout.Button("��",GUILayout.Height(30)))
            {
                Bind();
            }
        }

        private string GetFordPath()
        {
            var bindTool = (UiBindTool)target;
            return $"{DefaultScriptsPath}{bindTool.ClassName}/";
        }

        private string GetFilePath()
        {
            var bindTool = (UiBindTool)target;
            return $"{DefaultScriptsPath}{bindTool.ClassName}/{bindTool.ClassName}.Bind.cs";
        }
        private void Bind()
        {
            var bindTool = (UiBindTool)target;
            if (bindTool.ClassName.IsNullOrEmpty())
            {
                Debug.LogError("ClassName����Ϊ��!!!");
                return;
            }
            Clear();
            // �����Լ��������Ϳ��Ը��ø�Item�Զ�����
            for (int i = 0; i < bindTool.transform.childCount; i++)
            {
                // Debug.Log($"Bind:{i}");
                Scan(bindTool.transform.GetChild(i));
            }
            SetComponent();
            
            // ���Ԥ�������޸ģ�ʹunity�����޸ĺ�����л���Ϣ
            EditorUtility.SetDirty(bindTool);
            Generate();
        }

        private void SetComponent()
        {
            var bindTool = (UiBindTool)target;
            bindTool.SetButton(_buttonMap);
            bindTool.SetImage(_imageMap);
            bindTool.SetText(_textMap);
            bindTool.SetRect(_rectMap);
        }
        private void Generate()
        {
            var outputPath = GetFordPath();
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            string filePath=GetFilePath();
            try
            {
                var code = GetCode();
                File.WriteAllText(filePath, code, Encoding.UTF8);
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("�󶨳ɹ���", $"�󶨴��������ɵ���{filePath}", "�õ���");
                Debug.Log($"�󶨴��������ɵ���{filePath}");
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("�Բ�����������ʧ������", e.Message, "û��ϵ��");
                Debug.LogError($"��ʧ��{e}");
            }
        }

        private string GetCode()
        {
            StringBuilder sb = new();
            var bindTool = (UiBindTool)target;
            sb.AppendLine($"// �Զ������� {DateTime.Now:yyyy-MM-dd HH:mm:ss}���벻Ҫ�ֶ��޸�����");
            sb.AppendLine();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UnityEngine.UI;");
            sb.AppendLine("using GenBall.Utils.CodeGenerator.UI;");
            sb.AppendLine();
            sb.AppendLine("namespace GenBall.UI");
            sb.AppendLine("{");
            var bindable=bindTool.GetComponent<IBindable>();
            var parentClass=bindable.Type switch
            {
                TypeEnum.Form=>": FormBase",
                TypeEnum.Item=>": ItemBase",
                _ => ""
            };
            sb.AppendLine($"    public partial class {bindTool.ClassName} {parentClass}");
            sb.AppendLine("    {");
            sb.AppendLine($"        private UiBindTool _bindTool;");
            foreach (var button in _buttonMap)
            {
                sb.AppendLine($"        private Button {TransformPropertyName(button.Key)};");
            }

            foreach (var image in _imageMap)
            {
                sb.AppendLine($"        private Image {TransformPropertyName(image.Key)};");
            }

            foreach (var text in _textMap)
            {
                sb.AppendLine($"        private Text {TransformPropertyName(text.Key)};");
            }

            foreach (var rectTransform in _rectMap)
            {
                sb.AppendLine($"        private RectTransform {TransformPropertyName(rectTransform.Key)};");
            }

            sb.AppendLine();
            sb.AppendLine($"        private void Bind()");
            sb.AppendLine("        {");
            sb.AppendLine($"            _bindTool=GetComponent<UiBindTool>();");
            foreach (var button in _buttonMap)
            {
                sb.AppendLine($"            {TransformPropertyName(button.Key)} = _bindTool.GetButton(\"{button.Key}\");");
            }

            foreach (var image in _imageMap)
            {
                sb.AppendLine($"            {TransformPropertyName(image.Key)} = _bindTool.GetImage(\"{image.Key}\");");
            }

            foreach (var text in _textMap)
            {
                sb.AppendLine($"            {TransformPropertyName(text.Key)} = _bindTool.GetText(\"{text.Key}\");");
            }

            foreach (var rect in _rectMap)
            {
                sb.AppendLine($"            {TransformPropertyName(rect.Key)} = _bindTool.GetRect(\"{rect.Key}\");");
            }
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private string TransformPropertyName(string name)
        {
            if (name.StartsWith("Auto"))
            {
                return "_auto"+name[4..];
            }
            return name;
        }
        private void Clear()
        {
            _rectMap.Clear();
            _buttonMap.Clear();
            _imageMap.Clear();
            _textMap.Clear();
            // _items.Clear();
        }
        private void Scan(Transform transform)
        {
            Debug.Log(transform.gameObject.name);
            GetComponent(transform);
            if (transform.TryGetComponent<ItemBase>(out var item))
            {
                // _items.Add(item);
                return;
            }
            for (int i = 0; i < transform.childCount; i++)
            {
                // Debug.Log(i);
                var child = transform.GetChild(i);
                Scan(child);
            }
        }

        private void GetComponent(Transform transform)
        {
            var type=MatchUiType(transform.name);
            Debug.Log($"{transform.name}:{type}");
            switch (type)
            {
                case UiType.Button:
                    if (transform.TryGetComponent<Button>(out var btn))
                    {
                        _buttonMap.Add(btn.name, btn);
                    }
                    break;
                case UiType.Image:
                    if (transform.TryGetComponent<Image>(out var img))
                    {
                        _imageMap.Add(img.name, img);
                    }
                    break;
                case UiType.Text:
                    if (transform.TryGetComponent<Text>(out var txt))
                    {
                        _textMap.Add(txt.name, txt);
                    }
                    break;
                case UiType.RectTransform:
                    if (transform.TryGetComponent<RectTransform>(out var rect))
                    {
                        _rectMap.Add(rect.name, rect);
                    }
                    break;
                default:
                    break;
            }
        }
        private UiType MatchUiType(string name)=>name switch
        {
            not null when name.StartsWith("AutoBtn") => UiType.Button,
            not null when name.StartsWith("AutoTxt") => UiType.Text,
            not null when name.StartsWith("AutoImg") => UiType.Image,
            not null when name.StartsWith("AutoRect") => UiType.RectTransform,
            _=>UiType.Undefined,
        };
    }
}