using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace GenBall.UI
{
    public abstract class ItemBase:MonoBehaviour
    {
        private readonly List<ItemBase> _childrenItems = new();
        public FormBase Form;

        public TVm GetVm<TVm>() where TVm :VmBase  => Form.GetVm<TVm>();
        private void GetAndAddChild(Transform trans)
        {
            if (trans.TryGetComponent<ItemBase>(out var item))
            {
                _childrenItems.Add(item);
                return;
            }

            for (int i = 0; i < trans.childCount; i++)
            {
                GetAndAddChild(trans.GetChild(i));
            }
        }
        public void Init([NotNull] FormBase form ,object args = null)
        {
            Form = form;
            // 跳过自己以免递归终止
            for (int i = 0; i < transform.childCount; i++)
            {
                GetAndAddChild(transform.GetChild(i));
            }
            foreach (var child in _childrenItems)
            {
                child.Init(form, args);
            }
            OnInit(args);
        }

        protected virtual void OnInit(object args = null)
        {
            
        }

        public void Open(object args = null)
        {
            foreach (var childrenItem in _childrenItems)
            {
                childrenItem.Open(args);
            }
            OnOpen(args);
        }

        protected virtual void OnOpen(object args = null)
        {
            
        }

        public void Close(object args = null)
        {
            foreach (var child in _childrenItems)
            {
                child.Close(args);
            }
            OnClose(args);
        }

        protected virtual void OnClose(object args = null)
        {
            
        }

        public void Pause(object args = null)
        {
            foreach (var child in _childrenItems)
            {
                child.Pause(args);
            }
            OnPause(args);
        }
        protected virtual void OnPause(object args = null)
        {
        }

        public void Resume(object args = null)
        {
            foreach (var child in _childrenItems)
            {
                child.Resume(args);
            }
            OnResume(args);
        }

        protected virtual void OnResume(object args = null)
        {
            
        }

        public void Focus()
        {
            foreach (var child in _childrenItems)
            {
                child.Focus();
            }
            OnFocus();
        }

        protected virtual void OnFocus()
        {
            
        }

        public void Unfocus()
        {
            foreach (var child in _childrenItems)
            {
                child.Unfocus();
            }
            OnUnfocus();
        }

        protected virtual void OnUnfocus()
        {
            
        }
    }
}