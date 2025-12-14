// 自动生成于 2025-12-14 16:51:38，请不要手动修改喵！

using UnityEngine;
using UnityEngine.UI;
using GenBall.Utils.CodeGenerator.UI;

namespace GenBall.UI
{
    public partial class HpBar : ItemBase
    {
        private UiBindTool _bindTool;
        private Text _autoTxtHpText;

        private void Bind()
        {
            _bindTool=GetComponent<UiBindTool>();
            _autoTxtHpText = _bindTool.GetText("AutoTxtHpText");
        }
    }
}
