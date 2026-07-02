using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 无参数简单事件，提供自动移除监听的功能。
    /// </summary>
    public sealed class MiniEvent : IDisposable
    {
        Action _callbacks;

        /// <summary>
        /// 清空所有委托引用，释放内存
        /// </summary>
        public void Dispose()
        {
            _callbacks = null;
        }

        /// <summary>
        /// 添加监听者，并返回可自动移除的监听句柄
        /// </summary>
        public AutoRemoveListenerHandle AddListener(Action callback)
        {
            _callbacks += callback;
            return new AutoRemoveListenerHandle(() => RemoveListener(callback));
        }

        /// <summary>
        /// 移除监听者
        /// </summary>
        public void RemoveListener(Action callback) => _callbacks -= callback;

        /// <summary>
        /// 调用事件，通知所有监听者
        /// </summary>
        public void Invoke()
        {
            _callbacks?.Invoke();
        }
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
        public void Dispose()
        {
            _callbacks = null;
        }

        /// <summary>
        /// 添加监听者，返回可自动移除的监听句柄
        /// </summary>
        public AutoRemoveListenerHandle AddListener(Action<T> onEvent)
        {
            _callbacks += onEvent;
            return new AutoRemoveListenerHandle(() => RemoveListener(onEvent));
        }

        /// <summary>
        /// 移除监听者
        /// </summary>
        public void RemoveListener(Action<T> onEvent) => _callbacks -= onEvent;

        /// <summary>
        /// 调用事件，通知所有监听者
        /// </summary>
        public void Invoke(T t)
        {
            _callbacks?.Invoke(t);
        }
    }
}
