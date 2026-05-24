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

        public Image ImgAAA { get; private set; }

        protected override void DoBusinessStart()
        {
            base.DoBusinessStart();
            BindControls();
        }

        private void BindControls()
        {
            _binding = GetComponent<UiViewBinding>();
            ImgAAA = _binding.GetBinding<Image>("ImgAAA");
        }

        // ### GENERATED_BINDINGS_END ###

        protected override void RefreshView()
        {
            // TODO: 在此处实现 View 刷新逻辑
        }
    }
}
