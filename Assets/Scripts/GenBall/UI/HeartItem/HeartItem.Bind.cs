// 自动生成于 2025-12-30 18:12:39，请不要手动修改喵！

using UnityEngine;
using UnityEngine.UI;
using GenBall.Utils.CodeGenerator.UI;

namespace GenBall.UI
{
    public partial class HeartItem : ItemBase
    {
        private UiBindTool _bindTool;
        private Image _autoImgFullHeart;
        private Image _autoImgHalfHeart;
        private Image _autoImgFullArmor;
        private Image _autoImgHalfArmor;
        private Image _autoImgOutHeart;

        private void Bind()
        {
            _bindTool=GetComponent<UiBindTool>();
            _autoImgFullHeart = _bindTool.GetImage("AutoImgFullHeart");
            _autoImgHalfHeart = _bindTool.GetImage("AutoImgHalfHeart");
            _autoImgFullArmor = _bindTool.GetImage("AutoImgFullArmor");
            _autoImgHalfArmor = _bindTool.GetImage("AutoImgHalfArmor");
            _autoImgOutHeart = _bindTool.GetImage("AutoImgOutHeart");
        }
    }
}
