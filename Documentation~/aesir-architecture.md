# Aesir Architecture 使用手册

> **版本**: 0.2.1 | **Unity**: 2022.3+ | **命名空间**: `Runestone.AesirArchitecture`

---

## 目录

- [1. 概述](#1-概述)
  - [1.1 设计理念](#11-设计理念)
  - [1.2 核心特性](#12-核心特性)
  - [1.3 与 QFramework 的差异](#13-与-qframework-的差异)
- [2. 安装](#2-安装)
- [3. 架构总览](#3-架构总览)
  - [3.1 分层结构](#31-分层结构)
  - [3.2 能力矩阵](#32-能力矩阵)
  - [3.3 项目结构](#33-项目结构)
- [4. 核心概念](#4-核心概念)
  - [4.1 Context — 架构根](#41-context--架构根)
  - [4.2 Module — 模块层](#42-module--模块层)
  - [4.3 Capabilities — 能力接口](#43-capabilities--能力接口)
  - [4.4 ObservableValue — 响应式属性](#44-observablevalue--响应式属性)
  - [4.5 Command — 命令模式](#45-command--命令模式)
  - [4.6 MiniEventBus — 类型事件总线](#46-minieventbus--类型事件总线)
  - [4.7 PlayerLoop 生命周期](#47-playerloop-生命周期)
  - [4.8 AesirArchitectureLog — 统一日志](#48-aesirarchitecturelog--统一日志)
- [5. 快速开始](#5-快速开始)
  - [5.1 MVC 模式](#51-mvc-模式)
  - [5.2 MVP 模式](#52-mvp-模式)
- [6. API 参考](#6-api-参考)
  - [6.1 Context](#61-context)
  - [6.2 Module 接口与基类](#62-module-接口与基类)
  - [6.3 Command](#63-command)
  - [6.4 ObservableValue](#64-observablevalue)
  - [6.5 Event 系统](#65-event-系统)
  - [6.6 生命周期与工具](#66-生命周期与工具)
  - [6.7 Locator — 泛型定位器](#67-locator--泛型定位器)
- [7. 最佳实践与约定](#7-最佳实践与约定)
- [8. 单元测试](#8-单元测试)
- [9. Odin Inspector 集成](#9-odin-inspector-集成)
- [10. 路线图](#10-路线图)

---

## 1. 概述

AesirArchitecture 是一个面向团结引擎 / Unity 的渐进式架构框架，以 **Unity 原生优先** 为核心理念。它不构建与引擎平行的自建体系，而是深度绑定 Unity 的 PlayerLoop、ScriptableObject、Editor API 等原生能力，在保持轻量的同时为中小型到中大型项目提供清晰的 MVP / MVC 分层。

### 1.1 设计理念

1. **Unity 原生优先** — 优先使用 Unity 引擎能力（PlayerLoop、ScriptableObject、Editor API），而非自建平行体系
2. **Domain Reload 兼容（铁律）** — 静态变量必须显式重置，反复进出 Play Mode 无残留
3. **低 MonoBehaviour 依赖** — 核心框架由纯 C# 对象组成，MonoBehaviour 仅作适配层
4. **渐进式** — 小项目轻量使用，大项目逐步扩展，不强制全量引入
5. **SO 与纯代码双通道**（规划中） — 每个 SO 能力都有纯 C# 替代方案
6. **团结引擎优先** — 以团结引擎为一等公民

### 1.2 核心特性

| 特性 | 说明 |
|------|------|
| **PlayerLoop 原生生命周期** | 通过 `AesirArchitecturePlayerLoop` 将自定义子系统注入 Unity PlayerLoop，提供 `BeforeUpdate` / `AfterUpdate` 帧回调，无需 MonoBehaviour |
| **能力接口组合** | 通过 `ICanGetModel`、`ICanExecuteCommand`、`ICanAddListener` 等能力标记接口组合出 `IModel` / `IService` / `IView` / `IController` / `IPresenter`，按需暴露能力 |
| **命令模式** | `ICommand` / `IAsyncCommand` 负责写操作，支持同步与异步 |
| **ObservableValue 响应式属性** | Model 持有可写实例，View 通过 `IReadOnlyObservableValue<out T>` 协变只读访问，保障层级安全 |
| **MiniEventBus 类型事件总线** | 按事件类型注册/发布，支持自动移除监听句柄（`AutoRemoveListenerHandle`）与多种生命周期绑定（GameObject 销毁、场景卸载等） |
| **依赖声明与校验** | `IModel` / `IService` 通过 `GetDependencies()` 声明依赖，注册时自动校验初始化顺序 |
| **GenericLocator 泛型定位器** | 按类型注册/查询的通用定位器，支持全局单例 `GenericLocator<T>.Global` |
| **Inspector 可视化看板** | `ContextBoard` 展示上下文模块列表，`MiniEventBusBoard` 展示事件注册状态 |
| **Domain Reload 安全** | 静态变量通过 `[RuntimeInitializeOnLoadMethod]` 显式重置，反复进出 Play Mode 无残留 |
| **纯 C# 核心 + MonoBehaviour 适配** | 框架核心为纯 C# 对象，`AesirView<T>` / `MonoView<T>` 作为 MonoBehaviour 适配层 |
| **MVC + MVP 双模式** | `IController` 适合快速开发，`IPresenter` 提供更严格的 Model-View 隔离 |

### 1.3 与 QFramework 的差异

| 维度 | QFramework | AesirArchitecture |
|------|-----------|-------------------|
| 生命周期 | MonoBehaviour 事件回调 | PlayerLoop 原生注入（BeforeUpdate / AfterUpdate） |
| 架构根 | 泛型单例 `Architecture<T>` | 泛型静态单例 `AbstractContext<T>` + `GenericLocator` 全局定位 |
| 可观察属性 | `BindableProperty<T>` | `ObservableValue<T>` + `IReadOnlyObservableValue<out T>` 协变只读 |
| 事件通信 | 纯 C# TypeEvent | 纯 C# MiniEventBus + 委托（不使用 `event` 关键字） |
| 日志 | `Debug.Log` | `AesirArchitectureLog` 条件编译统一日志 |
| 静态状态 | 无 Domain Reset 保障 | `[RuntimeInitializeOnLoadMethod]` 显式重置 |
| 表现层 | 无明确抽象 | `IView` 只读契约 + `IController` / `IPresenter` 双模式 |

---

## 2. 安装

### 通过 UPM（Git URL）

在 Unity Package Manager 中通过 Git URL 安装：

```
https://github.com/yuumixcode/aesir-architecture.git
```

### 手动安装

将本包目录复制到项目的 `Packages/` 目录下即可。

### 依赖

- Unity 2022.3+
- **Odin Inspector**（可选）— 用于 ObservableValue 的 Inspector 可视化编辑

---

## 3. 架构总览

### 3.1 分层结构

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
│  ┌──────────────────────────────────────────────┐│
│  │    ContextBoard / MiniEventBusBoard (可视化) ││
│  └──────────────────────────────────────────────┘│
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
│   AesirView<T> / MonoView<T>         │
│        (MonoBehaviour 适配层)          │
└──────────────────────────────────────┘
                   │
                   ▼
┌──────────────────────────────────────┐
│     AesirArchitecturePlayerLoop      │
│  (PlayerLoop 原生注入: Before/After)   │
└──────────────────────────────────────┘
```

**数据流向**：

- **View → Controller/Presenter**：用户交互（按钮点击等）触发 Controller/Presenter 方法
- **Controller/Presenter → Model**：直接调用 Model 方法修改状态，或通过 Command 间接修改
- **Model → View**：通过 `ObservableValue` 值变更通知或 `MiniEventBus` 事件推送
- **Command → Model**：Command 通过 `GetModel<T>()` 获取 Model 并修改状态

### 3.2 能力矩阵

每个模块类型通过组合能力接口获得不同的操作权限：

| 模块 | GetModel | GetService | ExecuteCommand | AddListener | InvokeEvent | Initialize | Dispose |
|------|:--------:|:---------:|:--------------:|:---------:|:----------:|:----------:|:-------:|
| **IModel** | ✓ | | | | ✓ | ✓ | ✓ |
| **IService** | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| **IView** | ✓ | ✓ | | ✓ | ✓ | | |
| **IController** | ✓ | ✓ | ✓ | ✓ | | | |
| **IPresenter** | ✓ | ✓ | ✓ | ✓ | ✓ | | ✓ |

> **设计原则**：IService 包含 `ICanAddListener` 用于监听跨模块事件，IController 不包含 `ICanInvokeEvent`（不主动发布事件），事件发布交由 Presenter 或 Model 负责。

### 3.3 项目结构

```
cn.runestone.aesir.architecture/
├── package.json
├── README.md
├── CHANGELOG.md
├── LICENSE.md
├── Runtime/
│   ├── Runestone.AesirArchitecture.asmdef
│   ├── Engine/                    # 纯 C# + 使用 UnityEngine API（不依赖 MonoBehaviour）
│   │   ├── Common/
│   │   │   ├── AesirArchitectureLog.cs         # 统一日志
│   │   │   ├── AesirArchitecturePlayerLoop.cs   # PlayerLoop 注入
│   │   │   ├── AssemblyInfo.cs                  # InternalsVisibleTo 声明
│   │   │   └── GenericResetStaticsAssistant.cs  # 静态变量重置助手
│   │   ├── Core/
│   │   │   ├── Context/           # IContext, AbstractContext<T>, ContextDependencyAssistant
│   │   │   ├── Modules/           # IModel, IService, IView, IController, IPresenter + Abstract 基类
│   │   │   │   ├── Interfaces/    # 模块接口
│   │   │   │   └── Abstracts/     # 模块抽象基类
│   │   │   └── Capabilities/      # ICan* 能力标记接口 + Extension 方法
│   │   │       ├── Interfaces/
│   │   │       └── Extensions/
│   │   ├── Event/                 # MiniEventBus, MiniEvent<T>, AutoRemoveListenerHandle
│   │   ├── Observable/           # ObservableValue<T>, IReadOnlyObservableValue<T>
│   │   ├── Locator/              # GenericLocator<T>, ILocator
│   │   └── Utilities/            # PlayerLoopUtility
│   ├── Component/                # MonoBehaviour 组件（依赖 MonoBehaviour）
│   │   ├── Common/
│   │   │   ├── AesirArchitecture.cs       # 框架 MonoBehaviour 单例入口
│   │   │   └── AesirMonoBehaviour.cs      # Odin 自动适配基类
│   │   ├── Core/
│   │   │   ├── AesirView.cs              # Odin 适配 View 基类
│   │   │   ├── MonoView.cs               # 纯 MonoBehaviour View 基类
│   │   │   └── ContextBoard.cs           # 上下文 Inspector 可视化看板
│   │   ├── Event/
│   │   │   ├── MiniEventBusBoard.cs      # 事件总线 Inspector 可视化看板
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
    ├── aesir-architecture.md    # 本文档
    ├── Books/                   # 参考电子书
    ├── FAQ/                     # 常见问题
    ├── Manuals/                 # 模块手册
    └── Rules/                   # 代码规范
```

### 程序集划分

| 程序集 | 路径 | 说明 |
|--------|------|------|
| `Runestone.AesirArchitecture` | `Runtime/` | 运行时核心，无 Editor 依赖 |
| `Runestone.AesirArchitecture.Editor` | `Editor/` | Editor 扩展 |
| `Runestone.AesirArchitecture.Editor.OdinIntegration` | `Editor/OdinIntegration/` | Odin Inspector 集成（可选，需 Odin Inspector 插件） |
| `Runestone.AesirArchitecture.Tests` | `Tests/Runtime/` | 运行时测试 |
| `Runestone.AesirArchitecture.Tests.Editor` | `Tests/Editor/` | Editor 测试 |

> `AssemblyInfo.cs` 声明了 `InternalsVisibleTo("Runestone.AesirArchitecture.Tests")`，允许测试程序集访问内部成员。

---

## 4. 核心概念

### 4.1 Context — 架构根

Context 是整个架构的入口点，负责模块（Model/Service）的注册、获取和生命周期管理。

#### AbstractContext\<T\>

泛型静态单例，每个子类对应一个全局上下文实例。

```csharp
public class GameContext : AbstractContext<GameContext>
{
    protected override void Configure()
    {
        RegisterModel<IPlayerModel>(new PlayerModel());
        RegisterModel<IInventoryModel>(new InventoryModel());
        RegisterService<ICombatService>(new CombatService());
    }
}
```

- **首次访问** `AbstractContext<T>.Interface` 时自动触发初始化流程（调用 `Configure()` → 注册模块 → 初始化所有 Model 和 Service）
- 单例通过 `GenericLocator<IContext>.Global` 管理，注册到 `ContextBoard` 可视化
- **Dispose()** 释放资源并清除全局定位器引用，逆序释放（先 Service 后 Model）
- **Domain Reload 安全**：`GenericLocator<T>` 的静态构造函数自动注册到 `GenericResetStaticsAssistant`，在 `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` 时重置全局引用

> **注意**：`AbstractContext<T>` 是泛型类型，无法直接使用 `[RuntimeInitializeOnLoadMethod]`。因此通过非泛型的 `GenericResetStaticsAssistant` 统一管理所有静态变量的域重置回调。回调列表不在重置后清空，因为关闭 Domain Reload 时静态构造函数不会重新执行，回调需保留供下次进入 Play Mode 使用。

#### 事件操作

`AbstractContext<T>` 直接提供事件操作方法，路由到 `MiniEventBus.Global`：

```csharp
// 注册事件监听，返回可自动移除的监听句柄
public AutoRemoveListenerHandle AddListener<TEvent>(Action<TEvent> listener) where TEvent : IEventArgs;

// 移除事件监听
public void RemoveListener<TEvent>(Action<TEvent> listener) where TEvent : IEventArgs;

// 发布事件
public void InvokeEvent<TEvent>(TEvent args) where TEvent : IEventArgs;
```

#### ContextDependencyAssistant

依赖项校验辅助类，提供 Model 和 Service 的依赖类型检查与初始化状态检查：

- **ValidateModelDependencyTypes** — 校验 Model 的依赖类型是否合法（必须为 `IModel` 子类）
- **ValidateServiceDependencyTypes** — 校验 Service 的依赖类型是否合法（必须为 `IModel` 或 `IService` 子类）
- **CheckModelDependencies** — 检查 Model 的依赖项是否已全部注册且初始化
- **CheckServiceDependencies** — 检查 Service 的依赖项是否已全部注册且初始化

> 开发者需保证注册顺序满足依赖关系——被依赖的模块先注册。初始化每个模块前检查其依赖项是否已初始化，未初始化则直接报错。

#### IContext 接口

```csharp
public interface IContext : IDisposable
{
    bool Initialized { get; }
    void RegisterModel<T>(T model) where T : class, IModel;
    T GetModel<T>() where T : class, IModel;
    void RegisterService<T>(T service) where T : class, IService;
    T GetService<T>() where T : class, IService;
    IEnumerable<IModel> GetAllModels();
    IEnumerable<IService> GetAllServices();
    AutoRemoveListenerHandle AddListener<T>(Action<T> listener) where T : IEventArgs;
    void RemoveListener<T>(Action<T> listener) where T : IEventArgs;
    void InvokeEvent<T>(T args) where T : IEventArgs;
}
```

#### ContextBoard

MonoBehaviour 看板组件，在 Inspector 中以字典形式展示每个 Context 的 Model 和 Service 列表。无需手动添加，`AbstractContext<T>` 初始化时自动注册到 `ContextBoard.Instance`。

---

### 4.2 Module — 模块层

#### IModel

数据层接口，持有状态（通常使用 `ObservableValue<T>`）。

```csharp
public interface IModel : IContextHolder, ICanSetContext, ICanGetModel, ICanInvokeEvent,
    ICanInitialize
{
    HashSet<Type> GetDependencies();
}
```

**能力**：GetModel、InvokeEvent（发布事件）、Initialize、Dispose

**AbstractModel** 基类提供：
- `IContext` 引用持有
- `Initialized` 状态标记
- `OnInitialize()` 抽象方法（子类必须实现）
- `OnDispose()` 虚方法（子类可选覆写）

#### IService

服务层接口，封装跨模块业务逻辑，协调模块间交互与通信。

```csharp
public interface IService : IContextHolder, ICanSetContext, ICanGetModel, ICanGetService,
    ICanInvokeEvent, ICanAddListener, ICanInitialize
{
    HashSet<Type> GetDependencies();
}
```

**能力**：GetModel、GetService、ExecuteCommand、InvokeEvent、AddListener、Initialize、Dispose

> **设计注意**：IService 包含 `ICanAddListener` 用于监听跨模块事件，因为 Service 需要响应其他模块发出的事件来协调业务逻辑。

#### IView

表现层只读接口。

```csharp
public interface IView : IContextHolder, ICanGetModel, ICanGetService, ICanAddListener,
    ICanInvokeEvent { }
```

**能力**：GetModel、GetService、AddListener、InvokeEvent

> View 层可以读取数据、获取 Service、添加事件监听和发布事件，但不能执行 Command（写操作）。

#### AesirView\<T\>

继承 `AesirMonoBehaviour` 的 View 基类，自动支持 Odin Inspector 序列化。

```csharp
public abstract class AesirView<T> : AesirMonoBehaviour, IView where T : AbstractContext<T>, new()
{
    public IContext Context => AbstractContext<T>.Interface;
}
```

#### MonoView\<T\>

继承 `MonoBehaviour` 的 View 基类，无 Odin 依赖。

```csharp
public abstract class MonoView<T> : MonoBehaviour, IView where T : AbstractContext<T>, new()
{
    public IContext Context => AbstractContext<T>.Interface;
}
```

- 继承 `AesirView<T>` 或 `MonoView<T>` 后，View 自动获得 `GetModel<T>()`、`GetService<T>()`、`AddListener<T>()`、`InvokeEvent<T>()` 等能力
- 选择 `AesirView<T>` 当项目包含 Odin Inspector 时可获得更好的 Inspector 体验

#### IController

表现层完整接口（MVC 模式）。

```csharp
public interface IController : IContextHolder, ICanGetModel, ICanGetService, ICanExecuteCommand,
    ICanAddListener { }
```

**能力**：GetModel、GetService、AddListener、ExecuteCommand

`IController<T>` 泛型版本自动绑定 `AbstractContext<T>`：

```csharp
public interface IController<T> : IController where T : AbstractContext<T>, new()
{
    IContext IContextHolder.Context => AbstractContext<T>.Interface;
}
```

#### IPresenter

表现层 MVP 中介接口。

```csharp
public interface IPresenter : IContextHolder, ICanExecuteCommand, ICanGetModel, ICanGetService,
    ICanAddListener, ICanInvokeEvent, IDisposable { }
```

**能力**：GetModel、GetService、AddListener、InvokeEvent、ExecuteCommand、Dispose

> 与 `IController` 的区别：Controller 混合了 Model 和 View 的操作职责；Presenter 通过增加 `ICanInvokeEvent` 能力主动向 View 推送变更，同时保留 `ICanAddListener` 监听 Model 事件，实现双向中介。Presenter 继承 `IDisposable`，需在 View 销毁时手动 Dispose。

---

### 4.3 Capabilities — 能力接口

能力接口是框架的核心设计模式。每个能力接口是一个空标记接口，配合对应的扩展方法提供具体功能。模块通过组合多个能力接口获得不同的操作权限。

| 能力接口 | 扩展方法 | 说明 |
|----------|----------|------|
| `ICanGetModel` | `GetModel<T>()` | 获取已注册的 Model |
| `ICanGetService` | `GetService<T>()` | 获取已注册的 Service |
| `ICanExecuteCommand` | `ExecuteCommand<T>()` / `ExecuteCommandAsync<T>()` | 执行同步/异步命令 |
| `ICanAddListener` | `AddListener<T>()` / `RemoveListener<T>()` | 添加/移除事件监听 |
| `ICanInvokeEvent` | `InvokeEvent<T>()` | 发布事件 |
| `ICanSetContext` | `SetContext()` | 设置上下文引用（内部使用） |
| `ICanInitialize` | `Initialize()` / `Initialized` | 初始化与状态标记 |
| `IContextHolder` | `Context` | 持有的上下文（所有能力接口的基础） |

> 所有能力接口都继承 `IContextHolder`，扩展方法通过 `self.Context` 路由到上下文。

---

### 4.4 ObservableValue — 响应式属性

#### 核心接口

```
IReadOnlyObservableValue<out T>  (协变只读)
    ├── T Value { get; }
    ├── AutoRemoveListenerHandle AddListener(Action<T>)
    ├── void RemoveListener(Action<T>)
    ├── AutoRemoveListenerHandle AddListenerAndInvoke(Action<T>)
    └── void Invoke()

IObservableValue<T> : IReadOnlyObservableValue<T>  (可写)
    ├── new T Value { get; set; }
    ├── void SetValueSilently(T)
    ├── void SetValue(T)
    └── void Modify(Action<T>)
```

#### 使用模式

**Model 层**持有可写实例，对外暴露只读接口：

```csharp
public sealed class CounterModel : AbstractModel, ICounterModel
{
    readonly ObservableValue<int> _count = new ObservableValue<int>(0);

    // 对外暴露只读
    public IReadOnlyObservableValue<int> Count => _count;

    public void Increase() => _count.Value++;
}
```

**View 层**监听只读接口的值变更：

```csharp
_model.Count.AddListener(UpdateCountText)
           .RemoveListenerWhenGameObjectOnDestroyed(gameObject);
```

#### 关键 API

| API | 说明 |
|-----|------|
| `Value` | 获取/设置值。setter 会做相等性检查，值不同才触发通知 |
| `SetValue(T)` | 语义等价于 `Value` 的 setter |
| `SetValueSilently(T)` | 静默设置值，不触发通知。用于反序列化或批量更新后统一触发 |
| `AddListener(Action<T>)` | 监听值变更，返回 `AutoRemoveListenerHandle` 句柄 |
| `AddListenerAndInvoke(Action<T>)` | 监听并立即触发一次当前值，用于初始化时同步监听方状态 |
| `RemoveListener(Action<T>)` | 移除监听 |
| `Invoke()` | 强制触发值变更通知，绕过相等性检查 |
| `Modify(Action<T>)` | 原地修改值并强制触发通知。适用于引用类型（class）的就地修改场景 |
| `Clear()` | 清除所有监听 |

> **`out T` 协变**：`IReadOnlyObservableValue<out T>` 使用 `out` 修饰符，支持协变。例如 `IReadOnlyObservableValue<string>` 可赋值给 `IReadOnlyObservableValue<object>` 变量。

---

### 4.5 Command — 命令模式

#### ICommand / AbstractCommand

同步命令，只写无返回值。

```csharp
public class AddScoreCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        var model = this.GetModel<IScoreModel>();
        model.AddScore(10);
        this.InvokeEvent(new ScoreChangedEvent { NewScore = 100 });  // 发布事件
    }
}

// 执行（无参，自动 new）
this.ExecuteCommand<AddScoreCommand>();

// 执行（带参）
this.ExecuteCommand(new AddScoreCommand { Amount = 20 });
```

#### IAsyncCommand / AbstractAsyncCommand

异步命令，适用于网络请求、文件 IO 等异步写操作。

```csharp
public class SaveGameCommand : AbstractAsyncCommand
{
    protected override async Task OnExecuteAsync()
    {
        var model = this.GetModel<ISaveModel>();
        await model.SaveAsync();
    }
}

// 执行
await this.ExecuteCommandAsync<SaveGameCommand>();
```

#### 能力依赖

Command 内部可使用的能力：

| 类型 | GetModel | GetService | ExecuteCommand | InvokeEvent |
|------|:--------:|:---------:|:--------------:|:----------:|
| ICommand | ✓ | ✓ | ✓ | ✓ |
| IAsyncCommand | ✓ | ✓ | ✓ | ✓ |

> Command 可以嵌套执行其他 Command 和发布事件。

---

### 4.6 MiniEventBus — 类型事件总线

#### 基本用法

事件参数必须实现 `IEventArgs` 接口（空标记接口）：

```csharp
public struct ScoreChangedEvent : IEventArgs
{
    public int NewScore;
}
```

**监听**：

```csharp
// 通过能力接口（ICanAddListener）
this.AddListener<ScoreChangedEvent>(e => Debug.Log($"Score: {e.NewScore}"))
    .RemoveListenerWhenGameObjectOnDestroyed(gameObject);

// 直接通过 MiniEventBus 全局单例
MiniEventBus.Global.AddListener<ScoreChangedEvent>(e => { ... });

// 通过 IContext 接口
context.AddListener<ScoreChangedEvent>(e => { ... });
```

**发布**：

```csharp
// 通过能力接口（ICanInvokeEvent）
this.InvokeEvent(new ScoreChangedEvent { NewScore = 100 });

// 直接通过 MiniEventBus 全局单例
MiniEventBus.Global.InvokeEvent(new ScoreChangedEvent { NewScore = 100 });

// 通过 IContext 接口
context.InvokeEvent(new ScoreChangedEvent { NewScore = 100 });
```

**移除监听**：

```csharp
// 通过句柄移除监听
var handle = this.AddListener<ScoreChangedEvent>(OnScoreChanged);
handle.Dispose();

// 通过能力接口移除监听（需传入同一委托实例）
this.RemoveListener<ScoreChangedEvent>(OnScoreChanged);
```

#### 自动移除监听生命周期绑定

框架提供多种扩展方法，将监听句柄绑定到不同的生命周期自动移除监听：

| 扩展方法 | 触发移除监听时机 | 适用场景 |
|----------|-------------|----------|
| `RemoveListenerWhenGameObjectOnDestroyed(gameObject)` | GameObject 销毁时（OnDestroy） | View 组件监听 |
| `RemoveListenerWhenGameObjectOnDisable(gameObject)` | GameObject 禁用时（OnDisable） | 面板/窗口监听 |
| `RemoveListenerWhenOnSceneUnloaded()` | 任意场景卸载时 | 跨场景监听 |

实现原理：扩展方法在目标 GameObject 上挂载对应的 `RemoveListenerTrigger` 组件（若不存在），将 `AutoRemoveListenerHandle` 句柄添加到内部的 `RemoveListenerHandleCollection` 中。当 Unity 生命周期事件触发时，批量调用所有句柄的 `Dispose()`。

```
RemoveListenerTrigger (抽象基类, AesirMonoBehaviour)
    ├── RemoveListenerOnDestroyTrigger   → OnDestroy 时批量移除监听
    └── RemoveListenerOnDisableTrigger   → OnDisable 时批量移除监听

RemoveListenerOnSceneUnloadedTrigger     → 场景卸载时批量移除监听
```

#### MiniEvent（无参事件）

除了 `MiniEvent<T>`（泛型带参事件），框架还提供 `MiniEvent`（无参事件），不要求实现 `IEventArgs`：

```csharp
var evt = new MiniEvent();
var handle = evt.AddListener(() => Debug.Log("Triggered"));
evt.Invoke();        // 触发
handle.Dispose();    // 移除监听
```

> `MiniEvent` 不通过 `MiniEventBus` 管理，适合独立使用的简单无参事件场景。

#### AutoRemoveListenerHandle

`AddListener()` 返回的 `AutoRemoveListenerHandle` 句柄实现为 struct，确保重复调用 `Dispose()` 仅执行一次移除监听，安全高效。

#### MiniEventBusBoard

`MiniEventBusBoard` 是 MonoBehaviour 看板组件，在 Inspector 中展示当前 `MiniEventBus.Global` 的事件注册状态（事件类型、监听者列表）。在 `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` 时自动创建，无需手动添加。

---

### 4.7 PlayerLoop 生命周期

#### AesirArchitecturePlayerLoop

基于 Unity PlayerLoop 的生命周期钩子系统，无需 MonoBehaviour 即可接入游戏级帧回调。

```csharp
// 注册回调
AesirArchitecturePlayerLoop.Register(
    AesirArchitectureLifeCyclePhase.BeforeUpdate,
    () => { /* 每帧 Update 之前执行 */ },
    order: 0  // order 越小越先执行
);

// 移除监听回调（必须传入注册时的同一委托实例）
AesirArchitecturePlayerLoop.Unregister(
    AesirArchitectureLifeCyclePhase.BeforeUpdate,
    callback
);
```

#### 阶段

| 阶段 | 枚举值 | 执行时机 |
|------|--------|----------|
| `BeforeUpdate` | 1 | 在 `PlayerLoop.Update` 之前执行 |
| `AfterUpdate` | 2 | 在 `PlayerLoop.PostLateUpdate` 之后执行 |

#### 注入机制

- 在 `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` 时自动注入 PlayerLoop
- `BeforeUpdate` 注入到 `Update` 子系统之前
- `AfterUpdates` 注入到 `PostLateUpdate` 子系统之后
- 注入前检查是否已存在（防止重复注入）

#### 特性

- **order 排序**：`order` 越小越先执行；相同 `order` 按注册顺序（`InsertionIndex`）排序
- **延迟执行**：在回调执行期间注册/移除监听的回调会被缓存到 `DelayedCommands`，当前轮次执行完毕后统一处理
- **异常隔离**：单个回调抛异常不影响其他回调执行，异常通过 `AesirArchitectureLog.Error` 输出
- **Domain Reset**：`Reset()` 清空所有回调和延迟命令

> **警告**：回调持有者销毁前必须调用 `Unregister` 移除监听；若未移除监听，回调将永久残留并阻止目标对象被回收。

#### PlayerLoopUtility

提供通用的 PlayerLoop 操作工具，不局限于框架预定义的阶段：

```csharp
// 在指定子系统前插入
PlayerLoopUtility.InsertSystemBefore<Update>(new PlayerLoopSystem { ... });

// 在指定子系统后插入
PlayerLoopUtility.InsertSystemAfter<PostLateUpdate>(new PlayerLoopSystem { ... });

// 检测是否包含
bool contains = PlayerLoopUtility.ContainsSystem<MyCustomSystem>();

// 输出当前 PlayerLoop 结构
string desc = PlayerLoopUtility.GetCurrentPlayerLoopDescription();
// Aesir Architecture 注入的子系统会以 [Aesir Architecture] 前缀标注
```

#### AesirArchitecture 架构入口

`AesirArchitecture : AesirMonoBehaviour` 是框架的 MonoBehaviour 单例入口，用于接入 Unity 生命周期。

- 在 `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` 时自动创建实例
- 创建名为 `[Aesir Architecture]` 的 GameObject，`DontDestroyOnLoad`
- 同时初始化 `MiniEventBusBoard` 看板组件

---

### 4.8 AesirArchitectureLog — 统一日志

所有架构内部日志必须走此工具，禁止直接使用 `Debug.Log` / `Debug.LogWarning` / `Debug.LogError`。

```csharp
// 常规日志（仅 Editor，打包后自动剔除）
AesirArchitectureLog.Log("消息");
AesirArchitectureLog.Log("Source", "消息");  // 带来源标识

// 警告（仅 Editor）
AesirArchitectureLog.Warning("警告消息");

// 错误（Editor + Runtime）
AesirArchitectureLog.Error("错误消息");

// 测试日志（仅 UNITY_INCLUDE_TESTS 程序集）
AesirArchitectureLog.TestLog("测试消息");
```

| API | 条件编译标记 | 打包行为 |
|-----|-------------|----------|
| `Log` | `UNITY_EDITOR` | 完整剔除 |
| `Warning` | `UNITY_EDITOR` | 完整剔除 |
| `Error` | 无（始终生效） | 保留 |
| `TestLog` | `UNITY_INCLUDE_TESTS` | 完整剔除 |

日志带 HTML 颜色标签，在 Unity Console 中以 `[AesirArchitecture]` 前缀醒目显示：
- Log：绿色 `#00FF88`
- Warning：橙色 `#FFA500`
- Error：红色 `#FF4444`
- TestLog：蓝色 `#00BFFF`

---

## 5. 快速开始

### 5.1 MVC 模式

适合快速原型开发，Controller 直接持有 Model 引用并驱动 UI 更新。

#### 第 1 步：定义 Context

```csharp
using Runestone.AesirArchitecture;

public sealed class CounterContext : AbstractContext<CounterContext>
{
    protected override void Configure()
    {
        RegisterModel<ICounterModel>(new CounterModel());
    }
}
```

#### 第 2 步：定义 Model

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

#### 第 3 步：定义 Controller

```csharp
public interface ICounterController : IController<CounterContext>
{
    ICounterModel Model { get; }
    void Increase();
    void Decrease();
    void ResetCounter();
}

public class CounterController : ICounterController
{
    public CounterController(ICounterModel model) => Model = model;
    public ICounterModel Model { get; }

    public void Increase() => Model.Increase();
    public void Decrease() => Model.Decrease();
    public void ResetCounter() => Model.Reset();
}
```

#### 第 4 步：定义 View

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

### 5.2 MVP 模式

适合中大型项目，Presenter 作为 Model 与 View 之间的唯一通信桥梁。

#### 第 1 步：定义 Context 和 Model

（与 MVC 模式相同）

#### 第 2 步：定义 View 接口

```csharp
public interface ICounterView
{
    void UpdateCountText(int count);
    Action IncreaseClicked { get; set; }
    Action DecreaseClicked { get; set; }
    Action ResetClicked { get; set; }
}
```

#### 第 3 步：定义 Presenter

```csharp
public sealed class CounterPresenter : IPresenter<CounterContext>
{
    readonly ICounterView _view;
    readonly ICounterModel _model;
    AutoRemoveListenerHandle _countSubscription;

    public CounterPresenter(ICounterView view)
    {
        _view = view;
        _view.IncreaseClicked += HandleIncrease;
        _view.DecreaseClicked += HandleDecrease;
        _view.ResetClicked += HandleReset;
        _model = this.GetModel<ICounterModel>();
        // AddListenerAndInvoke: 监听并立即触发一次当前值，初始化 View 显示
        _countSubscription = _model.Count.AddListenerAndInvoke(UpdateView);
    }

    void HandleIncrease() => _model.Increase();
    void HandleDecrease() => _model.Decrease();
    void HandleReset() => _model.Reset();
    void UpdateView(int count) => _view.UpdateCountText(count);

    public void Dispose()
    {
        _view.IncreaseClicked -= HandleIncrease;
        _view.DecreaseClicked -= HandleDecrease;
        _view.ResetClicked -= HandleReset;
        _countSubscription?.Dispose();
        _countSubscription = null;
    }
}
```

#### 第 4 步：定义 View（被动视图）

```csharp
public class UICounterMvpPanel : AesirView<CounterContext>, ICounterView
{
    [SerializeField] Text countText;
    [SerializeField] Button increaseButton;

    CounterPresenter _presenter;

    public Action IncreaseClicked { get; set; }
    public Action DecreaseClicked { get; set; }
    public Action ResetClicked { get; set; }

    void Awake() => _presenter = new CounterPresenter(this);

    void OnEnable() => increaseButton.onClick.AddListener(OnIncreaseButtonClicked);
    void OnDisable() => increaseButton.onClick.RemoveListener(OnIncreaseButtonClicked);
    void OnDestroy() => _presenter?.Dispose();

    void OnIncreaseButtonClicked() => IncreaseClicked?.Invoke();

    public void UpdateCountText(int count) => countText.text = count.ToString();
}
```

### MVC vs MVP 选择指南

| 维度 | MVC | MVP |
|------|-----|-----|
| 适用场景 | 快速原型、小型项目 | 中大型项目、需要测试 |
| Model-View 隔离 | Controller 直接持有 Model | Presenter 通过接口隔离 View |
| 可测试性 | 较低（Controller 混合职责） | 高（可通过 Mock ICounterView 测试 Presenter） |
| 代码量 | 少 | 多（需 View 接口 + Presenter） |
| 事件发布 | 不支持（IController 无 InvokeEvent） | 支持（IPresenter 有 InvokeEvent） |

---

## 6. API 参考

### 6.1 Context

#### AbstractContext\<T\>

```csharp
// 静态属性
public static IContext Interface { get; }   // 获取上下文实例（首次访问自动初始化）

// 实例方法
public void RegisterModel<TModel>(TModel model) where TModel : class, IModel;
public TModel GetModel<TModel>() where TModel : class, IModel;
public void RegisterService<TService>(TService service) where TService : class, IService;
public TService GetService<TService>() where TService : class, IService;
public IEnumerable<IModel> GetAllModels();
public IEnumerable<IService> GetAllServices();
public AutoRemoveListenerHandle AddListener<TEvent>(Action<TEvent> listener) where TEvent : IEventArgs;
public void RemoveListener<TEvent>(Action<TEvent> listener) where TEvent : IEventArgs;
public void InvokeEvent<TEvent>(TEvent args) where TEvent : IEventArgs;
public override void Dispose();
```

#### IContext

```csharp
public interface IContext : IDisposable
{
    bool Initialized { get; }
    void RegisterModel<T>(T model) where T : class, IModel;
    T GetModel<T>() where T : class, IModel;
    void RegisterService<T>(T service) where T : class, IService;
    T GetService<T>() where T : class, IService;
    IEnumerable<IModel> GetAllModels();
    IEnumerable<IService> GetAllServices();
    AutoRemoveListenerHandle AddListener<T>(Action<T> listener) where T : IEventArgs;
    void RemoveListener<T>(Action<T> listener) where T : IEventArgs;
    void InvokeEvent<T>(T args) where T : IEventArgs;
}
```

#### ContextBoard

```csharp
public sealed class ContextBoard : AesirMonoBehaviour
{
    public static ContextBoard Instance { get; }
    public Dictionary<string, List<IModel>> Models { get; }
    public Dictionary<string, List<IService>> Services { get; }
    public void AddContext(IContext context);
}
```

---

### 6.2 Module 接口与基类

#### AbstractModel

```csharp
public abstract class AbstractModel : IModel
{
    public bool Initialized { get; }
    public void Dispose();                    // 触发 OnDispose()

    protected virtual void OnDispose() { }   // 子类可选覆写
    protected abstract void OnInitialize();  // 子类必须实现
}
```

#### AesirView\<T\>

```csharp
public abstract class AesirView<T> : AesirMonoBehaviour, IView where T : AbstractContext<T>, new()
{
    public IContext Context => AbstractContext<T>.Interface;
}
```

#### MonoView\<T\>

```csharp
public abstract class MonoView<T> : MonoBehaviour, IView where T : AbstractContext<T>, new()
{
    public IContext Context => AbstractContext<T>.Interface;
}
```

#### AesirMonoBehaviour

```csharp
// 根据运行环境自动选择基类：
// - 编辑器 + Odin Inspector → SerializedMonoBehaviour
// - 运行时 + Odin Inspector → SerializedMonoBehaviour（或 MonoBehaviour，取决于 ODIN_INSPECTOR_EDITOR_ONLY）
// - 无 Odin Inspector → MonoBehaviour
public abstract class AesirMonoBehaviour { }
```

#### AesirScriptableObject

```csharp
// 同 AesirMonoBehaviour，自动选择 SerializedScriptableObject 或 ScriptableObject
public abstract class AesirScriptableObject { }
```

---

### 6.3 Command

#### AbstractCommand

```csharp
public abstract class AbstractCommand : ICommand
{
    protected abstract void OnExecute();
}
```

#### AbstractAsyncCommand

```csharp
public abstract class AbstractAsyncCommand : IAsyncCommand
{
    protected abstract Task OnExecuteAsync();
}
```

#### 扩展方法（ICanExecuteCommand）

```csharp
// 同步
void ExecuteCommand<T>(this ICanExecuteCommand self) where T : ICommand, new();
void ExecuteCommand<T>(this ICanExecuteCommand self, T command) where T : ICommand;

// 异步
Task ExecuteCommandAsync<T>(this ICanExecuteCommand self) where T : IAsyncCommand, new();
Task ExecuteCommandAsync<T>(this ICanExecuteCommand self, T command) where T : IAsyncCommand;
```

---

### 6.4 ObservableValue

#### ObservableValue\<T\>

```csharp
[Serializable]
public sealed class ObservableValue<T> : IObservableValue<T>
{
    public const string PrivateValueFieldName = nameof(value);

    public ObservableValue();
    public ObservableValue(T initialValue);

    public T Value { get; set; }
    public void SetValueSilently(T v);
    public void SetValue(T v);
    public void Modify(Action<T> modifier);

    public AutoRemoveListenerHandle AddListener(Action<T> callback);
    public void RemoveListener(Action<T> callback);
    public AutoRemoveListenerHandle AddListenerAndInvoke(Action<T> callback);
    public void Invoke();
    public void Clear();
}
```

#### IReadOnlyObservableValue\<out T\>

```csharp
public interface IReadOnlyObservableValue<out T>
{
    T Value { get; }
    AutoRemoveListenerHandle AddListener(Action<T> callback);
    void RemoveListener(Action<T> callback);
    AutoRemoveListenerHandle AddListenerAndInvoke(Action<T> callback);
    void Invoke();
}
```

#### IObservableValue\<T\>

```csharp
public interface IObservableValue<T> : IReadOnlyObservableValue<T>
{
    new T Value { get; set; }
    void SetValueSilently(T value);
    void SetValue(T value);
    void Modify(Action<T> modifier);
}
```

---

### 6.5 Event 系统

#### MiniEventBus

```csharp
public sealed class MiniEventBus : IMiniEventBus
{
    public static readonly MiniEventBus Global;        // 全局单例
    public event Action OnEventRegistrationsChanged;   // 注册变化通知
    public Dictionary<Type, object> EventDictionary { get; }

    public AutoRemoveListenerHandle AddListener<T>(Action<T> listener) where T : IEventArgs;
    public void RemoveListener<T>(Action<T> listener) where T : IEventArgs;
    public void InvokeEvent<T>(T args) where T : IEventArgs;
    public void Clear();
}
```

#### IMiniEventBus

```csharp
public interface IMiniEventBus
{
    event Action OnEventRegistrationsChanged;
    AutoRemoveListenerHandle AddListener<T>(Action<T> listener) where T : IEventArgs;
    void RemoveListener<T>(Action<T> listener) where T : IEventArgs;
    void InvokeEvent<T>(T args) where T : IEventArgs;
    void Clear();
}
```

#### MiniEvent（无参事件）

```csharp
public sealed class MiniEvent : ISubscribe
{
    public AutoRemoveListenerHandle AddListener(Action callback);
    public AutoRemoveListenerHandle AddListenerAndInvoke(Action callback);
    public void RemoveListener(Action callback);
    public void Invoke();
    public void Dispose();
}
```

#### MiniEvent\<T\>（带参事件）

```csharp
public sealed class MiniEvent<T> : ISubscribe where T : IEventArgs
{
    public AutoRemoveListenerHandle AddListener(Action<T> onEvent);
    public AutoRemoveListenerHandle AddListenerAndInvoke(Action<T> onEvent, T eventArgs);
    public void RemoveListener(Action<T> onEvent);
    public void Invoke(T t);
    public void Dispose();
}
```

#### 自动移除监听扩展方法

```csharp
// AutoRemoveListenerHandle 扩展
void RemoveListenerWhenGameObjectOnDestroyed(this AutoRemoveListenerHandle handle, GameObject gameObject);
void RemoveListenerWhenGameObjectOnDisable(this AutoRemoveListenerHandle handle, GameObject gameObject);
void RemoveListenerWhenOnSceneUnloaded(this AutoRemoveListenerHandle handle);
```

#### AutoRemoveListenerHandle

```csharp
public struct AutoRemoveListenerHandle : IDisposable
{
    public AutoRemoveListenerHandle(Action removeListenerCallback);
    public void Dispose();  // 重复调用安全
}
```

#### MiniEventBusBoard

```csharp
public sealed class MiniEventBusBoard : AesirMonoBehaviour
{
    public static MiniEventBusBoard Instance { get; }
    public List<EventRegistrationInfo> EventRegistrations { get; }
}
```

---

### 6.6 生命周期与工具

#### AesirArchitecturePlayerLoop

```csharp
public static class AesirArchitecturePlayerLoop
{
    public static void Register(AesirArchitectureLifeCyclePhase phase, Action callback, int order = 0);
    public static void Unregister(AesirArchitectureLifeCyclePhase phase, Action callback);
    public static void Reset();
    public static int GetHookCount(AesirArchitectureLifeCyclePhase phase);
}

public enum AesirArchitectureLifeCyclePhase
{
    BeforeUpdate = 1,
    AfterUpdate = 2
}
```

#### PlayerLoopUtility

```csharp
public static class PlayerLoopUtility
{
    public static bool InsertSystemBefore<TTarget>(PlayerLoopSystem system);
    public static bool InsertSystemAfter<TTarget>(PlayerLoopSystem system);
    public static bool ContainsSystem<TTarget>();
    public static string GetCurrentPlayerLoopDescription();
}
```

#### AesirArchitecture

```csharp
public class AesirArchitecture : AesirMonoBehaviour
{
    public static AesirArchitecture Instance { get; }
    public static T GetOrAddComponent<T>() where T : MonoBehaviour;
}
```

#### AesirArchitectureLog

```csharp
public static class AesirArchitectureLog
{
    [Conditional("UNITY_EDITOR")] static void Log(string message);
    [Conditional("UNITY_EDITOR")] static void Log(object source, string message);
    [Conditional("UNITY_EDITOR")] static void Warning(string message);
    [Conditional("UNITY_EDITOR")] static void Warning(object source, string message);
    static void Error(string message);
    static void Error(object source, string message);
    [Conditional("UNITY_INCLUDE_TESTS")] static void TestLog(string message);
    [Conditional("UNITY_INCLUDE_TESTS")] static void TestLog(object source, string message);
}
```

#### GenericResetStaticsAssistant

```csharp
public static class GenericResetStaticsAssistant
{
    public static void Register(Action callback);
    // [RuntimeInitializeOnLoadMethod(SubsystemRegistration)] 时自动调用 ResetStaticsAll
}
```

---

### 6.7 Locator — 泛型定位器

#### GenericLocator\<T\>

```csharp
public sealed class GenericLocator<T> : IGenericLocator<T>, IDisposable where T : class
{
    public static GenericLocator<T> Global { get; }    // 全局单例

    public void Register<TItem>(TItem instance) where TItem : class, T;
    public void Register(Type type, T instance);
    public TItem Get<TItem>() where TItem : class, T;
    public bool TryGet<TItem>(out TItem instance) where TItem : class, T;
    public bool IsRegistered<TItem>() where TItem : class, T;
    public void Unregister<TItem>() where TItem : class, T;
    public void Clear();
    public T GetByType(Type type);
    public IEnumerable<T> GetAll();
    public Dictionary<Type, T> GetRegistry();
}
```

#### IGenericLocator\<T\>

```csharp
public interface IGenericLocator<T> where T : class
{
    void Register<TItem>(TItem instance) where TItem : class, T;
    TItem Get<TItem>() where TItem : class, T;
    bool TryGet<TItem>(out TItem instance) where TItem : class, T;
    bool IsRegistered<TItem>() where TItem : class, T;
    void Unregister<TItem>() where TItem : class, T;
    void Clear();
    T GetByType(Type type);
    IEnumerable<T> GetAll();
}
```

> **重要**：注册与查询必须使用相同的类型参数。若以具体类型注册（如 `Register<CounterModel>`），再以接口类型查询（如 `Get<ICounterModel>`），将返回 `null`。因此推荐统一使用接口类型进行注册和查询。

---

## 7. 最佳实践与约定

### 日志规范（铁律）

所有架构内部的信息输出必须走 `AesirArchitectureLog`，禁止直接使用 `Debug.Log` / `Debug.LogWarning` / `Debug.LogError`。无论是 Runtime、Editor 还是 Tests，一律使用此日志工具。

```csharp
// ✅ 正确
AesirArchitectureLog.Log("Context", "模块上下文已释放");

// ❌ 错误
Debug.Log("Context 已释放");
```

### 委托与事件规范

能用委托解决的，就不用 `event` 事件标记。内部直接声明委托字段，对外提供监听（`+=`）和移除监听（`-=`）的 API 方法即可达到与 `event` 相同的封装效果，且更灵活。

```csharp
// ✅ 正确：委托字段 + 公开属性
public Action IncreaseClicked { get; set; }

// ❌ 不推荐：event 关键字
public event Action IncreaseClicked;
```

### Context 注册规范

统一使用接口类型进行注册和查询：

```csharp
// ✅ 正确：注册和查询使用相同的接口类型
RegisterModel<ICounterModel>(new CounterModel());
GetModel<ICounterModel>();

// ❌ 错误：注册用具体类型，查询用接口类型（返回 null）
RegisterModel(new CounterModel());
GetModel<ICounterModel>();  // null!
```

### ObservableValue 使用规范

- **Model 持有可写实例**（`ObservableValue<T>`），对外暴露**只读接口**（`IReadOnlyObservableValue<T>`）
- 使用 `AddListenerAndInvoke` 初始化 View 显示，避免手动同步首次值
- 引用类型（class）的就地修改使用 `Modify`，不要依赖 setter 的相等性检查
- 反序列化使用 `SetValueSilently`，完成后统一 `Invoke()` 触发通知

### 监听管理规范

- View 中监听的事件务必绑定自动移除监听生命周期
- 优先使用 `RemoveListenerWhenGameObjectOnDestroyed` 确保不会泄漏
- Presenter 在 `Dispose()` 中手动移除所有监听

```csharp
// View 中
_model.Count.AddListener(UpdateCountText)
           .RemoveListenerWhenGameObjectOnDestroyed(gameObject);

// Presenter 中
public void Dispose()
{
    _countSubscription?.Dispose();
}
```

### Domain Reload 安全

- 所有静态变量必须通过 `[RuntimeInitializeOnLoadMethod]` 显式重置
- `GenericLocator<T>` 的静态构造函数自动注册到 `GenericResetStaticsAssistant`
- `AesirArchitecturePlayerLoop` 在 `SubsystemRegistration` 阶段重置

---

## 8. 单元测试

### 测试程序集

| 程序集 | 路径 | 测试内容 |
|--------|------|----------|
| `Runestone.AesirArchitecture.Tests.Editor` | `Tests/Editor/` | Editor 端核心测试 |
| `Runestone.AesirArchitecture.Tests` | `Tests/Runtime/` | 运行时测试（Unity 依赖） |

### 测试文件

| 文件 | 覆盖范围 |
|------|----------|
| `AesirArchitecturePlayerLoopTests.cs` | PlayerLoop Register/Unregister、order 排序、延迟命令、Reset |
| `UnityEngineObjectCheckNullTests.cs` | Unity 对象空检查测试 |

### 测试日志

测试中使用 `AesirArchitectureLog.TestLog` 输出日志（仅在 `UNITY_INCLUDE_TESTS` 程序集中生效）：

```csharp
AesirArchitectureLog.TestLog("开始测试 PlayerLoop 注册");
```

---

## 9. Odin Inspector 集成

### AesirMonoBehaviour / AesirScriptableObject

`AesirMonoBehaviour` 和 `AesirScriptableObject` 是框架标准基类，根据运行环境自动选择序列化方式：

- **安装了 Odin Inspector** — 继承 `SerializedMonoBehaviour` / `SerializedScriptableObject`，获得 Odin 序列化能力
- **未安装 Odin Inspector** — 继承 `MonoBehaviour` / `ScriptableObject`，无功能损失

使用 `AesirView<T>`（继承 `AesirMonoBehaviour`）即可自动获得 Odin 支持。

### ObservableValueAttributeProcessor

当项目包含 Odin Inspector 时，`ObservableValueAttributeProcessor<T>` 会自动为 `ObservableValue<T>` 字段注入以下 Inspector 特性：

- **HideLabel** — 隐藏字段本身的标签（显示内联值）
- **InlineProperty** — 内联显示子属性
- **OnValueChanged("Invoke", true)** — Inspector 中修改值时自动触发通知（包含子属性变更）
- **LabelText("@$property.Parent.Name", ...)** — 使用父属性名作为标签，附带眼睛图标

效果：在 Inspector 中直接编辑 `ObservableValue<T>` 字段时，值变更会自动通知所有监听者。

### 程序集

Odin 集成位于独立程序集 `Runestone.AesirArchitecture.Editor.OdinIntegration`，仅当项目中存在 Odin Inspector 时编译。若项目不包含 Odin Inspector，此程序集不会被加载，不影响框架核心功能。

### 示例

可导入 `ObservableValue` 示例（通过 Package Manager → Samples → Import）查看 Inspector 效果：

- 简单类型：`int`、`float`、`string`、`bool`、`Vector2`
- 复合类型：可序列化 `struct` / `class`

---

## 10. 路线图

### 已完成 (0.2.0)

- [x] 核心 MVP / MVC 分层
- [x] PlayerLoop 原生生命周期注入
- [x] 命令模式（同步 + 异步）
- [x] ObservableValue 响应式属性（协变只读）
- [x] MiniEventBus 类型事件总线 + 自动移除监听生命周期绑定
- [x] GenericLocator 泛型定位器
- [x] 依赖声明与校验机制
- [x] Inspector 可视化看板（ContextBoard / MiniEventBusBoard）
- [x] Domain Reload 安全
- [x] Odin Inspector 集成（ObservableValueAttributeProcessor）
- [x] 单元测试覆盖

### 规划中

- [ ] ScriptableObject 可视化配置层
- [ ] SO EventChannel 事件通道
- [ ] Editor 工具链（SO Inspector / MVP 脚手架 / 模块可视化）
- [ ] 运行时集合（RuntimeSet）
