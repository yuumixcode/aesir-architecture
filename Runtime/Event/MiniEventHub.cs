using System;
using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 类型键控事件集合。按事件类型注册、查询与调用事件实例。
    /// <para>
    /// 可选持有 <see cref="IContext" /> 引用，通过 <see cref="ICanSetContext.SetContext" /> 关联上下文后，
    /// 事件相关扩展方法可访问 Model 等上下文资源。不关联上下文时也可独立作为事件总线使用。
    /// </para>
    /// </summary>
    public sealed class MiniEventHub
    {
        readonly Dictionary<Type, ISubscribe> _eventDictionary = new Dictionary<Type, ISubscribe>();

        /// <summary>
        /// 获取已注册的事件实例，不存在则返回默认值
        /// </summary>
        public T Get<T>() where T : ISubscribe =>
            _eventDictionary.TryGetValue(typeof(T), out var @event) ? (T)@event : default;

        /// <summary>
        /// 获取或添加事件实例，不存在时自动创建
        /// </summary>
        public T AddAndGet<T>() where T : ISubscribe, new()
        {
            var eType = typeof(T);
            if (_eventDictionary.TryGetValue(eType, out var e))
            {
                return (T)e;
            }

            var t = new T();
            _eventDictionary.Add(eType, t);
            return t;
        }

        /// <summary>
        /// 移除已注册的事件实例，并释放其内部委托引用
        /// </summary>
        public void Remove<T>() where T : ISubscribe
        {
            var eType = typeof(T);
            if (_eventDictionary.Remove(eType, out var e))
            {
                e.Dispose();
            }
        }

        /// <summary>
        /// 清空所有已注册的事件
        /// </summary>
        public void Clear()
        {
            foreach (var e in _eventDictionary.Values)
            {
                e.Dispose();
            }

            _eventDictionary.Clear();
        }

        /// <summary>
        /// 注册事件监听，返回可自动注销的订阅句柄
        /// </summary>
        public IUnsubscribe Subscribe<T>(Action<T> onEvent) where T : IEventArgs =>
            AddAndGet<MiniEvent<T>>().Subscribe(onEvent);

        /// <summary>
        /// 注销事件监听
        /// </summary>
        public void Unsubscribe<T>(Action<T> onEvent) where T : IEventArgs =>
            Get<MiniEvent<T>>()?.Unsubscribe(onEvent);

        /// <summary>
        /// 发布无参事件
        /// </summary>
        public void Invoke<T>() where T : IEventArgs, new() => Get<MiniEvent<T>>()?.Invoke(new T());

        /// <summary>
        /// 发布事件
        /// </summary>
        public void Invoke<T>(T e) where T : IEventArgs => Get<MiniEvent<T>>()?.Invoke(e);
    }
}
