using GenBall.BattleSystem.Weapons;
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
            _mainHudVm.MagazineInfo.Observe(OnMagazineInfoChange);
        }

        private void UnRegisterEvents()
        {
            _mainHudVm.KillPoints.Unobserve(OnKillPointsChanged);
            _mainHudVm.Level.Unobserve(OnLevelChanged);
            _mainHudVm.MagazineInfo.Unobserve(OnMagazineInfoChange);
        }

        private void OnKillPointsChanged(int kills)
        {
            _autoTxtKills.text=$"KillPoints: {kills}";
        }

        private void OnLevelChanged(int level)
        {
            _autoTxtLevel.text=$"Level: {level}";
        }

        private void OnMagazineInfoChange(MagazineComponent.MagazineInfo magazineInfo)
        {
            _autoTxtMagazine.text=$"{magazineInfo.AmmunitionCount}/{magazineInfo.Capacity}";
        }
    }
}