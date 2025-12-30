// ValueEventTemplateConfig.cs

using System;
using UnityEngine;
using Yueyn.Event;

namespace GenBall.Event.Templates
{
    /// <summary>
    /// 值变化事件配置（用于模板生成）
    /// </summary>
    [Serializable]
    public class ValueEventTemplateConfig
    {
        // 在这里配置所有需要生成的值变化事件
        public static readonly ValueEventDefinition[] Events = 
        {
            // 基本类型
            new ValueEventDefinition("Health", typeof(int), "生命值变化", "Player"),
            new ValueEventDefinition("MaxHealth", typeof(int), "最大生命值变化", "Player"),
            new ValueEventDefinition("Armor", typeof(int), "护甲值变化", "Player"),
            new ValueEventDefinition("KillPoints", typeof(int), "击杀点数变化", "Player"),
        };
    }
    
    [Serializable]
    public class ValueEventDefinition
    {
        public string Name;
        public Type ValueType;
        public string Description;
        public string Module;
        
        public ValueEventDefinition(string name, Type valueType, string description, string module)
        {
            Name = name;
            ValueType = valueType;
            Description = description;
            Module = module;
        }
        
        public string FullName => $"{Module}.{Name}";
        
        // 获取类型名称（包含命名空间）
        public string TypeFullName => ValueType.FullName;
        
        // 获取简化的类型名称
        public string TypeName => ValueType.Name;
        
        // 获取命名空间
        public string TypeNamespace => ValueType.Namespace;
    }
}