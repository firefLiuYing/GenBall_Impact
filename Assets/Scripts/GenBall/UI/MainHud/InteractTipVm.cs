using System.Collections.Generic;
using GenBall.Interact;
using Yueyn.Base.Variable;
using Yueyn.Main;

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
            SystemRepository.Instance.GetSystem<IInteractSystem>().Interactables.Observe(Interactables.PostValue);
        }

        private void UnRegisterEvents()
        {
            SystemRepository.Instance.GetSystem<IInteractSystem>().Interactables.Unobserve(Interactables.PostValue);
        }
    }
}