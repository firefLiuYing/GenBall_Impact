using System;
using GenBall.Interact;
using UnityEngine;

namespace GenBall.UI
{
    public partial class InteractTipItem : ItemBase,ICellView
    {
        public class Args
        {
            public string OperationDescription;
        }
        private Args _args;
        private int _index;
        private bool _haveBind=false;
        public void OnRefresh(int index = 0, object args = null)
        {
            if (!_haveBind)
            {
                Bind();
                _haveBind = true;
            }
            RegisterEvents();
            _index = index;
            _args = args as Args;
            if(_args == null) return;
            Refresh();
        }

        private void OnDisable()
        {
            UnRegisterEvents();
        }

        private void Refresh()
        {
            if(_args == null) return;
            _autoTxtDiscription.text = _args.OperationDescription;
            _autoTxtDiscription.fontStyle=InteractSystem.Instance.CurrentSelectionIndex.Value==_index?FontStyle.Bold:FontStyle.Normal;
        }

        private void RegisterEvents()
        {
            InteractSystem.Instance.CurrentSelectionIndex.Observe(OnSelectionChanged);
        }

        private void UnRegisterEvents()
        {
            InteractSystem.Instance.CurrentSelectionIndex.Unobserve(OnSelectionChanged);
        }
        private void OnSelectionChanged(int index)
        {
            _autoTxtDiscription.fontStyle = index == _index ? FontStyle.Bold : FontStyle.Normal;
        }
    }
}