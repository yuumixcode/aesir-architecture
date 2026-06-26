# RAA 代码样式指南

> 基于 [Unity C# Style Guide (Unity 6 Edition)](https://unity.com/resources/c-sharp-style-guide-unity-6) 电子书、[Microsoft Framework Design Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/) 及项目现有代码归纳。
>
> 所有新增/修改脚本必须遵守本文档。当本文档与外部指南冲突时，以本文档为准。

---

## 0 核心原则

| 原则 | 含义 |
|------|------|
| **KISS** | 保持简单，不要过度工程化。能用的简单方案优于自造的复杂方案。 |
| **YAGNI** | 只实现当前需要的功能，不要预先构建"将来可能用到"的代码。 |
| **DRY** | 不要重复自己。提取公共逻辑，而非复制粘贴。复制逻辑比复制代码更危险。 |
| **SRP** | 单一职责。每个类/方法只做一件事，只有一个变化的理由。 |
| **可读性优先** | 可读性比简洁性更重要。Clarity > Brevity。 |
| **一致性** | 整个项目像同一个人写的。风格指南消除猜测，让代码审查聚焦于"做什么"而非"怎么写"。 |

---

## 1 命名规范

### 1.1 通用规则

- **标识符禁止特殊字符**（反斜杠、符号、Unicode），即使 C# 允许，它们会干扰 Unity 命令行工具
- **使用有意义的名称**，不缩写（数学表达式除外）。`movementSpeed` 优于 `mvmtSpd`
- **名称应可发音、可搜索**——这也帮助 AI 工具生成更准确的建议
- **可读性优于简洁性**：`HorizontalAlignment` 优于 `AlignmentHorizontal`
- **避免冗余名称**：如果类叫 `Player`，字段不需要 `PlayerScore`，直接 `Score`
- **避免玩笑/双关**：`infiniteMonkeys` 看一次可能有趣，读十次就是噪音
- **每行一个声明**：增强可读性

### 1.2 大小写与前缀速查

基于 Rider 默认命名规则：

| 标识符 | 样式 | 示例 |
|--------|------|------|
| 命名空间 | `PascalCase`，统一在 `Runestone.AesirArchitecture` 下 | `Runestone.AesirArchitecture` |
| 类 | `UpperCamelCase`（PascalCase），名词/名词短语 | `AesirLifeCycle`, `ObservableProperty<T>` |
| 公共 API 类 | `UpperCamelCase`，加 `Aesir` 前缀 | `AesirLifeCycle` |
| 接口 | `I` + `UpperCamelCase`，后接形容词 | `IObservableProperty<T>`, `IDamageable` |
| 枚举（普通） | `UpperCamelCase` 单数名词，值无前缀后缀 | `WeaponType.Knife` |
| 枚举（Flags） | `UpperCamelCase` **复数**名词 | `AttackModes.Melee` |
| 枚举成员 | `UpperCamelCase` | `Awake`, `OnEnable` |
| 结构体 | `UpperCamelCase` | `HookEntry`, `PlayerStats` |
| 非私有实例字段 | `UpperCamelCase` | `DamageMultiplier` |
| 私有实例字段 | `_lowerCamelCase`（下划线前缀） | `_hooks`, `_dispatching` |
| 非私有静态字段 | `UpperCamelCase` | `SharedCount` |
| 私有静态字段 | `_lowerCamelCase`（下划线前缀） | `_sharedCount` |
| 私有静态只读字段 | `UpperCamelCase` | `Hooks`, `Instance` |
| 常量字段（含非私有） | `UpperCamelCase` | `MaxItems` |
| 私有常量字段 | `UpperCamelCase` | `MaxItems` |
| 静态只读字段 | `UpperCamelCase` | `Instance` |
| 属性 | `UpperCamelCase` | `Value`, `MaxHealth` |
| 方法 | `UpperCamelCase`，**动词/动词短语开头** | `Register()`, `GetDirection()` |
| 返回 bool 的方法 | `UpperCamelCase`，**问句形式** | `IsGameOver()`, `HasStartedTurn()` |
| 参数 | `lowerCamelCase` | `callback`, `order` |
| 局部变量 | `lowerCamelCase` | `oldValue`, `list` |
| 局部常量 | `lowerCamelCase` | `maxRetries` |
| 循环/数学变量 | 单字母可接受 | `i`, `j`, `x` |
| 泛型类型参数 | `T` 或 `T` + 描述 | `T`, `TKey` |
| bool 变量 | **动词前缀** | `isDead`, `hasDamageMultiplier` |
| 事件 | `UpperCamelCase` | `DoorOpened`, `ValueChanged` |
| Unity 序列化字段 | `lowerCamelCase` | `movementSpeed`, `isPlayerDead` |

> **Rider 命名规则摘要**：私有实例/静态字段用 `_lowerCamelCase`；常量、静态只读、非私有字段统一用 `UpperCamelCase`；局部常量用 `lowerCamelCase`。

### 1.3 事件命名

RAA 采用如下事件命名方案（基于 Unity 习惯和 Action 委托模式）：

| 角色 | 命名规则 | 示例 |
|------|---------|------|
| 事件（字段） | `UpperCamelCase` 动词短语，**现在分词** = 之前，**过去分词** = 之后 | `OpeningDoor`, `DoorOpened` |
| 事件发布方法（发布者） | `Invoke` + 事件名 | `InvokeDoorOpened()` |
| 事件订阅方法（订阅者） | `On` + 事件名，表示由事件自动触发 | `OnDoorOpened()` |

```csharp
// 事件声明
public event Action OpeningDoor;      // 事件"前"
public event Action DoorOpened;       // 事件"后"
public event Action<int> PointsScored;

// 事件发布方法
public void InvokeDoorOpened() => DoorOpened?.Invoke();
public void InvokePointsScored(int points) => PointsScored?.Invoke(points);

// 事件订阅方法（订阅者端）
private void OnDoorOpened() { /* ... */ }
```

> **`On` = 事件自动触发**，`Invoke` = 代码主动发布。`On` 前缀仅用于表示"由事件系统调起"的回调方法。

### 1.4 枚举

- 普通枚举用**单数**名词：`WeaponType`、`FireMode`
- Flags 枚举用**复数**名词：`AttackModes`
- 枚举值无前缀后缀，UpperCamelCase
- **最后一个枚举值后不加逗号**
- Flags 值推荐列对齐二进制注释：

```csharp
[Flags]
public enum AttackModes
{
    // Decimal                         // Binary
    None = 0,                          // 000000
    Melee = 1,                         // 000001
    Ranged = 2,                        // 000010
    Special = 4,                       // 000100
    MeleeAndSpecial = Melee | Special  // 000101
}

---

## 2 文件组织

### 2.1 using 顺序

按以下分组排列，组间无空行，组内字母排序：

1. `System.*`
2. `System.Collections.*` / `System.Linq` 等
3. `UnityEngine.*`
4. `UnityEditor.*`（仅 Editor 脚本）
5. 其他第三方
6. 项目内部

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
```

**移除未使用的 using**（保留最小必需集）。

### 2.2 文件结构

一个文件一个主类型。如果包含 MonoBehaviour，**文件名必须与 MonoBehaviour 类名匹配**。

```
using 语句块

namespace Runestone.AesirArchitecture
{
    // 枚举（若全局使用则放在类外）
    // 接口
    // 类
    //   ├── 字段（含 private struct 的字段声明，struct 本体放末尾）
    //   ├── Unity 生命周期方法（紧跟字段，Awake → Start → OnEnable → ...）
    //   ├── 公有方法
    //   ├── 私有方法
    //   └── 嵌套结构体/类（放类末尾，省略 private）
}
```

### 2.3 命名空间

- 使用**块作用域**（不使用 file-scoped `namespace X;`）
- **所有代码统一在 `Runestone.AesirArchitecture` 下**，不创建子命名空间，防止命名空间数量爆炸
- 唯一例外：Editor 脚本可使用 `Runestone.AesirArchitecture.Editor`

```csharp
namespace Runestone.AesirArchitecture
{
    // ...
}
```

---

## 3 格式规范

### 3.1 大括号

**Allman 风格**：大括号独占一行。

```csharp
// ✅ 正确
public void Register()
{
    // ...
}

// ❌ 错误
public void Register() {
    // ...
}
```

**例外**：表达式体成员和单行属性 getter 使用 `=>`，不需要大括号：

```csharp
public T Value
{
    get => _value;
    set { /* ... */ }
}
```

### 3.2 大括号：if/for 必须使用

**if / for / while 等控制流语句必须加大括号**，即使只有一行：

```csharp
// ✅ 正确
if (!_sortDirty)
{
    return;
}

if (!_hooks.TryGetValue(phase, out var list))
{
    return;
}

for (var i = 0; i < list.Count; i++)
{
    list[i].Callback.Invoke();
}

// ❌ 错误（省略大括号）
if (!_sortDirty) return;
for (var i = 0; i < list.Count; i++) DoSomething(i);
```

### 3.3 缩进与空白

- **4 空格**缩进，不使用 Tab
- 成员之间 **1 行空行**
- 方法内逻辑段落之间 1 行空行
- 不同逻辑区域之间可使用 **2 行空行**（如字段与属性之间、类与接口之间）
- 不使用连续空行

### 3.4 访问修饰符

**省略 `private`**：最短的声明一眼就是私有的，更直观。包括嵌套结构体/类/枚举也省略。

```csharp
// ✅ 正确 — 省略 private，更简洁直观
readonly Dictionary<AesirArchitectureLifeCyclePhase, List<HookEntry>> _hooks =
    new Dictionary<AesirArchitectureLifeCyclePhase, List<HookEntry>>();
bool _sortDirty;

// ✅ 正确 — 嵌套结构体同样省略 private
struct HookEntry
{
    public Action Callback;
    public int Order;
}

// ❌ 错误（冗余的 private）
private readonly Dictionary<AesirArchitectureLifeCyclePhase, List<HookEntry>> _hooks =
    new Dictionary<AesirArchitectureLifeCyclePhase, List<HookEntry>>();
private struct HookEntry { }
```

`public`、`protected`、`internal` 等其他修饰符**必须显式声明**。

### 3.5 水平间距

| 规则 | 正确 | 错误 |
|------|------|------|
| 逗号后加一个空格 | `CollectItem(obj, 0, 1)` | `CollectItem(obj,0,1)` |
| 括号内不加空格 | `DropPowerUp(prefab, 0, 1)` | `DropPowerUp( prefab, 0, 1 )` |
| 函数名与括号间不加空格 | `DoSomething()` | `DoSomething ()` |
| 方括号内不加空格 | `x = data[index]` | `x = data[ index ]` |
| 流程控制条件前加空格 | `while (x == y)` | `while(x==y)` |
| 比较运算符两侧加空格 | `if (x == y)` | `if (x==y)` |

### 3.6 行长度

建议行宽上限 **120 字符**。长行拆分为更小的语句，而非让代码溢出。

### 3.7 列对齐

- 一般**不使用**列对齐（类型与名称配对变困难）
- 例外：Flags 枚举的二进制注释列对齐

```csharp
// ✅ 正确 — 不对齐
public float Speed = 12f;
public float Gravity = -10f;
public Transform GroundCheck;

// ❌ 避免 — 列对齐增加维护成本
public float     Speed      = 12f;
public float     Gravity    = -10f;
public Transform GroundCheck;
```

### 3.8 switch 语句

- `case` 相对 `switch` 缩进
- **始终包含 `default` 分支**，即使所有情况已覆盖

```csharp
switch (someExpression)
{
    case 0:
        DoSomething();
        break;
    case 1:
        DoSomethingElse();
        break;
    default:
        break;
}
```

### 3.9 文件长度与 #region

- **尽量不超过 500 行代码**。超过时应优先考虑拆分
- **500 行以内禁止使用 `#region`**
- 如果确实无法拆分，脚本超过 500 行，**必须使用 `#region` 分块**以提升可读性

---

## 4 序列化

- **使用 `[SerializeField]`** 而非 public 字段在 Inspector 中展示，保持封装性
- **使用 `[Range(min, max)]`** 限制数值范围并生成滑块
- **序列化字段仍须按要求编写注释**，`[Tooltip]` 不替代注释
- **使用 `[Serializable]` 结构体/类** 分组 Inspector 中的相关数据
- **每个特性独占一行**，多个特性不写在同一行

```csharp
[Serializable]
public struct PlayerStats
{
    public int MovementSpeed;
    public int HitPoints;
    public bool HasHealthPotion;
}

/// <summary>
/// 玩家核心统计数据
/// </summary>
[SerializeField]
PlayerStats _stats;

[Range(0f, 1f)]
[SerializeField]
float _rangedStat;
```

---

## 5 文档注释

### 5.1 规则

- **`public` / `protected` 成员必须添加 `<summary>`**，这是公开 API 的契约
- **`<param>`、`<typeparam>`、`<see cref>`、`<para>` 等子标签有选择地添加**——难以直接理解的才加，避免注释爆炸
- **私有成员克制地加注释**：简单的不加，逻辑复杂的私有方法应加注释说明
- 文档语言：**中文**
- 注释应回答 **"为什么"**，而非"什么"或"怎么"——代码本身应能解释后两者

### 5.2 何时加注释

| 加 ✅ | 不加 ❌ |
|------|--------|
| 解释意图、设计决策 | 重复代码已表达的信息 |
| 说明不明显的边界条件 | 每个变量都加注释 |
| 法律/许可证信息（链接外部） | 日志式记录（`// 2024-01-01 修改`） |
| TODO（附姓名和日期） | 已注释掉的代码 |
| 复杂私有方法的逻辑说明 | 显而易见的代码 |

### 5.3 注释样式

- 使用 `//` 行注释（非块注释），保持注释靠近代码
- `//` 与文本之间**插入一个空格**
- 首字母大写，句号结尾
- **不使用**星号框或其他装饰性格式
- **`<summary>` 标签必须换行**，不写成 `<summary>text</summary>` 内联形式
- **`<para>` 标签必须换行**，不写成 `<para>text</para>` 内联形式

### 5.4 示例

```csharp
/// <summary>
/// 注册回调，order 越小越先执行，默认 0
/// </summary>
public void Register(AesirArchitectureLifeCyclePhase phase, Action callback, int order = 0)

/// <summary>
/// 可观察属性。值变更时触发 <see cref="ValueChanged"/> 事件通知订阅者。
/// <para>Model 层持有可写实例，View 层通过 <see cref="IReadOnlyObservableProperty{T}"/> 只读访问。</para>
/// </summary>
/// <typeparam name="T">
/// 属性值类型
/// </typeparam>
```

---

## 6 属性格式

- **只读属性**：使用表达式体 `=>`
- **读写属性**：使用 `{ get => ...; set => ...; }`
- **无 backing field 的属性**：使用自动属性 `{ get; set; }`
- 复杂逻辑的 get/set 用**方法**代替属性

```csharp
// 只读，返回 backing field
public int MaxHealth => _maxHealth;

// 读写，显式实现
public int MaxHealth
{
    get => _maxHealth;
    set => _maxHealth = value;
}

// 自动属性
public string DescriptionName { get; set; } = "Fireball";
```

---

## 7 编码模式

### 7.1 类型推断 (var)

`var` 用于 **类型明显** 的场景；类型不明显时使用显式类型：

```csharp
// ✅ 类型明显（new 表达式右侧）
var powerUps = new List<PlayerStats>();
var dict = new Dictionary<string, List<GameObject>>();

// ✅ out var
if (!_hooks.TryGetValue(phase, out var list))
{
    return;
}

// ✅ for 循环变量（var 与集合元素类型保持一致）
for (var i = 0; i < list.Count; i++)
{
    // ...
}

// ❌ 类型不明显
var powerUps = PowerUpManager.GetPowerUps();  // 返回类型未知
```

> `var` 在 `foreach` 和 `for` 中特别有用，确保迭代变量类型与枚举器/集合匹配，避免显式声明错误类型导致运行时错误。

### 7.2 空值检查与防护

```csharp
// 参数空检查
if (callback == null)
{
    throw new ArgumentNullException(nameof(callback));
}

// 早期返回（guard clause）
if (!_sortDirty)
{
    return;
}
```

**不要用代码绕过问题**：如果用 `if-null` 修了 NullReferenceException，先确认是否是更深层调用的根本原因。

### 7.3 事件发布与订阅

```csharp
// 发布 — 使用 Invoke 前缀（详见 §1.3）
public void InvokeDoorOpened() => DoorOpened?.Invoke();

// 订阅 — 使用 On 前缀，表示由事件自动触发
private void OnDoorOpened() { /* ... */ }
```

使用 `System.Action` 委托（0-16 参数），仅在需要时创建自定义 EventArgs（struct 或继承 System.EventArgs）。

### 7.4 值相等比较

泛型值比较使用 `EqualityComparer<T>.Default.Equals`，不用 `==` 或 `.Equals()`：

```csharp
if (EqualityComparer<T>.Default.Equals(_value, value))
{
    return;
}
```

### 7.5 集合初始化

**不使用目标类型 `new()`**，显式写出泛型参数：

```csharp
// ✅ 正确 — 显式泛型参数
readonly Dictionary<AesirArchitectureLifeCyclePhase, List<HookEntry>> _hooks =
    new Dictionary<AesirArchitectureLifeCyclePhase, List<HookEntry>>();
readonly List<PendingChange> _pendingChanges = new List<PendingChange>();

// ❌ 错误 — 目标类型 new() 省略了类型信息
readonly Dictionary<AesirArchitectureLifeCyclePhase, List<HookEntry>> _hooks = new();
```

### 7.6 继承控制

- 不需要继承的类标记 `sealed`
- 单例式 MonoBehaviour 标记 `[DisallowMultipleComponent]`

```csharp
[DisallowMultipleComponent]
public sealed class AesirArchitectureLifeCycle : MonoBehaviour
```

### 7.7 try/catch/finally

**非必要不使用**。正确使用案例：调用 Unity 编辑器进度条 API 时，为防止无限卡住，需在 `finally` 中确保进度条关闭。

```csharp
// ✅ 正确 — finally 确保编辑器进度条关闭
EditorUtility.DisplayProgressBar("Processing", "Working...", progress);
try
{
    DoWork();
}
finally
{
    EditorUtility.ClearProgressBar();
}
```

### 7.8 表达式体成员

仅用于 **单行表达式** 的方法和属性：

```csharp
// ✅ 正确
void Awake() => Raise(AesirArchitectureLifeCyclePhase.Awake);
public int MaxHealth => _maxHealth;

// ❌ 错误（多行逻辑不应使用表达式体）
```

### 7.9 Unity 生命周期方法

紧跟字段声明之后，按 Unity 调用顺序排列：`Awake → OnEnable → Start → Update → FixedUpdate → LateUpdate → OnDisable → OnDestroy → OnApplicationPause → OnApplicationFocus → OnApplicationQuit`

单行方法使用表达式体：

```csharp
void Awake() => Raise(AesirArchitectureLifeCyclePhase.Awake);
void OnEnable() => Raise(AesirArchitectureLifeCyclePhase.OnEnable);
```

若需额外逻辑则使用方法体 + Allman 大括号。

### 7.10 冗余初始化

字段自动初始化为默认值（数值=0，引用=null，bool=false），**不重复写**：

```csharp
// ❌ 避免
int _count = 0;
GameObject _target = null;

// ✅ 正确
int _count;
GameObject _target;
```

---

## 8 方法设计

- **方法以动词开头**：`GetDirection`, `FindTarget`, `SetInitialPosition`
- **返回 bool 的方法用问句形式**：`IsGameOver`, `HasStartedTurn`
- **减少参数数量**：超过 5 个参数考虑用类/结构体分组。Attribute 构造函数除外——必须分析实际情况，即使超过 5 个也不一定需要分组
- **避免过度重载**：选择少数几种实际会用到的签名，确保参数个数有区分度
- **避免副作用**：方法只做名称所描述的事
- **不用 flag 参数拆分方法**：`GetAngleInDegrees()` + `GetAngleInRadians()` 优于 `GetAngle(useDegrees)`
- **避免长方法**：方法太长则拆分为更小的方法

---

## 9 UI Toolkit 命名

使用 **BEM（Block Element Modifier）** 命名规范：

```
block-name__element-name--modifier-name
```

- 用 **kebab-case**（与 CSS 一致）
- `block` = 高级组件（如 `navbar-menu`）
- `element` = block 的子部件（`__` 连接）
- `modifier` = 变体/状态（`--` 连接）

```
navbar-menu__shop-button--small
menu__home-button
menu__shop-button
```

- 不在 BEM 选择器中使用类型名（`Button`, `Label`）
- 使用语义命名而非视觉命名（`button--quit` 优于 `button--red`）
- 在构造函数中用 `AddToClassList()` 添加 USS 类

---

## 10 代码异味检测

出现以下信号时应考虑重构：

| 异味 | 说明 |
|------|------|
| 朦胧命名 | 类/方法/变量名不直白 |
| 不必要的复杂性 | 上帝对象、超长方法/类 |
| 僵化 | 一个小改动引起多处修改 |
| 脆弱 | 小改动导致大面积崩溃 |
| 不可移动 | 复用代码需要大量依赖 |
| 重复代码 | 复制粘贴而非提取公共逻辑 |
| 过度注释 | 注释每行代码，而非让代码自解释 |

---

## 11 文件命名

- 一个文件一个主类型，文件名 = 类型名（含泛型时不加 `` `1 `` 后缀）
- MonoBehaviour 文件名**必须与类名匹配**
- 示例：`AesirLifeCycle.cs`, `ObservableProperty.cs`, `IObservableProperty.cs`

---

## 12 EditorConfig

建议项目根目录添加 `.editorconfig` 以跨 IDE 强制一致格式：

```ini
root = true

[*]
end_of_line = lf
insert_final_newline = true

[*.cs]
indent_style = space
indent_size = 4
charset = utf-8
trim_trailing_whitespace = true
```

---

## 13 禁止事项

- ❌ 使用 file-scoped namespace
- ❌ 使用 `_` 丢弃参数（除非解构场景）
- ❌ 使用 `var` 隐藏不明显类型
- ❌ 使用 Hungarian notation（`strName`, `iCount`）
- ❌ 在类名上省略 `Aesir` 前缀（公共 API）
- ❌ 注释掉的代码留在文件中（用版本控制代替）
- ❌ 用注释做日志/署名（`// Modified by devA`）
- ❌ 冗余初始化（`= 0`, `= null`）
- ❌ 星号框注释或装饰性格式
- ❌ 使用无具体含义的分区注释（如 `// ── 内部实现 ──────────`、`// ── PlayerLoop 回调 ──────`），注释必须依附于具体的类、结构体、方法或代码行
- ❌ 非必要使用 try/catch/finally
- ❌ 纯防御性代码（无明确触发场景的防护）——添加防御性代码时必须清楚在什么情况下会发生问题，有目的才防御，不为防御而防御
- ❌ 创建 `Runestone.AesirArchitecture` 以外的子命名空间（Editor 除外）
- ❌ 在单元测试中使用 `[Test] async Task` 异步测试方法——团结引擎 NUnit 3.5 不支持，必须使用 `[UnityTest]` + `IEnumerator`（详见第 14 节）

---

## 14 单元测试规范

### 14.1 同步测试

同步测试使用 `[Test]`，方法签名为 `void`：

```csharp
/// <summary>
/// 验证 ExecuteCommand 执行命令并修改 Model 状态
/// </summary>
[Test]
public void ExecuteCommand_ModifiesModelState()
{
    var model = _ctx.GetModel<ICounterModel>();
    Assert.AreEqual(0, model.Count.Value);
    _controller.ExecuteCommand<IncreaseCountCommand>();
    Assert.AreEqual(1, model.Count.Value);
    AesirArchitectureLog.TestLog("ExecuteCommand: 命令成功修改了 Model 状态");
}
```

### 14.2 异步测试

团结引擎使用 `com.unity.test-framework` 1.1.33 + NUnit 3.5 (net35)，**不支持 `async Task` 测试方法**。使用 `async Task` 会导致编译器报错 "Method has non-void return value, but no result is expected"。

异步测试必须使用 `[UnityTest]` + `IEnumerator`，配合 `TaskEnumerator` 驱动异步操作：

```csharp
/// <summary>
/// 验证异步命令执行后正确修改 Model 状态
/// </summary>
[UnityTest]
public IEnumerator ExecuteCommandAsync_ModifiesModelState()
{
    var model = _ctx.GetModel<ICounterModel>();
    Assert.AreEqual(0, model.Count.Value);

    yield return new TaskEnumerator(_controller.ExecuteCommandAsync<IncreaseCountAsyncCommand>());

    Assert.AreEqual(1, model.Count.Value);
    AesirArchitectureLog.TestLog("ExecuteCommandAsync: 异步命令成功修改了 Model 状态");
}
```

带返回值的异步测试使用 `TaskEnumerator<T>`，通过回调获取结果：

```csharp
/// <summary>
/// 验证异步查询正确返回结果
/// </summary>
[UnityTest]
public IEnumerator ExecuteQueryAsync_ReturnsCorrectResult()
{
    _controller.ExecuteCommand<IncreaseCountCommand>();

    int result = 0;
    yield return new TaskEnumerator<int>(
        _controller.ExecuteQueryAsync<GetCountAsyncQuery, int>(), r => result = r);

    Assert.AreEqual(1, result);
    AesirArchitectureLog.TestLog("ExecuteQueryAsync: 异步查询正确返回结果");
}
```

`TaskEnumerator` 实现模板：

```csharp
/// <summary>
/// 将 Task 转为 IEnumerator，供 UnityTest 协程驱动异步操作。
/// </summary>
class TaskEnumerator : IEnumerator
{
    readonly Task _task;

    public TaskEnumerator(Task task) => _task = task;

    public object Current => null;

    public bool MoveNext() => !_task.IsCompleted;

    public void Reset() => throw new NotSupportedException();
}

/// <summary>
/// 将 Task{T} 转为 IEnumerator，完成后通过 callback 回传结果。
/// </summary>
class TaskEnumerator<T> : IEnumerator
{
    readonly Task<T> _task;
    readonly Action<T> _onComplete;

    public TaskEnumerator(Task<T> task, Action<T> onComplete)
    {
        _task = task;
        _onComplete = onComplete;
    }

    public object Current => null;

    public bool MoveNext()
    {
        if (!_task.IsCompleted)
        {
            return true;
        }

        _onComplete(_task.Result);
        return false;
    }

    public void Reset() => throw new NotSupportedException();
}
```

### 14.3 测试日志

所有测试断言通过后**必须**输出 `AesirArchitectureLog.TestLog`，禁止使用 `Debug.Log`：

```csharp
// ✅ 正确
AesirArchitectureLog.TestLog("ExecuteCommand: 命令成功修改了 Model 状态");

// ❌ 错误 — 违反架构日志铁律
Debug.Log("ExecuteCommand: 命令成功修改了 Model 状态");
```

### 14.4 测试方法结构

- 每个测试方法以 `<summary>` 文档注释说明作用
- 禁止使用注释行（如 `// ── InsertSystem 测试 ──`）或 `#region` 划分测试区域

---

## 参考文献

| 来源 | 链接 |
|------|------|
| Unity C# Style Guide (Unity 6 Edition) 电子书 | <https://unity.com/resources/c-sharp-style-guide-unity-6> |
| Unity 命名与代码风格 | <https://unity.com/how-to/naming-and-code-style-tips-c-scripting-unity> |
| Unity 格式化最佳实践 | <https://unity.com/how-to/formatting-best-practices-c-scripting-unity> |
| Unity 示例代码样式表 | <https://github.com/thomasjacobsen-unity/Unity-Code-Style-Guide> |
| Microsoft Framework Design Guidelines | <https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/> |
| Google C# Style Guide | <https://google.github.io/styleguide/csharp-style.html> |
