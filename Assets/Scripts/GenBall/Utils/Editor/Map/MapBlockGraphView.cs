using System.Collections.Generic;
using System.Linq;
using GenBall.Map;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GenBall.Utils.Editor.Map
{
    public class MapBlockGraphView : GraphView
    {
        private MapConfig _mapConfig;

        private float nodeWidth = 250;
        private float nodeHeight = 150;
        private float spacingX = 50;
        private float spacingY = 50;
        private int nodesPerRow = 5;

        public MapBlockGraphView(MapConfig mapConfig)
        {
            _mapConfig = mapConfig;

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));

            var grid = new GridBackground();
            Insert(0, grid);

            InitializeNodes();

            graphViewChanged += OnGraphChanged;
        }

        private void InitializeNodes()
        {
            if (_mapConfig.mapBlockConfigs == null) return;

            int column = 0;
            int row = 0;

            foreach (var blockConfig in _mapConfig.mapBlockConfigs)
            {
                var node = new MapBlockNode(blockConfig);

                float x = column * (nodeWidth + spacingX);
                float y = row * (nodeHeight + spacingY);
                node.SetPosition(new Rect(x, y, nodeWidth, nodeHeight));
                AddElement(node);

                column++;
                if (column >= nodesPerRow)
                {
                    column = 0;
                    row++;
                }
            }
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("添加节点", AddNode);

            evt.menu.AppendAction("删除节点", action =>
            {
                // 将 selection 转换成 GraphElement
                var elementsToRemove = selection.OfType<GraphElement>().ToList();
                foreach (var elem in elementsToRemove)
                    RemoveElement(elem);
            }, DropdownMenuAction.Status.Normal);
        }

        private void AddNode(DropdownMenuAction action)
        {
            int newIndex = 0;
            if (_mapConfig.mapBlockConfigs != null && _mapConfig.mapBlockConfigs.Count > 0)
                newIndex = _mapConfig.mapBlockConfigs.Max(b => b.mapBlockIndex) + 1;

            var blockConfig = new MapBlockConfig
            {
                mapBlockIndex = newIndex,
                neighbors = new List<int>(),
                mapBlockPrefabPath = ""
            };

            if (_mapConfig.mapBlockConfigs == null)
                _mapConfig.mapBlockConfigs = new List<MapBlockConfig>();
            _mapConfig.mapBlockConfigs.Add(blockConfig);

            var node = new MapBlockNode(blockConfig);
            node.SetPosition(new Rect(action.eventInfo.mousePosition, new Vector2(nodeWidth, nodeHeight)));
            AddElement(node);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            foreach (var port in ports)
            {
                // 不允许自连接
                if (startPort.node == port.node) continue;
                if (startPort.direction == port.direction) continue;

                compatiblePorts.Add(port);
            }

            return compatiblePorts;
        }

        private GraphViewChange OnGraphChanged(GraphViewChange change)
        {
            if (change.edgesToCreate != null)
            {
                foreach (var edge in change.edgesToCreate)
                {
                    var fromNode = edge.output.node as MapBlockNode;
                    var toNode = edge.input.node as MapBlockNode;
                    if (fromNode == null || toNode == null) continue;

                    AddNeighbor(fromNode.BlockConfig, toNode.BlockConfig.mapBlockIndex);
                    AddNeighbor(toNode.BlockConfig, fromNode.BlockConfig.mapBlockIndex);
                }
            }

            if (change.elementsToRemove != null)
            {
                foreach (var elem in change.elementsToRemove)
                {
                    if (elem is Edge edge)
                    {
                        var fromNode = edge.output.node as MapBlockNode;
                        var toNode = edge.input.node as MapBlockNode;
                        if (fromNode == null || toNode == null) continue;

                        RemoveNeighbor(fromNode.BlockConfig, toNode.BlockConfig.mapBlockIndex);
                        RemoveNeighbor(toNode.BlockConfig, fromNode.BlockConfig.mapBlockIndex);
                    }
                    else if (elem is MapBlockNode node)
                    {
                        // 移除节点
                        foreach (var other in _mapConfig.mapBlockConfigs)
                        {
                            if (other.neighbors != null && other.neighbors.Contains(node.BlockConfig.mapBlockIndex))
                                other.neighbors.Remove(node.BlockConfig.mapBlockIndex);
                        }

                        _mapConfig.mapBlockConfigs.Remove(node.BlockConfig);
                    }
                }
            }

            return change;
        }

        private void AddNeighbor(MapBlockConfig block, int neighborIndex)
        {
            if (block.neighbors == null)
                block.neighbors = new List<int>();
            if (!block.neighbors.Contains(neighborIndex))
                block.neighbors.Add(neighborIndex);
        }

        private void RemoveNeighbor(MapBlockConfig block, int neighborIndex)
        {
            if (block.neighbors == null) return;
            if (block.neighbors.Contains(neighborIndex))
                block.neighbors.Remove(neighborIndex);
        }
    }
}
