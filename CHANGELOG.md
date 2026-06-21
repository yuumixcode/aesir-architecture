# Changelog

本项目的所有重要变更均会记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，
版本号遵循 [Semantic Versioning](https://semver.org/lang/zh-CN/)。

## [Unreleased]

### Changed

- `ISystem` 继承 `ICanInitialize`，获得 Initialize / Dispose 能力
- `ContextBase` 系统生命周期管理：`RegisterSystem` 上下文已初始化时立即初始化系统，`GetSystem` 按需初始化，`Dispose` 逆序释放 System（先 System 后 Model）
- 修正能力矩阵：IModel Dispose、ISystem Initialize / Dispose 列
- 修正快速开始示例：`ExecuteQuery` 需要双类型参数 `<TQuery, TResult>`

### 规划中

- ScriptableObject 可视化配置层
- SO EventChannel 事件通道
- Editor 工具链（SO Inspector / MVP 脚手架 / 模块可视化）
- 运行时集合（RuntimeSet）

## [0.1.0] - 2026-06-21

### Added

- **Context 架构根** — `Context<T>` 泛型静态单例，支持 `RegisterModel` / `RegisterSystem` 模块注册，`MockContext` 用于测试隔离
- **能力接口组合系统** — `ICanGetModel`、`ICanExecuteCommand`、`ICanSubscribeWithContext` 等能力标记接口，组合出 `IModel` / `ISystem` / `IView` / `IController` / `IPresenter`
- **PlayerLoop 原生生命周期** — `AesirArchitectureLifeCycle` 将自定义子系统注入 Unity PlayerLoop，提供 `BeforeUpdate` / `AfterUpdate` 帧回调
- **CQRS 命令/查询分离** — `ICommand` / `IAsyncCommand` 写操作 + `IQuery<TResult>` / `IAsyncQuery<TResult>` 读操作
- **ObservableProperty 响应式属性** — `ObservableProperty<T>` 可写 + `IReadOnlyObservableProperty<out T>` 协变只读接口
- **MiniEventHub 类型事件总线** — 按事件类型注册/发布，支持 `IUnsubscribe` 自动注销句柄与 GameObject / 场景生命周期绑定
- **Domain Reload 安全** — 静态变量通过 `[RuntimeInitializeOnLoadMethod]` 显式重置
- **AesirArchitectureLog 统一日志** — 条件编译统一日志工具，禁止直接使用 `Debug.Log`
- **MonoBehaviour 适配层** — `AbstractView<T>` 作为纯 C# 核心与 Unity 之间的适配
- **Odin Inspector 集成**（可选） — `ObservablePropertyDrawer` 自定义 Inspector 绘制
- **Roslyn Analyzer** — AESIR001 规则：引用类型使用 `ObservableProperty<T>` 时未实现 `IEquatable<T>` 编译警告
- **单元测试** — Context / Container / ObservableProperty / LifeCycle 覆盖测试
- **示例** — UI Counter（MVC）、UI Counter（MVP）、ObservableProperty Inspector 演示
