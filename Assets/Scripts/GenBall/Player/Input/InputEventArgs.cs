using Yueyn.Event;
using Yueyn.Utils;

namespace GenBall.Player
{
    public class InputEventArgs<T> : GameEventArgs
    {
        public string Name;
        public override int Id =>GetHashCode(Name);
        public T Args;
        public override void Clear()
        {
            Name=string.Empty;
            Args = default(T);
        }
        public static int GetHashCode(string name)=>new TypeNamePair(typeof(InputEventArgs<T>),name).GetHashCode();
    }

    public class InputEventArgs : GameEventArgs
    {
        public override int Id=>GetHashCode(Name);
        public string Name;
        public override void Clear()
        {
            Name = string.Empty;
        }
        public static int GetHashCode(string name)=>new TypeNamePair(typeof(InputEventArgs),name).GetHashCode();
    }
    public enum ButtonState
    {
        None,
        Up,
        Hold,
        Down,
    }
}