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
    public sealed class MiniEventBus : IDisposable
    {
        readonly Dictionary<Type, object> _eventDictionary = new Dictionary<Type, object>();

        /// <summary>
        /// 清空所有已注册的事件
        /// </summary>
        public void Dispose()
        {
            foreach (var e in _eventDictionary.Values)
            {
                (e as IDisposable)?.Dispose();
            }

            _eventDictionary.Clear();
        }

        /// <summary>
        /// 注册事件监听，返回可自动注销的订阅句柄
        /// </summary>
        public AutoUnsubscribeHandle Subscribe<T>(Action<T> listener) where T : IEventArgs
        {
            var eType = typeof(T);
            if (_eventDictionary.TryGetValue(eType, out var e))
            {
                var callback = (Action<T>)e + listener;
                _eventDictionary[eType] = callback;
            }
            else
            {
                _eventDictionary.Add(eType, listener);
            }

            return new AutoUnsubscribeHandle(() => Unsubscribe(listener));
        }

        /// <summary>
        /// 注销事件监听
        /// </summary>
        public void Unsubscribe<T>(Action<T> listener) where T : IEventArgs
        {
            var eType = typeof(T);
            if (_eventDictionary.TryGetValue(eType, out var e))
            {
                var callback = (Action<T>)e - listener;
                if (callback == null)
                {
                    _eventDictionary.Remove(eType);
                }
                else
                {
                    _eventDictionary[eType] = callback;
                }
            }
        }

        /// <summary>
        /// 发布事件
        /// </summary>
        public void Invoke<T>(T args) where T : IEventArgs
        {
            if (_eventDictionary.TryGetValue(typeof(T), out var e))
            {
                ((Action<T>)e)?.Invoke(args);
            }
        }
    }
}
