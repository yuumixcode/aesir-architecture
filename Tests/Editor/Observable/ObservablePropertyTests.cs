using System;
using NUnit.Framework;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    public class ObservablePropertyTests
    {
        [Test]
        public void Constructor_Default_LeavesDefaultValue()
        {
            var prop = new ObservableProperty<int>();
            Assert.AreEqual(0, prop.Value);
            AesirArchitectureLog.TestLog("Constructor_Default: 默认值为 0");
        }

        [Test]
        public void Constructor_WithInitialValue_SetsValue()
        {
            var prop = new ObservableProperty<string>("hello");
            Assert.AreEqual("hello", prop.Value);
            AesirArchitectureLog.TestLog("Constructor_WithInitialValue: 初始值已设置");
        }

        [Test]
        public void Value_Set_DifferentValue_TriggersCallback()
        {
            var prop = new ObservableProperty<int>(10);
            var received = -1;
            prop.Subscribe(v => received = v);

            prop.Value = 20;

            Assert.AreEqual(20, prop.Value);
            Assert.AreEqual(20, received);
            AesirArchitectureLog.TestLog("Value_Set_DifferentValue: 不同值触发回调");
        }

        [Test]
        public void Value_Set_SameValue_DoesNotTriggerCallback()
        {
            var prop = new ObservableProperty<int>(5);
            var callCount = 0;
            prop.Subscribe(_ => callCount++);

            prop.Value = 5;

            Assert.AreEqual(0, callCount);
            AesirArchitectureLog.TestLog("Value_Set_SameValue: 相同值不触发回调");
        }

        [Test]
        public void SetValue_EquivalentToValueSetter()
        {
            var prop = new ObservableProperty<int>(1);
            var received = -1;
            prop.Subscribe(v => received = v);

            prop.SetValue(42);

            Assert.AreEqual(42, prop.Value);
            Assert.AreEqual(42, received);
            AesirArchitectureLog.TestLog("SetValue: 等价于 Value setter");
        }

        [Test]
        public void SetValueSilently_DoesNotTriggerCallback()
        {
            var prop = new ObservableProperty<int>(1);
            var callCount = 0;
            prop.Subscribe(_ => callCount++);

            prop.SetValueSilently(99);

            Assert.AreEqual(99, prop.Value);
            Assert.AreEqual(0, callCount);
            AesirArchitectureLog.TestLog("SetValueSilently: 静默设置不触发回调");
        }

        [Test]
        public void SetValueSilently_SameValue_DoesNotChangeValue()
        {
            var prop = new ObservableProperty<int>(7);
            prop.SetValueSilently(7);
            Assert.AreEqual(7, prop.Value);
            AesirArchitectureLog.TestLog("SetValueSilently_SameValue: 相同值不变");
        }

        [Test]
        public void Invoke_TriggersAllSubscribersWithCurrentValue()
        {
            var prop = new ObservableProperty<int>(30);
            int receivedA = -1, receivedB = -1;
            prop.Subscribe(v => receivedA = v);
            prop.Subscribe(v => receivedB = v);

            prop.Invoke();

            Assert.AreEqual(30, receivedA);
            Assert.AreEqual(30, receivedB);
            AesirArchitectureLog.TestLog("Invoke: 强制触发所有订阅者");
        }

        [Test]
        public void Subscribe_MultipleCallbacks_AllTriggeredOnChange()
        {
            var prop = new ObservableProperty<int>(0);
            int countA = 0, countB = 0;
            prop.Subscribe(_ => countA++);
            prop.Subscribe(_ => countB++);

            prop.Value = 1;

            Assert.AreEqual(1, countA);
            Assert.AreEqual(1, countB);
            AesirArchitectureLog.TestLog("Subscribe_Multiple: 多订阅者均触发");
        }

        [Test]
        public void Unsubscribe_RemovesCallback()
        {
            var prop = new ObservableProperty<int>(0);
            var callCount = 0;
            Action<int> cb = _ => callCount++;
            prop.Subscribe(cb);

            prop.Value = 1;
            Assert.AreEqual(1, callCount);

            prop.Unsubscribe(cb);
            prop.Value = 2;

            Assert.AreEqual(1, callCount);
            AesirArchitectureLog.TestLog("Unsubscribe: 移除回调成功");
        }

        [Test]
        public void Subscribe_ReturnsHandle_DisposeRemovesCallback()
        {
            var prop = new ObservableProperty<int>(0);
            var callCount = 0;
            var handle = prop.Subscribe(_ => callCount++);

            prop.Value = 1;
            Assert.AreEqual(1, callCount);

            handle.Dispose();
            prop.Value = 2;

            Assert.AreEqual(1, callCount);
            AesirArchitectureLog.TestLog("SubscribeHandle_Dispose: 句柄注销成功");
        }

        [Test]
        public void SubscribeHandle_Dispose_CalledMultipleTimes_IsSafe()
        {
            var prop = new ObservableProperty<int>(0);
            var callCount = 0;
            var handle = prop.Subscribe(_ => callCount++);

            handle.Dispose();
            handle.Dispose();
            handle.Dispose();

            prop.Value = 1;
            Assert.AreEqual(0, callCount);
            AesirArchitectureLog.TestLog("SubscribeHandle_MultiDispose: 重复 Dispose 安全");
        }

        [Test]
        public void SubscribeAndInvoke_SubscribesAndImmediatelyTriggers()
        {
            var prop = new ObservableProperty<int>(50);
            var received = -1;

            prop.SubscribeAndInvoke(v => received = v);

            Assert.AreEqual(50, received);

            prop.Value = 60;
            Assert.AreEqual(60, received);
            AesirArchitectureLog.TestLog("SubscribeAndInvoke: 订阅并立即触发");
        }

        [Test]
        public void Modify_MutatesValueAndTriggersNotification()
        {
            var prop = new ObservableProperty<TestData>(new TestData { Value = 1 });
            var received = -1;
            prop.Subscribe(v => received = v.Value);

            prop.Modify(data => data.Value = 5);

            Assert.AreEqual(5, prop.Value.Value);
            Assert.AreEqual(5, received);
            AesirArchitectureLog.TestLog("Modify: 原地修改并触发通知");
        }

        [Test]
        public void Modify_TriggersEvenIfValueContentUnchanged()
        {
            var prop = new ObservableProperty<TestData>(new TestData { Value = 1 });
            var callCount = 0;
            prop.Subscribe(_ => callCount++);

            prop.Modify(data => { });

            Assert.AreEqual(1, callCount);
            AesirArchitectureLog.TestLog("Modify_NoChange: 内容未变也触发");
        }

        [Test]
        public void Clear_RemovesAllSubscriptions()
        {
            var prop = new ObservableProperty<int>(0);
            var callCount = 0;
            prop.Subscribe(_ => callCount++);
            prop.Subscribe(_ => callCount++);

            prop.Clear();
            prop.Value = 99;

            Assert.AreEqual(0, callCount);
            AesirArchitectureLog.TestLog("Clear: 清除所有订阅");
        }

        [Test]
        public void Value_Get_ReturnsCurrentValue()
        {
            var prop = new ObservableProperty<int>(42);
            Assert.AreEqual(42, prop.Value);
            AesirArchitectureLog.TestLog("Value_Get: 返回当前值");
        }

        [Test]
        public void IReadOnlyObservableProperty_ExposesOnlyGetter()
        {
            IReadOnlyObservableProperty<int> readOnly = new ObservableProperty<int>(10);
            Assert.AreEqual(10, readOnly.Value);
            AesirArchitectureLog.TestLog("IReadOnlyObservableProperty: 仅暴露 getter");
        }

        [Test]
        public void Subscribe_AfterUnsubscribe_CanResubscribe()
        {
            var prop = new ObservableProperty<int>(0);
            var callCount = 0;
            Action<int> cb = _ => callCount++;

            prop.Subscribe(cb);
            prop.Unsubscribe(cb);
            prop.Subscribe(cb);

            prop.Value = 1;
            Assert.AreEqual(1, callCount);
            AesirArchitectureLog.TestLog("Subscribe_AfterUnsubscribe: 可重新订阅");
        }

        [Test]
        public void Value_Set_ReferenceType_DifferentReference_TriggersCallback()
        {
            var prop = new ObservableProperty<string>("a");
            var callCount = 0;
            prop.Subscribe(_ => callCount++);

            prop.Value = "b";

            Assert.AreEqual(1, callCount);
            AesirArchitectureLog.TestLog("Value_Set_DifferentReference: 不同引用触发回调");
        }

        [Test]
        public void Value_Set_ReferenceType_SameReference_DoesNotTriggerCallback()
        {
            var obj = new TestData { Value = 1 };
            var prop = new ObservableProperty<TestData>(obj);
            var callCount = 0;
            prop.Subscribe(_ => callCount++);

            prop.Value = obj;

            Assert.AreEqual(0, callCount);
            AesirArchitectureLog.TestLog("Value_Set_SameReference: 相同引用不触发回调");
        }

        class TestData
        {
            public int Value;
        }
    }
}
