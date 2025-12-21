using UnityEngine;

namespace GenBall.UI
{
    public partial class MainHud : FormBase
    {
        private HpBar _hpBar;
        private MainHudVm _mainHudVm;
        protected override void OnInit(object args = null)
        {
            base.OnInit(args);
            Bind();
            _hpBar = _autoRectHpBar.GetComponent<HpBar>();
        }

        protected override void OnOpen(object args = null)
        {
            base.OnOpen(args);
            _mainHudVm=GetVm<MainHudVm>();
            
            RegisterEvents();
            
            _mainHudVm.Init();
            // _autoImgImage.gameObject.SetActive(false);
        }

        protected override void OnClose(object args = null)
        {
            base.OnClose(args);
            
            UnRegisterEvents();
        }

        private void RegisterEvents()
        {
            _mainHudVm.KillPoints.Observe(OnKillPointsChanged);
            _mainHudVm.Level.Observe(OnLevelChanged);
        }

        private void UnRegisterEvents()
        {
            _mainHudVm.KillPoints.Unobserve(OnKillPointsChanged);
            _mainHudVm.Level.Unobserve(OnLevelChanged);
        }

        private void OnKillPointsChanged(int kills)
        {
            _autoTxtKills.text=$"KillPoints: {kills}";
        }

        private void OnLevelChanged(int level)
        {
            _autoTxtLevel.text=$"Level: {level}";
        }
    }
}