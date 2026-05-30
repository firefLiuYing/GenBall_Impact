using GenBall.UI;
using GenBall.Utils.CodeGenerator.UI;
using UnityEngine;
using UnityEngine.UI;
using Yueyn.UI;

namespace GenBall.GM
{
    public class GMConsoleFormView : UIBusinessFormBase<GMConsoleFormViewData>
    {
        // ### GENERATED_BINDINGS_START ###
        private UiViewBinding _binding;

        public Text TxtOutput { get; private set; }
        public InputField InputCmd { get; private set; }
        public Button BtnSubmit { get; private set; }

        private void BindControls()
        {
            _binding = GetComponentInParent<UiViewBinding>();
            TxtOutput = _binding.GetBinding<Text>("TxtOutput");
            InputCmd  = _binding.GetBinding<InputField>("InputCmd");
            BtnSubmit = _binding.GetBinding<Button>("BtnSubmit");
        }
        // ### GENERATED_BINDINGS_END ###

        protected override void DoBusinessStart()
        {
            base.DoBusinessStart();
            BindControls();
        }

        protected override void DoBusinessOpen()
        {
            base.DoBusinessOpen();
            BindButtonEvents();
        }

        private void BindButtonEvents()
        {
            if (BtnSubmit != null)
            {
                BtnSubmit.onClick.AddListener(() =>
                    {
                        Yueyn.UI.UIManager.Instance.UIEventRouter.FireNow((int)UIEventKey.GMConsole_Submit);
                    }
                );
            }
                

            if (InputCmd != null)
                InputCmd.onEndEdit.AddListener(_ => OnEndEdit());
        }

        private void OnEndEdit()
        {
            if (InputCmd != null && !string.IsNullOrWhiteSpace(InputCmd.text))
                Yueyn.UI.UIManager.Instance.UIEventRouter.FireNow((int)UIEventKey.GMConsole_Submit);
        }

        protected override void RefreshView()
        {
            if (ViewData == null) return;

            if (TxtOutput != null)
                TxtOutput.text = ViewData.OutputText;

            if (InputCmd != null)
                InputCmd.text = ViewData.InputText;
        }
    }
}
