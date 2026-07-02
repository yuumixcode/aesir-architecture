# 为什么要移除 IUnsubscribe 接口

> **适用范围**: AesirArchitecture `Runtime/Engine/Event/`

接口存在的两个合理理由是**多态需求**（≥2 个实现类需要统一调用）和**扩展契约**（为开发者提供扩展点）。`IUnsubscribe` 两个理由都不成立：唯一实现者是 `AutoRemoveListenerHandle`（struct），且 `AddListener` 内部硬编码返回该类型，开发者即使实现接口也无法被框架使用。

---

## 移除的好处

| 好处 | 说明 |
|------|------|
| **消除装箱** | `AutoRemoveListenerHandle` 是 struct，存入 `List<IUnsubscribe>` 时被装箱为接口引用，每次添加监听产生一次堆分配。移除后集合变为 `List<AutoRemoveListenerHandle>`，零分配 |
| **减少代码量** | 删除 `IUnsubscribe.cs`，所有 `AddListener`/扩展方法的返回值和参数类型从接口简化为具体类型 |
| **调用端零影响** | 所有 `.RemoveListenerWhenXxx()` 调用代码不变，`var handle = AddListener(cb)` 的 `var` 自动推导 |

## 对开发者易用性的影响

- **日常调用无变化**：`AddListener` 返回值从 `IUnsubscribe` 变为 `AutoRemoveListenerHandle`，但 `var` 推导使调用端代码完全一致
- **心智负担减少**：移除前，开发者在 IDE 中看到 `IUnsubscribe` 接口会犹豫"要不要实现它"；移除后直接看到 `AutoRemoveListenerHandle`——一个 struct，调 `Dispose()` 即可，没有选择困惑
- **扩展更简单**：第三方库想兼容 `RemoveListenerWhenXxx` 时，直接返回 `new AutoRemoveListenerHandle(callback)` 即可，无需定义类、实现接口。`AutoRemoveListenerHandle` 接受任意 `Action`，已覆盖所有移除监听逻辑变体
