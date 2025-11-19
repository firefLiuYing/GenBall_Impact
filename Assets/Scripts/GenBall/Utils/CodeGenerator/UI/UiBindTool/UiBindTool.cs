using System;
using System.Collections.Generic;
using System.Linq;
using GenBall.UI;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GenBall.Utils.CodeGenerator.UI
{
    public class UiBindTool : MonoBehaviour
    {
        [SerializeField] private string className;
        public string ClassName=>className;
        // [SerializeField] private readonly Dictionary<string, RectTransform> _rectMap = new();
        // [SerializeField] private readonly Dictionary<string, Button> _buttonMap = new();
        // [SerializeField] private readonly Dictionary<string, Image> _imageMap = new();
        // [SerializeField] private readonly Dictionary<string,Text> _textMap = new();
        [SerializeField] private List<BindText> textMap = new();
        [SerializeField] private List<BindImage> imageMap = new();
        [SerializeField] private List<BindButton> buttonMap = new();
        [SerializeField] private List<BindRect> rectMap = new();
        public Button GetButton(string name)=> buttonMap.FirstOrDefault(x => x.name == name)?.button;
        public Image GetImage(string name)=> imageMap.FirstOrDefault(x => x.name == name)?.image;
        public Text GetText(string name)=> textMap.FirstOrDefault(data => data.name==name)?.text;
        public RectTransform GetRect(string name)=> rectMap.FirstOrDefault(data => data.name==name)?.rect;

        public void SetText(Dictionary<string, Text> texts)
        {
            textMap.Clear();
            foreach (var pair in texts)
            {
                textMap.Add(new  BindText { name = pair.Key, text = pair.Value });
            }
        }
        public void SetImage(Dictionary<string, Image> images)
        {
            imageMap.Clear();
            foreach (var pair in images)
            {
                imageMap.Add(new  BindImage { name = pair.Key, image = pair.Value });
            }
        }
        public void SetButton(Dictionary<string, Button> buttons)
        {
            buttonMap.Clear();
            foreach (var pair in buttons)
            {
                buttonMap.Add(new  BindButton { name = pair.Key, button = pair.Value });
            }
        }
        public void SetRect([NotNull] Dictionary<string, RectTransform> rects)
        {
            rectMap.Clear();
            foreach (var pair in rects)
            {
                rectMap.Add(new  BindRect { name = pair.Key, rect = pair.Value });
            }
        }
        public void Clear()
        {
            rectMap.Clear();
            buttonMap.Clear();
            imageMap.Clear();
            textMap.Clear();
            // _items.Clear();
        }
    }
    [Serializable]
    public class BindText
    {
        public string name;
        public Text text;
    }

    [Serializable]
    public class BindImage
    {
        public string name;
        public Image image;
    }

    [Serializable]
    public class BindButton
    {
        public string name;
        public Button button;
    }
    [Serializable]
    public class BindRect
    {
        public string name;
        public RectTransform rect;
    }
}