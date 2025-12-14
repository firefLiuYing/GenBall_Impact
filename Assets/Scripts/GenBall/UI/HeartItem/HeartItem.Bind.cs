// 自动生成于 2025-12-13 17:08:20，请不要手动修改喵！

using UnityEngine;
using UnityEngine.UI;
using GenBall.Utils.CodeGenerator.UI;

namespace GenBall.UI
{
    public partial class HeartItem : ItemBase
    {
        private UiBindTool _bindTool;
        private Image _autoImgFullHeart;

        private void Bind()
        {
            _bindTool=GetComponent<UiBindTool>();
            _autoImgFullHeart = _bindTool.GetImage("AutoImgFullHeart");
        }
    }
}
