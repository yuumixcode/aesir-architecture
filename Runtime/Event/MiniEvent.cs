using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 无参数简单事件，提供自动取消订阅的功能。
    /// </summary>
    public sealed class MiniEvent : IDisposable
    {
        Action _callbacks;

        /// <summary>
        /// 清空所有委托引用，释放内存
        /// </summary>
        public void Dispose() => _callbacks = null;

        /// <summary>
        /// 订阅事件，并返回可自动注销的订阅句柄
        /// </summary>
        public AutoUnsubscribeHandle Subscribe(Action callback)
        {
            _callbacks += callback;
            return new AutoUnsubscribeHandle(() => Unsubscribe(callback));
        }

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        public void Unsubscribe(Action callback) => _callbacks -= callback;

        /// <summary>
        /// 调用事件，通知所有订阅者
        /// </summary>
        public void Invoke() => _callbacks?.Invoke();
    }

    /// <summary>
    /// 单参事件
    /// </summary>
    /// <typeparam name="T">事件参数类型</typeparam>
    public sealed class MiniEvent<T> : IDisposable
    {
        Action<T> _callbacks;

        /// <summary>
        /// 清空所有委托引用，释放内存
        /// </summary>
        public void Dispose() => _callbacks = null;

        /// <summary>
        /// 注册监听，返回可自动注销的订阅句柄
        /// </summary>
        public AutoUnsubscribeHandle Subscribe(Action<T> onEvent)
        {
            _callbacks += onEvent;
            return new AutoUnsubscribeHandle(() => Unsubscribe(onEvent));
        }

        /// <summary>
        /// 注销监听
        /// </summary>
        public void Unsubscribe(Action<T> onEvent) => _callbacks -= onEvent;

        /// <summary>
        /// 调用事件，通知所有监听者
        /// </summary>
        public void Invoke(T t)
        {
            _callbacks?.Invoke(t);
        }
    }
}
