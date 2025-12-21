using System;
using System.Collections.Generic;
using GenBall.Utils.CodeGenerator.UI;
using JetBrains.Annotations;
using UnityEngine;
using Yueyn.Base.ReferencePool;

namespace GenBall.UI
{
    [RequireComponent(typeof(Canvas))]
    public abstract class FormBase : MonoBehaviour, IUserInterface,IBindable
    {
        private Canvas _canvas;
        public bool IsTop { get;private set; }
        public Canvas Canvas=> _canvas ??= GetComponent<Canvas>();
        private readonly List<ItemBase> _items = new();
        private readonly Dictionary<Type,VmBase> _vmMap = new();

        // public void CloseSelf()
        // {
        //     if (!IsTop)
        //     {
        //         throw new Exception("????????????");
        //     }
        //     // GameEntry.GetModule<UIManager>().CloseForm<>()
        // }
        public TVm GetVm<TVm>() where TVm : VmBase =>(TVm)GetVm(typeof(TVm));
        private VmBase GetVm([NotNull] Type type)
        {
            if (!typeof(VmBase).IsAssignableFrom(type))
            {
                throw new Exception("VmBase must derived from VmBase");
            }

            if (_vmMap.TryGetValue(type, out VmBase vm))
            {
                return vm;
            }
            vm=ReferencePool.Acquire(type) as VmBase;
            _vmMap.Add(type, vm);
            return vm;
        }

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
            foreach (var vm in _vmMap.Values)
            {
                ReferencePool.Release(vm);
            }
            _vmMap.Clear();
        }

        protected virtual void OnClose(object args = null)
        {
            
        }

        public void Unfocus()
        {
            IsTop = false;
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
            IsTop = true;
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

        public TypeEnum Type => TypeEnum.Form;
    }
}