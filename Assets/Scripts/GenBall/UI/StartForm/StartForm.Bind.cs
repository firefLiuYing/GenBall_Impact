// 自动生成于 2026-01-07 11:03:43，请不要手动修改喵！

using UnityEngine;
using UnityEngine.UI;
using GenBall.Utils.CodeGenerator.UI;

namespace GenBall.UI
{
    public partial class StartForm : FormBase
    {
        private UiBindTool _bindTool;
        private Button _autoBtnBackground;
        private Button _autoBtnNewGame;
        private Button _autoBtnContinue;
        private Button _autoBtnLoad;
        private RectTransform _autoRectWelcome;
        private RectTransform _autoRectMenu;
        private RectTransform _autoRectLoad;

        private void Bind()
        {
            _bindTool=GetComponent<UiBindTool>();
            _autoBtnBackground = _bindTool.GetButton("AutoBtnBackground");
            _autoBtnNewGame = _bindTool.GetButton("AutoBtnNewGame");
            _autoBtnContinue = _bindTool.GetButton("AutoBtnContinue");
            _autoBtnLoad = _bindTool.GetButton("AutoBtnLoad");
            _autoRectWelcome = _bindTool.GetRect("AutoRectWelcome");
            _autoRectMenu = _bindTool.GetRect("AutoRectMenu");
            _autoRectLoad = _bindTool.GetRect("AutoRectLoad");
        }
    }
}
