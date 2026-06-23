# 基于 QFramework 的事件系统为什么要移除 IUnsubscribe 接口

> **文档版本**: 3.0 | **日期**: 2026-06-23
> **适用范围**: AesirArchitecture `Runtime/Event/`
> **前置阅读**: [事件系统拷打报告](event-system-critique.md)
> **源码参考**: `Documentation~/QFramework.cs`（QFramework v1.0 完整源码，MIT License, Copyright liangxiegame）

---

## 目录

1. [结论：移除的好处与保留的冗余](#一结论移除的好处与保留的冗余)
2. [QFramework 事件系统简介](#二qframework-事件系统简介)
3. [IUnsubscribe 在 AesirArchitecture 中的现状](#三iunsubscribe-在-aesirarchitecture-中的现状)
4. [为什么要移除：从实际使用场景出发](#四为什么要移除从实际使用场景出发)
5. [移除前后的差别](#五移除前后的差别)
6. [对开发者易用性的影响](#六对开发者易用性的影响)
7. [移除后开发者如何扩展](#七移除后开发者如何扩展)
8. [总结](#八总结)

---

## 一、结论：移除的好处与保留的冗余

### 移除的好处

| 好处 | 说明 | 来源 |
|------|------|------|
| **消除装箱** | `AutoUnsubscribeHandle` 是 struct，但存入 `List<IUnsubscribe>` 时被装箱为接口引用，每次订阅产生一次堆分配。移除接口后集合变为 `List<AutoUnsubscribeHandle>`，零分配 | `AutoUnsubscribeHandle.cs`（struct）、`UnsubscribeHandleCollection.cs`（`List<IUnsubscribe>`） |
| **减少心智负担** | 开发者不再面对"要不要实现 `IUnsubscribe`"的抉择——一个只有一个实现者的接口会让人误以为需要扩展 | IDE 智能提示会列出 `IUnsubscribe` 接口，诱导开发者研究其用途 |
| **减少代码量** | 删除 `IUnsubscribe.cs`，所有 `Subscribe`/`Unsubscribe`/扩展方法的返回值和参数类型从接口简化为具体类型 | `IUnsubscribe.cs`、`MiniEvent.cs`、`MiniEventBus.cs`、`UnsubscribeExtensions.cs`、`UnsubscribeHandleCollection.cs`、`UnsubscribeInvoker.cs` 共 6 个文件受影响 |
| **调用端零影响** | 所有 `.UnsubscribeWhenXxx()` 调用代码不变 | 调用端操作的是返回值，`var handle = Subscribe(cb)` 的 `var` 自动推导 |

### 保留的冗余

接口存在的合理理由有两种：**多态需求**（系统中有 ≥2 个实现类，调用方需要通过接口统一使用）和 **扩展契约**（为开发者提供扩展点，让开发者自行实现接口接入框架）。

`IUnsubscribe` 在 QFramework 和 AesirArchitecture 中**两个理由都不成立**：

| 理由 | 是否成立 | 依据 |
|------|---------|------|
| 多态需求 | ✗ | QFramework 中 `IUnRegister` 唯一实现者是 `CustomUnRegister`（QFramework.cs L312-321）；AesirArchitecture 中 `IUnsubscribe` 唯一实现者是 `AutoUnsubscribeHandle`。`UnRegisterTrigger` / `UnsubscribeInvoker` **不实现**接口，只是持有接口集合 |
| 扩展契约 | ✗ | `Subscribe` 方法内部硬编码 `new AutoUnsubscribeHandle(...)`（与 QFramework 中 `EasyEvent.Register()` 硬编码 `new CustomUnRegister(...)` 一致）。开发者即使实现了接口，也无法让 `Subscribe` 返回自定义类型。开发者唯一的扩展路径是手动 new 后传入扩展方法，但 `AutoUnsubscribeHandle` 接受任意 `Action`，已覆盖所有注销逻辑变体 |

**详细场景验证见 [第四章](#四为什么要移除从实际使用场景出发)。**

---

## 二、QFramework 事件系统简介

> 以下所有代码引用均来自 `Documentation~/QFramework.cs`，标注行号。

### 2.1 IUnRegister 接口

QFramework 定义了 `IUnRegister` 接口，只有一个方法 `UnRegister()`：

```csharp
// QFramework.cs L293-296
public interface IUnRegister
{
    void UnRegister();
}
```

### 2.2 唯一实现者：CustomUnRegister（struct）

`CustomUnRegister` 是 `IUnRegister` 在整个 QFramework 源码中的**唯一实现类**：

```csharp
// QFramework.cs L312-321
public struct CustomUnRegister : IUnRegister
{
    private Action mOnUnRegister { get; set; }
    public CustomUnRegister(Action onUnRegister) => mOnUnRegister = onUnRegister;

    public void UnRegister()
    {
        mOnUnRegister.Invoke();
        mOnUnRegister = null;
    }
}
```

注意：`CustomUnRegister` **不是幂等的**——第二次调用 `UnRegister()` 会触发 `NullReferenceException`（`mOnUnRegister` 被置 null 后再 Invoke）。

### 2.3 事件类：EasyEvent / EasyEvent<T>

`EasyEvent` 和 `EasyEvent<T>` 的 `Register()` 方法内部硬编码返回 `new CustomUnRegister(...)`：

```csharp
// QFramework.cs L580-587（EasyEvent）
public class EasyEvent : IEasyEvent
{
    private Action mOnEvent = () => { };

    public IUnRegister Register(Action onEvent)
    {
        mOnEvent += onEvent;
        return new CustomUnRegister(() => { UnRegister(onEvent); });  // ← 硬编码
    }
    // ...
}
```

```csharp
// QFramework.cs L589-601（EasyEvent<T>）
public class EasyEvent<T> : IEasyEvent
{
    private Action<T> mOnEvent = e => { };

    public IUnRegister Register(Action<T> onEvent)
    {
        mOnEvent += onEvent;
        return new CustomUnRegister(() => { UnRegister(onEvent); });  // ← 硬编码
    }
    // ...
}
```

### 2.4 类型事件总线：TypeEventSystem

`TypeEventSystem` 委托给 `EasyEvents` → `EasyEvent<T>.Register()`，最终也是返回 `CustomUnRegister`：

```csharp
// QFramework.cs L431-447
public class TypeEventSystem
{
    private readonly EasyEvents mEvents = new EasyEvents();
    public static readonly TypeEventSystem Global = new TypeEventSystem();

    public void Send<T>() where T : new() => mEvents.GetEvent<EasyEvent<T>>()?.Trigger(new T());
    public void Send<T>(T e) => mEvents.GetEvent<EasyEvent<T>>()?.Trigger(e);

    public IUnRegister Register<T>(Action<T> onEvent) =>
        mEvents.GetOrAddEvent<EasyEvent<T>>().Register(onEvent);  // ← 最终返回 CustomUnRegister

    public void UnRegister<T>(Action<T> onEvent)
    {
        var e = mEvents.GetEvent<EasyEvent<T>>();
        e?.UnRegister(onEvent);
    }
}
```

### 2.5 生命周期触发器：UnRegisterTrigger

`UnRegisterTrigger` 是抽象 MonoBehaviour，内部持有 `HashSet<IUnRegister>` 集合：

```csharp
// QFramework.cs L325-345
public abstract class UnRegisterTrigger : UnityEngine.MonoBehaviour
{
    private readonly HashSet<IUnRegister> mUnRegisters = new HashSet<IUnRegister>();

    public IUnRegister AddUnRegister(IUnRegister unRegister)
    {
        mUnRegisters.Add(unRegister);
        return unRegister;
    }

    public void RemoveUnRegister(IUnRegister unRegister) => mUnRegisters.Remove(unRegister);

    public void UnRegister()
    {
        foreach (var unRegister in mUnRegisters)
        {
            unRegister.UnRegister();
        }
        mUnRegisters.Clear();
    }
}
```

**关键事实**：`UnRegisterTrigger` 自身**不实现** `IUnRegister` 接口。它只是一个容器，持有 `IUnRegister` 集合。其子类 `UnRegisterOnDestroyTrigger`、`UnRegisterOnDisableTrigger`、`UnRegisterCurrentSceneUnloadedTrigger` 也都**不实现** `IUnRegister`。

### 2.6 生命周期扩展方法

扩展方法接收 `IUnRegister`，将其添加到对应触发器的集合中：

```csharp
// QFramework.cs L383-401
public static class UnRegisterExtension
{
    public static IUnRegister UnRegisterWhenGameObjectDestroyed(this IUnRegister unRegister,
        UnityEngine.GameObject gameObject) =>
        GetOrAddComponent<UnRegisterOnDestroyTrigger>(gameObject)
            .AddUnRegister(unRegister);

    public static IUnRegister UnRegisterWhenDisabled(this IUnRegister unRegister,
        UnityEngine.GameObject gameObject) =>
        GetOrAddComponent<UnRegisterOnDisableTrigger>(gameObject)
            .AddUnRegister(unRegister);

    public static IUnRegister UnRegisterWhenCurrentSceneUnloaded(this IUnRegister self) =>
        UnRegisterCurrentSceneUnloadedTrigger.Get.AddUnRegister(self);
}
```

### 2.7 BindableProperty<T>

`BindableProperty<T>` 内部组合 `EasyEvent<T>`，`Register()` 返回 `IUnRegister`：

```csharp
// QFramework.cs L494-507
private EasyEvent<T> mOnValueChanged = new EasyEvent<T>();

public IUnRegister Register(Action<T> onValueChanged)
{
    return mOnValueChanged.Register(onValueChanged);  // ← 最终返回 CustomUnRegister
}

public IUnRegister RegisterWithInitValue(Action<T> onValueChanged)
{
    onValueChanged(mValue);
    return Register(onValueChanged);
}
```

### 2.8 IUnRegisterList 批量管理

```csharp
// QFramework.cs L298-310
public interface IUnRegisterList
{
    List<IUnRegister> UnregisterList { get; }
}

public static class IUnRegisterListExtension
{
    public static void AddToUnregisterList(this IUnRegister self, IUnRegisterList unRegisterList) =>
        unRegisterList.UnregisterList.Add(self);

    public static void UnRegisterAll(this IUnRegisterList self)
    {
        foreach (var unRegister in self.UnregisterList)
        {
            unRegister.UnRegister();
        }
        self.UnregisterList.Clear();
    }
}
```

### 2.9 架构总结

```
QFramework 事件系统结构：

IUnRegister (interface)
└── CustomUnRegister (struct)           ← 唯一实现者

EasyEvent / EasyEvent<T> / EasyEvent<T,K> / EasyEvent<T,K,S>
└── Register() → new CustomUnRegister(...)  ← 硬编码返回

TypeEventSystem
└── Register<T>() → EasyEvent<T>.Register() → new CustomUnRegister(...)

BindableProperty<T>
└── Register() → EasyEvent<T>.Register() → new CustomUnRegister(...)

UnRegisterTrigger (abstract MonoBehaviour)  ← 不实现 IUnRegister
├── UnRegisterOnDestroyTrigger
├── UnRegisterOnDisableTrigger
└── UnRegisterCurrentSceneUnloadedTrigger
    └── 内部持有 HashSet<IUnRegister>

UnRegisterExtension
└── this IUnRegister → GetOrAddComponent<Trigger>().AddUnRegister(unRegister)

IUnRegisterList
└── List<IUnRegister>  ← 批量管理
```

---

## 三、IUnsubscribe 在 AesirArchitecture 中的现状

AesirArchitecture 移植了 QFramework 的事件系统，结构几乎一一对应：

| QFramework | AesirArchitecture | 对应关系 |
|---|---|---|
| `IUnRegister` | `IUnsubscribe` | 接口，方法名 `UnRegister()` → `Dispose()` |
| `CustomUnRegister` (struct) | `AutoUnsubscribeHandle` (struct) | 唯一实现者，**AesirArchitecture 版本增加了幂等性**（`_disposed` 标志） |
| `EasyEvent` / `EasyEvent<T>` | `MiniEvent` / `MiniEvent<T>` | `Register()` → `Subscribe()` |
| `TypeEventSystem` | `MiniEventBus` | 类型键控事件总线 |
| `BindableProperty<T>` | `ObservableProperty<T>` | 可观察属性 |
| `UnRegisterTrigger` | `UnsubscribeInvoker` | 抽象 MonoBehaviour，**不实现接口** |
| `UnRegisterOnDestroyTrigger` | `UnsubscribeOnDestroyInvoker` | OnDestroy 触发 |
| `UnRegisterOnDisableTrigger` | `UnsubscribeOnDisableInvoker` | OnDisable 触发 |
| `UnRegisterCurrentSceneUnloadedTrigger` | `UnsubscribeOnSceneUnloadedInvoker` | 场景卸载触发 |
| `UnRegisterExtension` | `UnsubscribeExtensions` | Fluent 扩展方法 |
| `IUnRegisterList` / `IUnRegisterListExtension` | `UnsubscribeHandleCollection` | 批量管理 |

### 关键事实

`IUnsubscribe` 在 AesirArchitecture 中的**唯一实现者是 `AutoUnsubscribeHandle`**（struct），与 QFramework 中 `IUnRegister` 唯一实现者是 `CustomUnRegister` 完全对应。

**两个框架中，接口都只有一个实现者，且 `Subscribe`/`Register` 方法内部都硬编码返回该实现者。**

---

## 四、为什么要移除：从实际使用场景出发

### 4.1 接口存在的两个理由

一个接口存在的合理理由有两种：

1. **多态需求**：系统中已存在两个或以上的实现类，调用方需要通过接口统一使用它们。
2. **扩展契约**：设计意图是为开发者提供扩展点，让开发者可以自行实现接口来接入框架（即使框架自身只提供一个默认实现）。

第 1 种理由不适用——QFramework 中 `IUnRegister` 的实现者只有一个 `CustomUnRegister`（QFramework.cs L312-321），`UnRegisterTrigger`（QFramework.cs L325-345）**不实现** `IUnRegister` 接口。AesirArchitecture 中情况完全相同：`IUnsubscribe` 的实现者只有 `AutoUnsubscribeHandle`。

第 2 种理由需要认真评估——如果接口是"让开发者自行扩展"的契约，那即使当前只有一个实现，接口也有存在价值。下一节的场景推演专门验证这一点。

### 4.2 场景推演：开发者会想实现 IUnsubscribe 吗？

#### 场景 A："我想自定义注销句柄"

假设开发者想实现一个带日志的注销句柄：

```csharp
class LoggingUnsubscribeHandle : IUnsubscribe
{
    Action _callback;
    string _name;

    public LoggingUnsubscribeHandle(Action callback, string name)
    {
        _callback = callback;
        _name = name;
    }

    public void Dispose()
    {
        Debug.Log($"Unsubscribing: {_name}");
        _callback?.Invoke();
        _callback = null;
    }
}
```

**问题**：这个句柄无法被任何 `Subscribe` 方法返回。`MiniEvent.Subscribe()` 内部硬编码了 `new AutoUnsubscribeHandle(...)`（与 QFramework 中 `EasyEvent.Register()` 硬编码 `new CustomUnRegister(...)` 完全一致）。开发者要使用 `LoggingUnsubscribeHandle`，只能自己 `new` 一个，然后传入扩展方法——但 `AutoUnsubscribeHandle` 已经能做同样的事。

**结论**：场景不成立。`Subscribe` 方法不会返回开发者自定义的句柄类型。

#### 场景 B："第三方库想兼容 UnsubscribeWhenGameObjectOnDestroyed"

假设第三方库想让用户能用 `.UnsubscribeWhenGameObjectOnDestroyed(gameObject)`：

移除前，第三方库需要返回 `IUnsubscribe`：

```csharp
public IUnsubscribe ThirdPartySubscribe(Action cb)
{
    // 必须返回实现了 IUnsubscribe 的类型
    return new MyCustomHandle(...);  // ← 需要自己实现接口
}
```

移除后，直接返回 `AutoUnsubscribeHandle`：

```csharp
public AutoUnsubscribeHandle ThirdPartySubscribe(Action cb)
{
    return new AutoUnsubscribeHandle(() => /* 注销逻辑 */);
}
```

`AutoUnsubscribeHandle` 是 public struct，接受一个 `Action` 参数——任何第三方库都能直接使用，无需自己实现接口。

**结论**：场景不成立。直接用 `AutoUnsubscribeHandle` 即可。

#### 场景 C："我想做一个复合句柄，管理多个子句柄"

QFramework 有 `IUnRegisterList`（QFramework.cs L298-310）和 `OrEvent`（QFramework.cs L661-686）处理这个需求。AesirArchitecture 有 `UnsubscribeHandleCollection`。

开发者如果需要这个功能，直接用 `UnsubscribeHandleCollection` 或 `List<AutoUnsubscribeHandle>`。不需要通过接口抽象。

**结论**：场景不成立。已有工具覆盖此需求。

#### 场景 D："未来可能需要不同类型的句柄"

这是 YAGNI（You Aren't Gonna Need It）。句柄的唯一职责是"执行一次注销回调"。`AutoUnsubscribeHandle` 已经完美实现了这个职责（幂等、零分配 struct）。如果未来真有不同需求，那是一个全新的 API 设计，不应该通过实现现有接口来强行兼容。

**结论**：基于不存在的未来需求保留接口是过度设计。

### 4.3 "扩展契约"理由的验证

上一节场景 A–D 逐一验证了"开发者自行扩展"的实际可行性。结论是：即使开发者实现了 `IUnsubscribe` 接口，也无法被 `Subscribe` 方法返回（因为 `Subscribe` 内部硬编码了 `new AutoUnsubscribeHandle(...)`，与 QFramework 中 `EasyEvent.Register()` 硬编码 `new CustomUnRegister(...)` 完全一致）。

开发者唯一的扩展路径是：自己 `new` 一个自定义句柄，手动传入 `UnsubscribeExtensions` 扩展方法或 `UnsubscribeHandleCollection`。但这条路径上，开发者直接用 `AutoUnsubscribeHandle` 就能达到完全相同的效果——`AutoUnsubscribeHandle` 接受任意 `Action`，已经覆盖了所有注销逻辑的变体（日志、条件判断等只需在 `Action` 内实现）。

**因此"扩展契约"理由不成立**：接口确实为开发者提供了"我能造一个不同的句柄"的入口，但这个入口没有实际价值——开发者想要的所有注销行为都能通过传入不同的 `Action` 到 `AutoUnsubscribeHandle` 来实现，不需要新的句柄类型。

---

## 五、移除前后的差别

### 5.1 API 层面

#### 移除前

```csharp
// 接口定义
public interface IUnsubscribe { void Dispose(); }

// Subscribe 返回接口
public IUnsubscribe Subscribe(Action callback)
{
    _callbacks += callback;
    return new AutoUnsubscribeHandle(() => Unsubscribe(callback));
}

// 扩展方法接收接口
public static void UnsubscribeWhenGameObjectOnDestroyed(
    this IUnsubscribe unsubscribe, GameObject gameObject)

// 集合存储接口
readonly List<IUnsubscribe> _handles = new();
```

#### 移除后

```csharp
// 接口删除，AutoUnsubscribeHandle 直接作为类型

// Subscribe 返回具体类型
public AutoUnsubscribeHandle Subscribe(Action callback)
{
    _callbacks += callback;
    return new AutoUnsubscribeHandle(() => Unsubscribe(callback));
}

// 扩展方法接收具体类型
public static void UnsubscribeWhenGameObjectOnDestroyed(
    this AutoUnsubscribeHandle unsubscribe, GameObject gameObject)

// 集合存储具体类型
readonly List<AutoUnsubscribeHandle> _handles = new();
```

#### 开发者调用代码

```csharp
// 移除前
var handle = _context.Subscribe<PlayerDiedEvent>(OnPlayerDied);
handle.UnsubscribeWhenGameObjectOnDestroyed(gameObject);

// 移除后 — 无变化
var handle = _context.Subscribe<PlayerDiedEvent>(OnPlayerDied);
handle.UnsubscribeWhenGameObjectOnDestroyed(gameObject);
```

**结论**：调用端代码完全不变。移除是内部实现层面的改动，对使用者透明。

### 5.2 性能层面

| 指标 | 移除前 | 移除后 |
|------|--------|--------|
| 每次订阅的堆分配 | 1 次（struct → `List<IUnsubscribe>` 装箱） | 0 次 |
| GC 压力 | 随订阅数量线性增长 | 无 |
| `AutoUnsubscribeHandle` 设计意图 | struct 但被装箱抵消 | struct 真正零分配 |

> **来源**：`AutoUnsubscribeHandle` 是 struct（`AutoUnsubscribeHandle.cs`），`UnsubscribeHandleCollection._handles` 是 `List<IUnsubscribe>`（`UnsubscribeHandleCollection.cs`）。struct 实现接口后存入 `List<IUnsubscribe>` 会产生装箱，这是 C# 语言规范行为。

### 5.3 与 QFramework 的对比

QFramework 同样存在装箱问题：`CustomUnRegister` 是 struct（QFramework.cs L312），`UnRegisterTrigger.mUnRegisters` 是 `HashSet<IUnRegister>`（QFramework.cs L327），`IUnRegisterListExtension` 操作 `List<IUnRegister>`（QFramework.cs L304-310）。struct 存入接口集合均会产生装箱。

**AesirArchitecture 移除 `IUnsubscribe` 后，消除了 QFramework 遗留的装箱问题，且不改变任何调用端 API。**

---

## 六、对开发者易用性的影响

### 6.1 日常使用：零影响

```csharp
// 这些代码移除前后完全一致
_context.Subscribe<PlayerDiedEvent>(OnPlayerDied)
       .UnsubscribeWhenGameObjectOnDestroyed(gameObject);

// 手动注销也一致
var handle = _context.Subscribe<PlayerDiedEvent>(OnPlayerDied);
handle.Dispose();
```

### 6.2 心智负担：减少

移除前，开发者在 IDE 中看到 `IUnsubscribe` 接口时，会自然产生疑问：
- "我需要实现这个接口吗？"
- "有没有现成的实现？"
- "不同的实现有什么区别？"

移除后，开发者看到 `AutoUnsubscribeHandle`，直接知道：这是一个 struct，用完调 `Dispose()` 即可。没有选择，没有困惑。

### 6.3 扩展负担：减少

移除前，开发者想写自定义的事件系统组件时会面临"要不要实现 `IUnsubscribe`"的抉择。移除后，要么用 `AutoUnsubscribeHandle`（简单场景），要么直接用 `Action`（复杂场景），不需要理解一个没有实际多态价值的接口。

---

## 七、移除后开发者如何扩展

### 7.1 场景一：自定义事件源（第三方库兼容）

**需求**：我有一个非 AesirArchitecture 的事件系统，想让它能和 `UnsubscribeWhenGameObjectOnDestroyed` 一起用。

#### 移除前的做法

开发者需要实现 `IUnsubscribe` 接口：

```csharp
public class MyEventSource : IUnsubscribe  // ← 实现接口
{
    public void Dispose() { /* 注销逻辑 */ }
}
```

但 `Subscribe` 方法不会返回这个类型——开发者只能自己 `new` 它，再传入扩展方法。

#### 移除后的做法

直接返回 `AutoUnsubscribeHandle`：

```csharp
public AutoUnsubscribeHandle SubscribeMyEvent(Action callback)
{
    // 内部注册逻辑...
    return new AutoUnsubscribeHandle(() => UnregisterMyCallback(callback));
}

// 使用
var handle = myEventSource.SubscribeMyEvent(OnSomething);
handle.UnsubscribeWhenGameObjectOnDestroyed(gameObject);
```

**区别**：从"定义类 + 实现接口"变成"一行 `new AutoUnsubscribeHandle`"。

### 7.2 场景二：批量注销管理

**需求**：我有一个非 MonoBehaviour 的纯 C# 类，需要管理多个订阅的注销。

#### 移除前

```csharp
class MyManager : IDisposable
{
    readonly List<IUnsubscribe> _handles = new();

    public void Setup(IContext context)
    {
        _handles.Add(context.Subscribe<EventA>(OnA));
        _handles.Add(context.Subscribe<EventB>(OnB));
    }

    public void Dispose()
    {
        foreach (var h in _handles) h.Dispose();
        _handles.Clear();
    }
}
```

#### 移除后

```csharp
class MyManager : IDisposable
{
    readonly List<AutoUnsubscribeHandle> _handles = new();  // ← 仅类型变化

    public void Setup(IContext context)
    {
        _handles.Add(context.Subscribe<EventA>(OnA));
        _handles.Add(context.Subscribe<EventB>(OnB));
    }

    public void Dispose()
    {
        foreach (var h in _handles) h.Dispose();
        _handles.Clear();
    }
}
```

**区别**：`List<IUnsubscribe>` → `List<AutoUnsubscribeHandle>`，消除装箱。其余完全一致。

或直接使用已有的 `UnsubscribeHandleCollection`（AesirArchitecture 内置的批量管理工具），与 QFramework 的 `IUnRegisterList` 用法对应。

### 7.3 场景三：自定义注销策略

**需求**：我想在特定条件下才注销订阅。

直接用逻辑控制，不绕接口：

```csharp
// 方式一：在回调中判断
AutoUnsubscribeHandle? handle = null;
handle = context.Subscribe<PlayerDiedEvent>(e =>
{
    if (someCondition && handle.HasValue)
    {
        handle.Value.Dispose();
    }
});

// 方式二：用 AutoUnsubscribeHandle 包装条件逻辑
var handle = context.Subscribe<PlayerDiedEvent>(e =>
{
    if (someCondition)
    {
        // 执行注销
    }
});
```

**区别**：QFramework 中 `CustomUnRegister` 不是幂等的（QFramework.cs L317-318，第二次调用会 NRE），开发者如果想安全地条件注销需要自己封装。AesirArchitecture 的 `AutoUnsubscribeHandle` 是幂等的（`AutoUnsubscribeHandle.cs` 有 `_disposed` 标志），直接调 `Dispose()` 即可。

### 7.4 扩展方式总结

| 扩展需求 | 移除前 | 移除后 | 区别 |
|---------|--------|--------|------|
| 兼容 `UnsubscribeWhenXxx` | 实现 `IUnsubscribe`（但 `Subscribe` 不会返回它） | 返回 `AutoUnsubscribeHandle` | 从"定义类 + 实现接口"变成"一行 new" |
| 批量管理 | `List<IUnsubscribe>`（装箱） | `List<AutoUnsubscribeHandle>`（无装箱）或用 `UnsubscribeHandleCollection` | 消除装箱 |
| 自定义注销逻辑 | 被 `IUnsubscribe` 接口误导 | 直接用 `AutoUnsubscribeHandle.Dispose()` | 不再被接口误导 |

---

## 八、总结

### 事实纠正

> **v1.0 文档错误声明**："QFramework 中 `IUnRegister` 至少有两个不同的实现类"，"`UnRegisterOnDestroyTrigger` 自身实现 `IUnRegister`"。
>
> **实际情况**（基于 QFramework.cs 源码 L293-345）：`IUnRegister` 在 QFramework 中的唯一实现者是 `CustomUnRegister`（struct, L312-321）。`UnRegisterTrigger`（abstract MonoBehaviour, L325-345）**不实现** `IUnRegister` 接口，它只是持有 `HashSet<IUnRegister>` 集合。QFramework 和 AesirArchitecture 的事件系统结构完全对应，接口在两个框架中都只有一个实现者。

### 核心论点

接口存在的两个理由——"多态需求"和"扩展契约"——在 `IUnsubscribe` 上都不成立：

1. **多态需求不成立**：QFramework 和 AesirArchitecture 中接口都只有一个实现者（`CustomUnRegister` / `AutoUnsubscribeHandle`），调用方从不需要通过接口多态地使用不同的注销句柄。
2. **扩展契约不成立**：即使开发者实现了 `IUnsubscribe`，也无法被 `Subscribe` 方法返回（内部硬编码 `new AutoUnsubscribeHandle(...)`）。开发者唯一的扩展路径是手动 new 后传入扩展方法，但 `AutoUnsubscribeHandle` 接受任意 `Action`，已覆盖所有注销逻辑变体，没有理由另造句柄类型。

AesirArchitecture 移植了 QFramework 的事件系统，包括这个在两个框架中都过度设计的接口。移除 `IUnsubscribe` 是修正这一遗留问题，而非改变设计方向。

### 移除后的收益

| 收益 | 说明 |
|------|------|
| 消除装箱 | `List<IUnsubscribe>` → `List<AutoUnsubscribeHandle>`，每次订阅零堆分配 |
| 减少心智负担 | 开发者不再面对"要不要实现接口"的抉择 |
| 减少代码量 | 删除 `IUnsubscribe.cs`，所有 API 签名简化 |
| 调用端零影响 | 所有 `.UnsubscribeWhenXxx()` 调用代码不变 |
