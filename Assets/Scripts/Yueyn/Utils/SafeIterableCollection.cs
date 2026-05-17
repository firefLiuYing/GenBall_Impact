using System.Collections.Generic;

namespace Yueyn.Utils
{
    /// <summary>
    /// 双缓冲列表（迭代安全）
    /// 用于解决在迭代过程中插入/删除元素导致的异常问题
    /// 使用双缓冲 + 脏标记优化性能，正常运行时零拷贝零 GC
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public class SafeIterableList<T>
    {
        private List<T> _mainList = new();
        private List<T> _cacheList = new();
        private bool _isDirty = false;

        /// <summary>
        /// 获取元素数量
        /// </summary>
        public int Count => _mainList.Count;

        /// <summary>
        /// 添加元素
        /// </summary>
        public void Add(T item)
        {
            _mainList.Add(item);
            _isDirty = true;
        }

        /// <summary>
        /// 移除元素
        /// </summary>
        public bool Remove(T item)
        {
            bool removed = _mainList.Remove(item);
            if (removed)
            {
                _isDirty = true;
            }
            return removed;
        }

        /// <summary>
        /// 清空列表
        /// </summary>
        public void Clear()
        {
            _mainList.Clear();
            _isDirty = true;
        }

        /// <summary>
        /// 检查是否包含元素
        /// </summary>
        public bool Contains(T item)
        {
            return _mainList.Contains(item);
        }

        /// <summary>
        /// 获取迭代快照（用于安全迭代）
        /// 只有在列表被修改时才会触发拷贝，否则直接返回主列表
        /// </summary>
        public List<T> GetIterableSnapshot()
        {
            if (_isDirty)
            {
                _cacheList.Clear();
                _cacheList.AddRange(_mainList);
                _isDirty = false;
            }
            return _cacheList;
        }

        /// <summary>
        /// 直接访问主列表（不安全，仅用于不需要迭代的场景）
        /// </summary>
        public List<T> GetMainList()
        {
            return _mainList;
        }
    }

    /// <summary>
    /// 双缓冲字典（迭代安全）
    /// 用于解决在迭代过程中插入/删除元素导致的异常问题
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    public class SafeIterableDict<TKey, TValue>
    {
        private Dictionary<TKey, TValue> _mainDict = new();
        private List<TValue> _cacheList = new();
        private bool _isDirty = false;

        /// <summary>
        /// 获取元素数量
        /// </summary>
        public int Count => _mainDict.Count;

        /// <summary>
        /// 添加或更新元素
        /// </summary>
        public void Set(TKey key, TValue value)
        {
            _mainDict[key] = value;
            _isDirty = true;
        }

        /// <summary>
        /// 移除元素
        /// </summary>
        public bool Remove(TKey key)
        {
            bool removed = _mainDict.Remove(key);
            if (removed)
            {
                _isDirty = true;
            }
            return removed;
        }

        /// <summary>
        /// 清空字典
        /// </summary>
        public void Clear()
        {
            _mainDict.Clear();
            _isDirty = true;
        }

        /// <summary>
        /// 尝试获取值
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _mainDict.TryGetValue(key, out value);
        }

        /// <summary>
        /// 检查是否包含键
        /// </summary>
        public bool ContainsKey(TKey key)
        {
            return _mainDict.ContainsKey(key);
        }

        /// <summary>
        /// 获取值的迭代快照（用于安全迭代）
        /// 只有在字典被修改时才会触发拷贝，否则直接返回缓存列表
        /// </summary>
        public List<TValue> GetIterableSnapshot()
        {
            if (_isDirty)
            {
                _cacheList.Clear();
                _cacheList.AddRange(_mainDict.Values);
                _isDirty = false;
            }
            return _cacheList;
        }

        /// <summary>
        /// 直接访问主字典（不安全，仅用于不需要迭代的场景）
        /// </summary>
        public Dictionary<TKey, TValue> GetMainDict()
        {
            return _mainDict;
        }
    }
}
