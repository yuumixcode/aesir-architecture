# MiniEvent 模块使用手册

> **适用范围**: `Runtime/Event/`
> **前置阅读**: [架构概览](../aesir-architecture.md)

---

## 快速定位

`MiniEvent` 和 `MiniEvent<T>` 等同于增强了自动取消订阅功能的 `Action` 和 `Action<T>`——订阅返回一个句柄，可绑定到 GameObject 生命周期自动注销，也可手动 `Dispose()`。

---

## 模块全貌

```
MiniEvent              — 无参事件 (Action)
MiniEvent<T>           — 单参事件 (Action<T>)
MiniEventBus           — 类型键控事件总线，按 typeof(T) 路由
AutoUnsubscribeHandle  — 幂等注销句柄 (struct)
UnsubscribeExtensions   — Fluent 自动注销 (.UnsubscribeWhenXxx())
UnsubscribeHandleCollection — 句柄集合，批量注销
UnsubscribeInvoker      — MonoBehaviour 自动注销基类
  ├── UnsubscribeOnDestroyInvoker   — OnDestroy 触发
  └── UnsubscribeOnDisableInvoker   — OnDisable 触发
UnsubscribeOnSceneUnloadedInvoker — 场景卸载触发 (按场景名分桶)
```

---

## 使用方法

### 1. 通过 Context 事件总线（推荐）

适用于 MonoBehaviour / Controller / View 等已接入 `ICanSubscribeWithContext` + `ICanInvokeWithContext` 的对象。

**定义事件：**

```csharp
public struct PlayerDiedEvent : IEventArgs
{
    public int KillerId;
}
```

**订阅 + 自动注销（策略择一，不可混用）：**

```csharp
// 策略 A：一次性订阅，与 GameObject 同生命周期
void Start()
{
    this.Subscribe<PlayerDiedEvent>(OnPlayerDied)
        .UnsubscribeWhenGameObjectOnDestroyed(gameObject);
}

// 策略 B：Enable/Disable 配对，每次启用时刷新订阅
void OnEnable()
{
    this.Subscribe<PlayerDiedEvent>(OnPlayerDied)
        .UnsubscribeWhenGameObjectOnDisable(gameObject);
}
```

**发布：**

```csharp
this.Invoke(new PlayerDiedEvent { KillerId = 42 });
```

### 2. 独立使用 MiniEvent

适用于不经过 Context 的场景，如 `ObservableProperty<T>` 内部或自定义数据源。

```csharp
var event_ = new MiniEvent<int>();

// 订阅
var handle = event_.Subscribe(value => { /* ... */ });

// 发布
event_.Invoke(100);

// 手动注销
handle.Dispose();
```

### 3. 独立使用 MiniEventBus

适用于需要类型路由但不接入 Context 的场景。

```csharp
var bus = new MiniEventBus();

// 订阅
var handle = bus.Subscribe<PlayerDiedEvent>(e => { /* ... */ });

// 发布
bus.Invoke(new PlayerDiedEvent { KillerId = 1 });

// 手动注销
handle.Dispose();
```

### 4. 非 MonoBehaviour 的批量注销

纯 C# 类无法使用 `UnsubscribeWhenXxx`，手动管理句柄：

```csharp
readonly UnsubscribeHandleCollection _handles = new();

void Setup(ICanSubscribeWithContext context)
{
    _handles.Add(context.Subscribe<EventA>(OnA));
    _handles.Add(context.Subscribe<EventB>(OnB));
}

void Dispose()
{
    _handles.UnsubscribeAll();
}
```

---

## 注销策略速查

| 订阅时机 | 匹配的注销方式 | 适用场景 |
|---------|-------------|---------|
| `Start` / `Awake` | `.UnsubscribeWhenGameObjectOnDestroyed(gameObject)` | 一次性订阅，与 GameObject 同生命周期 |
| `OnEnable` | `.UnsubscribeWhenGameObjectOnDisable(gameObject)` | 需每次启用时刷新订阅（如 UI 状态变化） |
| 纯 C# 类 | 手动 `handle.Dispose()` 或 `UnsubscribeHandleCollection` | 无 MonoBehaviour 生命周期可绑定 |
| 场景级 | `.UnsubscribeWhenOnSceneUnloaded()` | 订阅与场景同生命周期 |

> **错误搭配**：`OnEnable` 订阅 + `UnsubscribeWhenGameObjectOnDestroyed` 会导致订阅重复注册和句柄堆积，因为 `OnEnable` 会被多次调用而 `OnDestroy` 只触发一次。

---

## IEventArgs 约束

- **`MiniEventBus` 的公共 API** 约束 `T : IEventArgs`，强制每种事件是独立命名类型，防止 `Dictionary<Type, ...>` 键碰撞
- **`MiniEvent<T>`** 无约束，可作为任意类型的委托容器独立使用
