using GenBall.Utils.CodeGenerator.UI;
using UnityEngine;
using UnityEngine.UI;
using Yueyn.UI;

namespace GenBall.UI
{
    public class WheelSlotPartView : PartViewBase<WheelSlotPartViewData>
    {
        // ### GENERATED_BINDINGS_START ###
        private UiViewBinding _binding;

        public Image ImgHighlight { get; private set; }
        public Image ImgIcon { get; private set; }
        public Text TxtName { get; private set; }
        public Text TxtCooldown { get; private set; }

        private void BindControls()
        {
            _binding = GetComponent<UiViewBinding>();
            ImgHighlight = _binding.GetBinding<Image>("ImgHighlight");
            ImgIcon      = _binding.GetBinding<Image>("ImgIcon");
            TxtName      = _binding.GetBinding<Text>("TxtName");
            TxtCooldown  = _binding.GetBinding<Text>("TxtCooldown");
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

            TxtName.text = ViewData.DisplayName;
            ImgHighlight.gameObject.SetActive(ViewData.IsHighlighted);

            if (ViewData.Icon != null)
                ImgIcon.sprite = ViewData.Icon;

            if (ViewData.IsCancelSlot)
            {
                TxtCooldown.gameObject.SetActive(false);
            }
            else
            {
                TxtCooldown.text = ViewData.CooldownText;
                TxtCooldown.gameObject.SetActive(ViewData.ShowCooldown);
            }
        }
    }
}
