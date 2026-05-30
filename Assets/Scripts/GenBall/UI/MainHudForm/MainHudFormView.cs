using GenBall.Utils.CodeGenerator.UI;
using UnityEngine;
using UnityEngine.UI;
using Yueyn.UI;

namespace GenBall.UI
{
    public class MainHudFormView : UIBusinessFormBase<MainHudFormViewData>
    {
        // ### GENERATED_BINDINGS_START ###
        private UiViewBinding _binding;

        public Text TxtKillPoints { get; private set; }
        public Text TxtLevel { get; private set; }
        public Text TxtArmor { get; private set; }
        public Text TxtHealth { get; private set; }
        public Text TxtAmmo { get; private set; }
        public Image ImgAim { get; private set; }

        private void BindControls()
        {
            _binding = GetComponent<UiViewBinding>();
            TxtKillPoints = _binding.GetBinding<Text>("TxtKillPoints");
            TxtLevel      = _binding.GetBinding<Text>("TxtLevel");
            TxtArmor      = _binding.GetBinding<Text>("TxtArmor");
            TxtHealth     = _binding.GetBinding<Text>("TxtHealth");
            TxtAmmo       = _binding.GetBinding<Text>("TxtAmmo");
            ImgAim        = _binding.GetBinding<Image>("ImgAim");
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

            if (TxtKillPoints != null) TxtKillPoints.text = $"击杀: {ViewData.KillPoints}";
            if (TxtLevel != null) TxtLevel.text = $"Lv.{ViewData.WeaponLevel}";
            if (TxtHealth != null) TxtHealth.text = $"HP: {ViewData.Health}/{ViewData.MaxHealth}";
            if (TxtArmor != null) TxtArmor.text = $"护甲: {ViewData.Armor}/{ViewData.MaxHealth}";
            if (TxtAmmo != null) TxtAmmo.text = $"{ViewData.AmmoCount}/{ViewData.AmmoCapacity}";
        }
    }
}
