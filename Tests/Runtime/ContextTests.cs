using System.Threading.Tasks;
using NUnit.Framework;

namespace Runestone.AesirArchitecture.Tests
{
    public class ContextTests
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
        /// 验证 ModelA 在 OnInitialize 中通过 GetModel 获取 ModelB 时，ModelB 已完成初始化
        /// </summary>
        [Test]
        public void ModelDependency_OnInitialize_GetsInitializedDependency()
        {
            var modelA = _ctx.GetModel<IDependantModel>() as DependantModel;
            Assert.IsNotNull(modelA);
            Assert.IsTrue(modelA.Initialized);
            Assert.IsTrue(modelA.DependencyReady);
            AesirArchitectureLog.TestLog("ModelDependency: ModelA 在 OnInitialize 中获取 ModelB，ModelB 已完成初始化");
        }

        /// <summary>
        /// 验证注册顺序不影响按需初始化——先注册 ModelA 再注册 ModelB，ModelA 仍能正确获取已初始化的 ModelB
        /// </summary>
        [Test]
        public void ModelDependency_ReverseRegistrationOrder_StillWorks()
        {
            IContext ctx = new MockContext(c =>
            {
                c.RegisterModel<IDependantModel>(new DependantModel());
                c.RegisterModel<ICounterModel>(new CounterModel());
            });

            var modelA = ctx.GetModel<IDependantModel>() as DependantModel;
            Assert.IsTrue(modelA.DependencyReady);
            ctx.Dispose();
            AesirArchitectureLog.TestLog("ModelDependency(反序注册): 先注册 ModelA 再注册 ModelB，按需初始化正常工作");
        }

        /// <summary>
        /// 验证循环依赖检测——ModelA 依赖 ModelB，ModelB 依赖 ModelA，抛出 InvalidOperationException
        /// </summary>
        [Test]
        public void ModelDependency_Circular_ThrowsInvalidOperationException()
        {
            IContext ctx = new MockContext(c =>
            {
                c.RegisterModel<ICircularModelA>(new CircularModelA());
                c.RegisterModel<ICircularModelB>(new CircularModelB());
            });

            Assert.Throws<System.InvalidOperationException>(() => ctx.GetModel<ICircularModelA>());
            ctx.Dispose();
            AesirArchitectureLog.TestLog("ModelDependency(循环): 正确抛出 InvalidOperationException");
        }

        /// <summary>
        /// 验证异步命令执行后正确修改 Model 状态
        /// </summary>
        [Test]
        public async Task ExecuteCommandAsync_ModifiesModelState()
        {
            var model = _ctx.GetModel<ICounterModel>();

            Assert.AreEqual(0, model.Count.Value);

            await _controller.ExecuteCommandAsync<IncreaseCountAsyncCommand>();

            Assert.AreEqual(1, model.Count.Value);
            AesirArchitectureLog.TestLog("ExecuteCommandAsync: 异步命令成功修改了 Model 状态");
        }

        /// <summary>
        /// 验证异步查询正确返回结果
        /// </summary>
        [Test]
        public async Task ExecuteQueryAsync_ReturnsCorrectResult()
        {
            _controller.ExecuteCommand<IncreaseCountCommand>();

            var result = await _controller.ExecuteQueryAsync<GetCountAsyncQuery, int>();

            Assert.AreEqual(1, result);
            AesirArchitectureLog.TestLog("ExecuteQueryAsync: 异步查询正确返回结果");
        }

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

        class DependantModel : AbstractModel, IDependantModel
        {
            public bool DependencyReady { get; private set; }

            protected override void OnInitialize()
            {
                var counter = this.GetModel<ICounterModel>();
                DependencyReady = counter != null && counter.Initialized;
            }
        }

        class CircularModelA : AbstractModel, ICircularModelA
        {
            protected override void OnInitialize()
            {
                this.GetModel<ICircularModelB>();
            }
        }

        class CircularModelB : AbstractModel, ICircularModelB
        {
            protected override void OnInitialize()
            {
                this.GetModel<ICircularModelA>();
            }
        }
    }
}
