using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Yueyn.Utils
{
    public sealed class MultiDictionary<TKey, TValue>:IEnumerable<KeyValuePair<TKey,LinkedList<TValue>>>,IEnumerable
    {
        private readonly LinkedList<TValue> _list;
        private readonly Dictionary<TKey,LinkedList<TValue>> _dict = new();
        public LinkedList<TValue> this[TKey key]=>_dict[key];

        public void Clear()
        {
            _list.Clear();
            _dict.Clear();
        }
        public bool Contains(TKey key)=>_dict.ContainsKey(key);
        public bool Contains(TKey key,TValue value)=>_dict.ContainsKey(key)&&_dict[key].Contains(value);
        public bool TryGetValue(TKey key,out LinkedList<TValue> value)=>_dict.TryGetValue(key, out value);

        public void Add(TKey key, TValue value)
        {
            var node = new LinkedListNode<TValue>(value);
            if (_dict.TryGetValue(key, out var list))
            {
                list.AddLast(node);
            }
            else
            {
                list = new LinkedList<TValue>();
                list.AddLast(node);
                _dict.Add(key,list);
            }
        }

        public bool Remove(TKey key, TValue value)
        {
            if (_dict.TryGetValue(key, out var list))
            {
                return list.Remove(value);
            }
            return false;
        }

        public bool RemoveAll(TKey key)
        {
            if (_dict.Remove(key, out var list))
            {
                list.Clear();
                return true;
            }
            return false;
        }
        public Enumerator GetEnumerator()=>new Enumerator(_dict);


        IEnumerator<KeyValuePair<TKey, LinkedList<TValue>>> IEnumerable<KeyValuePair<TKey, LinkedList<TValue>>>.GetEnumerator()
            => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [StructLayout(LayoutKind.Auto)]
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, LinkedList<TValue>>>, IEnumerator
        {
            private Dictionary<TKey, LinkedList<TValue>>.Enumerator _enumerator;

            internal Enumerator(Dictionary<TKey, LinkedList<TValue>> dict)
            {
                if (dict == null)
                {
                    throw new System.ArgumentNullException(nameof(dict));
                }
                _enumerator = dict.GetEnumerator();
            }
            public KeyValuePair<TKey, LinkedList<TValue>> Current => _enumerator.Current;
            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            public void Reset()=>((IEnumerator<KeyValuePair<TKey, LinkedList<TValue>>>)_enumerator).Reset();


            object IEnumerator.Current => _enumerator.Current;

            public void Dispose()
            {
                _enumerator.Dispose();
            }
        }
    }
}