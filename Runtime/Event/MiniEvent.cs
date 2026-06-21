using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 无参数简单事件，提供自动取消订阅的功能。
    /// </summary>
    public sealed class MiniEvent : ISubscribe
    {
        Action _callbacks;

        /// <summary>
        /// 订阅事件，并返回可自动注销的订阅句柄
        /// </summary>
        public IUnsubscribe Subscribe(Action callback)
        {
            _callbacks += callback;
            return new AutoUnsubscribeHandle(() => Unsubscribe(callback));
        }

        /// <summary>
        /// 清空所有委托引用，释放内存
        /// </summary>
        public void Dispose() => _callbacks = null;

        /// <summary>
        /// 订阅事件并立刻调用事件一次，并返回可自动注销的订阅句柄
        /// </summary>
        public IUnsubscribe SubscribeAndInvoke(Action callback)
        {
            callback?.Invoke();
            return Subscribe(callback);
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
    public sealed class MiniEvent<T> : ISubscribe where T : IEventArgs
    {
        Action<T> _callbacks;

        /// <summary>
        /// 清空所有委托引用，释放内存
        /// </summary>
        public void Dispose() => _callbacks = null;

        IUnsubscribe ISubscribe.Subscribe(Action callback)
        {
            return Subscribe(Wrapper);

            void Wrapper(T value)
            {
                callback();
            }
        }

        /// <summary>
        /// 注册监听，返回可自动注销的订阅句柄
        /// </summary>
        public IUnsubscribe Subscribe(Action<T> onEvent)
        {
            _callbacks += onEvent;
            return new AutoUnsubscribeHandle(() => Unsubscribe(onEvent));
        }

        /// <summary>
        /// 订阅事件并立即使用指定参数调用一次回调，返回可自动注销的订阅句柄
        /// </summary>
        public IUnsubscribe SubscribeAndInvoke(Action<T> onEvent, T eventArgs)
        {
            if (eventArgs != null)
            {
                onEvent?.Invoke(eventArgs);
            }

            return Subscribe(onEvent);
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
