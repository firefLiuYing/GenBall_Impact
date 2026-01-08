using GenBall.Map;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GenBall.Utils.Editor.Map
{
    public class MapBlockNode : Node
    {
        public MapBlockConfig BlockConfig { get; private set; }
        public Port InputPort { get; private set; }
        public Port OutputPort { get; private set; }

        public MapBlockNode(MapBlockConfig blockConfig)
        {
            BlockConfig = blockConfig;
            title = BlockConfig.BlockName;

            // 输入端口
            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            InputPort.portName = "In";
            inputContainer.Add(InputPort);

            // 输出端口
            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            OutputPort.portName = "Out";
            outputContainer.Add(OutputPort);

            // 预制体路径文本框
            var pathField = new TextField("预制体路径") { value = BlockConfig.mapBlockPrefabPath };
            pathField.style.minWidth = 250;  // 宽度更大
            pathField.style.width = 300;
            pathField.style.height = 25;
            pathField.style.flexGrow = 1;

            // 监听修改
            pathField.RegisterValueChangedCallback(evt =>
            {
                BlockConfig.mapBlockPrefabPath = evt.newValue;
                // 空路径提示
                pathField.style.unityTextOutlineColor = string.IsNullOrEmpty(evt.newValue) ? Color.red : Color.clear;
            });

            mainContainer.Add(pathField);

            RefreshExpandedState();
            RefreshPorts();
        }
    }
}