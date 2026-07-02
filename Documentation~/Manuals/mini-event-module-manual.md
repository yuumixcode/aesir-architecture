# MiniEvent 模块使用手册

> **适用范围**: `Runtime/Engine/Event/` + `Runtime/Component/Event/`
> **前置阅读**: [架构概览](../aesir-architecture.md)

---

## 快速定位

`MiniEvent` 和 `MiniEvent<T>` 等同于增强了自动移除监听功能的 `Action` 和 `Action<T>`——添加监听返回一个句柄，可绑定到 GameObject 生命周期自动移除，也可手动 `Dispose()`。

---

## 模块全貌

```
IMiniEventBus              — 事件总线接口（纯 C#）
MiniEventBus               — 纯 C# 事件总线实现 + MiniEventBus.Global 全局单例
MiniEventBusBoard          — Unity MonoBehaviour 看板，Inspector 中可视化事件注册状态
MiniEvent                  — 无参事件 (Action)
MiniEvent<T>               — 单参事件 (Action<T>)
AutoRemoveListenerHandle   — 幂等移除监听句柄 (struct)
RemoveListenerExtensions   — Fluent 自动移除监听 (.RemoveListenerWhenXxx())
RemoveListenerHandleCollection — 句柄集合，批量移除监听
RemoveListenerTrigger      — MonoBehaviour 自动移除监听基类
  ├── RemoveListenerOnDestroyTrigger   — OnDestroy 触发
  └── RemoveListenerOnDisableTrigger   — OnDisable 触发
RemoveListenerOnSceneUnloadedTrigger — 场景卸载触发 (按场景名分桶)
```

---

## 事件总线架构

### Context 级事件总线

每个 `IContext`（`AbstractContext<T>`）内部通过 `MiniEventBus.Global` 路由事件操作。`IContext` 接口直接提供 `AddListener<T>` / `RemoveListener<T>` / `InvokeEvent<T>` 方法。通过能力扩展方法 `this.AddListener<T>()` / `this.InvokeEvent<T>()` 自动路由到所属 Context 的事件总线。

### 全局事件总线

**`MiniEventBus.Global`** — 纯 C# 全局单例，不依赖 Unity 生命周期，所有 Context 的事件操作均路由到此全局实例。`MiniEventBusBoard` 组件在 Inspector 中可视化展示其事件注册状态。

---

## 使用方法

### 1. 通过 Context 事件总线（推荐）

适用于已接入 `ICanAddListener` + `ICanInvokeEvent` 的对象（Model / Service / Controller / View / Presenter）。

**定义事件：**

```csharp
public struct PlayerDiedEvent : IEventArgs
{
    public int KillerId;
}
```

**添加监听 + 自动移除（策略择一，不可混用）：**

```csharp
// 策略 A：一次性监听，与 GameObject 同生命周期
void Start()
{
    this.AddListener<PlayerDiedEvent>(OnPlayerDied)
        .RemoveListenerWhenGameObjectOnDestroyed(gameObject);
}

// 策略 B：Enable/Disable 配对，每次启用时刷新监听
void OnEnable()
{
    this.AddListener<PlayerDiedEvent>(OnPlayerDied)
        .RemoveListenerWhenGameObjectOnDisable(gameObject);
}
```

**发布：**

```csharp
this.InvokeEvent(new PlayerDiedEvent { KillerId = 42 });
```

### 2. 独立使用 MiniEventBus

适用于不经过 Context 的纯 C# 场景。

```csharp
var bus = new MiniEventBus();

// 添加监听
var handle = bus.AddListener<PlayerDiedEvent>(e => { /* ... */ });

// 发布
bus.InvokeEvent(new PlayerDiedEvent { KillerId = 1 });

// 手动移除监听
handle.Dispose();
```

### 3. 使用全局 MiniEventBus

```csharp
MiniEventBus.Global.AddListener<PlayerDiedEvent>(e => { /* ... */ });
MiniEventBus.Global.InvokeEvent(new PlayerDiedEvent { KillerId = 1 });
```

### 4. 独立使用 MiniEvent

适用于不经过事件总线的场景，如 `ObservableValue<T>` 内部或自定义数据源。

```csharp
var event_ = new MiniEvent<int>();

// 添加监听
var handle = event_.AddListener(value => { /* ... */ });

// 发布
event_.Invoke(100);

// 手动移除监听
handle.Dispose();
```

### 5. 非 MonoBehaviour 的批量移除监听

纯 C# 类无法使用 `RemoveListenerWhenXxx`，手动管理句柄：

```csharp
readonly RemoveListenerHandleCollection _handles = new RemoveListenerHandleCollection();

void Setup(ICanAddListener context)
{
    _handles.Add(context.AddListener<EventA>(OnA));
    _handles.Add(context.AddListener<EventB>(OnB));
}

void Dispose()
{
    _handles.RemoveAllListeners();
}
```

---

## 移除监听策略速查

| 添加监听时机 | 匹配的移除方式 | 适用场景 |
|---------|-------------|---------|
| `Start` / `Awake` | `.RemoveListenerWhenGameObjectOnDestroyed(gameObject)` | 一次性监听，与 GameObject 同生命周期 |
| `OnEnable` | `.RemoveListenerWhenGameObjectOnDisable(gameObject)` | 需每次启用时刷新监听（如 UI 状态变化） |
| 纯 C# 类 | 手动 `handle.Dispose()` 或 `RemoveListenerHandleCollection` | 无 MonoBehaviour 生命周期可绑定 |
| 场景级 | `.RemoveListenerWhenOnSceneUnloaded()` | 监听与场景同生命周期 |

> **错误搭配**：`OnEnable` 添加监听 + `RemoveListenerWhenGameObjectOnDestroyed` 会导致监听重复注册和句柄堆积，因为 `OnEnable` 会被多次调用而 `OnDestroy` 只触发一次。

---

## IEventArgs 约束

- **`MiniEventBus` 的公共 API** 约束 `T : IEventArgs`，强制每种事件是独立命名类型，防止 `Dictionary<Type, ...>` 键碰撞
- **`MiniEvent<T>`** 无约束，可作为任意类型的委托容器独立使用
