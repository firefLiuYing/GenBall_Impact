# UI 白模设计规范 — 布局/字体/颜色/间距

> **目标读者**: AI（Claude Code）。本文档提供创建 UI prefab 时的**确定性布局参数**。
>
> **何时读**: 使用 MCP 工具 `unity_create_prefab` / `unity_add_gameobject` / `unity_set_component_property` 搭建 prefab 结构时。
>
> **设计原则**:
> - 所有数值基于 **4px 基础网格**，确保视觉节奏一致
> - 深色白模方案：柔和暗色调，适合长时间屏幕工作
> - 锚点按位置分类：利于分辨率适配
> - 层级尽量扁平：便于程序化搭建

---

## 1. 字体层级

等比缩放，公比约 1.25–1.33×。全部使用 `LegacyRuntime.ttf`。

| 角色 | Font Size | Font Style | 颜色 | 使用场景 |
|------|-----------|------------|------|---------|
| Hero / 主标题 | **48** | Bold | `#e2e2e2` | 启动画面标题、大 Banner |
| Title / 页标题 | **36** | Normal | `#e2e2e2` | Form 标题（如"选择存档"） |
| Body / 正文 | **28** | Normal | `#e2e2e2` | HUD 数值、主要内容 |
| Button / 按钮 | **22** | Normal | `#e2e2e2` | 按钮标签文字 |
| Info / 辅助 | **16** | Normal | `#9999aa` | 说明文字、场景名 |
| Caption / 元数据 | **12** | Normal | `#555566` | 时间戳、版本号 |

**统一 Text 设置**:
- `RichText: true`
- `LineSpacing: 1`
- `HorizontalOverflow: Overflow`
- `Alignment`: 标题居中 `MiddleCenter`，数值/信息左对齐 `MiddleLeft`

---

## 2. 颜色系统

深色白模方案——柔和暗色调，不刺眼。

### 背景层级

```
由深到浅 = 由远到近（视觉深度）

#12121f  ← Canvas 底色（最远）
#1a1a2e  ← Panel/卡片面
#1e1e32  ← 输入框/列表项
#282840  ← Hover/选中态
```

| 名称 | Hex | 用途 |
|------|-----|------|
| Canvas 底 | `#12121f` | Canvas 背景色 |
| 卡片/面板 | `#1a1a2e` | 弹窗主体、Panel |
| 控件面 | `#1e1e32` | 按钮 Normal、输入框背景 |
| 悬停态 | `#282840` | 按钮 Highlighted |
| 按下态 | `#141428` | 按钮 Pressed |
| 全屏遮罩 | `#12121fCC` | 弹窗遮罩（a≈0.8） |
| 半透遮罩 | `#12121f80` | 轻遮罩（a≈0.5） |

### 文字层级

| 名称 | Hex | 用途 |
|------|-----|------|
| Primary | `#e2e2e2` | 标题、正文、重要数值 |
| Secondary | `#9999aa` | 辅助说明、次要标签 |
| Disabled | `#555566` | 禁用态、占位符、元数据 |

### 功能色（白模可选）

| 名称 | Hex | 用途 |
|------|-----|------|
| Accent | `#5b8dee` | 主操作按钮、选中态 |
| Positive | `#4ec9a4` | 成功/正向（HP 满、升级） |
| Warning | `#eeb55b` | 警告（HP 低、冷却中） |
| Danger | `#ee5b5b` | 危险（死亡、删除确认） |

> 白模阶段可以不使用功能色，保持灰度即可。功能色留给实际业务逻辑。

### MCP 参数格式

```jsonc
{"componentType": "Image",   "property": "m_Color", "value": "#1a1a2eFF"}  // 面板
{"componentType": "Text",    "property": "m_Color", "value": "#e2e2e2FF"}  // 主文字
{"componentType": "Image",   "property": "m_Color", "value": "#12121fCC"}  // 遮罩
```

---

## 3. 间距体系（4px 网格）

所有间距、尺寸对齐到 **4px 的倍数**。

### 间距尺度

| Token | px | 用途 |
|-------|-----|------|
| `xs` | 4 | 紧密排列（图标-文字） |
| `sm` | 8 | 相关元素间距 |
| `md` | **16** | 标准 padding、同级间距 |
| `lg` | 24 | 组间间距 |
| `xl` | 32 | 大区块间距 |
| `2xl` | 48 | 页面级边距、安全区 |

### 控件尺寸

| 类型 | 尺寸 (W×H) | 说明 |
|------|-----------|------|
| Button（主要） | **200 × 56** | 宽高比 3.6:1，1-2 个按钮 |
| Button（紧凑） | **160 × 44** | 次要操作、工具栏按钮 |
| Button（小） | **120 × 36** | 图标旁标签 |
| InputField | **320 × 44** | 单行输入 |
| Text 块（单行） | **160 × 32** | HUD 数值显示 |

### 垂直布局步进

以 Popup 中 3 个按钮为例：
```
Btn1:  anchoredPosition.y =  72      ← 56÷2 + 72 = 100 (第一个按钮中心)
Btn2:  anchoredPosition.y =   0      ← 正中心
Btn3:  anchoredPosition.y = -72      ← -56÷2 - 72 = -100
步进: 72px = 按钮高(56) + 间距 md(16)
```

### 弹窗安全区

```
┌──────────────────────────────────┐
│         2xl (48px)               │  ← 顶部安全边距
│    ┌──────────────────────┐      │
│ lg │  TxtTitle (36px)     │ lg   │  ← 左右安全边距
│    ├──────────────────────┤      │
│    │                      │      │
│    │   内容区              │      │
│    │                      │      │
│    ├──────────────────────┤      │
│    │   Btn1  Btn2  Btn3   │      │  ← 按钮行
│    └──────────────────────┘      │
│         2xl (48px)               │  ← 底部安全边距
└──────────────────────────────────┘
```

---

## 4. 锚点策略

**按控件位置分类选择锚点**——这样在 21:9 / 16:9 / 4:3 下都能正确适配。

| 位置 | AnchorMin | AnchorMax | Pivot | 适用 |
|------|-----------|-----------|-------|------|
| **左上** | (0, 1) | (0, 1) | (0, 1) | 血条、击杀、等级 |
| **右上** | (1, 1) | (1, 1) | (1, 1) | 弹药、小地图、货币 |
| **底部居中** | (0.5, 0) | (0.5, 0) | (0.5, 0) | 技能栏、快捷栏 |
| **居中** | (0.5, 0.5) | (0.5, 0.5) | (0.5, 0.5) | Popup、对话框、轮盘 |
| **全屏拉伸** | (0, 0) | (1, 1) | (0.5, 0.5) | 遮罩背景 |
| **填充父容器** | (0, 0) | (1, 1) | (0.5, 0.5) | Button 的 Text 子节点 |

### 位置计算

```csharp
// 左上角（HUD 元素）
anchorMin = (0, 1), anchorMax = (0, 1), pivot = (0, 1)
anchoredPosition = (48, -48)   // 距离左上角 48px

// 右上角
anchorMin = (1, 1), anchorMax = (1, 1), pivot = (1, 1)
anchoredPosition = (-48, -48)  // 距离右上角 48px

// 居中 Popup
anchorMin = (0.5, 0.5), anchorMax = (0.5, 0.5), pivot = (0.5, 0.5)
anchoredPosition = (0, 0)      // 正中心

// HUD 数值行（多行左对齐时上方偏移）
anchoredPosition = (0, -48)    // 中心偏上
```

### MCP 参数格式

```jsonc
// 左上角锚点
{"componentType":"RectTransform", "property":"anchorMin", "value":"(0, 1)"}
{"componentType":"RectTransform", "property":"anchorMax", "value":"(0, 1)"}
{"componentType":"RectTransform", "property":"m_Pivot", "value":"(0, 1)"}
{"componentType":"RectTransform", "property":"m_AnchoredPosition", "value":"(48, -48)"}
```

### CanvasScaler

```
Reference Resolution: 1920 × 1080
Screen Match Mode: MatchWidthOrHeight
Match: 0.5            ← 宽高均衡匹配
Reference Pixels Per Unit: 100
```

---

## 5. 布局模板

### 5A. Popup 弹窗

```
MyForm (Form prefab, Popup)
│   CanvasScaler: 1920×1080, Match=0.5
│
├── ImgBg (Image)  ← 全屏遮罩
│   anchorMin=(0,0) anchorMax=(1,1)
│   color=#12121fCC (a≈0.8)
│
├── RectPanel (RectTransform)  ← 弹窗主体卡片
│   anchorMin=(0.5,0.5) anchorMax=(0.5,0.5) pivot=(0.5,0.5)
│   sizeDelta=(560, 400)        ← Default 弹窗尺寸
│   anchoredPosition=(0, 0)
│
│   ├── TxtTitle (Text)
│   │   fontSize=36, alignment=MiddleCenter
│   │   anchoredPosition=(0, 152)   ← Panel 顶部偏下
│   │
│   ├── RectContent (RectTransform) ← 内容区
│   │   sizeDelta=(496, 200)        ← Panel 宽 - 2×32
│   │   anchoredPosition=(0, 0)
│   │
│   └── RectButtonRow (RectTransform) ← 按钮行
│       anchoredPosition=(0, -144)
│       │
│       ├── BtnConfirm (Button+Image)
│       │   sizeDelta=(200, 56), color=#1e1e32
│       │   anchoredPosition=(0, 0)        ← 居中（单按钮）
│       │   └── Text (Legacy), fontSize=22, text="确认"
│       │
│       └── BtnCancel (Button+Image)
│           sizeDelta=(200, 56), color=#1e1e32
│           anchoredPosition=(-116, 0)     ← 双按钮时左偏
│           └── Text (Legacy), fontSize=22, text="取消"
```

**弹窗尺寸参考**：

| 类型 | Panel sizeDelta | 说明 |
|------|----------------|------|
| 简单确认 | (400, 240) | 标题 + 文字 + 1-2 按钮 |
| 标准弹窗 | (560, 400) | 标题 + 内容 + 按钮行 |
| 大弹窗 | (720, 520) | 复杂内容（列表/表单） |

### 5B. Persistent HUD

```
MainHudForm (Form prefab, Persistent)
│   CanvasScaler: 1920×1080, Match=0.5
│
├── TxtHealth (Text)  ← 左上角
│   anchorMin=(0,1) anchorMax=(0,1) pivot=(0,1)
│   anchoredPosition=(48, -48)
│   sizeDelta=(200, 32), fontSize=28, alignment=MiddleLeft
│
├── TxtKillPoints (Text)  ← 左上角，TxtHealth 下方
│   anchorMin=(0,1) anchorMax=(0,1) pivot=(0,1)
│   anchoredPosition=(48, -96)     ← 48 + 32 + 16(gap) = 96
│   sizeDelta=(200, 32), fontSize=28
│
├── TxtAmmo (Text)  ← 右下角
│   anchorMin=(1,0) anchorMax=(1,0) pivot=(1,0)
│   anchoredPosition=(-48, 48)
│   sizeDelta=(200, 32), fontSize=28, alignment=MiddleRight
│
├── ImgCrosshair (Image)  ← 正中心
│   anchorMin=(0.5,0.5) anchorMax=(0.5,0.5) pivot=(0.5,0.5)
│   sizeDelta=(32, 32), anchoredPosition=(0, 0)
│
└── InteractTipPart (Part)  ← 自动发现的子 Part
    anchorMin=(0.5,0) anchorMax=(0.5,0) pivot=(0.5,0)
    anchoredPosition=(0, 160)     ← 底部上方
```

### 5C. Part（容器 + 动态子组件）

```
MyPart (Part prefab, RectTransform only, 无 Canvas)
│
├── ImgBg (Image)  ← 可选背景
│   anchorMin=(0,0) anchorMax=(1,1) sizeDelta=(0,0)
│   color=#1a1a2e
│
└── RectSlotContainer (RectTransform)  ← 动态子 Part 挂载点
    anchorMin=(0,0) anchorMax=(1,1)    ← 填充父容器
    sizeDelta=(0,0)                      ← 由父容器决定
    → 代码中：p.ParentTransform = BoundView.RectSlotContainer
```

### 5D. 列表项（代码动态创建）

当需要代码创建列表项时（如存档槽位），参考参数：

```csharp
// 单个 slot 条目
entryRt.sizeDelta = new Vector2(0, 64);        // 高度对齐 4px 网格
entryRt.anchorMin = new Vector2(0, 1);
entryRt.anchorMax = new Vector2(1, 1);
entryRt.pivot = new Vector2(0.5f, 1);
entryRt.anchoredPosition = new Vector2(0, -index * 80);  // 64 + 16(gap) = 80

// 槽位背景
bgImage.color = new Color(0.102f, 0.102f, 0.18f, 0.8f);  // #1a1a2eCC

// 槽位文字 — Primary
indexTxt.fontSize = 22;  // Button 级
indexTxt.color = new Color(0.886f, 0.886f, 0.886f);  // #e2e2e2

// 槽位文字 — Info（场景名/时间）
infoTxt.fontSize = 16;  // Info 级
infoTxt.color = new Color(0.6f, 0.6f, 0.667f);  // #9999aa

// 槽位文字 — Caption（创建时间）
timeTxt.fontSize = 12;  // Caption 级
timeTxt.color = new Color(0.333f, 0.333f, 0.4f);  // #555566
```

---

## 6. MCP 工具创建流程

### 完整示例：创建 Popup 弹窗

```jsonc
// Step 1: 创建 prefab
unity_create_prefab {
  "path": "Assets/AssetBundles/UI/MyForm/MyForm.prefab",
  "viewType": "Form"
}

// Step 2: 添加控件
// 2a. 全屏遮罩
unity_add_gameobject {
  "prefabPath": "Assets/AssetBundles/UI/MyForm/MyForm.prefab",
  "name": "ImgBg"
}
unity_set_component_property {
  "path": "ImgBg", "componentType": "Image",
  "property": "m_Color", "value": "#12121fCC"
}

// 2b. 弹窗主体
unity_add_gameobject {
  "prefabPath": "Assets/AssetBundles/UI/MyForm/MyForm.prefab",
  "name": "RectPanel"    // Rect 前缀 = 纯 RectTransform，不添加其他组件
}
unity_set_component_property {
  "path": "RectPanel", "componentType": "RectTransform",
  "property": "m_SizeDelta", "value": "(560, 400)"
}

// 2c. 标题
unity_add_gameobject {
  "prefabPath": "Assets/AssetBundles/UI/MyForm/MyForm.prefab",
  "name": "TxtTitle",
  "parentPath": "RectPanel"
}
unity_set_component_property {
  "path": "RectPanel/TxtTitle", "componentType": "Text",
  "property": "m_FontSize", "value": "36"
}
unity_set_component_property {
  "path": "RectPanel/TxtTitle", "componentType": "RectTransform",
  "property": "m_AnchoredPosition", "value": "(0, 152)"
}

// 2d. 确认按钮
unity_add_gameobject {
  "prefabPath": "Assets/AssetBundles/UI/MyForm/MyForm.prefab",
  "name": "BtnConfirm",
  "parentPath": "RectPanel"
}
unity_set_component_property {
  "path": "RectPanel/BtnConfirm", "componentType": "RectTransform",
  "property": "m_SizeDelta", "value": "(200, 56)"
}
unity_set_component_property {
  "path": "RectPanel/BtnConfirm", "componentType": "RectTransform",
  "property": "m_AnchoredPosition", "value": "(0, 0)"
}

// Step 3: 代码生成
unity_generate_ui_code {
  "prefabPath": "Assets/AssetBundles/UI/MyForm/MyForm.prefab",
  "viewType": "Form",
  "formType": "Popup"
}
```

### 前缀速查

| 前缀 | 自动添加组件 | 用途 |
|------|-------------|------|
| `Btn` | Button + Image | 可点击按钮 |
| `Txt` | Text | 文字标签 |
| `Img` | Image | 图片/图标/背景 |
| `RawImg` | RawImage | Raw 纹理 |
| `Rect` | (仅 RectTransform) | 容器/面板（无额外组件） |
| `Input` | InputField + Image | 文本输入 |
| `Scroll` | ScrollRect | 滚动视图 |
| `HLayout` | HorizontalLayoutGroup | 水平自动排列（尽量不用） |
| `VLayout` | VerticalLayoutGroup | 垂直自动排列（列表容器） |

### 程序化搭建原则

1. **不要嵌套过深**。Btn 放在 RectPanel 下即可，不需要额外的 ButtonRow wrapper（除非需要 LayoutGroup）。
2. **Panel 做定位容器**。弹窗内所有控件以 Panel 为参考定位，Panel 本身居中。
3. **尽量避免 LayoutGroup**。用绝对定位更可控，分辨率适配靠 CanvasScaler + 锚点。
4. **先加控件，设置属性，最后生成代码**。

---

## 关联文档

- `ui-ai-guide.md` — UI 代码决策树、模板、通信方式选择
- `ui-architecture.md` — 完整架构参考
- `../skills/create-ui/SKILL.md` — /create-ui 工作流
