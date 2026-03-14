using System.Collections.Generic;
using GenBall.Interact;
using Yueyn.Base.Variable;

namespace GenBall.UI
{
    public class InteractTipVm : VmBase
    {
        public readonly Variable<List<IInteractable>>  Interactables;

        public InteractTipVm()
        {
            Interactables=Variable<List<IInteractable>>.Create();
            AddDispose(Interactables);
        }

        public void Init()
        {
            RegisterEvents();
        }

        public override void Clear()
        {
            base.Clear();
            UnRegisterEvents();
        }

        private void RegisterEvents()
        {
            InteractSystem.Instance.Interactables.Observe(Interactables.PostValue);
        }

        private void UnRegisterEvents()
        {
            InteractSystem.Instance.Interactables.Unobserve(Interactables.PostValue);
        }
    }
}