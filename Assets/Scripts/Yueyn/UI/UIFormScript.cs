using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Yueyn.UI
{
    /// <summary>
    /// UI页面脚本（通用容器，无子类）
    /// 管理页面的生命周期、组件、分辨率适配、渐显渐隐等
    ///
    /// 设计原则：
    /// - 只负责Unity层面的管理（生命周期、组件、事件分发）
    /// - 不包含业务逻辑（业务逻辑由第二层次的BusinessLogic处理）
    /// - 通过 public 方法暴露接口，protected virtual 方法允许子类重写
    /// </summary>
    public class UIFormScript : MonoBehaviour
    {
        // ===== 序列化字段（Inspector可配置） =====

        [Header("Form Configuration")]
        [SerializeField]
        [Tooltip("是否可复用（同一个预制体可以被多次打开）")]
        private bool _canReuse = false;

        // ===== 标识信息 =====

        /// <summary>
        /// 页面唯一ID（由UIManager分配）
        /// </summary>
        public int FormId { get; private set; } = -1;

        /// <summary>
        /// 预制体路径（用于标识页面类型）
        /// </summary>
        public string PrefabPath { get; private set; }

        /// <summary>
        /// 是否可复用
        /// </summary>
        public bool CanReuse => _canReuse;

        /// <summary>
        /// 页面打开时间（用于排序）
        /// </summary>
        public float OpenTime { get; private set; }

        /// <summary>
        /// 初始化参数（保存以便后续使用）
        /// </summary>
        public object Param { get; private set; }

        // ===== 状态标志 =====

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// 是否已打开
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// 是否有焦点
        /// </summary>
        public bool IsFocused { get; private set; }

        // ===== Canvas组件 =====

        /// <summary>
        /// Canvas组件
        /// </summary>
        public Canvas Canvas { get; private set; }

        private CanvasScaler _canvasScaler;
        private GraphicRaycaster _raycaster;
        private CanvasGroup _canvasGroup; // 用于渐显渐隐

        // ===== Component管理 =====

        private List<UIComponent> _components = new List<UIComponent>();
        private List<UIComponent> _cachedComponents = new List<UIComponent>();

        // ===== 分辨率监听 =====

        private Vector2 _lastResolution;
        private Coroutine _resolutionMonitor;

        // ===== 内部方法（由UIManager调用） =====

        /// <summary>
        /// 设置页面ID和路径（由UIManager调用）
        /// </summary>
        internal void SetFormInfo(int id, string prefabPath)
        {
            FormId = id;
            PrefabPath = prefabPath;
        }

        /// <summary>
        /// 初始化（由UIManager调用）
        /// </summary>
        internal void InternalInit(object param)
        {
            if (IsInitialized) return;

            Param = param;

            // 1. 初始化Canvas
            InitializeCanvas();

            // 2. 收集所有UIComponent
            CollectComponents();

            // 3. 初始化所有Component
            DispatchToComponents(c => c.DoStart(this));

            IsInitialized = true;
        }

        /// <summary>
        /// 打开（由UIManager调用）
        /// </summary>
        internal void InternalOpen()
        {
            if (IsOpen) return;

            OpenTime = Time.realtimeSinceStartup;

            // 1. 分发到所有Component
            DispatchToComponents(c => c.DoOpen());

            // 2. 渐显动画
            PlayFadeIn();

            IsOpen = true;
        }

        /// <summary>
        /// 关闭（由UIManager调用）
        /// </summary>
        internal void InternalClose()
        {
            if (!IsOpen) return;

            // 1. 分发到所有Component
            DispatchToComponents(c => c.DoClose());

            // 3. 渐隐动画
            PlayFadeOut();

            // 4. 停止分辨率监听
            if (_resolutionMonitor != null)
            {
                StopCoroutine(_resolutionMonitor);
                _resolutionMonitor = null;
            }

            IsOpen = false;
        }

        /// <summary>
        /// 获得焦点（由UIManager调用）
        /// </summary>
        internal void InternalFocus()
        {
            if (IsFocused) return;

            DispatchToComponents(c => c.DoFocus());

            IsFocused = true;
        }

        /// <summary>
        /// 失去焦点（由UIManager调用）
        /// </summary>
        internal void InternalUnfocus()
        {
            if (!IsFocused) return;

            DispatchToComponents(c => c.DoUnfocus());

            IsFocused = false;
        }

        /// <summary>
        /// 暂停（由UIManager调用）
        /// </summary>
        internal void InternalPause()
        {
            DispatchToComponents(c => c.DoPause());
        }

        /// <summary>
        /// 恢复（由UIManager调用）
        /// </summary>
        internal void InternalResume()
        {
            DispatchToComponents(c => c.DoResume());
        }

        // ===== Canvas初始化和分辨率适配 =====

        /// <summary>
        /// 初始化Canvas组件
        /// </summary>
        private void InitializeCanvas()
        {
            Canvas = GetComponent<Canvas>();
            if (Canvas == null)
            {
                Canvas = gameObject.AddComponent<Canvas>();
            }

            Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            Canvas.worldCamera = UIManager.Instance.UICamera;

            _canvasScaler = GetComponent<CanvasScaler>();
            if (_canvasScaler == null)
            {
                _canvasScaler = gameObject.AddComponent<CanvasScaler>();
            }

            _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _canvasScaler.referenceResolution = new Vector2(1920, 1080);
            _canvasScaler.matchWidthOrHeight = 0.5f;

            _raycaster = GetComponent<GraphicRaycaster>();
            if (_raycaster == null)
            {
                _raycaster = gameObject.AddComponent<GraphicRaycaster>();
            }

            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // 启动分辨率监听
            _resolutionMonitor = StartCoroutine(MonitorResolution());
        }

        /// <summary>
        /// 监听分辨率变化
        /// </summary>
        private IEnumerator MonitorResolution()
        {
            _lastResolution = new Vector2(Screen.width, Screen.height);

            while (true)
            {
                yield return new WaitForSeconds(0.5f);

                var currentResolution = new Vector2(Screen.width, Screen.height);
                if (currentResolution != _lastResolution)
                {
                    _lastResolution = currentResolution;
                    DispatchToComponents(c => c.OnResolutionChanged(currentResolution));
                }
            }
        }

        // ===== 渐显渐隐动画 =====

        /// <summary>
        /// 渐显动画
        /// </summary>
        private void PlayFadeIn()
        {
            gameObject.SetActive(true);

            if (_canvasGroup != null)
            {
                StopAllCoroutines();
                StartCoroutine(FadeCoroutine(0, 1, 0.3f));
            }
        }

        /// <summary>
        /// 渐隐动画
        /// </summary>
        private void PlayFadeOut()
        {
            if (_canvasGroup != null)
            {
                StopAllCoroutines();
                StartCoroutine(FadeCoroutine(1, 0, 0.3f, () => gameObject.SetActive(false)));
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 渐变协程
        /// </summary>
        private IEnumerator FadeCoroutine(float from, float to, float duration, Action onComplete = null)
        {
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            _canvasGroup.alpha = to;

            onComplete?.Invoke();
        }

        // ===== Component管理 =====

        /// <summary>
        /// 收集所有UIComponent
        /// </summary>
        private void CollectComponents()
        {
            _components.Clear();
            GetComponentsInChildren(true, _components);
            // 按优先级排序
            _components = _components.OrderBy(c => c.Priority).ToList();
        }

        /// <summary>
        /// 运行时动态添加Component（会补偿生命周期）
        /// </summary>
        public void AddComponent(UIComponent component)
        {
            if (_components.Contains(component))
                return;

            _components.Add(component);
            _components = _components.OrderBy(c => c.Priority).ToList();

            // 生命周期补偿
            if (IsInitialized)
            {
                component.DoStart(this);
            }

            if (IsOpen)
            {
                component.DoOpen();
            }

            if (IsFocused)
            {
                component.DoFocus();
            }
        }

        /// <summary>
        /// 移除Component
        /// </summary>
        public void RemoveComponent(UIComponent component)
        {
            if (!_components.Contains(component))
                return;

            // 先执行关闭生命周期
            if (IsFocused)
            {
                component.DoUnfocus();
            }

            if (IsOpen)
            {
                component.DoClose();
            }

            _components.Remove(component);
        }

        /// <summary>
        /// 分发生命周期事件到所有Component
        /// </summary>
        private void DispatchToComponents(Action<UIComponent> action)
        {
            _cachedComponents.Clear();
            _cachedComponents.AddRange(_components);

            foreach (var component in _cachedComponents)
            {
                if (component != null)
                {
                    action(component);
                }
            }
        }
    }
}



