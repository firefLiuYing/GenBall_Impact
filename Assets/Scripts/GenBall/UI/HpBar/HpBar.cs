using System.Collections.Generic;
using UnityEngine;

namespace GenBall.UI
{
    public partial class HpBar : ItemBase
    {
        private MainHudVm _mainHudVm;
        private CellViewSpawner _heartSpawner;
        private readonly List<HeartItem.Args> _heartArgs = new();
        protected override void OnInit(object args = null)
        {
            base.OnInit(args);
            Bind();
            _heartSpawner = GetComponent<CellViewSpawner>();
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
            _mainHudVm.MaxHealth.Observe(OnMaxHealthChanged);
            _mainHudVm.Armor.Observe(OnArmorChanged);
        }

        private void UnRegisterEvents()
        {
            _mainHudVm.Health.Unobserve(OnHealthChanged);
            _mainHudVm.MaxHealth.Unobserve(OnMaxHealthChanged);
            _mainHudVm.Armor.Unobserve(OnArmorChanged);
        }

        private void OnArmorChanged(int armor)
        {
            UpdateHeart();
        }
        private void OnHealthChanged(int health)
        {
            UpdateHeart();
        }

        private void OnMaxHealthChanged(int maxHealth)
        {
            UpdateHeart();
        }

        private void UpdateHeart()
        {
            UpdateHeartArgs(_mainHudVm.MaxHealth.Value,_mainHudVm.Health.Value,_mainHudVm.Armor.Value);
            _heartSpawner.SetDate(_heartArgs);
        }
        private void UpdateHeartArgs(int maxHealth, int health, int armor)
        {
            _heartArgs.Clear();
            var heartCount = (maxHealth + 1) / 2;
            for (int i = 0; i < heartCount; i++)
            {
                _heartArgs.Add(new HeartItem.Args
                {
                    Health = Mathf.Min(Mathf.Max(health-i*2,0),2),
                    Armor = Mathf.Min(Mathf.Max(armor-i*2,0),2)
                });
            }
        }
    }
}