using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Yueyn.Resource;
using Yueyn.Utils;

namespace Yueyn.UI
{
    /// <summary>
    /// UI管理器（第一层次：Unity UI封装）
    /// 职责：
    /// 1. 管理UIFormScript的生命周期（创建、打开、关闭、销毁）
    /// 2. 提供通用接口（按路径打开、按ID关闭、层级管理）
    /// 3. UIFormScript池管理（复用）
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        // ===== 页面管理 =====

        private readonly Dictionary<int, UIFormScript> _allForms = new Dictionary<int, UIFormScript>();
        private readonly Dictionary<string, int> _pathToFormId = new Dictionary<string, int>();
        private readonly Dictionary<UIFormType, List<UIFormScript>> _formsByType = new Dictionary<UIFormType, List<UIFormScript>>();
        private int _nextFormId = 1;

        // ===== UI根节点 =====

        private Transform _uiRoot;
        private Transform _persistentRoot;
        private Transform _popupRoot;
        private Transform _transitionRoot;
        // private Canvas _uiCanvas;
        private Camera _uiCamera;

        public Camera UICamera=>_uiCamera;
        // ===== 协程运行器 =====

        private CoroutineRunner _coroutineRunner;

        protected override void Init()
        {
            CreateUIRoot();
            CreateCamera();
            CreateEventSystem();

            _coroutineRunner = CoroutineRunner.Instance;

            // 初始化分类字典
            _formsByType[UIFormType.Persistent] = new List<UIFormScript>();
            _formsByType[UIFormType.Popup] = new List<UIFormScript>();
            _formsByType[UIFormType.Transition] = new List<UIFormScript>();

            Debug.Log("[UIManager] Initialized");
        }

        // ===== 打开页面 =====

        /// <summary>
        /// 同步打开UI页面
        /// </summary>
        /// <param name="prefabPath">预制体路径</param>
        /// <param name="formType">UI类型</param>
        /// <param name="param">初始化参数</param>
        /// <returns>页面ID（失败返回-1）</returns>
        public int OpenForm(string prefabPath, UIFormType formType = UIFormType.Popup, object param = null)
        {
            // 检查是否已存在
            if (_pathToFormId.TryGetValue(prefabPath, out var existingId))
            {
                var existingForm = _allForms[existingId];
                if (!existingForm.CanReuse)
                {
                    Debug.LogWarning($"[UIManager] Form already exists and cannot be reused: {prefabPath}");
                    return existingId;
                }
            }

            // 同步加载预制体
            var prefab = CResourceManager.Instance.LoadSync<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[UIManager] Failed to load prefab: {prefabPath}");
                return -1;
            }

            // 实例化
            var parent = GetLayerRoot(formType);
            var go = UnityEngine.Object.Instantiate(prefab, parent);
            var formScript = go.GetComponent<UIFormScript>();
            if (formScript == null)
            {
                Debug.LogError($"[UIManager] Prefab does not have UIFormScript: {prefabPath}");
                UnityEngine.Object.Destroy(go);
                return -1;
            }

            // 分配ID并初始化
            var formId = _nextFormId++;
            formScript.SetFormInfo(formId, prefabPath);
            formScript.InternalInit(param);

            // 注册
            _allForms[formId] = formScript;
            _pathToFormId[prefabPath] = formId;
            AddToLayer(formScript, formType);

            // 打开
            formScript.InternalOpen();
            formScript.InternalFocus();

            Debug.Log($"[UIManager] Opened form: {prefabPath} (ID: {formId})");
            return formId;
        }

        /// <summary>
        /// 异步打开UI页面
        /// </summary>
        public void OpenFormAsync(string prefabPath, UIFormType formType = UIFormType.Popup, object param = null,
            Action<int, UIFormScript> onComplete = null, Action<float> onProgress = null)
        {
            _coroutineRunner.StartCoroutine(OpenFormAsyncCoroutine(prefabPath, formType, param, onComplete, onProgress));
        }

        private IEnumerator OpenFormAsyncCoroutine(string prefabPath, UIFormType formType, object param,
            Action<int, UIFormScript> onComplete, Action<float> onProgress)
        {
            // 检查是否已存在
            if (_pathToFormId.TryGetValue(prefabPath, out var existingId))
            {
                var existingForm = _allForms[existingId];
                if (!existingForm.CanReuse)
                {
                    Debug.LogWarning($"[UIManager] Form already exists and cannot be reused: {prefabPath}");
                    onComplete?.Invoke(existingId, existingForm);
                    yield break;
                }
            }

            // 异步加载预制体
            GameObject prefab = null;
            bool loadComplete = false;

            CResourceManager.Instance.Load(
                path: prefabPath,
                onLoadSuccess: obj =>
                {
                    prefab = obj as GameObject;
                    loadComplete = true;
                },
                onLoadFailed: error =>
                {
                    Debug.LogError($"[UIManager] Failed to load prefab: {prefabPath}, Error: {error}");
                    loadComplete = true;
                },
                onProgress: onProgress
            );

            // 等待加载完成
            yield return new WaitUntil(() => loadComplete);

            if (prefab == null)
            {
                onComplete?.Invoke(-1, null);
                yield break;
            }

            // 实例化
            var parent = GetLayerRoot(formType);
            var go = UnityEngine.Object.Instantiate(prefab, parent);
            var formScript = go.GetComponent<UIFormScript>();
            if (formScript == null)
            {
                Debug.LogError($"[UIManager] Prefab does not have UIFormScript: {prefabPath}");
                UnityEngine.Object.Destroy(go);
                onComplete?.Invoke(-1, null);
                yield break;
            }

            // 分配ID并初始化
            var formId = _nextFormId++;
            formScript.SetFormInfo(formId, prefabPath);
            formScript.InternalInit(param);

            // 注册
            _allForms[formId] = formScript;
            _pathToFormId[prefabPath] = formId;
            AddToLayer(formScript, formType);

            // 打开
            formScript.InternalOpen();
            formScript.InternalFocus();

            onComplete?.Invoke(formId, formScript);

            Debug.Log($"[UIManager] Opened form async: {prefabPath} (ID: {formId})");
        }

        // ===== 关闭页面 =====

        /// <summary>
        /// 按ID关闭页面
        /// </summary>
        public bool CloseForm(int formId, bool immediate = false)
        {
            if (!_allForms.TryGetValue(formId, out var form))
                return false;

            // 失去焦点
            if (form.IsFocused)
            {
                form.InternalUnfocus();
            }

            // 关闭
            form.InternalClose();

            // 从层级移除
            RemoveFromLayer(form);

            // 注销
            _allForms.Remove(formId);
            _pathToFormId.Remove(form.PrefabPath);

            // 销毁
            if (immediate)
            {
                UnityEngine.Object.Destroy(form.gameObject);
            }
            else
            {
                UnityEngine.Object.Destroy(form.gameObject, 0.3f);
            }

            Debug.Log($"[UIManager] Closed form: {form.PrefabPath} (ID: {formId})");
            return true;
        }

        /// <summary>
        /// 按路径关闭页面
        /// </summary>
        public bool CloseFormByPath(string prefabPath, bool immediate = false)
        {
            if (!_pathToFormId.TryGetValue(prefabPath, out var formId))
                return false;

            return CloseForm(formId, immediate);
        }

        /// <summary>
        /// 关闭所有弹窗UI
        /// </summary>
        public void CloseAllPopups(bool immediate = false)
        {
            var formsToClose = new List<UIFormScript>(_formsByType[UIFormType.Popup]);
            foreach (var form in formsToClose)
            {
                CloseForm(form.FormId, immediate);
            }
        }

        /// <summary>
        /// 关闭所有UI
        /// </summary>
        public void CloseAllForms(bool immediate = false)
        {
            var formsToClose = new List<UIFormScript>(_allForms.Values);
            foreach (var form in formsToClose)
            {
                CloseForm(form.FormId, immediate);
            }
        }

        // ===== 查询页面 =====

        /// <summary>
        /// 获取指定ID的页面
        /// </summary>
        public UIFormScript GetForm(int formId)
        {
            _allForms.TryGetValue(formId, out var form);
            return form;
        }

        /// <summary>
        /// 获取指定路径的页面ID
        /// </summary>
        public int GetFormId(string prefabPath)
        {
            _pathToFormId.TryGetValue(prefabPath, out var formId);
            return formId;
        }

        /// <summary>
        /// 检查是否存在指定ID的页面
        /// </summary>
        public bool HasForm(int formId)
        {
            return _allForms.ContainsKey(formId);
        }

        /// <summary>
        /// 检查是否存在指定路径的页面
        /// </summary>
        public bool HasFormByPath(string prefabPath)
        {
            return _pathToFormId.ContainsKey(prefabPath);
        }

        // ===== UI根节点创建 =====

        private void CreateUIRoot()
        {
            var uiRoot = new GameObject("UIRoot");
            // 为_uiRoot赋值
            _uiRoot = uiRoot.transform;
            Transform frameworkRoot= GameObject.Find("Framework").transform;
            if (frameworkRoot != null)
            {
                uiRoot.transform.SetParent(frameworkRoot);
            }

            // 创建层级根节点
            _persistentRoot = new GameObject("Persistent").transform;
            _persistentRoot.SetParent(_uiRoot, false);
            _persistentRoot.gameObject.AddComponent<RectTransform>();

            _popupRoot = new GameObject("Popup").transform;
            _popupRoot.SetParent(_uiRoot, false);
            _popupRoot.gameObject.AddComponent<RectTransform>();

            _transitionRoot = new GameObject("Transition").transform;
            _transitionRoot.SetParent(_uiRoot, false);
            _transitionRoot.gameObject.AddComponent<RectTransform>();

            Debug.Log("[UIManager] UI Root created");
        }

        private void CreateCamera()
        {
            var go = new GameObject("UICamera");
            go.transform.SetParent(_uiRoot, false);
            Debug.Log($"[UIManager] UI UIRoot {_uiRoot}");
            _uiCamera = go.AddComponent<Camera>();
            _uiCamera.clearFlags = CameraClearFlags.Depth;
            _uiCamera.cullingMask = 1 << LayerMask.NameToLayer("UI");
            _uiCamera.orthographic = true;
            _uiCamera.orthographicSize = 5;
            _uiCamera.depth = 100;
            Debug.Log("[UIManager] UI Camera created");
        }

        private void CreateEventSystem()
        {
            if ( UnityEngine.Object.FindObjectOfType<EventSystem>() != null)
            {
                Debug.Log("[UIManager] EventSystem already exists");
                return;
            }

            var go = new GameObject("EventSystem");
            go.transform.SetParent(_uiRoot,false);
            // UnityEngine.Object.DontDestroyOnLoad(go);
            
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();

            Debug.Log("[UIManager] EventSystem created");
        }

        // ===== 辅助方法 =====

        private Transform GetLayerRoot(UIFormType formType)
        {
            return formType switch
            {
                UIFormType.Persistent => _persistentRoot,
                UIFormType.Popup => _popupRoot,
                UIFormType.Transition => _transitionRoot,
                _ => _popupRoot
            };
        }

        private void AddToLayer(UIFormScript form, UIFormType formType)
        {
            _formsByType[formType].Add(form);
        }

        private void RemoveFromLayer(UIFormScript form)
        {
            foreach (var list in _formsByType.Values)
            {
                list.Remove(form);
            }
        }
    }
}
