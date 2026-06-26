using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    public class BaseContextTests
    {
        IController _controller;
        IContext _ctx;

        [SetUp]
        public void SetUp()
        {
            _ctx = new MockContext(c =>
            {
                c.RegisterModel<ICounterModel>(new CounterModel());
                c.RegisterModel<IDependantModel>(new DependantModel());
            });
            _controller = new TestController(_ctx);
        }

        [TearDown]
        public void TearDown()
        {
            _ctx?.Dispose();
            _ctx = null;
            _controller = null;
        }

        /// <summary>
        /// 验证 InitContext 后上下文标记为已初始化
        /// </summary>
        [Test]
        public void InitContext_SetsInitializedFlag()
        {
            Assert.IsTrue(_ctx.Initialized);
            AesirArchitectureLog.TestLog("InitContext: 上下文初始化成功");
        }

        /// <summary>
        /// 验证注册的 Model 在初始化阶段自动调用 OnInitialize
        /// </summary>
        [Test]
        public void RegisterModel_AutoInitializesOnInitialize()
        {
            var model = _ctx.GetModel<ICounterModel>() as CounterModel;
            Assert.IsNotNull(model);
            Assert.IsTrue(model.Initialized);
            AesirArchitectureLog.TestLog("RegisterModel: Model 在初始化阶段自动调用了 OnInitialize");
        }

        /// <summary>
        /// 验证 GetModel 返回已注册的实例
        /// </summary>
        [Test]
        public void GetModel_ReturnsRegisteredInstance()
        {
            var model = _ctx.GetModel<ICounterModel>();
            Assert.IsNotNull(model);
            AesirArchitectureLog.TestLog("GetModel: 正确返回了已注册的实例");
        }

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

        /// <summary>
        /// 验证查询正确返回结果
        /// </summary>
        [Test]
        public void ExecuteQuery_ReturnsCorrectResult()
        {
            _controller.ExecuteCommand<IncreaseCountCommand>();
            var result = _controller.ExecuteQuery<GetCountQuery, int>();
            Assert.AreEqual(1, result);
            AesirArchitectureLog.TestLog("ExecuteQuery: 查询正确返回结果");
        }

        /// <summary>
        /// 验证事件通信正常工作（使用 ICanSubscribeWithContext / ICanInvokeWithContext 能力闭环）
        /// </summary>
        [Test]
        public void Invoke_RegisteredListener_ReceivesEvent()
        {
            CountChangedEvent received = default;
            _controller.Subscribe<CountChangedEvent>(e => received = e);
            _controller.ExecuteCommand<IncreaseCountCommand>();
            Assert.AreEqual(1, received.NewCount);
            AesirArchitectureLog.TestLog(
                "Invoke: 事件闭环正常工作（ICanSubscribeWithContext → Context.EventHub → ICanInvokeWithContext）");
        }

        /// <summary>
        /// 验证 Dispose 后上下文不再标记为已初始化
        /// </summary>
        [Test]
        public void Dispose_ClearsInitializedFlag()
        {
            _ctx.Dispose();
            Assert.IsFalse(_ctx.Initialized);
            AesirArchitectureLog.TestLog("Dispose: 上下文初始化标志已清除");
        }

        /// <summary>
        /// 验证重复调用 Dispose 不会抛异常
        /// </summary>
        [Test]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            _ctx.Dispose();
            Assert.DoesNotThrow(() => _ctx.Dispose());
            AesirArchitectureLog.TestLog("Dispose(两次): 重复调用不抛异常");
        }

        /// <summary>
        /// 验证正确注册顺序下，被依赖的 Model 先完成初始化
        /// </summary>
        [Test]
        public void ModelDependency_CorrectOrder_DependencyInitialized()
        {
            var modelA = _ctx.GetModel<IDependantModel>() as DependantModel;
            Assert.IsNotNull(modelA);
            Assert.IsTrue(modelA.Initialized);
            Assert.IsTrue(modelA.DependencyReady);
            AesirArchitectureLog.TestLog("ModelDependency: 正确注册顺序下被依赖项先初始化");
        }

        /// <summary>
        /// 验证反序注册时，依赖项尚未初始化则抛出 InvalidOperationException
        /// </summary>
        [Test]
        public void ModelDependency_ReverseRegistrationOrder_Throws()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                new MockContext(c =>
                {
                    c.RegisterModel<IDependantModel>(new DependantModel());
                    c.RegisterModel<ICounterModel>(new CounterModel());
                });
            });
            AesirArchitectureLog.TestLog("ModelDependency(反序注册): 依赖项未初始化，正确抛出 InvalidOperationException");
        }

        /// <summary>
        /// 验证循环依赖——A 依赖 B，B 依赖 A，注册顺序无法满足，抛出 InvalidOperationException
        /// </summary>
        [Test]
        public void ModelDependency_Circular_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                new MockContext(c =>
                {
                    c.RegisterModel<ICircularModelA>(new CircularModelA());
                    c.RegisterModel<ICircularModelB>(new CircularModelB());
                });
            });
            AesirArchitectureLog.TestLog("ModelDependency(循环): 依赖项未初始化，正确抛出 InvalidOperationException");
        }

        /// <summary>
        /// 验证 Model 声明非 IModel 依赖项时抛出 InvalidOperationException
        /// </summary>
        [Test]
        public void ModelDependency_InvalidType_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                new MockContext(c =>
                {
                    c.RegisterModel<IInvalidDepModel>(new InvalidDepModel());
                });
            });
            AesirArchitectureLog.TestLog("ModelDependency(非法依赖): 正确抛出 InvalidOperationException");
        }

        /// <summary>
        /// 验证 Model 声明了未注册的依赖项时，统一初始化抛出 InvalidOperationException
        /// </summary>
        [Test]
        public void ModelDependency_UnregisteredDependency_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                new MockContext(c =>
                {
                    c.RegisterModel<IMissingDepModel>(new MissingDepModel());
                });
            });
            AesirArchitectureLog.TestLog("ModelDependency(未注册依赖): 统一初始化阶段正确抛出 InvalidOperationException");
        }

        /// <summary>
        /// 验证 System 依赖 Model 时，Model 已完成初始化
        /// </summary>
        [Test]
        public void SystemDependency_OnInitialize_GetsInitializedModel()
        {
            IContext ctx = new MockContext(c =>
            {
                c.RegisterModel<ICounterModel>(new CounterModel());
                c.RegisterSystem<ICounterSystem>(new CounterSystem());
            });

            var system = ctx.GetSystem<ICounterSystem>() as CounterSystem;
            Assert.IsNotNull(system);
            Assert.IsTrue(system.Initialized);
            Assert.IsTrue(system.ModelReady);
            ctx.Dispose();
            AesirArchitectureLog.TestLog("SystemDependency: System 在 OnInitialize 中获取 Model，Model 已完成初始化");
        }

        /// <summary>
        /// 验证 System 之间的依赖关系——SystemB 依赖 SystemA，SystemA 先完成初始化
        /// </summary>
        [Test]
        public void SystemDependency_SystemToSystem_CorrectOrder()
        {
            IContext ctx = new MockContext(c =>
            {
                c.RegisterSystem<ISystemA>(new SystemA());
                c.RegisterSystem<ISystemB>(new SystemB());
            });

            var systemB = ctx.GetSystem<ISystemB>() as SystemB;
            Assert.IsNotNull(systemB);
            Assert.IsTrue(systemB.Initialized);
            Assert.IsTrue(systemB.SystemAReady);
            ctx.Dispose();
            AesirArchitectureLog.TestLog("SystemDependency(System→System): 正确注册顺序保证被依赖 System 先初始化");
        }

        /// <summary>
        /// 验证 System 声明非 IModel/ISystem 依赖项时抛出 InvalidOperationException
        /// </summary>
        [Test]
        public void SystemDependency_InvalidType_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                new MockContext(c =>
                {
                    c.RegisterSystem<IInvalidDepSystem>(new InvalidDepSystem());
                });
            });
            AesirArchitectureLog.TestLog("SystemDependency(非法依赖): 正确抛出 InvalidOperationException");
        }

        /// <summary>
        /// 验证 System 循环依赖——A 依赖 B，B 依赖 A，注册顺序无法满足，抛出 InvalidOperationException
        /// </summary>
        [Test]
        public void SystemDependency_Circular_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                new MockContext(c =>
                {
                    c.RegisterSystem<ICircularSystemA>(new CircularSystemA());
                    c.RegisterSystem<ICircularSystemB>(new CircularSystemB());
                });
            });
            AesirArchitectureLog.TestLog("SystemDependency(循环): 依赖项未初始化，正确抛出 InvalidOperationException");
        }

        /// <summary>
        /// 验证统一初始化完成后注册新 Model，若依赖项全部已初始化则立即初始化
        /// </summary>
        [Test]
        public void PostInit_RegisterModelWithAllDependencies_InitializesImmediately()
        {
            IContext ctx = new MockContext(c =>
            {
                c.RegisterModel<ICounterModel>(new CounterModel());
            });

            var lateModel = new DependantModel();
            ctx.RegisterModel<IDependantModel>(lateModel);

            Assert.IsTrue(lateModel.Initialized);
            Assert.IsTrue(lateModel.DependencyReady);
            ctx.Dispose();
            AesirArchitectureLog.TestLog("PostInit(全依赖): 上下文初始化后注册 Model，依赖项已初始化则立即初始化");
        }

        /// <summary>
        /// 验证统一初始化完成后注册新 Model，若依赖项未注册则抛出 InvalidOperationException
        /// </summary>
        [Test]
        public void PostInit_RegisterModelWithMissingDependency_ThrowsInvalidOperationException()
        {
            IContext ctx = new MockContext(c => { });

            Assert.Throws<InvalidOperationException>(() =>
            {
                ctx.RegisterModel<IMissingDepModel>(new MissingDepModel());
            });
            ctx.Dispose();
            AesirArchitectureLog.TestLog("PostInit(缺依赖): 上下文初始化后注册 Model，依赖项未注册则抛出异常");
        }

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

        // ──────────────────────────────────────────────────────────
        //  Test Fixtures — Model
        // ──────────────────────────────────────────────────────────

        interface ICounterModel : IModel
        {
            ObservableProperty<int> Count { get; }
            void Increase();
        }

        interface IDependantModel : IModel
        {
            bool DependencyReady { get; }
        }

        interface ICircularModelA : IModel { }

        interface ICircularModelB : IModel { }

        interface IInvalidDepModel : IModel { }

        interface IMissingDepModel : IModel { }

        struct CountChangedEvent : IEventArgs
        {
            public int NewCount;
        }

        class CounterModel : AbstractModel, ICounterModel
        {
            public ObservableProperty<int> Count { get; } = new ObservableProperty<int>();

            public void Increase()
            {
                Count.Value++;
                this.Invoke(new CountChangedEvent { NewCount = Count.Value });
            }

            protected override void OnInitialize() { }
        }

        class DependantModel : AbstractModel, IDependantModel
        {
            public bool DependencyReady { get; private set; }

            public override HashSet<Type> GetDependencies() => new HashSet<Type> { typeof(ICounterModel) };

            protected override void OnInitialize()
            {
                var counter = this.GetModel<ICounterModel>();
                DependencyReady = counter != null && counter.Initialized;
            }
        }

        class CircularModelA : AbstractModel, ICircularModelA
        {
            public override HashSet<Type> GetDependencies() => new HashSet<Type> { typeof(ICircularModelB) };

            protected override void OnInitialize() { }
        }

        class CircularModelB : AbstractModel, ICircularModelB
        {
            public override HashSet<Type> GetDependencies() => new HashSet<Type> { typeof(ICircularModelA) };

            protected override void OnInitialize() { }
        }

        class InvalidDepModel : AbstractModel, IInvalidDepModel
        {
            public override HashSet<Type> GetDependencies() => new HashSet<Type> { typeof(ICounterSystem) };

            protected override void OnInitialize() { }
        }

        class MissingDepModel : AbstractModel, IMissingDepModel
        {
            public override HashSet<Type> GetDependencies() => new HashSet<Type> { typeof(ICounterModel) };

            protected override void OnInitialize() { }
        }

        // ──────────────────────────────────────────────────────────
        //  Test Fixtures — System
        // ──────────────────────────────────────────────────────────

        interface ICounterSystem : ISystem
        {
            bool ModelReady { get; }
        }

        interface ISystemA : ISystem { }

        interface ISystemB : ISystem
        {
            bool SystemAReady { get; }
        }

        interface IInvalidDepSystem : ISystem { }

        interface ICircularSystemA : ISystem { }

        interface ICircularSystemB : ISystem { }

        class CounterSystem : AbstractSystem, ICounterSystem
        {
            public bool ModelReady { get; private set; }

            public override HashSet<Type> GetDependencies() => new HashSet<Type> { typeof(ICounterModel) };

            protected override void OnInitialize()
            {
                var model = this.GetModel<ICounterModel>();
                ModelReady = model != null && model.Initialized;
            }

            protected override void OnDispose() { }
        }

        class SystemA : AbstractSystem, ISystemA
        {
            protected override void OnInitialize() { }
            protected override void OnDispose() { }
        }

        class SystemB : AbstractSystem, ISystemB
        {
            public bool SystemAReady { get; private set; }

            public override HashSet<Type> GetDependencies() => new HashSet<Type> { typeof(ISystemA) };

            protected override void OnInitialize()
            {
                var systemA = this.GetSystem<ISystemA>();
                SystemAReady = systemA != null && systemA.Initialized;
            }

            protected override void OnDispose() { }
        }

        class InvalidDepSystem : AbstractSystem, IInvalidDepSystem
        {
            public override HashSet<Type> GetDependencies() =>
                new HashSet<Type> { typeof(InvalidDependencyMarker) };

            protected override void OnInitialize() { }
            protected override void OnDispose() { }
        }

        class InvalidDependencyMarker { }

        class CircularSystemA : AbstractSystem, ICircularSystemA
        {
            public override HashSet<Type> GetDependencies() => new HashSet<Type> { typeof(ICircularSystemB) };

            protected override void OnInitialize() { }
            protected override void OnDispose() { }
        }

        class CircularSystemB : AbstractSystem, ICircularSystemB
        {
            public override HashSet<Type> GetDependencies() => new HashSet<Type> { typeof(ICircularSystemA) };

            protected override void OnInitialize() { }
            protected override void OnDispose() { }
        }

        // ──────────────────────────────────────────────────────────
        //  Test Fixtures — Command / Query / Controller
        // ──────────────────────────────────────────────────────────

        class IncreaseCountCommand : AbstractCommand
        {
            protected override void OnExecute()
            {
                this.GetModel<ICounterModel>().Increase();
            }
        }

        class GetCountQuery : AbstractQuery<int>
        {
            protected override int OnExecute() => this.GetModel<ICounterModel>().Count.Value;
        }

        class IncreaseCountAsyncCommand : AbstractAsyncCommand
        {
            protected override async Task OnExecuteAsync()
            {
                await Task.Yield();
                this.GetModel<ICounterModel>().Increase();
            }
        }

        class GetCountAsyncQuery : AbstractAsyncQuery<int>
        {
            protected override async Task<int> OnExecuteAsync()
            {
                await Task.Yield();
                return this.GetModel<ICounterModel>().Count.Value;
            }
        }

        class TestController : IController
        {
            readonly IContext _context;

            public TestController(IContext context) => _context = context;

            IContext IContextHolder.GetContext() => _context;
        }

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
    }
}
