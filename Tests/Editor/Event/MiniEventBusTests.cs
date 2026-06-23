using System;
using NUnit.Framework;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    public class MiniEventBusTests
    {
        MiniEventBus _bus;

        [SetUp]
        public void SetUp()
        {
            _bus = new MiniEventBus();
        }

        [TearDown]
        public void TearDown()
        {
            _bus?.Dispose();
            _bus = null;
        }

        // ── Subscribe + Invoke ──

        [Test]
        public void Subscribe_ThenInvoke_ListenerReceivesEvent()
        {
            TestEvent received = default;
            _bus.Subscribe<TestEvent>(e => received = e);

            _bus.Invoke(new TestEvent { Value = 42 });

            Assert.AreEqual(42, received.Value);
            AesirArchitectureLog.TestLog("Subscribe_ThenInvoke: 订阅者收到事件");
        }

        [Test]
        public void Invoke_NoSubscribers_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _bus.Invoke(new TestEvent()));
            AesirArchitectureLog.TestLog("Invoke_NoSubscribers: 无订阅者时不抛异常");
        }

        [Test]
        public void Invoke_Parameterless_ListenerReceivesDefaultEvent()
        {
            TestEvent received = default;
            _bus.Subscribe<TestEvent>(e => received = e);

            _bus.Invoke(new TestEvent());

            Assert.IsNotNull(received);
            AesirArchitectureLog.TestLog("Invoke_Parameterless: 无参 Invoke 正常创建并派发事件");
        }

        // ── 多订阅者 ──

        [Test]
        public void Subscribe_MultipleListeners_AllReceiveEvent()
        {
            int receivedA = 0, receivedB = 0;
            _bus.Subscribe<TestEvent>(e => receivedA = e.Value);
            _bus.Subscribe<TestEvent>(e => receivedB = e.Value);

            _bus.Invoke(new TestEvent { Value = 7 });

            Assert.AreEqual(7, receivedA);
            Assert.AreEqual(7, receivedB);
            AesirArchitectureLog.TestLog("Subscribe_Multiple: 多订阅者均收到事件");
        }

        [Test]
        public void Subscribe_DifferentEventTypes_DoNotInterfere()
        {
            int eventA = 0, eventB = 0;
            _bus.Subscribe<TestEvent>(e => eventA = e.Value);
            _bus.Subscribe<OtherEvent>(e => eventB = e.Message.Length);

            _bus.Invoke(new TestEvent { Value = 10 });

            Assert.AreEqual(10, eventA);
            Assert.AreEqual(0, eventB);

            _bus.Invoke(new OtherEvent { Message = "hello" });

            Assert.AreEqual(5, eventB);
            AesirArchitectureLog.TestLog("Subscribe_DifferentTypes: 不同事件类型互不干扰");
        }

        // ── Unsubscribe ──

        [Test]
        public void Unsubscribe_RemovesListener()
        {
            var callCount = 0;
            Action<TestEvent> cb = e => callCount++;
            _bus.Subscribe(cb);

            _bus.Invoke(new TestEvent());
            Assert.AreEqual(1, callCount);

            _bus.Unsubscribe(cb);
            _bus.Invoke(new TestEvent());

            Assert.AreEqual(1, callCount);
            AesirArchitectureLog.TestLog("Unsubscribe: 移除订阅者成功");
        }

        [Test]
        public void Unsubscribe_NeverSubscribed_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _bus.Unsubscribe<TestEvent>(e => { }));
            AesirArchitectureLog.TestLog("Unsubscribe_NeverSubscribed: 注销未注册事件不抛异常");
        }

        [Test]
        public void Unsubscribe_RemovesOneListener_KeepsOthers()
        {
            int countA = 0, countB = 0;
            Action<TestEvent> cbA = e => countA++;
            Action<TestEvent> cbB = e => countB++;

            _bus.Subscribe(cbA);
            _bus.Subscribe(cbB);

            _bus.Unsubscribe(cbA);
            _bus.Invoke(new TestEvent());

            Assert.AreEqual(0, countA);
            Assert.AreEqual(1, countB);
            AesirArchitectureLog.TestLog("Unsubscribe_OneKeepsOther: 移除单个订阅者不影响其他");
        }

        // ── Handle Dispose ──

        [Test]
        public void Subscribe_ReturnsHandle_DisposeRemovesListener()
        {
            var callCount = 0;
            var handle = _bus.Subscribe<TestEvent>(e => callCount++);

            _bus.Invoke(new TestEvent());
            Assert.AreEqual(1, callCount);

            handle.Dispose();
            _bus.Invoke(new TestEvent());

            Assert.AreEqual(1, callCount);
            AesirArchitectureLog.TestLog("Handle_Dispose: 句柄注销成功");
        }

        [Test]
        public void Subscribe_HandleDispose_CalledMultipleTimes_IsSafe()
        {
            var callCount = 0;
            var handle = _bus.Subscribe<TestEvent>(e => callCount++);

            handle.Dispose();
            handle.Dispose();
            handle.Dispose();

            _bus.Invoke(new TestEvent());
            Assert.AreEqual(0, callCount);
            AesirArchitectureLog.TestLog("Handle_MultiDispose: 重复 Dispose 安全");
        }

        // ── Re-subscribe ──

        [Test]
        public void Subscribe_AfterUnsubscribe_CanResubscribe()
        {
            var callCount = 0;
            Action<TestEvent> cb = e => callCount++;

            _bus.Subscribe(cb);
            _bus.Unsubscribe(cb);
            _bus.Subscribe(cb);

            _bus.Invoke(new TestEvent());
            Assert.AreEqual(1, callCount);
            AesirArchitectureLog.TestLog("Subscribe_AfterUnsubscribe: 可重新订阅");
        }

        // ── Dispose ──

        [Test]
        public void Dispose_ClearsAllListeners()
        {
            var callCount = 0;
            _bus.Subscribe<TestEvent>(e => callCount++);
            _bus.Subscribe<TestEvent>(e => callCount++);

            _bus.Dispose();
            _bus.Invoke(new TestEvent());

            Assert.AreEqual(0, callCount);
            AesirArchitectureLog.TestLog("Dispose: 清空所有订阅");
        }

        [Test]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            _bus.Subscribe<TestEvent>(e => { });
            _bus.Dispose();

            Assert.DoesNotThrow(() => _bus.Dispose());
            AesirArchitectureLog.TestLog("Dispose_Twice: 重复调用安全");
        }

        // ── Test event types ──

        struct TestEvent : IEventArgs
        {
            public int Value;
        }

        struct OtherEvent : IEventArgs
        {
            public string Message;
        }
    }
}
