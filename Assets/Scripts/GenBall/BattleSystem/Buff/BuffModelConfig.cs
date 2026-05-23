using System;
using System.Collections.Generic;
using UnityEngine;

namespace GenBall.BattleSystem.Buff
{
    [CreateAssetMenu(fileName = "BuffModelConfig", menuName = "Buff/BuffModelConfig")]
    public class BuffModelConfig : ScriptableObject
    {
        [SerializeField] private List<BuffModel> buffModels=new();
        private readonly Dictionary<string,BuffModel> _buffDict = new();
        private readonly Dictionary<string, System.Type> _buffTypeCache = new();
        public void Init()
        {
            _buffDict.Clear();
            _buffTypeCache.Clear();
            foreach (var buffModel in buffModels)
            {
                _buffDict.TryAdd(buffModel.BuffId,buffModel);
                if (!string.IsNullOrEmpty(buffModel.BuffType))
                {
                    var type = System.Type.GetType(buffModel.BuffType);
                    if (type != null)
                    {
                        _buffTypeCache.TryAdd(buffModel.BuffId, type);
                    }
                }
            }
        }

        public BuffModel GetBuffModel(string buffId)
        {
            return _buffDict.GetValueOrDefault(buffId);
        }

        public System.Type GetBuffType(string buffId)
        {
            return _buffTypeCache.GetValueOrDefault(buffId);
        }
    }

    [Serializable]
    public class BuffModel
    {
        [SerializeField,Tooltip("����������������Ҷ�ӦbuffЧ��")] private string buffId;
        [SerializeField] private string buffType;
        [SerializeField] private string displayName;
        [SerializeField] private bool canMultiExist;
        [SerializeField,Tooltip("�������ȼ�")] private int priority;
        [SerializeField] private List<string> tags;
        [SerializeField] private List<BuffParam> parameters;
        [SerializeField,Tooltip("���ɵ���ʱ��1")] private int maxStack;
        
        public string BuffId => buffId;
        public string BuffType => buffType;
        public string DisplayName => displayName;
        public bool CanMultiExist => canMultiExist;
        public int Priority => priority;
        public IReadOnlyList<string> Tags => tags;
        public IReadOnlyList<BuffParam> Parameters => parameters;
        public int MaxStack => maxStack;
    }

    [Serializable]
    public struct BuffParam
    {
        [SerializeField] private string key;
        [SerializeField] private int intValue;
        [SerializeField] private float floatValue;
        [SerializeField] private string stringValue;
        
        public string Key => key;
        public int IntValue => intValue;
        public float FloatValue => floatValue;
        public string StringValue => stringValue;
    }
}