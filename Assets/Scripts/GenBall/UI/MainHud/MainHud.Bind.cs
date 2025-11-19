// 自动生成于 2025-11-19 20:49:35，请不要手动修改喵！

using UnityEngine;
using UnityEngine.UI;
using GenBall.Utils.CodeGenerator.UI;

namespace GenBall.UI
{
    public partial class MainHud
    {
        private UiBindTool _bindTool;
        private Image _autoImgImage;

        private void Bind()
        {
            _bindTool=GetComponent<UiBindTool>();
            _autoImgImage = _bindTool.GetImage("AutoImgImage");
        }
    }
}
