using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace GenBall.UI
{
    [RequireComponent(typeof(Canvas))]
    public abstract class FormBase : MonoBehaviour, IUserInterface
    {
        private Canvas _canvas;
        public Canvas Canvas=> _canvas ??= GetComponent<Canvas>();
        private readonly List<ItemBase> _items = new();

        private void GetAndAddItems(Transform trans)
        {
            if (trans.TryGetComponent<ItemBase>(out var item))
            {
                _items.Add(item);
                return;
            }

            for (int i = 0; i < trans.childCount; i++)
            {
                GetAndAddItems(trans.GetChild(i));
            }
        }
        public void Init(object args = null)
        {
            GetAndAddItems(transform);
            foreach (var item in _items)
            {
                item.Init(this,args);
            }
            OnInit();
        }

        protected virtual void OnInit(object args = null)
        {
            
        }

        


        public void Open(object args = null)
        {
            foreach (var item in _items)
            {
                item.Open(args);
            }
            OnOpen(args);
        }

        protected virtual void OnOpen(object args = null)
        {
            
        }

        public void Close(object args = null)
        {
            foreach (var item in _items)
            {
                item.Close(args);
            }
            OnClose(args);
        }

        protected virtual void OnClose(object args = null)
        {
            
        }

        public void Unfocus()
        {
            foreach (var item in _items)
            {
                item.Unfocus();
            }
            OnUnfocus();
        }

        protected virtual void OnUnfocus()
        {
            
        }

        public void Focus()
        {
            foreach (var item in _items)
            {
                item.Focus();
            }
        }

        protected virtual void OnFocus()
        {
            
        }

        public void Pause(object args = null)
        {
            foreach (var item in _items)
            {
                item.Pause(args);
            }
            OnPause(args);
        }

        protected virtual void OnPause(object args = null)
        {
            
        }
        public void Resume(object args = null)
        {
            foreach (var item in _items)
            {
                item.Resume(args);
            }
            OnResume(args);
        }

        protected virtual void OnResume(object args = null)
        {
            
        }

        public virtual void EntityUpdate(float deltaTime)
        {
            
        }

        public virtual void EntityFixedUpdate(float fixedDeltaTime)
        {
            
        }
        public virtual void OnRecycle()
        {
            
        }
    }
}