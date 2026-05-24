using GenBall.Utils.CodeGenerator.UI;
using UnityEngine;
using UnityEngine.UI;
using Yueyn.UI;

namespace GenBall.UI
{
    public class SplashFormView : UIBusinessFormBase<SplashFormViewData>
    {
        // ### GENERATED_BINDINGS_START ###
        private UiViewBinding _binding;

        public Text TxtProgress { get; private set; }
        public Image ImgProgress { get; private set; }

        protected override void DoBusinessStart()
        {
            base.DoBusinessStart();
            BindControls();

            // Splash 页面短暂显示，禁用渐显渐隐动画
            var formScript = GetComponentInParent<UIFormScript>();
            if (formScript != null) formScript.FadeDuration = 0f;
        }

        private void BindControls()
        {
            _binding = GetComponent<UiViewBinding>();
            TxtProgress = _binding.GetBinding<Text>("TxtProgress");
            ImgProgress = _binding.GetBinding<Image>("ImgProgress");
        }

        // ### GENERATED_BINDINGS_END ###

        protected override void RefreshView()
        {
            if (ViewData == null) return;

            if (TxtProgress != null)
            {
                TxtProgress.text = $"{(ViewData.Progress * 100f):F0}%";
            }

            if (ImgProgress != null)
            {
                ImgProgress.fillAmount = ViewData.Progress;
            }
        }
    }
}
