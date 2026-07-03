# Aesir Architecture

> 面向团结引擎 / Unity 的渐进式 MVP 架构框架，以 Unity 原生特性为一等公民。

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE.md)
[![Version](https://img.shields.io/badge/version-0.3.1-blue.svg)](./CHANGELOG.md)
[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black.svg)](https://unity.com/)

## 概述

AesirArchitecture（RAA）是一个以 **Unity 原生优先** 为核心理念的架构框架。它不构建与引擎平行的自建体系，而是深度绑定 Unity 的 PlayerLoop、ScriptableObject、Editor API 等原生能力，在保持轻量的同时为中小型到中大型项目提供清晰的 MVP / MVC 分层。

### 核心特性

- **PlayerLoop 原生生命周期** — 通过 `AesirArchitectureLifeCycle` 将自定义子系统注入 Unity PlayerLoop，提供 `BeforeUpdate` / `AfterUpdate` 帧回调，无需 MonoBehaviour
- **能力接口组合** — 通过 `ICanGetModel`、`ICanExecuteCommand`、`ICanAddListener` 等能力标记接口组合出 `IModel` / `IService` / `IView` / `IController` / `IPresenter`，按需暴露能力
- **命令模式** — `ICommand` / `IAsyncCommand` 负责写操作，支持同步与异步
- **ObservableValue 响应式属性** — Model 持有可写实例，View 通过 `IReadOnlyObservableValue<out T>` 协变只读访问，保障层级安全
- **MiniEventBus 类型事件总线** — 按事件类型注册/发布，支持自动移除监听句柄与多种生命周期绑定（GameObject 销毁、场景卸载等）
- **运行时错误日志** — `GetModel<T>()` / `GetService<T>()` 在目标未注册时抛出含调用者类型和目标类型信息的异常，替代前置依赖校验，兼容运行时替换 Model 的调试模式
- **AbstractSubmodule 统一子模块生命周期** — Model 和 Service 的公共生命周期逻辑提取到 `AbstractSubmodule` 基类，消除代码重复
- **GenericLocator 泛型定位器** — 按类型注册/查询的通用定位器，替代旧版 Container，支持全局单例
- **Domain Reload 安全** — 静态变量通过 `[RuntimeInitializeOnLoadMethod]` 显式重置，反复进出 Play Mode 无残留
- **纯 C# 核心 + MonoBehaviour 适配** — 框架核心为纯 C# 对象，Engine 层不依赖任何 Component 层类型，`AesirView<T>` / `MonoView<T>` / `AesirViewController<T>` 作为 MonoBehaviour 适配层
- **MVC + MVP 双模式** — `IController` 适合快速开发，`IPresenter` 提供更严格的 Model-View 隔离

### 与 QFramework 的差异

| 维度 | QFramework | AesirArchitecture |
|------|-----------|-------------------|
| 生命周期 | MonoBehaviour 事件回调 | PlayerLoop 原生注入（BeforeUpdate / AfterUpdate） |
| 架构根 | 泛型单例 `Architecture<T>` | 泛型静态单例 `AbstractContext<T>` + `GenericLocator` 全局定位 |
| 可观察属性 | `BindableProperty<T>` | `ObservableValue<T>` + `IReadOnlyObservableValue<out T>` 协变只读 |
| 事件通信 | 纯 C# TypeEvent | 纯 C# MiniEventBus + 委托（不使用 `event` 关键字） |
| 日志 | `Debug.Log` | `AesirArchitectureLog` 条件编译统一日志 |
| 静态状态 | 无 Domain Reset 保障 | `[RuntimeInitializeOnLoadMethod]` 显式重置 |
| 表现层 | 无明确抽象 | `IView` 表现层接口 + `IController` / `IPresenter` 双模式 |

## 安装

### 通过 UPM（Git URL）

在 Unity Package Manager 中通过 Git URL 安装：

```
https://github.com/yuumixcode/aesir-architecture.git
```

### 手动安装

将本包目录复制到项目的 `Packages/` 目录下即可。

## 快速开始

### 1. 定义 Context

```csharp
using Runestone.AesirArchitecture;

public class CounterContext : AbstractContext<CounterContext>
{
    protected override void Configure()
    {
        RegisterModel<ICounterModel>(new CounterModel());
    }
}
```

### 2. 定义 Model

```csharp
public interface ICounterModel : IModel
{
    IReadOnlyObservableValue<int> Count { get; }
    void Increase();
    void Decrease();
    void Reset();
}

public sealed class CounterModel : AbstractModel, ICounterModel
{
    readonly ObservableValue<int> _count = new ObservableValue<int>(0);

    public IReadOnlyObservableValue<int> Count => _count;

    public void Increase() => _count.Value++;
    public void Decrease() => _count.Value--;
    public void Reset() => _count.Value = 0;

    protected override void OnInitialize() { }
}
```

### 3. 定义 View（MVC 模式）

```csharp
public class UICounterMvcPanel : AesirView<CounterContext>
{
    [SerializeField] Text countText;
    [SerializeField] Button increaseButton;

    ICounterModel _model;
    ICounterController _ctrl;

    void Awake()
    {
        _model = this.GetModel<ICounterModel>();
        _model.Count.AddListener(UpdateCountText)
                   .RemoveListenerWhenGameObjectOnDestroyed(gameObject);
        _ctrl = new CounterController(_model);
    }

    void OnEnable() => increaseButton.onClick.AddListener(_ctrl.Increase);
    void OnDisable() => increaseButton.onClick.RemoveListener(_ctrl.Increase);

    public void UpdateCountText(int count) => countText.text = count.ToString();
}
```

### 4. 使用 Command

```csharp
// 定义命令
public class AddScoreCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        var model = this.GetModel<IScoreModel>();
        model.AddScore(10);
    }
}

// 执行命令
this.ExecuteCommand<AddScoreCommand>();
```

### 5. 使用事件总线

```csharp
// 定义事件参数
public struct ScoreChangedEvent : IEventArgs
{
    public int NewScore;
}

// 添加监听
this.AddListener<ScoreChangedEvent>(e => Debug.Log($"Score: {e.NewScore}"))
    .RemoveListenerWhenGameObjectOnDestroyed(gameObject);

// 发布
this.InvokeEvent(new ScoreChangedEvent { NewScore = 100 });
```

## 架构总览

```
┌─────────────────────────────────────────────────┐
│               AbstractContext<T>                 │
│     (泛型静态单例 + Domain Reset)                │
│                                                  │
│  ┌──────────┐  ┌──────────┐  ┌───────────────┐ │
│  │  Models  │  │ Services │  │ MiniEventBus  │ │
│  │          │  │          │  │   (Global)    │ │
│  └──────────┘  └──────────┘  └───────────────┘ │
│  ┌──────────────────────────────────────────────┐│
│  │       GenericLocator<T> (类型定位器)         ││
│  └──────────────────────────────────────────────┘│
└──────────────────┬──────────────────────────────┘
                   │ 能力接口组合
     ┌─────────────┼─────────────┐
     ▼             ▼             ▼
┌─────────┐ ┌───────────┐ ┌────────────┐
│  IView  │ │IController│ │ IPresenter │
│         │ │  (MVC)    │ │   (MVP)    │
└─────────┘ └───────────┘ └────────────┘
     │             │             │
     ▼             ▼             ▼
┌──────────────────────────────────────┐
│  AesirView<T> / MonoView<T>          │
│  AesirViewController<T>              │
│        (MonoBehaviour 适配层)          │
└──────────────────────────────────────┘
                   │
                   ▼
┌──────────────────────────────────────┐
│     AesirArchitecturePlayerLoop       │
│  (PlayerLoop 原生注入: Before/After)   │
└──────────────────────────────────────┘
```

### 能力矩阵

| 模块 | GetModel | GetService | ExecuteCommand | AddListener | InvokeEvent | Initialize | Dispose |
|------|:--------:|:---------:|:--------------:|:---------:|:----------:|:----------:|:-------:|
| **IModel** | ✓ | | | | ✓ | ✓ | ✓ |
| **IService** | ✓ | ✓ | | ✓ | ✓ | ✓ | ✓ |
| **IView** | ✓ | ✓ | | ✓ | ✓ | | |
| **IController** | ✓ | ✓ | ✓ | | | | |
| **IPresenter** | ✓ | ✓ | ✓ | ✓ | ✓ | | ✓ |

## 项目结构

```
cn.runestone.aesir.architecture/
├── package.json
├── README.md
├── CHANGELOG.md
├── LICENSE.md
├── .gitignore
├── Runtime/
│   ├── Runestone.AesirArchitecture.asmdef
│   ├── Engine/                    # 纯 C# + 使用 UnityEngine API（不依赖 MonoBehaviour）
│   │   ├── Common/
│   │   │   ├── AesirArchitectureLog.cs         # 统一日志
│   │   │   ├── AesirArchitecturePlayerLoop.cs  # PlayerLoop 注入
│   │   │   ├── AssemblyInfo.cs                 # InternalsVisibleTo 声明
│   │   │   └── ResetStaticsAssistant.cs        # 静态变量重置助手
│   │   ├── Core/
│   │   │   ├── Context/           # IContext, AbstractContext<T>
│   │   │   ├── Modules/           # IModel, IService, IView, IController, IPresenter + Abstract 基类
│   │   │   │   ├── Interfaces/    # 模块接口
│   │   │   │   └── Abstracts/     # AbstractSubmodule, AbstractModel, AbstractService
│   │   │   └── Capabilities/      # Capabilities.cs (ICan* 接口) + CapabilityExtensions.cs (扩展方法)
│   │   ├── Event/                 # MiniEventBus, MiniEvent<T>, AutoRemoveListenerHandle
│   │   ├── Observable/           # ObservableValue<T>, IReadOnlyObservableValue<T>
│   │   ├── Locator/              # GenericLocator<T>, IGenericLocator<T>
│   │   └── Utilities/            # PlayerLoopUtility
│   ├── Component/                # MonoBehaviour 组件（依赖 MonoBehaviour）
│   │   ├── Common/
│   │   │   ├── AesirArchitecture.cs       # 框架 MonoBehaviour 单例入口
│   │   │   └── AesirMonoBehaviour.cs      # Odin 自动适配基类
│   │   ├── Core/
│   │   │   ├── AesirView.cs              # Odin 适配 View 基类
│   │   │   ├── MonoView.cs               # 纯 MonoBehaviour View 基类
│   │   │   └── AesirViewController.cs    # View + Controller 双角色基类
│   │   ├── Event/
│   │   │   ├── RemoveListenerTrigger.cs  # 自动移除监听触发器基类
│   │   │   ├── RemoveListenerOnDestroyTrigger.cs
│   │   │   ├── RemoveListenerOnDisableTrigger.cs
│   │   │   ├── RemoveListenerOnSceneUnloadedTrigger.cs
│   │   │   └── RemoveListenerExtensions.cs
│   │   └── ScriptableObject/
│   │       └── AesirScriptableObject.cs  # Odin 自动适配 SO 基类
│   └── OdinIntergration/         # 独立程序集（依赖 Odin Inspector）
│       └── Runestone.AesirArchitecture.OdinIntegration.asmdef
├── Editor/
│   ├── Runestone.AesirArchitecture.Editor.asmdef
│   ├── Common/
│   │   └── EnsureAesirArchitectureDefine.cs  # 编译符号管理
│   ├── Utilities/
│   │   └── ScriptingSymbolUtility.cs
│   └── OdinIntegration/          # Odin Inspector 集成（可选）
│       └── Runestone.AesirArchitecture.Editor.OdinIntegration.asmdef
├── Tests/
│   ├── Runtime/
│   │   └── Runestone.AesirArchitecture.Tests.asmdef
│   └── Editor/
│       └── Runestone.AesirArchitecture.Tests.Editor.asmdef
├── Samples~/
│   ├── Counter-MVC/             # MVC 模式计数器 Demo
│   ├── Counter-MVP/             # MVP 模式计数器 Demo
│   ├── ObservableValue/         # ObservableValue Inspector 演示（Odin Inspector）
│   └── MiniEvent/               # MiniEvent 使用案例
└── Documentation~/
    ├── aesir-architecture.md    # 主手册
    ├── Books/                   # 参考电子书
    ├── FAQ/                     # 常见问题
    ├── Manuals/                 # 模块手册
    └── Rules/                   # 代码规范
```

## 设计原则

1. **Unity 原生优先** — 优先使用 Unity 引擎能力（PlayerLoop、ScriptableObject、Editor API），而非自建平行体系
2. **Domain Reload 兼容（铁律）** — 静态变量必须显式重置，反复进出 Play Mode 无残留
3. **低 MonoBehaviour 依赖** — 核心框架由纯 C# 对象组成，MonoBehaviour 仅作适配层
4. **渐进式** — 小项目轻量使用，大项目逐步扩展，不强制全量引入
5. **SO 与纯代码双通道**（规划中） — 每个 SO 能力都有纯 C# 替代方案
6. **团结引擎优先** — 以团结引擎为一等公民

## 路线图

- [x] 核心 MVP / MVC 分层
- [x] PlayerLoop 原生生命周期注入
- [x] 命令模式（同步 + 异步）
- [x] ObservableValue 响应式属性
- [x] MiniEventBus 类型事件总线
- [x] GenericLocator 泛型定位器
- [x] AbstractSubmodule 统一子模块生命周期
- [x] 运行时错误日志（替代前置依赖校验）
- [x] Engine 层脱离 Component 层（纯 C#）
- [x] Domain Reload 安全
- [ ] ScriptableObject 可视化配置层
- [ ] SO EventChannel 事件通道
- [ ] Editor 工具链（SO Inspector / MVP 脚手架 / 模块可视化）
- [ ] 运行时集合（RuntimeSet）

## 许可证

[MIT](./LICENSE.md)
