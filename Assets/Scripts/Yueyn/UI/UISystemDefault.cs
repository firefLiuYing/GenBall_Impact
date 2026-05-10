using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Yueyn.Main;
using Object = UnityEngine.Object;

namespace Yueyn.UI
{
    /// <summary>
    /// UI系统默认实现
    /// 使用Dictionary+List管理所有页面，支持按ID关闭任意页面
    /// 支持FormScript对象池复用
    /// </summary>
    public class UISystemDefault : IUISystem, ILogicUpdate
    {
        // ===== 页面管理 =====

        private Dictionary<int, UIFormScript> _allForms = new Dictionary<int, UIFormScript>();
        private Dictionary<int, string> _formPrefabPaths = new Dictionary<int, string>(); // FormId -> PrefabPath映射
        private List<UIFormScript> _persistentForms = new List<UIFormScript>();
        private List<UIFormScript> _popupForms = new List<UIFormScript>();
        private UIFormScript _transitionForm;
        private UIFormScript _focusedForm;
        private int _nextFormId = 1;

        // ===== 预制体缓存 =====

        private Dictionary<string, GameObject> _prefabCache = new Dictionary<string, GameObject>();

        // ===== FormScript对象池（支持复用） =====

        private Dictionary<string, Queue<UIFormScript>> _formPool = new Dictionary<string, Queue<UIFormScript>>();

        // ===== 资源系统引用 =====

        private Yueyn.Resource.IResourceSystem _resourceSystem;

        // ===== 协程运行器 =====

        private MonoBehaviour _coroutineRunner;

        // ===== UI根节点 =====

        private Transform _uiRoot;
        private Transform _persistentRoot;
        private Transform _popupRoot;
        private Transform _transitionRoot;

        // ===== ISystem实现 =====

        public void Init()
        {
            // 获取资源系统
            _resourceSystem = SystemRepository.Instance.GetSystem<Yueyn.Resource.IResourceSystem>();

            // 创建UI根节点
            CreateUIRoot();
            // 创建Camera
            CreateCamera();
            // 创建EventSystem
            CreateEventSystem();

            // 创建协程运行器
            var runnerGo = new GameObject("UISystemCoroutineRunner");
            Object.DontDestroyOnLoad(runnerGo);
            _coroutineRunner = runnerGo.AddComponent<UICoroutineRunner>();

            Debug.Log("[UISystem] Initialized");
        }

        public void UnInit()
        {
            // 关闭所有UI
            CloseAllForms(immediate: true);

            // 清理对象池
            ClearFormPool();

            // 清理协程运行器
            if (_coroutineRunner != null)
            {
                Object.Destroy(_coroutineRunner.gameObject);
                _coroutineRunner = null;
            }

            // 清理根节点
            if (_uiRoot != null)
            {
                Object.Destroy(_uiRoot.gameObject);
                _uiRoot = null;
            }

            Debug.Log("[UISystem] UnInitialized");
        }

        private void CreateUIRoot()
        {
            var rootGo = new GameObject("UIRoot");
            Object.DontDestroyOnLoad(rootGo);
            _uiRoot = rootGo.transform;

            _persistentRoot = new GameObject("Persistent").transform;
            _persistentRoot.SetParent(_uiRoot);

            _popupRoot = new GameObject("Popup").transform;
            _popupRoot.SetParent(_uiRoot);

            _transitionRoot = new GameObject("Transition").transform;
            _transitionRoot.SetParent(_uiRoot);
        }

        // 创建Camera
        private void CreateCamera()
        {
            var cameraGo = new GameObject("UICamera");
            // Object.DontDestroyOnLoad(cameraGo);
            cameraGo.AddComponent<Camera>();
            cameraGo.transform.SetParent(_uiRoot);
        }
        
        // 创建EventSystem
        private void CreateEventSystem()
        {
            var eventSystem = new GameObject("EventSystem");
            // Object.DontDestroyOnLoad(eventSystem);
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            eventSystem.transform.SetParent(_uiRoot);
        }
        public void LogicUpdate(float deltaTime)
        {
            // 可以在这里处理UI动画、渐变等
        }

        // ===== IUISystem实现 =====

        public UIFormScript OpenForm(string prefabPath, UILogicBase logic, object param = null)
        {
            if (string.IsNullOrEmpty(prefabPath))
            {
                Debug.LogError("[UISystem] PrefabPath cannot be null or empty!");
                return null;
            }

            if (logic == null)
            {
                Debug.LogError("[UISystem] Logic cannot be null!");
                return null;
            }

            if (_resourceSystem == null)
            {
                Debug.LogWarning("[UISystem] ResourceSystem not initialized, using Resources.Load fallback");
            }

            UIFormScript form = null;

            // 1. 尝试从对象池获取（如果可复用）
            if (_formPool.TryGetValue(prefabPath, out var pool) && pool.Count > 0)
            {
                form = pool.Dequeue();
                Debug.Log($"[UISystem] Reused form from pool: {prefabPath}");
            }
            else
            {
                // 2. 加载预制体
                var prefab = LoadPrefab(prefabPath);
                if (prefab == null) return null;

                // 3. 实例化
                var go = Object.Instantiate(prefab, _popupRoot);
                form = go.GetComponent<UIFormScript>();

                if (form == null)
                {
                    Debug.LogError($"[UISystem] Prefab {prefabPath} does not have UIFormScript component");
                    Object.Destroy(go);
                    return null;
                }
            }

            // 4. 分配ID并记录路径
            int formId = _nextFormId++;
            form.SetFormId(formId);
            _formPrefabPaths[formId] = prefabPath;

            // 5. 初始化（传入Logic）
            form.InternalInit(logic, param);

            // 6. 注册到管理器
            _allForms[formId] = form;

            // 7. 根据FormType分类并设置父节点
            var formView = form.GetComponentInChildren<UIFormView>();
            if (formView != null)
            {
                switch (formView.FormType)
                {
                    case UIFormType.Persistent:
                        _persistentForms.Add(form);
                        form.transform.SetParent(_persistentRoot);
                        break;

                    case UIFormType.Popup:
                        _popupForms.Add(form);
                        form.transform.SetParent(_popupRoot);
                        break;

                    case UIFormType.Transition:
                        if (_transitionForm != null)
                        {
                            CloseForm(_transitionForm.FormId, immediate: true);
                        }
                        _transitionForm = form;
                        form.transform.SetParent(_transitionRoot);
                        HideAllUI();
                        break;
                }
            }

            // 8. 打开
            form.InternalOpen();

            // 9. 更新焦点和层级
            UpdateFocus();
            UpdateSortingOrder();

            Debug.Log($"[UISystem] Opened form: {prefabPath} (FormID: {formId}, LogicID: {logic.LogicId})");

            return form;
        }

        /// <summary>
        /// 异步打开UI页面
        /// </summary>
        public void OpenFormAsync(string prefabPath, UILogicBase logic, object param = null,
            Action<UIFormScript> onComplete = null, Action<float> onProgress = null)
        {
            if (string.IsNullOrEmpty(prefabPath))
            {
                Debug.LogError("[UISystem] PrefabPath cannot be null or empty!");
                onComplete?.Invoke(null);
                return;
            }

            if (logic == null)
            {
                Debug.LogError("[UISystem] Logic cannot be null!");
                onComplete?.Invoke(null);
                return;
            }

            if (_coroutineRunner == null)
            {
                Debug.LogError("[UISystem] CoroutineRunner not initialized!");
                onComplete?.Invoke(null);
                return;
            }

            _coroutineRunner.StartCoroutine(OpenFormCoroutine(prefabPath, logic, param, onComplete, onProgress));
        }

        /// <summary>
        /// 异步打开UI页面的协程
        /// </summary>
        private IEnumerator OpenFormCoroutine(string prefabPath, UILogicBase logic, object param,
            Action<UIFormScript> onComplete, Action<float> onProgress)
        {
            onProgress?.Invoke(0f);

            UIFormScript form = null;

            // 1. 尝试从对象池获取
            if (_formPool.TryGetValue(prefabPath, out var pool) && pool.Count > 0)
            {
                form = pool.Dequeue();
                Debug.Log($"[UISystem] Reused form from pool: {prefabPath}");

                // 直接初始化并返回
                InitializeAndOpenForm(form, prefabPath, logic, param);
                onProgress?.Invoke(1f);
                onComplete?.Invoke(form);
                yield break;
            }

            onProgress?.Invoke(0.2f);

            // 2. 异步加载预制体
            GameObject prefab = null;
            bool loadComplete = false;
            string loadError = null;

            if (_resourceSystem != null)
            {
                _resourceSystem.Load(prefabPath,
                    onLoadSuccess: (obj) =>
                    {
                        prefab = obj as GameObject;
                        loadComplete = true;
                    },
                    onLoadFailed: (error) =>
                    {
                        loadError = error;
                        loadComplete = true;
                    },
                    onProgress: (p) => onProgress?.Invoke(0.2f + p * 0.6f));
            }
            else
            {
                Debug.LogWarning("[UISystem] ResourceSystem not found, using Resources.Load fallback");
                prefab = Resources.Load<GameObject>(prefabPath);
                loadComplete = true;
            }

            // 等待加载完成
            yield return new WaitUntil(() => loadComplete);

            if (prefab == null)
            {
                Debug.LogError($"[UISystem] Failed to load prefab: {prefabPath}. Error: {loadError}");
                onProgress?.Invoke(1f);
                onComplete?.Invoke(null);
                yield break;
            }

            // 缓存预制体
            if (!_prefabCache.ContainsKey(prefabPath))
            {
                _prefabCache[prefabPath] = prefab;
            }

            onProgress?.Invoke(0.8f);

            // 3. 实例化
            var go = Object.Instantiate(prefab, _popupRoot);
            form = go.GetComponent<UIFormScript>();

            if (form == null)
            {
                Debug.LogError($"[UISystem] Prefab {prefabPath} does not have UIFormScript component");
                Object.Destroy(go);
                onProgress?.Invoke(1f);
                onComplete?.Invoke(null);
                yield break;
            }

            // 4. 初始化并打开
            InitializeAndOpenForm(form, prefabPath, logic, param);

            onProgress?.Invoke(1f);
            onComplete?.Invoke(form);
        }

        /// <summary>
        /// 初始化并打开Form（提取公共逻辑）
        /// </summary>
        private void InitializeAndOpenForm(UIFormScript form, string prefabPath, UILogicBase logic, object param)
        {
            // 分配ID并记录路径
            int formId = _nextFormId++;
            form.SetFormId(formId);
            _formPrefabPaths[formId] = prefabPath;

            // 初始化（传入Logic）
            form.InternalInit(logic, param);

            // 注册到管理器
            _allForms[formId] = form;

            // 根据FormType分类并设置父节点
            var formView = form.GetComponentInChildren<UIFormView>();
            if (formView != null)
            {
                switch (formView.FormType)
                {
                    case UIFormType.Persistent:
                        _persistentForms.Add(form);
                        form.transform.SetParent(_persistentRoot);
                        break;

                    case UIFormType.Popup:
                        _popupForms.Add(form);
                        form.transform.SetParent(_popupRoot);
                        break;

                    case UIFormType.Transition:
                        if (_transitionForm != null)
                        {
                            CloseForm(_transitionForm.FormId, immediate: true);
                        }
                        _transitionForm = form;
                        form.transform.SetParent(_transitionRoot);
                        HideAllUI();
                        break;
                }
            }

            // 打开
            form.InternalOpen();

            // 更新焦点和层级
            UpdateFocus();
            UpdateSortingOrder();

            Debug.Log($"[UISystem] Opened form: {prefabPath} (FormID: {formId}, LogicID: {logic.LogicId})");
        }

        private GameObject LoadPrefab(string path)
        {
            // 先从缓存获取
            if (_prefabCache.TryGetValue(path, out var cached))
            {
                return cached;
            }

            // 同步加载预制体
            GameObject prefab = null;
            if (_resourceSystem != null)
            {
                prefab = _resourceSystem.LoadSync<GameObject>(path);
                if (prefab == null)
                {
                    Debug.LogError($"[UISystem] Failed to load prefab: {path}");
                }
            }
            else
            {
                Debug.LogWarning("[UISystem] ResourceSystem not found, using Resources.Load fallback");
                prefab = Resources.Load<GameObject>(path);
            }

            if (prefab == null)
            {
                Debug.LogError($"[UISystem] Failed to load prefab: {path}");
                return null;
            }

            // 缓存预制体
            _prefabCache[path] = prefab;
            return prefab;
        }
        

        public bool CloseForm(int formId, bool immediate = false)
        {
            if (!_allForms.TryGetValue(formId, out var form))
            {
                Debug.LogWarning($"[UISystem] Form {formId} not found");
                return false;
            }

            // 1. 执行关闭生命周期
            form.InternalClose();

            // 2. 从管理器移除
            _allForms.Remove(formId);
            _formPrefabPaths.TryGetValue(formId, out var prefabPath);
            _formPrefabPaths.Remove(formId);

            var formView = form.GetComponentInChildren<UIFormView>();
            if (formView != null)
            {
                switch (formView.FormType)
                {
                    case UIFormType.Persistent:
                        _persistentForms.Remove(form);
                        break;

                    case UIFormType.Popup:
                        _popupForms.Remove(form);
                        break;

                    case UIFormType.Transition:
                        if (_transitionForm == form)
                        {
                            _transitionForm = null;
                            ShowAllUI();
                        }
                        break;
                }
            }

            // 3. 销毁Logic
            UILogicManager.Instance.DestroyLogic(form.LogicId);

            // 4. 更新焦点和层级
            UpdateFocus();
            UpdateSortingOrder();

            // 5. 处理GameObject（复用或销毁）
            if (form.CanReuse && !immediate && !string.IsNullOrEmpty(prefabPath))
            {
                RecycleForm(form, prefabPath);
            }
            else
            {
                if (immediate)
                {
                    Object.Destroy(form.gameObject);
                }
                else
                {
                    Object.Destroy(form.gameObject, 0.3f);
                }
            }

            Debug.Log($"[UISystem] Closed form: FormID={formId}");

            return true;
        }

        public bool CloseFormByType<T>(bool immediate = false) where T : UIFormScript
        {
            var form = _allForms.Values.FirstOrDefault(f => f is T);
            if (form != null)
            {
                return CloseForm(form.FormId, immediate);
            }

            Debug.LogWarning($"[UISystem] Form of type {typeof(T).Name} not found");
            return false;
        }

        public void CloseAllPopups(bool immediate = false)
        {
            var popups = _popupForms.ToList();
            foreach (var popup in popups)
            {
                CloseForm(popup.FormId, immediate);
            }
        }

        public void CloseAllForms(bool immediate = false)
        {
            var allForms = _allForms.Values.ToList();
            foreach (var form in allForms)
            {
                CloseForm(form.FormId, immediate);
            }
        }

        private void RecycleForm(UIFormScript form, string prefabPath)
        {
            if (!_formPool.ContainsKey(prefabPath))
            {
                _formPool[prefabPath] = new Queue<UIFormScript>();
            }

            form.gameObject.SetActive(false);
            form.transform.SetParent(_uiRoot);
            _formPool[prefabPath].Enqueue(form);

            Debug.Log($"[UISystem] Recycled form to pool: {prefabPath}");
        }

        private void ClearFormPool()
        {
            foreach (var pool in _formPool.Values)
            {
                while (pool.Count > 0)
                {
                    var form = pool.Dequeue();
                    if (form != null)
                    {
                        Object.Destroy(form.gameObject);
                    }
                }
            }
            _formPool.Clear();
            Debug.Log("[UISystem] Cleared form pool");
        }

        public UIFormScript GetForm(int formId)
        {
            _allForms.TryGetValue(formId, out var form);
            return form;
        }

        public T GetForm<T>() where T : UIFormScript
        {
            return _allForms.Values.FirstOrDefault(f => f is T) as T;
        }

        public bool HasForm(int formId)
        {
            return _allForms.ContainsKey(formId);
        }

        public bool HasForm<T>() where T : UIFormScript
        {
            return _allForms.Values.Any(f => f is T);
        }

        // ===== 焦点管理 =====

        private void UpdateFocus()
        {
            // 失焦当前页面（检查是否响应焦点事件）
            if (_focusedForm != null)
            {
                var formView = _focusedForm.GetComponentInChildren<UIFormView>();
                if (formView == null || formView.RespondToFocus)
                {
                    _focusedForm.InternalUnfocus();
                }
                _focusedForm = null;
            }

            // 如果有过场UI，过场UI获得焦点
            if (_transitionForm != null && _transitionForm.IsOpen)
            {
                _focusedForm = _transitionForm;
                var formView = _focusedForm.GetComponentInChildren<UIFormView>();
                if (formView == null || formView.RespondToFocus)
                {
                    _focusedForm.InternalFocus();
                }
                return;
            }

            // 找到最上层的Popup作为焦点（按打开时间排序）
            _focusedForm = _popupForms
                .Where(f => f.IsOpen)
                .OrderByDescending(f => f.OpenTime)
                .FirstOrDefault();

            // 聚焦新页面（检查是否响应焦点事件）
            if (_focusedForm != null)
            {
                var formView = _focusedForm.GetComponentInChildren<UIFormView>();
                if (formView == null || formView.RespondToFocus)
                {
                    _focusedForm.InternalFocus();
                }
            }
        }

        // ===== 层级管理 =====

        private void UpdateSortingOrder()
        {
            // 常驻UI: 0-99
            for (int i = 0; i < _persistentForms.Count; i++)
            {
                if (_persistentForms[i] != null)
                {
                    _persistentForms[i].Canvas.sortingOrder = i;
                }
            }

            // 弹窗UI: 100+，按打开时间排序
            var sortedPopups = _popupForms
                .OrderBy(f => f.OpenTime)
                .ToList();

            for (int i = 0; i < sortedPopups.Count; i++)
            {
                sortedPopups[i].Canvas.sortingOrder = 100 + i * 10;
            }

            // 过场UI: 1000+
            if (_transitionForm != null)
            {
                _transitionForm.Canvas.sortingOrder = 1000;
            }
        }

        // ===== 辅助方法 =====

        private void HideAllUI()
        {
            foreach (var form in _persistentForms)
            {
                if (form != null && form.gameObject.activeSelf)
                {
                    form.gameObject.SetActive(false);
                }
            }

            foreach (var form in _popupForms)
            {
                if (form != null && form.gameObject.activeSelf)
                {
                    form.gameObject.SetActive(false);
                }
            }
        }

        private void ShowAllUI()
        {
            foreach (var form in _persistentForms)
            {
                if (form != null && form.IsOpen)
                {
                    form.gameObject.SetActive(true);
                }
            }

            foreach (var form in _popupForms)
            {
                if (form != null && form.IsOpen)
                {
                    form.gameObject.SetActive(true);
                }
            }
        }

        // ===== 暂停/恢复 =====

        public void PauseAllUI()
        {
            foreach (var form in _allForms.Values)
            {
                form?.InternalPause();
            }

            Debug.Log("[UISystem] All UI paused");
        }

        public void ResumeAllUI()
        {
            foreach (var form in _allForms.Values)
            {
                form?.InternalResume();
            }

            Debug.Log("[UISystem] All UI resumed");
        }
    }

    /// <summary>
    /// UI系统协程运行器
    /// </summary>
    internal class UICoroutineRunner : MonoBehaviour
    {
    }
}



