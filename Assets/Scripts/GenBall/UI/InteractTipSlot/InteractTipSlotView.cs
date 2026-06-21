using GenBall.Utils.CodeGenerator.UI;
using UnityEngine;
using UnityEngine.UI;
using Yueyn.UI;

namespace GenBall.UI
{
    public class InteractTipSlotView : PartViewBase<InteractTipSlotViewData>
    {
        // ### GENERATED_BINDINGS_START ###
        private UiViewBinding _binding;

        public Text TxtDescription { get; private set; }

        private void BindControls()
        {
            _binding = GetComponent<UiViewBinding>();
            TxtDescription = _binding.GetBinding<Text>("TxtDescription");
        }

        // ### GENERATED_BINDINGS_END ###

        protected override void DoBusinessStart()
        {
            base.DoBusinessStart();
            BindControls();
        }

        protected override void RefreshView()
        {
            if (ViewData == null) return;

            if (TxtDescription != null)
            {
                TxtDescription.text = ViewData.OperationDescription;
                TxtDescription.fontStyle = ViewData.IsSelected ? FontStyle.Bold : FontStyle.Normal;
            }
        }
    }
}
