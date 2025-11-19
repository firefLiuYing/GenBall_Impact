using System.Collections.Generic;
using GenBall.Utils.EntityCreator;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.UI
{
    public partial class UIManager : MonoBehaviour,IComponent
    {
        private EntityCreator<IUserInterface> UiCreator => GameEntry.GetModule<EntityCreator<IUserInterface>>();
        private readonly Stack<IUserInterface> _activeUI = new();
        [SerializeField] private Transform uiRoot;
        [SerializeField] private int orderInterval;

        #region CloseForm

        public void CloseTopForm(object args=null)
        {
            if (_activeUI.Count <= 0)
            {
                Debug.Log("当前没有打开的UI界面");
                return;
            }

            var topForm = _activeUI.Pop();
            topForm.OnUnfocus();
            topForm.OnClose(args);
            if (topForm is MonoBehaviour monoBehaviour)
            {
                UiCreator.RecycleEntity(monoBehaviour.gameObject);
            }
        }

        #endregion
        
        #region OpenForm

        public void OpenForm<TUiForm>(object args=null) where TUiForm : MonoBehaviour, IUserInterface => InternalOpenForm<TUiForm>(args);
        public void OpenForm<TUiForm>(string name,object args=null) where TUiForm : MonoBehaviour, IUserInterface => InternalOpenForm<TUiForm>(name,args);
        private void InternalOpenForm<TUiForm>(object args=null) where TUiForm :MonoBehaviour, IUserInterface
        {
            if (_activeUI.Count > 0)
            {
                _activeUI.Peek().OnUnfocus();
            }
            var uiForm = UiCreator.CreateEntity<TUiForm>(uiRoot);
            uiForm.OnInit(args);
            _activeUI.Push(uiForm);
            uiForm.Canvas.sortingOrder=orderInterval*_activeUI.Count;
            uiForm.OnOpen(args);
            uiForm.gameObject.SetActive(true);
            uiForm.OnFocus();
        }
        private void InternalOpenForm<TUiForm>(string name,object args=null) where TUiForm :MonoBehaviour, IUserInterface
        {
            if (_activeUI.Count > 0)
            {
                _activeUI.Peek().OnUnfocus();
            }
            var uiForm = UiCreator.CreateEntity<TUiForm>(name,uiRoot);
            uiForm.OnInit(args);
            _activeUI.Push(uiForm);
            uiForm.Canvas.sortingOrder=orderInterval*_activeUI.Count;
            uiForm.OnOpen(args);
            uiForm.gameObject.SetActive(true);
            uiForm.OnFocus();
        }
        #endregion

        
        
        #region 没用的东西喵

        public void OnRegister()
        {
            
        }

        public void OnUnregister()
        {
            
        }

        public void ComponentUpdate(float elapsedSeconds, float realElapseSeconds)
        {
            
        }

        public void ComponentFixedUpdate(float fixedDeltaTime)
        {
            
        }

        public void Shutdown()
        {
            
        }

        #endregion
    }
}