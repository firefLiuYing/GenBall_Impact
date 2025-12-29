// EventTemplateConfig.cs - 配置数据，独立文件
using System;

namespace GenBall.BattleSystem.Templates
{
    /// <summary>
    /// 事件定义配置（用于模板生成）
    /// </summary>
    [Serializable]
    public class EventTemplateConfig
    {
        // 在这里配置所有事件
        public static readonly EventDefinition[] Events = 
        {
            // 格式: new EventDefinition("事件名", "参数类型", "描述", "分类")
            new EventDefinition("Update", "float", "每帧更新", "System"),
            new EventDefinition("FixedUpdate", "float", "固定更新", "System"),
            new EventDefinition("Trigger", "ButtonState", "触发输入", "Input"),
            
            // 两个参数的事件
            new EventDefinition("BeforeAttackJustify", new[] { "IAttackable", "AttackInfo" }, 
                "攻击前判定", "Combat"),
            new EventDefinition("AfterAttackCalculate", new[] { "AttackResult" }, 
                "攻击后结算", "Combat"),
        };
    }
    
    [Serializable]
    public class EventDefinition
    {
        public string Name;
        public string[] ParamTypes;
        public string Description;
        public string Category;
        
        public EventDefinition(string name, string singleParamType, string description, string category)
        {
            Name = name;
            ParamTypes = singleParamType != null ? new[] { singleParamType } : null;
            Description = description;
            Category = category;
        }
        
        public EventDefinition(string name, string[] paramTypes, string description, string category)
        {
            Name = name;
            ParamTypes = paramTypes;
            Description = description;
            Category = category;
        }
        
        public int ParamCount => ParamTypes?.Length ?? 0;
    }
    
    
    
    
}