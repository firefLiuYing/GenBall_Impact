using System;
using System.Runtime.InteropServices;

namespace Yueyn.Utils
{
    [StructLayout(LayoutKind.Auto)]
    public struct TypeNamePair:IEquatable<TypeNamePair>
    {
        private readonly Type _type;
        private readonly string _name;
        public Type Type => _type;
        public string Name => _name;

        public TypeNamePair(Type type):this(type,string.Empty){}
        public TypeNamePair(Type type, string name)
        {
            _type = type ?? throw new Exception("type is null");
            _name = name??string.Empty;
        }
        public bool Equals(TypeNamePair other)=>_type==other._type&&_name==other._name;
        public override bool Equals(object obj)=>obj is TypeNamePair other && Equals(other);
        public override int GetHashCode()=>_type.GetHashCode()^_name.GetHashCode();

        public override string ToString()
        {
            if (_type == null)
            {
                throw new Exception("type is null");
            }

            return $"{{{_type}.{_name}}}";
        }
        public static bool operator ==(TypeNamePair a, TypeNamePair b) => a.Equals(b);

        public static bool operator !=(TypeNamePair a, TypeNamePair b) => !(a == b);
    }
}