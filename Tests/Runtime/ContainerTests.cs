using NUnit.Framework;

namespace Runestone.AesirArchitecture.Tests
{
    public class ContainerTests
    {
        Container<ITestServiceBase> _container;

        [SetUp]
        public void SetUp()
        {
            _container = new Container<ITestServiceBase>();
        }

        /// <summary>
        /// 验证 Register + Get 可正常注册和获取实例
        /// </summary>
        [Test]
        public void RegisterAndGet_ReturnsInstance()
        {
            var service = new TestService();
            _container.Register<ITestService>(service);

            var result = _container.Get<ITestService>();
            Assert.AreSame(service, result);
            AesirArchitectureLog.TestLog("Register+Get: 成功注册和获取实例");
        }

        /// <summary>
        /// 验证 Get 未注册类型时返回 null
        /// </summary>
        [Test]
        public void Get_Unregistered_ReturnsNull()
        {
            var result = _container.Get<ITestService>();
            Assert.IsNull(result);
            AesirArchitectureLog.TestLog("Get(未注册): 正确返回 null");
        }

        /// <summary>
        /// 验证重复注册会覆盖
        /// </summary>
        [Test]
        public void Register_Duplicate_Overwrites()
        {
            var service1 = new TestService { Name = "first" };
            var service2 = new TestService { Name = "second" };

            _container.Register<ITestService>(service1);
            _container.Register<ITestService>(service2);

            var result = _container.Get<ITestService>();
            Assert.AreSame(service2, result);
            AesirArchitectureLog.TestLog("Register(重复): 后注册的实例覆盖了先前的");
        }

        /// <summary>
        /// 验证 GetAll 返回所有已注册的实例
        /// </summary>
        [Test]
        public void GetAll_ReturnsAllRegisteredInstances()
        {
            _container.Register<ITestService>(new TestService { Name = "a" });
            _container.Register<ITestService2>(new TestService2());

            var instances = _container.GetAll();
            int count = 0;
            foreach (var _ in instances)
            {
                count++;
            }

            Assert.AreEqual(2, count);
            AesirArchitectureLog.TestLog("GetAll: 正确返回所有已注册的实例");
        }

        /// <summary>
        /// 验证 Clear 清空所有注册
        /// </summary>
        [Test]
        public void Clear_RemovesAllRegistrations()
        {
            _container.Register<ITestService>(new TestService());
            _container.Register<ITestService2>(new TestService2());

            _container.Clear();

            Assert.IsNull(_container.Get<ITestService>());
            Assert.IsNull(_container.Get<ITestService2>());
            AesirArchitectureLog.TestLog("Clear: 成功清空所有注册");
        }

        interface ITestServiceBase { }
        interface ITestService : ITestServiceBase { string Name { get; set; } }
        interface ITestService2 : ITestServiceBase { }
        class TestService : ITestService { public string Name { get; set; } }
        class TestService2 : ITestService2 { }
    }
}
