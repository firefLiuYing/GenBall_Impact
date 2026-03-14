// 自动生成于 2026-03-14 15:46:45，请不要手动修改喵！

using UnityEngine;
using UnityEngine.UI;
using GenBall.Utils.CodeGenerator.UI;

namespace GenBall.UI
{
    public partial class MainHud : FormBase
    {
        private UiBindTool _bindTool;
        private Text _autoTxtKills;
        private Text _autoTxtLevel;
        private Text _autoTxtMagazine;
        private RectTransform _autoRectHpBar;
        private RectTransform _autoRectInteractTips;

        private void Bind()
        {
            _bindTool=GetComponent<UiBindTool>();
            _autoTxtKills = _bindTool.GetText("AutoTxtKills");
            _autoTxtLevel = _bindTool.GetText("AutoTxtLevel");
            _autoTxtMagazine = _bindTool.GetText("AutoTxtMagazine");
            _autoRectHpBar = _bindTool.GetRect("AutoRectHpBar");
            _autoRectInteractTips = _bindTool.GetRect("AutoRectInteractTips");
        }
    }
}
