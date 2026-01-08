// 自动生成于 2026-01-06 17:46:00，请不要手动修改喵！

using UnityEngine;
using UnityEngine.UI;
using GenBall.Utils.CodeGenerator.UI;

namespace GenBall.UI
{
    public partial class SplashForm : FormBase
    {
        private UiBindTool _bindTool;
        private Text _autoTxtProcess;

        private void Bind()
        {
            _bindTool=GetComponent<UiBindTool>();
            _autoTxtProcess = _bindTool.GetText("AutoTxtProcess");
        }
    }
}
