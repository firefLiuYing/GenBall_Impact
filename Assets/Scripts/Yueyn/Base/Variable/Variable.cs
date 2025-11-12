using System;
using Yueyn.Base.ReferencePool;

namespace Yueyn.Base.Variable
{
    public abstract class Variable:IReference
    {
        public Variable()
        {
        }

        public abstract Type Type { get; }
        public abstract object GetValue();
        public abstract void SetValue(object value);
        public abstract void Clear();
    }
}