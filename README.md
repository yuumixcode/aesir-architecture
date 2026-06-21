# Aesir Architecture

> 面向团结引擎 / Unity 的渐进式 MVP 架构框架，以 Unity 原生特性为一等公民。

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE.md)
[![Version](https://img.shields.io/badge/version-0.1.0-blue.svg)](./CHANGELOG.md)
[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black.svg)](https://unity.com/)

## 概述

AesirArchitecture（RAA）是一个以 **Unity 原生优先** 为核心理念的架构框架。它不构建与引擎平行的自建体系，而是深度绑定 Unity 的 PlayerLoop、ScriptableObject、Editor API 等原生能力，在保持轻量的同时为中小型到中大型项目提供清晰的 MVP / MVC 分层。

### 核心特性

- **PlayerLoop 原生生命周期** — 通过 `AesirArchitectureLifeCycle` 将自定义子系统注入 Unity PlayerLoop，提供 `BeforeUpdate` / `AfterUpdate` 帧回调，无需 MonoBehaviour
- **能力接口组合** — 通过 `ICanGetModel`、`ICanExecuteCommand`、`ICanSubscribeWithContext` 等能力标记接口组合出 `IModel` / `ISystem` / `IView` / `IController` / `IPresenter`，按需暴露能力
- **CQRS 读写分离** — `ICommand` / `IAsyncCommand` 负责写操作，`IQuery<TResult>` / `IAsyncQuery<TResult>` 负责读操作，支持同步与异步
- **ObservableProperty 响应式属性** — Model 持有可写实例，View 通过 `IReadOnlyObservableProperty<out T>` 协变只读访问，保障层级安全
- **MiniEventHub 类型事件总线** — 按事件类型注册/发布，支持自动注销句柄（`IUnsubscribe`）与多种生命周期绑定（GameObject 销毁、场景卸载等）
- **Domain Reload 安全** — 静态变量通过 `[RuntimeInitializeOnLoadMethod]` 显式重置，反复进出 Play Mode 无残留
- **纯 C# 核心 + MonoBehaviour 适配** — 框架核心为纯 C# 对象，仅 `AbstractView<T>` 作为 MonoBehaviour 适配层
- **MVC + MVP 双模式** — `IController` 适合快速开发，`IPresenter` 提供更严格的 Model-View 隔离

### 与 QFramework 的差异

| 维度 | QFramework | AesirArchitecture |
|------|-----------|-------------------|
| 生命周期 | MonoBehaviour 事件回调 | PlayerLoop 原生注入（BeforeUpdate / AfterUpdate） |
| 架构根 | 泛型单例 `Architecture<T>` | 泛型静态单例 `Context<T>` + `MockContext` 测试隔离 |
| 可观察属性 | `BindableProperty<T>` | `ObservableProperty<T>` + `IReadOnlyObservableProperty<out T>` 协变只读 |
| 事件通信 | 纯 C# TypeEvent | 纯 C# MiniEventHub + 委托（不使用 `event` 关键字） |
| 日志 | `Debug.Log` | `AesirArchitectureLog` 条件编译统一日志 |
| 静态状态 | 无 Domain Reset 保障 | `[RuntimeInitializeOnLoadMethod]` 显式重置 |
| 表现层 | 无明确抽象 | `IView` 只读契约 + `IController` / `IPresenter` 双模式 |

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

public class CounterContext : Context<CounterContext>
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
    IReadOnlyObservableProperty<int> Count { get; }
    void Increase();
    void Decrease();
    void Reset();
}

public sealed class CounterModel : AbstractModel, ICounterModel
{
    readonly ObservableProperty<int> _count = new ObservableProperty<int>(0);

    public IReadOnlyObservableProperty<int> Count => _count;

    public void Increase() => _count.Value++;
    public void Decrease() => _count.Value--;
    public void Reset() => _count.Value = 0;

    protected override void OnInitialize() { }
}
```

### 3. 定义 View（MVC 模式）

```csharp
public class UICounterMvcPanel : AbstractView<CounterContext>
{
    [SerializeField] Text countText;
    [SerializeField] Button increaseButton;

    ICounterModel _model;
    ICounterController _ctrl;

    void Awake()
    {
        _model = this.GetModel<ICounterModel>();
        _model.Count.Subscribe(UpdateCountText)
                   .UnsubscribeWhenGameObjectOnDestroyed(gameObject);
        _ctrl = new CounterController(_model);
    }

    void OnEnable() => increaseButton.onClick.AddListener(_ctrl.Increase);
    void OnDisable() => increaseButton.onClick.RemoveListener(_ctrl.Increase);

    public void UpdateCountText(int count) => countText.text = count.ToString();
}
```

### 4. 使用 Command / Query

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

// 查询
public class GetScoreQuery : AbstractQuery<int>
{
    protected override int OnExecute() => this.GetModel<IScoreModel>().Score;
}

int score = this.ExecuteQuery<GetScoreQuery, int>();
```

### 5. 使用事件总线

```csharp
// 定义事件参数
public struct ScoreChangedEvent : IEventArgs
{
    public int NewScore;
}

// 订阅
this.Subscribe<ScoreChangedEvent>(e => Debug.Log($"Score: {e.NewScore}"))
    .UnsubscribeWhenGameObjectOnDestroyed(gameObject);

// 发布
this.Invoke<ScoreChangedEvent>(); // 无参构造
```

## 架构总览

```
┌─────────────────────────────────────────────────┐
│                   Context<T>                    │
│     (泛型静态单例 + Domain Reset)                │
│                                                  │
│  ┌──────────┐  ┌──────────┐  ┌───────────────┐ │
│  │  Models  │  │ Systems  │  │  MiniEventHub │ │
│  │          │  │          │  │   (Event Bus)  │ │
│  └──────────┘  └──────────┘  └───────────────┘ │
└──────────────────┬──────────────────────────────┘
                   │ 能力接口组合
     ┌─────────────┼─────────────┐
     ▼             ▼             ▼
┌─────────┐ ┌───────────┐ ┌────────────┐
│  IView  │ │IController│ │ IPresenter │
│ (只读)  │ │  (MVC)    │ │   (MVP)    │
└─────────┘ └───────────┘ └────────────┘
     │             │             │
     ▼             ▼             ▼
┌──────────────────────────────────────┐
│        AbstractView<T> : MonoBehaviour│
│         (MonoBehaviour 适配层)          │
└──────────────────────────────────────┘
                   │
                   ▼
┌──────────────────────────────────────┐
│     AesirArchitectureLifeCycle        │
│  (PlayerLoop 原生注入: Before/After)   │
└──────────────────────────────────────┘
```

### 能力矩阵

| 模块 | GetModel | GetSystem | ExecuteCommand | ExecuteQuery | Subscribe | Invoke | Initialize | Dispose |
|------|:--------:|:---------:|:--------------:|:------------:|:---------:|:------:|:----------:|:-------:|
| **IModel** | ✓ | | | | | ✓ | ✓ | ✓ |
| **ISystem** | ✓ | ✓ | ✓ | ✓ | | ✓ | ✓ | ✓ |
| **IView** | ✓ | | | ✓ | ✓ | | | |
| **IController** | ✓ | ✓ | ✓ | ✓ | ✓ | | | |
| **IPresenter** | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | | ✓ |

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
│   ├── Core/
│   │   ├── Context/          # IContext, Context<T>, ContextBase, MockContext
│   │   ├── Module/           # IModel, ISystem, IView, IController, IPresenter + Abstract 基类
│   │   ├── Capabilities/      # ICan* 能力标记接口 + Extension 方法
│   │   ├── AesirArchitecture.cs            # 架构入口静态类
│   │   ├── AesirArchitectureLifeCycle.cs   # PlayerLoop 注入
│   │   ├── AesirArchitectureLog.cs         # 统一日志
│   │   ├── AssemblyInfo.cs                # InternalsVisibleTo 声明
│   │   ├── Container.cs                   # 模块容器
│   │   └── SingletonMonoBehaviour.cs      # 单例 MonoBehaviour 基类
│   ├── Command/              # ICommand, IAsyncCommand + Abstract 基类
│   ├── Query/                # IQuery<TResult>, IAsyncQuery<TResult> + Abstract 基类
│   ├── Event/                # MiniEventHub, MiniEvent<T>, IUnsubscribe + 生命周期绑定
│   ├── Observer/             # ObservableProperty<T>, IReadOnlyObservableProperty<T>
│   └── Utilities/            # PlayerLoopUtility
├── Editor/
│   ├── Runestone.AesirArchitecture.Editor.asmdef
│   └── OdinIntegration/      # Odin Inspector 集成（可选）
│       └── Runestone.AesirArchitecture.Editor.OdinIntegration.asmdef
├── Tests/
│   ├── Runtime/
│   │   └── Runestone.AesirArchitecture.Tests.asmdef
│   └── Editor/
│       └── Runestone.AesirArchitecture.Tests.Editor.asmdef
├── Samples~/
│   ├── UI-Counter-MVC/       # MVC 模式计数器 Demo
│   ├── UI-Counter-MVP/       # MVP 模式计数器 Demo
│   └── ObservableProperty-Inspector/  # ObservableProperty Inspector 演示
└── Documentation~/
    └── aesir-architecture.md
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
- [x] CQRS 命令/查询分离
- [x] ObservableProperty 响应式属性
- [x] MiniEventHub 类型事件总线
- [x] Domain Reload 安全
- [x] MockContext 测试隔离
- [ ] ScriptableObject 可视化配置层
- [ ] SO EventChannel 事件通道
- [ ] Editor 工具链（SO Inspector / MVP 脚手架 / 模块可视化）
- [ ] 运行时集合（RuntimeSet）

## 许可证

[MIT](./LICENSE.md)
