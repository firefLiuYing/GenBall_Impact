using UnityEngine;

namespace GenBall.UI
{
    public partial class UpgradeTip : FormBase
    {
        protected override void OnInit(object args = null)
        {
            base.OnInit(args);
            Bind();
        }

        protected override void OnOpen(object args = null)
        {
            base.OnOpen(args);
            _isOpen = true;
        }

        private static bool _isOpen = false;
        protected override void OnClose(object args = null)
        {
            base.OnClose(args);
            _isOpen = false;
        }

        public static void Open()
        {
            if(_isOpen)  return;
            GameEntry.GetModule<UIManager>().OpenForm<UpgradeTip>();
        }
    }
}