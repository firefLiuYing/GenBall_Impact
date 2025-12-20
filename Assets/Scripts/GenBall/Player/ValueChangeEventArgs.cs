using Yueyn.Base.ReferencePool;
using Yueyn.Event;
using Yueyn.Utils;

namespace GenBall.Player
{
    public class ValueChangeEventArgs<T> : GameEventArgs where T : struct
    {
        public override int Id => GetId(Name);
        public T Value;
        public string Name{get; private set;}

        public static ValueChangeEventArgs<T> Create(T value, string name)
        {
            var e = ReferencePool.Acquire<ValueChangeEventArgs<T>>();
            e.Value = value;
            e.Name = name;
            return e;
        }
        public static int GetId(string name) => (new TypeNamePair(typeof(T), name)).GetHashCode();
        public override void Clear()
        {
            Value = default(T);
            Name = null;
        }
    }
}