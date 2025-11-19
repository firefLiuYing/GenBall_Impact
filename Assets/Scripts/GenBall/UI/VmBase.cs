using System.Collections.Generic;
using Yueyn.Base.ReferencePool;
using Yueyn.Base.Variable;

namespace GenBall.UI
{
    public abstract class VmBase:IReference
    {
        private readonly List<Variable> _variables = new();

        protected void AddDispose(Variable variable)
        {
            _variables.Add(variable);
        }
        public virtual void Clear()
        {
            foreach (var variable in _variables)
            {
                ReferencePool.Release(variable);
            }
            _variables.Clear();
        }
    }
}