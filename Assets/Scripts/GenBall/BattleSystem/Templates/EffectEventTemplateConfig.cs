// EffectEventTemplateConfig.cs
using System;
using GenBall.Player;
using UnityEngine;

namespace GenBall.BattleSystem.Templates
{
    /// <summary>
    /// Effect事件配置（用于模板生成）
    /// </summary>
    [Serializable]
    public class EffectEventTemplateConfig
    {
        // 在这里配置所有需要生成的Effect事件
        public static readonly EffectEventDefinition[] Events = 
        {
            // 格式: new EffectEventDefinition("事件名", "参数类型", "描述", "分类")
            
            // 系统事件
            new EffectEventDefinition("Update", typeof(float), "每帧更新", "System"),
            new EffectEventDefinition("FixedUpdate", typeof(float), "固定更新", "System"),
            
            // 输入事件
            new EffectEventDefinition("Trigger", typeof(ButtonState), "触发输入", "Input"),
            
            // 战斗事件
            
            // 生命周期事件
            
            // 攻击相关事件
            new EffectEventDefinition("BeforeAttackJustify", new Type[] { typeof(IAttackable), typeof(AttackInfo) }, 
                "攻击前判定", "Combat"),
            new EffectEventDefinition("AfterAttackCalculate", new Type[] { typeof(AttackResult) }, 
                "攻击后结算", "Combat"),
            
            // 效果事件
            
            // 属性事件
            
            // 弹丸事件
            
            // 无参数事件
            
            // 新增：武器相关事件
        };
    }
    
    [Serializable]
    public class EffectEventDefinition
    {
        public string Name;
        public Type[] ParamTypes; // null表示无参数
        public string Description;
        public string Category;
        
        public EffectEventDefinition(string name, Type singleParamType, string description, string category)
        {
            Name = name;
            ParamTypes = singleParamType != null ? new[] { singleParamType } : null;
            Description = description;
            Category = category;
        }
        
        public EffectEventDefinition(string name, Type[] paramTypes, string description, string category)
        {
            Name = name;
            ParamTypes = paramTypes;
            Description = description;
            Category = category;
        }
        
        public int ParamCount => ParamTypes?.Length ?? 0;
        
        // 获取类型名称列表
        public string GetParamTypeNames()
        {
            if (ParamTypes == null || ParamTypes.Length == 0) 
                return "";
                
            return string.Join(", ", Array.ConvertAll(ParamTypes, t => t.Name));
        }
        
        // 获取参数列表字符串
        public string GetParamList()
        {
            if (ParamTypes == null || ParamTypes.Length == 0) 
                return "";
                
            var result = new System.Text.StringBuilder();
            for (int i = 0; i < ParamTypes.Length; i++)
            {
                result.Append($"{ParamTypes[i].Name} arg{i + 1}");
                if (i < ParamTypes.Length - 1)
                    result.Append(", ");
            }
            return result.ToString();
        }
        
        // 获取参数名列表字符串
        public string GetParamNameList()
        {
            if (ParamTypes == null || ParamTypes.Length == 0) 
                return "";
                
            var result = new System.Text.StringBuilder();
            for (int i = 0; i < ParamTypes.Length; i++)
            {
                result.Append($"arg{i + 1}");
                if (i < ParamTypes.Length - 1)
                    result.Append(", ");
            }
            return result.ToString();
        }
    }
}