// 自动生成于 2026-03-14 15:17:55，请不要手动修改喵！

using UnityEngine;
using UnityEngine.UI;
using GenBall.Utils.CodeGenerator.UI;

namespace GenBall.UI
{
    public partial class InteractTipItem : ItemBase
    {
        private UiBindTool _bindTool;
        private Text _autoTxtDiscription;

        private void Bind()
        {
            _bindTool=GetComponent<UiBindTool>();
            _autoTxtDiscription = _bindTool.GetText("AutoTxtDiscription");
        }
    }
}
