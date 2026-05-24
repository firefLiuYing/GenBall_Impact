using GenBall.Utils.CodeGenerator.UI;
using UnityEngine;
using UnityEngine.UI;
using Yueyn.UI;

namespace GenBall.UI
{
    public class GMConsoleFormView : UIBusinessFormBase<GMConsoleFormViewData>
    {
        // ### GENERATED_BINDINGS_START ###
        private UiViewBinding _binding;

        public Text TxtOutput { get; private set; }
        public InputField InputCmd { get; private set; }
        public Button BtnSubmit { get; private set; }

        protected override void DoBusinessStart()
        {
            base.DoBusinessStart();
            BindControls();
        }

        private void BindControls()
        {
            _binding = GetComponent<UiViewBinding>();
            TxtOutput = _binding.GetBinding<Text>("TxtOutput");
            InputCmd  = _binding.GetBinding<InputField>("InputCmd");
            BtnSubmit = _binding.GetBinding<Button>("BtnSubmit");
        }

        // ### GENERATED_BINDINGS_END ###

        protected override void RefreshView()
        {
            // TODO: 在此处实现 View 刷新逻辑
        }
    }
}

