# AesirArchitecture 事件系统拷打报告

> **审查日期**: 2026-06-23
> **审查范围**: `Runtime/Event/` 全部 12 个文件 + Context 集成 + ObservableProperty + Samples
> **审查维度**: 易用性、效率、使用步骤、代码结构、唯一流程、灵活性

---

## 目录

1. [系统全貌](#一系统全貌)
2. [易用性](#二易用性)
3. [效率](#三效率)
4. [使用步骤与心智负担](#四使用步骤与心智负担)
5. [代码结构](#五代码结构)
6. [是否为唯一使用流程](#六是否为唯一使用流程)
7. [灵活性](#七灵活性)
8. [优化建议方案](#八优化建议方案)
9. [IEventArgs 键碰撞问题与分层约束方案](#九iargs-键碰撞问题与分层约束方案)
10. [推荐的唯一使用流程](#十推荐的唯一使用流程)

---

## 一、系统全貌

```
IEventArgs (marker)
├── MiniEvent           — 无参事件 (Action)
├── MiniEvent<T>        — 单参事件 (Action<T>) where T : IEventArgs
└── MiniEventBus        — Type→ISubscribe 字典，类型键控路由

ISubscribe : IDisposable
├── MiniEvent          ✓ 实现
└── MiniEvent<T>       ✓ 实现 (但接口方法 Subscribe(Action) 用 Wrapper 丢弃参数)

IUnsubscribe : IDisposable          ← 过度设计，唯一实现者 AutoUnsubscribeHandle
└── AutoUnsubscribeHandle (struct, 幂等)

生命周期自动注销：
├── UnsubscribeInvoker (abstract MonoBehaviour)
│   ├── UnsubscribeOnDestroyInvoker
│   └── UnsubscribeOnDisableInvoker
├── UnsubscribeOnSceneUnloadedInvoker (Singleton)
├── UnsubscribeHandleCollection (List<IUnsubscribe>)
└── UnsubscribeExtensions (fluent: .UnsubscribeWhenXxx())

Context 集成：
ICanSubscribeWithContext / ICanInvokeWithContext → IContextHolder.GetContext().EventBus
```

### 文件清单

| 文件 | 职责 |
|------|------|
| `IEventArgs.cs` | 事件参数标记接口 |
| `ISubscribe.cs` | 订阅能力接口，继承 `IDisposable` |
| `IUnsubscribe.cs` | 注销句柄接口，继承 `IDisposable`（过度设计，见 [第五章 问题 5](#问题-5iunsubscribe-接口是过度设计)） |
| `MiniEvent.cs` | `MiniEvent`（无参）+ `MiniEvent<T>`（单参）实现 |
| `MiniEventBus.cs` | 类型键控事件总线 |
| `AutoUnsubscribeHandle.cs` | 幂等注销句柄（struct） |
| `UnsubscribeExtensions.cs` | Fluent 自动注销扩展方法 |
| `UnsubscribeHandleCollection.cs` | 句柄集合，批量注销 |
| `UnsubscribeInvoker.cs` | MonoBehaviour 自动注销基类 |
| `UnsubscribeOnDestroyInvoker.cs` | OnDestroy 触发注销 |
| `UnsubscribeOnDisableInvoker.cs` | OnDisable 触发注销 |
| `UnsubscribeOnSceneUnloadedInvoker.cs` | 场景卸载触发注销（全局单例） |

---

## 二、易用性

### 做得好的

- **Fluent 自动注销链**：`event.Subscribe(cb).UnsubscribeWhenGameObjectOnDestroyed(gameObject)` 一行搞定订阅 + 生命周期绑定，读起来直观。
- **`SubscribeAndInvoke`**：解决了"订阅时同步当前状态"的常见需求。
- **`AutoUnsubscribeHandle` 幂等 Dispose**：重复调用安全，不用操心。

### 问题

#### 问题 1：`MiniEvent`（无参）与 `MiniEventBus` 完全脱节

`MiniEventBus` 的所有 API 都约束 `T : IEventArgs`：

```csharp
public IUnsubscribe Subscribe<T>(Action<T> onEvent) where T : IEventArgs =>
    AddAndGet<MiniEvent<T>>().Subscribe(onEvent);
```

这意味着 `MiniEvent`（无参版本）**无法注册到 EventBus**。用户如果要通过 Bus 发无参事件，必须创建一个空结构体：

```csharp
struct EmptyEvent : IEventArgs { }  // 仅为满足约束的 ceremony
bus.Invoke<EmptyEvent>();             // 等价于无参调用，但多了一层包装
```

这违背了"减少心智负担"原则。`MiniEvent` 的存在暗示用户可以直接用它，但一旦走 Bus 路线就失效了。

#### 问题 2：`ISubscribe.Subscribe(Action)` 在 `MiniEvent<T>` 上是语义错误的

```csharp
// MiniEvent<T> 的显式接口实现
ISubscribe ISubscribe.Subscribe(Action callback)
{
    return Subscribe(Wrapper);
    void Wrapper(T value) { callback(); }  // ← 丢弃了事件参数！
}
```

这个方法存在的唯一原因是满足 `ISubscribe` 接口。它让用户可以写出 `((ISubscribe)miniEventT).Subscribe(action)` 这样的代码——订阅了一个带参事件却完全忽略参数。这是**一个没有任何合理用例的 API**，反而制造了误用入口。

#### 问题 3：`ObservableProperty` 重复实现了整套订阅逻辑

`ObservableProperty<T>` 没有组合 `MiniEvent<T>`，而是自己维护 `Action<T> _onValueChanged`，自己实现 `Subscribe`/`Unsubscribe`/`SubscribeAndInvoke`，自己创建 `AutoUnsubscribeHandle`：

```csharp
// ObservableProperty.cs — 完全重复 MiniEvent<T> 的模式
public IUnsubscribe Subscribe(Action<T> callback)
{
    _onValueChanged += callback;
    return new AutoUnsubscribeHandle(() => Unsubscribe(callback));
}
```

这是明显的代码重复。如果 `MiniEvent<T>` 的订阅逻辑要修改（比如加线程安全、加日志），`ObservableProperty` 必须同步修改，否则行为不一致。

---

## 三、效率

### 做得好的

- `AutoUnsubscribeHandle` 是 `struct`，理论上零堆分配。
- `Action +=`/`-=` 委托组合是高效的。
- `MiniEventBus` 用 `Dictionary<Type, ISubscribe>` 做 O(1) 查找。

### 问题

#### 问题 1：`AutoUnsubscribeHandle` 是 struct，但一进集合就被装箱

`UnsubscribeHandleCollection._handles` 是 `List<IUnsubscribe>`：

```csharp
readonly List<IUnsubscribe> _handles = new List<IUnsubscribe>();
```

`AutoUnsubscribeHandle` 是 struct，实现 `IUnsubscribe`（interface）。每次 `Add(handle)` 都会把 struct 装箱为 `IUnsubscribe` 引用——**每个订阅产生一次堆分配**。这直接抵消了用 struct 的初衷。

**根因**：`IUnsubscribe` 接口的存在迫使集合使用接口类型。删除 `IUnsubscribe`（P0-2）后集合自然变为 `List<AutoUnsubscribeHandle>`，装箱自动消失。

#### 问题 2：`MiniEvent<T>.ISubscribe.Subscribe(Action)` 的 Wrapper 闭包分配

每次调用都会创建一个闭包（捕获 `callback`），产生一次堆分配。虽然这个 API 实际上不应被使用，但它存在就意味着有人可能误用并产生不必要的 GC。

#### 问题 3：`UnsubscribeOnSceneUnloadedInvoker` 是全局单例，但无场景隔离

```csharp
void OnSceneUnloaded(Scene scene) => _handles.UnsubscribeAll();
```

**任何场景**卸载都会清空**所有**通过 `UnsubscribeWhenOnSceneUnloaded()` 注册的订阅。如果用户在场景 A 和场景 B 各注册了订阅，场景 A 卸载时场景 B 的订阅也被清空了。这是**静默的正确性错误**——不会报错，但行为不符合预期。

---

## 四、使用步骤与心智负担

这是最严重的问题。当前系统存在**多条并行路径**，用户需要理解全部才能做出选择。

### 订阅路径（8 种）

| # | 路径 | 注销方式 |
|---|------|---------|
| 1 | `miniEvent.Subscribe(cb)` | 手动 `Dispose()` handle |
| 2 | `miniEvent.Subscribe(cb).UnsubscribeWhenGameObjectOnDestroyed(go)` | 自动 |
| 3 | `miniEvent.Subscribe(cb).UnsubscribeWhenGameObjectOnDisable(go)` | 自动 |
| 4 | `miniEvent.Subscribe(cb).UnsubscribeWhenOnSceneUnloaded()` | 自动（全局） |
| 5 | `bus.Subscribe<T>(cb)` | 手动 `bus.Unsubscribe<T>(cb)` |
| 6 | `this.Subscribe<T>(cb)`（Context 能力） | 手动 `this.Unsubscribe<T>(cb)` |
| 7 | 手动存 `List<IUnsubscribe>` | 手动遍历 Dispose |
| 8 | 用 `UnsubscribeHandleCollection` | 手动 `UnsubscribeAll()` |

### 发布路径（6 种）

| # | 路径 | 约束 |
|---|------|------|
| 1 | `miniEvent.Invoke()` | 无参 |
| 2 | `miniEvent.Invoke(arg)` | 带参 |
| 3 | `bus.Invoke<T>()` | `T : IEventArgs, new()` |
| 4 | `bus.Invoke<T>(arg)` | `T : IEventArgs` |
| 5 | `this.Invoke<T>()`（Context） | `T : IEventArgs, new()` |
| 6 | `this.Invoke<T>(arg)`（Context） | `T : IEventArgs` |

### 心智负担分析

用户在写第一行事件代码前，需要搞清楚：

- 用 `MiniEvent` 还是 `MiniEventBus`？
- 走 Context 还是不走？
- 自动注销选 OnDestroy / OnDisable / SceneUnloaded？
- `IEventArgs` marker 接口是必须的吗？
- 无参事件怎么通过 Bus 发？

**这远超"一个心智模型"的合理范围。**

---

## 五、代码结构

### 做得好的

- `UnsubscribeHandleCollection` 从 Invoker 中提取，消除了重复。
- `[DisallowMultipleComponent]` 防止重复挂载。
- 能力接口（Capability）组合是干净的 Mixin 模式。

### 问题

#### 问题 1：`ISubscribe` 接口是失败的抽象

`ISubscribe` 唯一方法是 `Subscribe(Action)`（无参）。但 `MiniEvent<T>` 的核心 API 是 `Subscribe(Action<T>)`。为了满足接口，`MiniEvent<T>` 被迫实现了一个语义错误的 Wrapper。

`MiniEventBus` 内部用 `Dictionary<Type, ISubscribe>` 存储所有事件，但 Bus 的公共 API `Subscribe<T>` / `Unsubscribe<T>` / `Invoke<T>` 完全绕过 `ISubscribe`，直接操作 `MiniEvent<T>` 的具体方法。**`ISubscribe` 在 Bus 场景下零作用**。

`ISubscribe` 唯一发挥作用的场景是 `MiniEventBus._eventDictionary` 的值类型声明——但它只是 `Dictionary<Type, ISubscribe>` 的值类型约束，换成 `object` 或 `MiniEvent` 基类都一样。

#### 问题 2：`MiniEvent` 和 `MiniEvent<T>` 没有共同基类

它们都实现 `ISubscribe`，但：

- `MiniEvent` 有 `Invoke()` 和 `SubscribeAndInvoke(Action)`
- `MiniEvent<T>` 有 `Invoke(T)` 和 `SubscribeAndInvoke(Action<T>, T)`
- 方法签名完全不兼容，多态无意义

`ISubscribe` 作为共同接口，唯一能做的事就是 `Subscribe(Action)` + `Dispose()`。其中 `Subscribe(Action)` 在 `MiniEvent<T>` 上是错误的。

#### 问题 3：`MiniEvent` / `MiniEvent<T>` / `MiniEventBus` 都不是 `IDisposable`

它们都有 `Dispose()` 或 `Clear()` 方法（语义相同——清空所有委托引用），但都不实现 `IDisposable`。而 `ISubscribe` 继承了 `IDisposable`。这是不一致的。

#### 问题 4：`UnsubscribeOnDisableInvoker` 的生命周期配对关系未文档化

`UnsubscribeOnDisableInvoker` 本身设计是正确的——它与 `OnEnable` 中订阅形成天然配对：`OnEnable` 订阅 ↔ `OnDisable` 注销。GameObject 重新启用时 `OnEnable` 再次触发，重新订阅。

**但当前文档和 Sample 代码都没有说明这一配对关系**。Sample 代码（`UICounterMVCPanel`）在 `Start` 中订阅并用 `UnsubscribeWhenGameObjectOnDestroyed`，未展示 `OnEnable` + `UnsubscribeWhenGameObjectOnDisable` 的正确用法。用户不了解两者各自适用的场景。

更严重的问题是：如果用户错误地在 `OnEnable` 中订阅并使用 `UnsubscribeWhenGameObjectOnDestroyed`，由于 `OnEnable` 会被多次调用而 `OnDestroy` 只触发一次，会导致**订阅重复注册且句柄堆积**——每次 enable/disable 循环都往 `UnsubscribeOnDestroyInvoker` 的列表中追加新句柄，但从不清理，直到 GameObject 销毁。

#### 问题 5：`IUnsubscribe` 接口是过度设计

```csharp
public interface IUnsubscribe
{
    void Dispose();
}
```

`IUnsubscribe` 唯一实现者是 `AutoUnsubscribeHandle`（struct）。整个系统中不存在第二个实现，也不存在合理的场景需要第二个实现。

**推理链**：

1. 所有 `Subscribe` 方法内部都硬编码 `new AutoUnsubscribeHandle(...)` 返回，返回类型声明为 `IUnsubscribe`
2. 开发者无法在不修改 `MiniEvent` / `MiniEventBus` 源码的前提下，让 `Subscribe` 返回别的 `IUnsubscribe` 实现
3. 唯一能利用接口多态的路径是：开发者自己 `new` 一个 `IUnsubscribe` 实现类，传入 `UnsubscribeExtensions` / `UnsubscribeHandleCollection` — 但开发者手上已经有 `AutoUnsubscribeHandle` 了，没有理由另造一个
4. "第三方库想兼容 `.UnsubscribeWhenGameObjectOnDestroyed()`" — 直接用 `AutoUnsubscribeHandle` 即可，不需要实现接口

**无法列举出需要 `IUnsubscribe` 接口的完整使用场景。** 删掉它，所有 API 直接用 `AutoUnsubscribeHandle`，还能顺便消除装箱问题的根因（第三章问题 1 的根因正是 `List<IUnsubscribe>` 对 struct 的装箱）。

---

## 六、是否为唯一使用流程

**结论：否。存在 3 个入口 × 4 种注销策略 = 12 种组合，且无官方推荐路径。**

Sample 代码自身都不统一：

- **MVC Sample**：`_model.Count.Subscribe(UpdateCountText).UnsubscribeWhenGameObjectOnDestroyed(gameObject)`
- **MVP Sample**：`_presenter.Dispose()` 在 `OnDestroy` 中手动调用（内部存了 `IUnsubscribe` handle）

两种 Sample 展示了两种完全不同的注销策略，没有文档说明何时用哪种。

---

## 七、灵活性

### 做得好的

- `MiniEvent`/`MiniEventBus` 可独立于 Context 使用。
- 能力接口可组合。
- `MiniEventBus.AddAndGet<T>()` 懒加载，按需创建。

### 问题

#### 问题 1：`IEventArgs` 约束应分层处理，而非全局保留或全局移除

```csharp
public interface IEventArgs { }
```

`IEventArgs` 是空 marker 接口，约束了 `MiniEvent<T>` 和 `MiniEventBus` 的泛型参数。原批评建议直接移除，但经复查发现：**`IEventArgs` 在 `MiniEventBus` 场景下有不可替代的作用**。

`MiniEventBus` 使用 `Dictionary<Type, ...>` 做类型键控路由，键为 `typeof(T)`。如果移除约束，允许 `T = int`，则 Bus 中**只能存在一个 `int` 事件**——两个不同语义的 int 事件会键碰撞：

```csharp
// 如果移除 IEventArgs 约束，以下代码会冲突：
bus.Subscribe<int>(OnHealthChanged);   // 键 = typeof(int)
bus.Subscribe<int>(OnScoreChanged);    // 键 = typeof(int) ← 覆盖了上面！
```

`IEventArgs` 强制用户为每种事件创建独立的命名类型，天然解决了键碰撞：

```csharp
// 当前方式（IEventArgs 约束下）——安全，不会碰撞
struct HealthChangedEvent : IEventArgs { public int Value; }  // 键 = typeof(HealthChangedEvent)
struct ScoreChangedEvent  : IEventArgs { public int Value; }  // 键 = typeof(ScoreChangedEvent)
```

**但 `IEventArgs` 约束不应该施加在 `MiniEvent<T>` 本身上**。`MiniEvent<T>` 作为独立事件对象使用时（例如 `ObservableProperty<T>` 内部组合），`T` 不需要也不应该实现 `IEventArgs`——它只是个内部委托容器，不经过 Bus 路由，不存在键碰撞问题。

当前的问题是约束粒度一刀切：`MiniEvent<T>` 上的 `where T : IEventArgs` 约束使得 `ObservableProperty<T>` 无法直接组合 `MiniEvent<T>`（除非 `T : IEventArgs`），迫使用户为 `ObservableProperty<int>` 的 `int` 也实现 `IEventArgs`——这毫无意义。

#### 问题 2：无订阅查询能力

没有 `HasSubscribers`、`SubscriberCount`、`Contains(callback)` 等方法。调试时无法快速判断"这个事件有没有人监听"。

#### 问题 3：`MiniEventBus.Unsubscribe` 静默失败

```csharp
public void Unsubscribe<T>(Action<T> onEvent) where T : IEventArgs =>
    Get<MiniEvent<T>>()?.Unsubscribe(onEvent);
```

如果事件类型从未注册过，`Get` 返回 `null`，`?.` 静默跳过。用户可能因为拼写错误或类型不匹配导致注销失败却完全不知情。

---

## 八、优化建议方案

### 方案总览

| 优先级 | 改动 | 影响 |
|--------|------|------|
| P0 | 删除 `ISubscribe` 接口，让 `MiniEvent` / `MiniEvent<T>` 各自独立 | 消除失败的抽象 |
| P0 | 删除 `IUnsubscribe` 接口，所有 API 直接用 `AutoUnsubscribeHandle` | 消除过度设计 + 根除装箱 |
| P0 | `MiniEventBus` 内部改用 `object` 存储，公共 API 统一走 `MiniEvent<T>` | 简化 Bus 实现 |
| P0 | `UnsubscribeHandleCollection` 改用 `List<AutoUnsubscribeHandle>` 消除装箱（P0-2 删除接口后自动解决） | 修复性能 |
| P0 | `UnsubscribeOnSceneUnloadedInvoker` 支持场景名过滤或改为 per-scene 注册 | 修复正确性 |
| P1 | `ObservableProperty<T>` 组合 `MiniEvent<T>` 而非重复实现 | 消除重复 |
| P1 | `MiniEvent<T>` 移除 `IEventArgs` 约束，`MiniEventBus` 保留约束 | 消除 `ObservableProperty` ceremony，同时保留 Bus 类型键安全 |
| P1 | 补充 `UnsubscribeOnDisableInvoker` 配对文档，明确两种生命周期策略 | 防止用户错误搭配 `OnEnable` + `OnDestroy` |
| P2 | 统一 `IDisposable` 实现 | 一致性 |
| P2 | 加 `SubscriberCount` / `HasSubscribers` 查询方法 | 可调试性 |
| P2 | Bus 注销失败时走 `AesirArchitectureLog.Warning` | 可观测性 |

### 详细方案

#### P0-1：删除 `ISubscribe` 接口

```csharp
// 删除 ISubscribe.cs
// MiniEventBus 内部改为 Dictionary<Type, object>
// 或直接 Dictionary<Type, MiniEvent<object>>（需要重构 MiniEvent<T> 去掉 IEventArgs 约束后用 object）

// MiniEvent 和 MiniEvent<T> 不再实现任何公共接口
// 它们各自的 Subscribe/Unsubscribe/Invoke 方法签名已经足够清晰
```

**理由**：`ISubscribe` 唯一的作用是作为 `Dictionary<Type, ISubscribe>` 的值类型约束，但 Bus 的公共 API 从不通过 `ISubscribe` 调用。删除后不影响任何功能，消除了 `MiniEvent<T>` 上的语义错误 Wrapper。

#### P0-2：删除 `IUnsubscribe` 接口

`IUnsubscribe` 是过度设计（详见 [第五章 问题 5](#问题-5iunsubscribe-接口是过度设计)）。唯一实现者是 `AutoUnsubscribeHandle`，不存在也不需要第二个实现。

删除后所有涉及 `IUnsubscribe` 的位置改为直接用 `AutoUnsubscribeHandle`：

```csharp
// Subscribe 方法返回类型
// 修改前
public IUnsubscribe Subscribe(Action callback)
// 修改后
public AutoUnsubscribeHandle Subscribe(Action callback)

// UnsubscribeExtensions 扩展方法 this 参数
// 修改前
public static void UnsubscribeWhenGameObjectOnDestroyed(this IUnsubscribe unsubscribe, ...)
// 修改后
public static void UnsubscribeWhenGameObjectOnDestroyed(this AutoUnsubscribeHandle unsubscribe, ...)

// UnsubscribeHandleCollection 存储
// 修改前
readonly List<IUnsubscribe> _handles = new();
// 修改后
readonly List<AutoUnsubscribeHandle> _handles = new();  // ← 无装箱

// UnsubscribeInvoker.AddUnsubscribeHandle 参数
// 修改前
public IUnsubscribe AddUnsubscribeHandle(IUnsubscribe handle)
// 修改后
public AutoUnsubscribeHandle AddUnsubscribeHandle(AutoUnsubscribeHandle handle)
```

**连锁效果**：P0-2（原"消除装箱"）的根因正是 `List<IUnsubscribe>` 对 struct 的装箱。删除 `IUnsubscribe` 后，集合自然变为 `List<AutoUnsubscribeHandle>`，装箱问题自动消失，无需单独处理。

#### P0-3：`UnsubscribeHandleCollection` 消除装箱

> **注**：删除 `IUnsubscribe` 接口（P0-2）后，此问题自动解决。保留此节说明根因。

```csharp
public sealed class UnsubscribeHandleCollection
{
    readonly List<AutoUnsubscribeHandle> _handles = new();

    public void Add(AutoUnsubscribeHandle handle) => _handles.Add(handle);

    public void UnsubscribeAll()
    {
        foreach (var handle in _handles)
        {
            handle.Dispose();
        }
        _handles.Clear();
    }
}
```

**理由**：`AutoUnsubscribeHandle` 是 struct 就是为了避免分配，但 `List<IUnsubscribe>` 让每个 handle 都被装箱。改为具体类型集合即可。`UnsubscribeOnSceneUnloadedInvoker` 同理改用 `List<AutoUnsubscribeHandle>`。

#### P0-4：`UnsubscribeOnSceneUnloadedInvoker` 场景隔离

```csharp
public sealed class UnsubscribeOnSceneUnloadedInvoker : SingletonMonoBehaviour<UnsubscribeOnSceneUnloadedInvoker>
{
    readonly Dictionary<string, UnsubscribeHandleCollection> _sceneHandles = new();

    public AutoUnsubscribeHandle AddUnsubscribeHandle(AutoUnsubscribeHandle handle, string sceneName = null)
    {
        var key = sceneName ?? SceneManager.GetActiveScene().name;
        if (!_sceneHandles.TryGetValue(key, out var collection))
        {
            collection = new UnsubscribeHandleCollection();
            _sceneHandles[key] = collection;
        }
        collection.Add(handle);
        return handle;
    }

    void OnSceneUnloaded(Scene scene)
    {
        if (_sceneHandles.TryGetValue(scene.name, out var collection))
        {
            collection.UnsubscribeAll();
            _sceneHandles.Remove(scene.name);
        }
    }
}
```

**理由**：当前实现是全局"一键清空"，场景 A 卸载会误杀场景 B 的订阅。改为按场景名分桶。

#### P1-1：`ObservableProperty<T>` 组合 `MiniEvent<T>`

```csharp
public sealed class ObservableProperty<T> : IObservableProperty<T>
{
    readonly MiniEvent<T> _changedEvent = new();

    T value;

    public T Value
    {
        get => value;
        set
        {
            if (EqualityComparer<T>.Default.Equals(value, this.value)) return;
            this.value = value;
            _changedEvent.Invoke(value);
        }
    }

    public AutoUnsubscribeHandle Subscribe(Action<T> callback) => _changedEvent.Subscribe(callback);

    public AutoUnsubscribeHandle SubscribeAndInvoke(Action<T> callback)
    {
        var handle = Subscribe(callback);
        callback?.Invoke(value);
        return handle;
    }
    // ... 其他方法委托给 _changedEvent
}
```

**理由**：消除代码重复，让订阅逻辑只有一处实现。

#### P1-2：分层处理 `IEventArgs` 约束（`MiniEvent<T>` 移除，`MiniEventBus` 保留）

**背景**：原批评建议全局移除 `IEventArgs` 约束，但经复查发现 `IEventArgs` 在 `MiniEventBus` 场景下有不可替代的作用——它强制每种事件是独立命名类型，防止 `Dictionary<Type, ...>` 键碰撞。详见 [第七章 问题 1](#问题-1iargs-约束应分层处理而非全局保留或全局移除)。

**方案**：将约束粒度从"一刀切"改为"按场景分层"。

**`MiniEvent<T>` 移除约束**——作为独立事件容器，不经过 Bus 路由，不存在键碰撞：

```csharp
// 修改前
public sealed class MiniEvent<T> : ISubscribe where T : IEventArgs

// 修改后
public sealed class MiniEvent<T>  // 无约束
```

这样 `ObservableProperty<T>` 可以直接组合 `MiniEvent<int>`、`MiniEvent<string>` 等，无需为 `int` 实现 `IEventArgs`。

**`MiniEventBus` 保留约束**——类型键控字典要求每种事件是独立命名类型：

```csharp
// MiniEventBus 的公共 API 不变，仍约束 T : IEventArgs
public AutoUnsubscribeHandle Subscribe<T>(Action<T> onEvent) where T : IEventArgs
public void Invoke<T>(T e) where T : IEventArgs
```

**但 Bus 内部存储类型需要同步调整**，因为 `MiniEvent<T>` 不再实现 `ISubscribe`（P0-1 删除了该接口）。Bus 内部字典值类型改为 `object`：

```csharp
// 修改前
readonly Dictionary<Type, ISubscribe> _eventDictionary = new();

// 修改后
readonly Dictionary<Type, object> _eventDictionary = new();
```

`AddAndGet<T>()` 创建 `MiniEvent<T>`（`T : IEventArgs`），存入 `object`。`Get<T>()` 取出后强转 `MiniEvent<T>`。类型安全由 Bus 公共 API 的泛型约束保证，内部 `object` 存储不暴露给用户。

**完整效果**：

| 使用场景 | 约束 | 效果 |
|---------|------|------|
| `MiniEvent<int>` 独立使用 | 无 | `ObservableProperty<int>` 可直接组合 |
| `MiniEvent<string>` 独立使用 | 无 | 任意类型均可 |
| `bus.Subscribe<PlayerDiedEvent>(cb)` | `T : IEventArgs` | 键 = `typeof(PlayerDiedEvent)`，不会碰撞 |
| `bus.Subscribe<int>(cb)` | 编译错误 | 防止键碰撞 |

**对 `ObservableProperty<T>` 的连锁影响**：P1-1 方案中 `ObservableProperty<T>` 组合 `MiniEvent<T>` 后，由于 `MiniEvent<T>` 无约束，`ObservableProperty<int>` 可以正常工作：

```csharp
// ObservableProperty 内部——修改后可用
readonly MiniEvent<int> _changedEvent = new();  // ← 不再要求 int : IEventArgs
```

#### P1-3：明确两种生命周期配对策略（保留 `UnsubscribeOnDisableInvoker`）

`UnsubscribeOnDisableInvoker` 不应删除，它的设计是正确的。问题在于缺乏文档说明配对关系，导致用户可能误用。

**正确的两种配对策略**：

| 策略 | 订阅时机 | 注销方式 | 适用场景 |
|------|---------|---------|---------|
| A | `Awake` / `Start` | `UnsubscribeWhenGameObjectOnDestroyed(gameObject)` | 一次性订阅，订阅与 GameObject 同生命周期 |
| B | `OnEnable` | `UnsubscribeWhenGameObjectOnDisable(gameObject)` | 需要在每次启用时刷新订阅（如 UI Panel 状态可能变化） |

**错误搭配（必须避免）**：

```csharp
// ❌ OnEnable 订阅 + OnDestroy 注销
// OnEnable 会被多次调用（每次 enable/disable 循环），但 OnDestroy 只触发一次
// 导致：订阅重复注册 + 句柄列表不断堆积
void OnEnable()
{
    _context.Subscribe<T>(cb)
            .UnsubscribeWhenGameObjectOnDestroyed(gameObject);  // 错！
}
```

```csharp
// ✅ 正确策略 A：一次性订阅
void Start()
{
    _context.Subscribe<T>(cb)
            .UnsubscribeWhenGameObjectOnDestroyed(gameObject);
}
```

```csharp
// ✅ 正确策略 B：Enable/Disable 配对
void OnEnable()
{
    _context.Subscribe<T>(cb)
            .UnsubscribeWhenGameObjectOnDisable(gameObject);
    // OnDisable 触发时自动注销，下次 OnEnable 重新订阅
}
```

---

## 九、IEventArgs 键碰撞问题与分层约束方案

### 问题复现

`MiniEventBus` 使用 `Dictionary<Type, ...>` 做类型键控路由，键为 `typeof(T)`。如果移除 `IEventArgs` 约束、允许 `T = int`，则 Bus 中**只能存在一个 `int` 事件**——两个不同语义的 int 事件会键碰撞：

```csharp
// 假设移除 IEventArgs 约束，以下代码产生静默覆盖：
bus.Subscribe<int>(OnHealthChanged);   // 键 = typeof(int)
bus.Subscribe<int>(OnScoreChanged);    // 键 = typeof(int) ← 覆盖了 HealthChanged！

bus.Invoke(100);  // 两个回调都会被调用（因为都挂在同一个 MiniEvent<int> 上）
// 用户以为发的是 "Score = 100"，但 HealthChanged 订阅者也收到了
```

`IEventArgs` 通过强制每种事件创建独立命名类型，天然解决了这个问题：

```csharp
struct HealthChangedEvent : IEventArgs { public int Value; }  // 键 = typeof(HealthChangedEvent)
struct ScoreChangedEvent  : IEventArgs { public int Value; }  // 键 = typeof(ScoreChangedEvent)
// 两个键不同，各自独立的 MiniEvent<T> 实例，不会干扰
```

### 核心洞察

问题不在"`IEventArgs` 该不该存在"，而在"约束施加在哪个层级"：

| 层级 | 是否需要 `IEventArgs` 约束 | 原因 |
|------|--------------------------|------|
| `MiniEvent<T>` 定义 | **不需要** | 它是通用委托容器，`ObservableProperty<int>` 等内部组合它，`int` 无需实现 `IEventArgs` |
| `MiniEventBus` 公共 API | **需要** | 类型键控字典要求每种事件是独立命名类型，否则键碰撞 |

### 解决方案：分层约束

**`MiniEvent<T>` 移除约束**——作为独立事件容器，不经过 Bus 路由，不存在键碰撞：

```csharp
// 修改前
public sealed class MiniEvent<T> : ISubscribe where T : IEventArgs

// 修改后（P0-1 删除 ISubscribe 后）
public sealed class MiniEvent<T>  // 无约束
```

**`MiniEventBus` 保留约束**——类型键控字典的安全性由 Bus 层强制：

```csharp
// MiniEventBus 公共 API 不变，仍约束 T : IEventArgs
public AutoUnsubscribeHandle Subscribe<T>(Action<T> onEvent) where T : IEventArgs
public void Invoke<T>(T e) where T : IEventArgs
```

**Bus 内部存储调整为 `object`**（因为 `MiniEvent<T>` 不再实现 `ISubscribe`）：

```csharp
readonly Dictionary<Type, object> _eventDictionary = new();

public T Get<T>() where T : class =>
    _eventDictionary.TryGetValue(typeof(T), out var @event) ? (T)@event : default;
```

### 效果验证

| 使用场景 | 约束 | 结果 |
|---------|------|------|
| `new MiniEvent<int>()` 独立使用 | 无 | ✅ `ObservableProperty<int>` 可直接组合 |
| `new MiniEvent<string>()` 独立使用 | 无 | ✅ 任意类型可用 |
| `bus.Subscribe<PlayerDiedEvent>(cb)` | `T : IEventArgs` | ✅ 键 = `typeof(PlayerDiedEvent)`，不碰撞 |
| `bus.Subscribe<int>(cb)` | 编译错误 | ✅ 编译期阻止键碰撞 |
| `bus.Subscribe<HealthChangedEvent>(cb)` + `bus.Subscribe<ScoreChangedEvent>(cb)` | 各自 `T : IEventArgs` | ✅ 两个独立事件实例 |

### 对 `ObservableProperty<T>` 的连锁影响

P1-1 方案中 `ObservableProperty<T>` 组合 `MiniEvent<T>` 后，由于 `MiniEvent<T>` 已无约束，`ObservableProperty<int>` 可以正常工作：

```csharp
public sealed class ObservableProperty<T> : IObservableProperty<T>
{
    readonly MiniEvent<T> _changedEvent = new();  // ← T 无需 IEventArgs

    // ... 委托给 _changedEvent
}
```

---

## 十、推荐的唯一使用流程

精简后，推荐给用户的**唯一流程**：

```csharp
// 1. 定义事件（经 Bus 路由需要 IEventArgs，独立 MiniEvent 不需要）
public struct PlayerDiedEvent : IEventArgs { public int KillerId; }

// 2. 订阅（在 MonoBehaviour 中）——根据生命周期选择一种
void Start()
{
    // 策略 A：一次性订阅，与 GameObject 同生命周期
    _context.Subscribe<PlayerDiedEvent>(OnPlayerDied)
            .UnsubscribeWhenGameObjectOnDestroyed(gameObject);
}

// 或——两者择一，不要混用
void OnEnable()
{
    // 策略 B：Enable/Disable 配对，每次启用时刷新订阅
    _context.Subscribe<PlayerDiedEvent>(OnPlayerDied)
            .UnsubscribeWhenGameObjectOnDisable(gameObject);
}

// 3. 发布
_context.Invoke(new PlayerDiedEvent { KillerId = 42 });
```

**注销策略**：根据订阅时机选择匹配的注销方式，不可混用：

| 订阅时机 | 匹配的注销方式 |
|---------|-------------|
| `Start` / `Awake` | `UnsubscribeWhenGameObjectOnDestroyed(gameObject)` |
| `OnEnable` | `UnsubscribeWhenGameObjectOnDisable(gameObject)` |
| 纯 C# 类（非 MonoBehaviour） | 手动 `Dispose()` handle 或 `UnsubscribeWhenOnSceneUnloaded()` |

这样用户只需记住：

1. 定义事件结构体（`: IEventArgs`）
2. 在合适的生命周期函数中 `Subscribe` + 匹配的 `UnsubscribeWhenXxx`
3. `Invoke`

**三条规则，零选择困难。**
