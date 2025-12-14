// 自动生成于 2025-12-14 20:42:07，请不要手动修改喵！

using UnityEngine;
using UnityEngine.UI;
using GenBall.Utils.CodeGenerator.UI;

namespace GenBall.UI
{
    public partial class MainHud : FormBase
    {
        private UiBindTool _bindTool;
        private Text _autoTxtKills;
        private RectTransform _autoRectHpBar;

        private void Bind()
        {
            _bindTool=GetComponent<UiBindTool>();
            _autoTxtKills = _bindTool.GetText("AutoTxtKills");
            _autoRectHpBar = _bindTool.GetRect("AutoRectHpBar");
        }
    }
}
