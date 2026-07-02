# Changelog

本项目的所有重要变更均会记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，
版本号遵循 [Semantic Versioning](https://semver.org/lang/zh-CN/)。

## [Unreleased]

### 规划中

- ScriptableObject 可视化配置层
- SO EventChannel 事件通道
- Editor 工具链（SO Inspector / MVP 脚手架 / 模块可视化）
- 运行时集合（RuntimeSet）

## [0.2.0] - 2026-07-02

### Changed

- **事件 API 重命名** — `Subscribe` → `AddListener`、`Unsubscribe` → `RemoveListener`、`SubscribeAndInvoke` → `AddListenerAndInvoke`，对齐 Unity `UnityEvent.AddListener` / `RemoveListener` 命名习惯。同步重命名 `AutoUnsubscribeHandle` → `AutoRemoveListenerHandle`、`UnsubscribeExtensions` → `RemoveListenerExtensions`、`UnsubscribeHandleCollection` → `RemoveListenerHandleCollection`、`UnsubscribeInvoker` → `RemoveListenerTrigger`、`UnsubscribeOnDestroyInvoker` → `RemoveListenerOnDestroyTrigger`、`UnsubscribeOnDisableInvoker` → `RemoveListenerOnDisableTrigger`、`UnsubscribeOnSceneUnloadedInvoker` → `RemoveListenerOnSceneUnloadedTrigger`，扩展方法 `UnsubscribeWhenXxx` → `RemoveListenerWhenXxx`，`UnsubscribeAll` → `RemoveAllListeners`
- **能力接口简化** — `ICanSubscribeWithContext` → `ICanAddListener`（移除 "WithContext" 后缀）、`ICanInvokeWithContext` → `ICanInvokeEvent`，对应扩展方法 `CanSubscribeWithContextExtensions` → `CanAddListenerExtensions`、`CanInvokeWithContextExtensions` → `CanInvokeExtensions`
- **事件发布方法重命名** — `Invoke<T>()` → `InvokeEvent<T>()`，`IContext.Invoke` → `IContext.InvokeEvent`，语义更明确地表达"发布事件"而非泛型方法调用
- **System → Service 重命名** — 架构层 System 概念全面替换为 Service：`ISystem` → `IService`、`AbstractSystem` → `AbstractService`、`RegisterSystem` → `RegisterService`、`GetSystem` → `GetService`、`ICanGetSystem` → `ICanGetService`、`CanGetSystemExtensions` → `CanGetServiceExtensions`
- **GetContext() → Context 属性** — `IContextHolder` 的 `GetContext()` 方法改为 `Context` 只读属性，语义更清晰地表达"持有一个 Context"
- **Context 类重命名** — `Context<T>` → `AbstractContext<T>`（统一 `Abstract*` 命名规范）、`BaseContext` → `AbstractContext`（同上）、`MockContext` → `FakeContext`（按 Fowler 测试替身分类，它是 Fake 而非 Mock）
- **ContextResetAssistant → GenericResetStaticsAssistant** — 重命名为泛型静态变量重置助手，不再局限于 Context 类型
- **AesirArchitectureLifeCycle → AesirArchitecturePlayerLoop** — 重命名 PlayerLoop 生命周期管理类，更准确描述其职责
- **AssemblyVisibleSettings → AssemblyInfo** — 重命名程序集可见性声明文件
- **View 基类拆分** — `AbstractView<T>` 拆分为 `AesirView<T>`（继承 `AesirMonoBehaviour`，自动支持 Odin Inspector 序列化）和 `MonoView<T>`（继承 `MonoBehaviour`，无 Odin 依赖）
- **SingletonMonoBehaviour → AesirMonoBehaviour + AesirArchitecture** — 移除泛型单例基类 `SingletonMonoBehaviour<T>`，新增 `AesirMonoBehaviour`（根据运行环境自动选择序列化方式的基类）和 `AesirArchitecture`（框架 MonoBehaviour 单例入口）
- **Container → GenericLocator\<T\>** — 移除 `Container<T>` 类，引入 `GenericLocator<T>` / `IGenericLocator<T>` / `ILocator` 泛型定位器体系，支持类型注册/查询/全局单例
- **目录三级重构** — `Runtime/` 下按 `Engine/`（纯 C# + 使用 UnityEngine API，不依赖 MonoBehaviour）、`Component/`（MonoBehaviour 组件）、`OdinIntergration/`（独立程序集，依赖 Odin Inspector）三级分离

### Added

- **GenericLocator\<T\>** — 泛型对象定位器，按类型注册、查询与获取对象实例，支持全局单例 `GenericLocator<T>.Global`，兼容 Domain Reload
- **ILocator / IGenericLocator\<T\>** — 定位器接口抽象，提供 `Register` / `Get` / `TryGet` / `IsRegistered` / `Unregister` / `GetByType` / `GetAll` 完整 API
- **ContextBoard** — 上下文看板 MonoBehaviour 组件，在 Inspector 中以字典形式展示每个 Context 的 Model 和 Service 列表
- **MiniEventBusBoard** — 事件总线看板 MonoBehaviour 组件，在 Inspector 中展示当前事件注册状态（事件类型、监听者列表）
- **ContextDependencyAssistant** — 依赖项校验辅助类，提供 Model 和 Service 的依赖类型检查与初始化状态检查，支持声明式依赖 `GetDependencies()`
- **AesirMonoBehaviour** — RAA 框架标准 MonoBehaviour 基类，根据运行环境（编辑器/运行时、是否安装 Odin Inspector）自动选择 `SerializedMonoBehaviour` 或 `MonoBehaviour` 作为基类
- **AesirScriptableObject** — RAA 框架标准 ScriptableObject 基类，同样根据运行环境自动选择序列化方式
- **IModel / IService 依赖声明** — `IModel` 和 `IService` 新增 `GetDependencies()` 方法，返回 `HashSet<Type>` 声明依赖的其他模块类型，注册时自动校验依赖是否已初始化
- **IView 能力扩展** — `IView` 新增 `ICanGetService` 和 `ICanInvokeEvent` 能力，View 层现在可以获取 Service 和发布事件
- **IContext 事件方法** — `IContext` 接口直接提供 `AddListener<T>` / `RemoveListener<T>` / `InvokeEvent<T>` 方法，事件操作不再依赖独立的事件总线属性
- **AbstractContext\<T\> 全局定位** — `AbstractContext<T>.Interface` 通过 `GenericLocator<IContext>.Global` 管理单例，注册到 `ContextBoard` 可视化
- **MiniEventBus.Global** — 全局事件总线单例，`AbstractContext` 的事件操作直接路由到 `MiniEventBus.Global`
- **MiniEventBusBoard** — 事件注册信息看板，Inspector 中可视化展示当前所有事件类型及其监听者
- **EventRegistrationInfo** — 事件注册信息数据类，记录事件类型和监听者列表
- **Odin Inspector AttributeProcessor 扩展** — 新增 `ContextBoardAttributeProcessor`、`MiniEventBusBoardAttributeProcessor`、`AbstractModelAttributeProcessor` 等 Odin 属性处理器
- **Editor 脚本符号管理** — 新增 `EnsureAesirArchitectureDefine` 和 `ScriptingSymbolUtility`，自动管理 `ODIN_INSPECTOR` 等编译符号
- **Documentation~/Books/** — 新增设计模式与 SOLID、ScriptableObject 模块化架构参考电子书
- **Documentation~/Manuals/mini-event-module-manual.md** — 新增 MiniEvent 模块完整使用手册
- **Documentation~/FAQ/why-fake-not-mock-context.md** — 新增 FakeContext 命名决策文档

### Removed

- **Query 系统** — 移除 `IQuery<TResult>` / `IAsyncQuery<TResult>` / `AbstractQuery<T>` / `AbstractAsyncQuery<T>` 及对应的能力接口 `ICanExecuteQuery` 和扩展方法，保持架构核心简单。读操作直接通过 `GetModel<T>()` 访问 Model 数据
- **FakeContext** — 移除测试用 Fake 上下文类（`MockContext` / `FakeContext`），测试改为直接实例化 `AbstractContext<T>` 子类或使用 `GenericLocator` 隔离
- **Container\<T\>** — 移除旧的模块容器类，由 `GenericLocator<T>` 替代
- **SingletonMonoBehaviour\<T\>** — 移除泛型单例基类，由 `AesirMonoBehaviour` + `AesirArchitecture` 替代
- **event-system-critique.md** — 移除旧的事件系统设计文档（内容已整合到 Manuals 和 FAQ 中）
- **旧目录结构** — 移除 `Runtime/Core/`、`Runtime/Command/`、`Runtime/Event/`、`Runtime/Observable/`、`Runtime/Utilities/` 等平铺目录

## [0.1.1] - 2026-06-24

### Changed

- **事件系统重构** — 移除 `ISubscribe` / `IUnsubscribe` 接口，简化事件系统设计，直接使用委托字段和扩展方法
- **MiniEventHub → MiniEventBus** — 重命名并优化实现，提升 API 一致性
- **ObservableValue 目录调整** — 从 `Runtime/Observer/` 移至 `Runtime/Observable/`，更符合 C# 命名习惯
- **ContextBase → AbstractContext** — 重命名统一上下文基类命名规范，对齐 `Abstract*` 前缀约定
- **AssemblyInfo → AssemblyVisibleSettings** — 重命名更准确描述文件用途

### Added

- **AbstractService** — 新增服务层标准基类，提供 `Initialize` / `Dispose` 生命周期管理
- **代码样式指南** — 新增 `Documentation~/Rules/raa-code-style.md`，完整定义 RAA 代码规范
- **事件系统设计文档** — 新增 `Documentation~/event-system-critique.md`，记录事件系统设计决策
- **IUnsubscribe 移除决策文档** — 新增 `Documentation~/why-remove-iunsubscribe.md`，说明移除接口的思考过程
- **Editor 端单元测试** — 将部分测试从 Runtime 移至 Editor，提升测试分类合理性
- **UnityEngineObjectCheckNullTests** — 新增 Unity 对象空检查测试

### Removed

- **ISubscribe / IUnsubscribe 接口** — 简化事件系统，直接使用委托字段
- **MiniEventHub** — 重构为 `MiniEventBus`

## [0.1.0] - 2026-06-21

### Added

- **Context 架构根** — `Context<T>` 泛型静态单例，支持 `RegisterModel` / `RegisterService` 模块注册，`FakeContext` 用于测试隔离
- **能力接口组合系统** — `ICanGetModel`、`ICanExecuteCommand`、`ICanSubscribeWithContext` 等能力标记接口，组合出 `IModel` / `IService` / `IView` / `IController` / `IPresenter`
- **PlayerLoop 原生生命周期** — `AesirArchitectureLifeCycle` 将自定义子系统注入 Unity PlayerLoop，提供 `BeforeUpdate` / `AfterUpdate` 帧回调
- **CQRS 命令/查询分离** — `ICommand` / `IAsyncCommand` 写操作 + `IQuery<TResult>` / `IAsyncQuery<TResult>` 读操作
- **ObservableValue 响应式属性** — `ObservableValue<T>` 可写 + `IReadOnlyObservableValue<out T>` 协变只读接口
- **MiniEventHub 类型事件总线** — 按事件类型注册/发布，支持 `IUnsubscribe` 自动注销句柄与 GameObject / 场景生命周期绑定
- **Domain Reload 安全** — 静态变量通过 `[RuntimeInitializeOnLoadMethod]` 显式重置
- **AesirArchitectureLog 统一日志** — 条件编译统一日志工具，禁止直接使用 `Debug.Log`
- **MonoBehaviour 适配层** — `AbstractView<T>` 作为纯 C# 核心与 Unity 之间的适配
- **Odin Inspector 集成**（可选） — `ObservableValueAttributeProcessor` 属性注入
- **Roslyn Analyzer** — AESIR001 规则：引用类型使用 `ObservableValue<T>` 时未实现 `IEquatable<T>` 编译警告
- **单元测试** — Context / Container / ObservableValue / LifeCycle 覆盖测试
- **示例** — UI Counter（MVC）、UI Counter（MVP）、ObservableValue（Odin Inspector）

### Changed

- `IService` 继承 `ICanInitialize`，获得 Initialize / Dispose 能力
- `AbstractContext` 服务生命周期管理：`RegisterService` 上下文已初始化时立即初始化服务，`GetService` 按需初始化，`Dispose` 逆序释放 Service（先 Service 后 Model）
- 移除 `ObservableValueDrawer`，Odin 集成仅保留 `ObservableValueAttributeProcessor` 属性注入
