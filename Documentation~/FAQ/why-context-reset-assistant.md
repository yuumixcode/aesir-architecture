# 为什么需要 ContextResetAssistant

> **适用范围**: `Runtime/Core/Context/Context.cs`

`Context<T>` 是泛型静态单例，静态字段 `_context` 在退出 Play Mode 后仍保留引用。如果不重置，反复进出 Play Mode 会导致旧上下文残留，引发空引用、事件重复注册等问题。

Unity 提供了 `[RuntimeInitializeOnLoadMethod]` 在域加载时执行重置逻辑，但该特性**不能用在泛型类型上**。因此需要一个非泛型的中间类——`ContextResetAssistant`——来承载这个回调。

---

## 运行逻辑

### 注册阶段

`Context<T>` 的静态构造函数在首次被访问时执行，向 `ContextResetAssistant` 注册一个重置回调：

```csharp
static Context()
{
    ContextResetAssistant.Register(ResetForDomainReload);
}
```

每个 `T` 类型参数（如 `GameContext`、`BattleContext`）会各自触发一次静态构造，各自注册一份回调。

### 重置阶段

Unity 在进入 Play Mode 前的 `SubsystemRegistration` 阶段调用 `ResetAll`，遍历所有已注册回调：

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
static void ResetAll()
{
    foreach (var callback in ResetCallbacks)
        callback?.Invoke();
}
```

回调执行的是 `Context<T>.ResetForDomainReload()`——Dispose 旧上下文并将 `_context` 置 null，下次访问 `Instance` 时重新初始化。

### 为什么不在重置后清空 ResetCallbacks

因为关闭 Domain Reload 时静态构造函数不会重新执行，清空后回调无法重新注册，下一次进入 Play Mode 将无法重置。

---

## Domain Reload 开启 vs 关闭

| | Domain Reload 开启（默认） | Domain Reload 关闭 |
|---|---|---|
| 退出 Play Mode 时 | 整个脚本域被卸载重建，所有静态字段被重置为默认值 | 静态字段保持不变，`_context` 仍持有旧实例 |
| 进入 Play Mode 时 | 静态构造函数重新执行，回调重新注册，`ResetAll` 执行 | 静态构造函数**不执行**，回调列表保留上次注册的内容，`ResetAll` 仍执行 |
| `ResetCallbacks` 列表 | 每次 Play Mode 重新构建 | 始终是第一次 Play Mode 时构建的那份，反复使用 |
| `_context` 旧实例 | 随域卸载被 GC 回收 | 由 `ResetForDomainReload` 显式 Dispose 后置 null |
| 是否需要 `ContextResetAssistant` | 不需要——域重建天然重置一切 | **必须需要**——唯一能重置静态状态的机会 |

---

## 补充：静态构造函数的执行时机

**不是**打开编辑器或编译完类就执行。C# 的静态构造函数是**延迟执行**的——只有当代码在运行时首次访问该类型的成员时才触发。

对 `Context<T>` 来说，触发时机是第一次访问 `Context<GameContext>.Instance`（或任何其他静态成员），通常发生在 Play Mode 中某段代码首次订阅事件或获取 Model 时。

这意味着 `ContextResetAssistant.Register()` 的注册时机取决于实际使用，而非编译时刻：

| 场景 | 静态构造函数何时执行 | `ResetCallbacks` 何时被填充 |
|------|---------------------|--------------------------|
| Domain Reload 开启 | 每次 Play Mode 中首次访问 `Context<T>` 时 | 每次 Play Mode 都重新填充（域重建后列表为空） |
| Domain Reload 关闭 | **仅第一次** Play Mode 中首次访问 `Context<T>` 时 | 仅第一次 Play Mode 填充，之后反复使用 |

### 对 `ResetAll` 执行顺序的影响

`[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` 在进入 Play Mode 早期触发，**早于**用户代码访问 `Context<T>`。因此：

- **第一次进入 Play Mode**：`ResetAll` 执行时 `ResetCallbacks` 为空（回调尚未注册），什么都不做——这没有问题，因为是首次进入，没有旧状态需要重置
- **后续进入 Play Mode**（Domain Reload 关闭）：`ResetCallbacks` 已包含上次注册的回调，`ResetAll` 正常执行重置
