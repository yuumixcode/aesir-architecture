# 为什么叫 FakeContext 而不是 MockContext

> **适用范围**: 历史决策文档（v0.1.x）
>
> **注意**: v0.2.0 已移除 `FakeContext`，测试改为直接实例化 `AbstractContext<T>` 子类。本文保留作为命名决策的历史记录。

`FakeContext` 是 RAA 框架中曾用于单元测试的非单例上下文（v0.1.x）。它最初命名为 `MockContext`，后来改为 `FakeContext`。本文解释命名背后的依据。

---

## 测试替身分类

Martin Fowler 在 [Mocks Aren't Stubs](https://martinfowler.com/articles/mocksArentStubs.html) 一文中，将测试替身（Test Double）分为五类：

| 类型 | 定义 | 是否有真实实现 | 是否验证行为 |
|------|------|:--------------:|:------------:|
| **Dummy** | 传入但不被使用的占位对象 | ❌ | ❌ |
| **Stub** | 返回预设的固定回答，不响应预设之外的调用 | ❌ | ❌ |
| **Spy** | 记录调用以便事后验证 | ✅ | ✅（事后） |
| **Mock** | 预设期望，在测试结束时自动验证调用是否匹配 | ❌ | ✅（自动） |
| **Fake** | 有真实但简化的实现，走捷径但不影响功能正确性 | ✅ | ❌ |

核心区别在于 **是否有真实实现** 和 **是否验证行为**。

---

## FakeContext 是 Fake 而非 Mock

`FakeContext` 的实际行为：

```csharp
var context = new FakeContext(ctx =>
{
    ctx.RegisterModel<ICounterModel>(new CounterModel());
    ctx.RegisterService<ICounterService>(new CounterService());
});

// ——

var model = context.GetModel<ICounterModel>();
Assert.IsNotNull(model);
Assert.IsTrue(model.Initialized);
```

它具备完整的模块注册、依赖校验、初始化、释放等全部逻辑——和 `Context<T>` 走的是同一条代码路径（继承自 `AbstractContext`），只是少了静态单例和域重置。

对照分类标准：

| 判据 | Mock | Fake | FakeContext 实际 |
|------|:----:|:----:|:---------------:|
| 有真实实现 | ❌ | ✅ | ✅ 继承 AbstractContext 全部逻辑 |
| 走捷径但功能完整 | — | ✅ | ✅ 用回调代替子类，用构造即初始化代替惰性单例 |
| 预设期望并自动验证 | ✅ | ❌ | ❌ 无任何期望验证 |
| 用于行为验证 | ✅ | ❌ | ❌ 用于状态验证（Assert 返回值） |

结论：`FakeContext` 是一个有真实实现的简化版本，用于状态验证而非行为验证——这是 Fake 的定义。

---

## 为什么 Mock 是误用

"Mock" 在日常交流中常被泛化为"测试中用的替身"的统称，但在 .NET 生态中：

- **Moq**、**NSubstitute** 等库被称为"Mocking Framework"，它们的特征是：设定期望（`mock.Expect(x => x.Foo())`）、在测试结束时自动验证调用是否满足期望
- `FakeContext` 不做任何期望设定，不自动验证调用序列

如果叫 `MockContext`，开发者可能预期它具备期望验证能力（类似 Moq 的 `Mock<T>`），实际上却没有——这是误导。

---

## FakeContext 走的"捷径"

| 与生产环境 Context\<T\> 的差异 | 说明 |
|------|------|
| 无静态字段 | 不污染全局状态，测试间天然隔离 |
| 无域重置注册 | 不向 `ResetStaticsAssistant` 注册回调 |
| 构造即初始化 | `Context<T>` 在首次访问 `Interface` 时惰性初始化；`FakeContext` 在构造函数中立即调用 `Initialize()` |
| 无泛型子类约束 | 通过 `Action<IContext>` 回调配置模块，无需为每个测试定义子类 |

这些捷径不影响功能正确性——注册、初始化、依赖校验、释放等全部按生产逻辑执行。

---

## 参考

- Martin Fowler, ["Mocks Aren't Stubs"](https://martinfowler.com/articles/mocksArentStubs.html) (2007)
- Gerard Meszaros, *xUnit Test Patterns*, "Test Double" 章节
- Martin Fowler, ["TestDouble"](https://martinfowler.com/bliki/TestDouble.html) bliki 条目
