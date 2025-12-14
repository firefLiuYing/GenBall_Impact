using UnityEngine;

namespace GenBall.UI
{
    public partial class HpBar : ItemBase
    {
        private MainHudVm _mainHudVm;
        protected override void OnInit(object args = null)
        {
            base.OnInit(args);
            Bind();
        }

        protected override void OnOpen(object args = null)
        {
            base.OnOpen(args);
            _mainHudVm=GetVm<MainHudVm>();
            
            RegisterEvents();
        }

        protected override void OnClose(object args = null)
        {
            base.OnClose(args);
            UnRegisterEvents();
        }

        private void RegisterEvents()
        {
            _mainHudVm.Health.Observe(OnHealthChanged);
        }

        private void UnRegisterEvents()
        {
            _mainHudVm.Health.Unobserve(OnHealthChanged);
        }

        private void OnHealthChanged(int health)
        {
            _autoTxtHpText.text=health.ToString();
        }
    }
}