---
description: GenBall_Impact 项目中 UI 开发的强制规范，包括框架选择、文本组件、命名约定等。
alwaysApply: false
enabled: true
updatedAt: 2026-05-13T14:32:00.000Z
provider: ""
---

# UI 框架开发规范

## 目标
确保所有新 UI 使用新框架（MVP 架构），避免使用已废弃的 TextMeshPro 组件，保持命名一致性。

## 触发条件
- 文件路径匹配 `**/UI/**/*.cs` 或 `**/GenBall/UI/**/*.cs`
- 用户提到"UI"、"界面"、"页面"、"弹窗"
- 创建或修改 UI 相关代码

## 行为约束

### 1. 新 UI 必须使用新框架（必须）
- **必须**使用 `UISystemDefault`（MVP 架构）
- **禁止**使用旧 `UIManager`（MVVM 架构）
- **必须**Logic 层继承 `UIFormLogic` 或 `UILogicBase`
- **必须**View 层继承 `UIFormView` 或 `UIComponent`

### 2. 禁用 TextMeshPro（必须）
- **必须**所有 UI 文本组件使用 `UnityEngine.UI.Text`（Legacy Text）
- **禁止**使用 `TMP_Text` / `TextMeshProUGUI` / `TMPro` 相关类型
- **禁止**在代码中引用 `TMPro` 命名空间

### 3. 命名约定（必须）
- **必须**Logic 层文件命名：`XxxLogic.cs`（如 `TestFormLogic.cs`）
- **必须**View 层文件命名：`XxxView.cs`（如 `TestFormView.cs`）
- **禁止**使用旧命名方式（`FormName.cs` + `FormNameVm.cs`）

### 4. 页面类型选择（建议）
- **建议**常驻 UI（血条、HUD）使用 `UIFormType.Persistent`
- **建议**弹窗（背包、设置）使用 `UIFormType.Popup`
- **建议**过场界面（加载）使用 `UIFormType.Transition`

## 示例

### ✅ 正确示例：新框架 UI
```csharp
// TestFormLogic.cs
namespace GenBall.UI {
    public class TestFormLogic : UIFormLogic {
        protected override string PrefabPath => "Assets/Prefabs/UI/TestForm.prefab";

        internal override void BindView(UIFormScript form) {
            base.BindView(form);
            if (View is TestFormView testView) {
                testView.SetLogic(this);
            }
        }

        public override void SetViewData(object param) {
            if (View is TestFormView testView) {
                testView.SetTitle($"Title - {param}");
            }
        }

        public void OnCloseButtonClicked() {
            CloseForm();
        }
    }
}

// TestFormView.cs
using UnityEngine.UI; // ✅ 使用 Legacy Text

namespace GenBall.UI {
    public class TestFormView : UIFormView {
        [SerializeField] private Text titleText; // ✅ UnityEngine.UI.Text
        [SerializeField] private Button closeButton;

        private TestFormLogic _logic;

        public void SetLogic(TestFormLogic logic) => _logic = logic;

        public void SetTitle(string title) {
            titleText.text = title;
        }

        private void Awake() {
            closeButton.onClick.AddListener(() => _logic?.OnCloseButtonClicked());
        }
    }
}

// 使用方式
var logic = UILogicManager.Instance.CreateLogic<TestFormLogic>();
logic.OpenFormAsync("Hello World");
```

### ❌ 错误示例
```csharp
// ❌ 错误 1：使用旧框架
public class TestForm : FormBase { /* 旧 MVVM 架构 */ }

// ❌ 错误 2：使用 TextMeshPro
using TMPro; // ❌ 禁止
public class TestFormView : UIFormView {
    [SerializeField] private TMP_Text titleText; // ❌ 禁止
}

// ❌ 错误 3：命名不规范
public class TestFormVm : VmBase { /* 旧命名方式 */ }

// ❌ 错误 4：直接使用 UIManager
UIManager.Instance.OpenForm("TestForm"); // ❌ 旧 API
```

## 例外情况
- 旧 UI 代码维护时可继续使用旧框架，但不得新建
- 非 UI 文本（如 3D 世界空间文本）可根据需求选择组件
