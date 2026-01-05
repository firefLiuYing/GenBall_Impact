using GenBall.BattleSystem.Accessory;
using GenBall.Event.Generated;
using GenBall.Player;
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
            
            RegisterEvents();
        }

        private static bool _isOpen = false;
        protected override void OnClose(object args = null)
        {
            UnRegisterEvents();
            
            base.OnClose(args);
            _isOpen = false;
        }

        public static void Open()
        {
            if(_isOpen)  return;
            GameEntry.GetModule<UIManager>().OpenForm<UpgradeTip>();
        }

        private void RegisterEvents()
        {
            GameEntry.Event.SubscribeInputUpgrade(OnInputUpgrade);
        }

        private void UnRegisterEvents()
        {
            GameEntry.Event.UnsubscribeInputUpgrade(OnInputUpgrade);
        }

        private void OnInputUpgrade(ButtonState  buttonState)
        {
            if (buttonState == ButtonState.Down)
            {
                AccessoryController.Instance.Upgrade();
            }
        }
    }
}