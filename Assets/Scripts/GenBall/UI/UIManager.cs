using System;
using System.Collections.Generic;
using UnityEngine;
using Yueyn.Main;
using Yueyn.Resource;
using Object = UnityEngine.Object;

namespace GenBall.UI
{
    public partial class UIManager : MonoBehaviour,IComponent
    {
        public int Priority => 1000;
        private static readonly Dictionary<Type, string> FormPrefabPaths = new();
        private readonly Stack<IUserInterface> _activeUI = new();
        [SerializeField] private Transform uiRoot;
        [SerializeField] private int orderInterval;

        public static void RegisterForm<T>(string path) where T : IUserInterface
        {
            FormPrefabPaths[typeof(T)] = path;
        }

        #region CloseForm

        public bool CloseForm<TUiForm>(object args = null) where TUiForm : MonoBehaviour, IUserInterface
        {
            var topForm = _activeUI.Peek();
            if(topForm is not TUiForm uiForm) return false;
            CloseTopForm();
            return true;
        }
        private void CloseTopForm(object args=null)
        {
            if (_activeUI.Count <= 0)
            {
                Debug.Log("当前没有打开的UI窗体");
                return;
            }

            var topForm = _activeUI.Pop();
            topForm.Unfocus();
            topForm.Close(args);
            if (topForm is MonoBehaviour monoBehaviour)
            {
                Object.Destroy(monoBehaviour.gameObject);
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
                _activeUI.Peek().Unfocus();
            }

            var path = FormPrefabPaths[typeof(TUiForm)];
            var prefab = CResourceManager.Instance.LoadSync<GameObject>(path);
            var go = Object.Instantiate(prefab, uiRoot);
            var uiForm = go.GetComponent<TUiForm>();
            uiForm.Init(args);
            _activeUI.Push(uiForm);
            uiForm.Canvas.sortingOrder=orderInterval*_activeUI.Count;
            uiForm.Open(args);
            uiForm.gameObject.SetActive(true);
            uiForm.Focus();
        }
        private void InternalOpenForm<TUiForm>(string name,object args=null) where TUiForm :MonoBehaviour, IUserInterface
        {
            if (_activeUI.Count > 0)
            {
                _activeUI.Peek().Unfocus();
            }

            var path = FormPrefabPaths[typeof(TUiForm)];
            var prefab = CResourceManager.Instance.LoadSync<GameObject>(path);
            var go = Object.Instantiate(prefab, uiRoot);
            var uiForm = go.GetComponent<TUiForm>();
            uiForm.Init(args);
            _activeUI.Push(uiForm);
            uiForm.Canvas.sortingOrder=orderInterval*_activeUI.Count;
            uiForm.Open(args);
            uiForm.gameObject.SetActive(true);
            uiForm.Focus();
        }
        #endregion



        #region 没用的东西

        public void Init()
        {
            RegisterForm<MainHud>("Assets/AssetBundles/UI/MainHud/Form/MainHud.prefab");
            RegisterForm<AccessoryForm>("Assets/AssetBundles/UI/MainHud/Form/AccessoryForm.prefab");
            RegisterForm<UpgradeTip>("Assets/AssetBundles/UI/MainHud/Form/UpgradeTip.prefab");
            RegisterForm<SplashForm>("Assets/AssetBundles/UI/MainHud/Form/SplashForm.prefab");
            RegisterForm<StartForm>("Assets/AssetBundles/UI/MainHud/Form/StartForm.prefab");
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
