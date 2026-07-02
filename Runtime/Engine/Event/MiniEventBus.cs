using System;
using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 轻量级事件总线。按事件类型注册、移除与发布监听，不依赖 MonoBehaviour。
    /// </summary>
    public sealed class MiniEventBus : IMiniEventBus
    {
        /// <summary>
        /// 全局唯一的事件总线单例
        /// </summary>
        public static readonly MiniEventBus Global = new MiniEventBus();

        /// <summary>
        /// 当事件注册表发生变化时触发（添加或移除监听）
        /// </summary>
        public event Action OnEventRegistrationsChanged;

        /// <summary>
        /// 存储所有事件类型及其对应监听委托的字典
        /// </summary>
        public Dictionary<Type, object> EventDictionary { get; } = new Dictionary<Type, object>();

        /// <summary>
        /// 注册事件监听，返回可自动移除的监听句柄
        /// </summary>
        public AutoRemoveListenerHandle AddListener<T>(Action<T> listener) where T : IEventArgs
        {
            var eType = typeof(T);
            if (EventDictionary.TryGetValue(eType, out var e))
            {
                var callback = (Action<T>)e + listener;
                EventDictionary[eType] = callback;
            }
            else
            {
                EventDictionary.Add(eType, listener);
            }

            OnEventRegistrationsChanged?.Invoke();
            return new AutoRemoveListenerHandle(() => RemoveListener(listener));
        }

        /// <summary>
        /// 移除事件监听
        /// </summary>
        public void RemoveListener<T>(Action<T> listener) where T : IEventArgs
        {
            var eType = typeof(T);
            if (!EventDictionary.TryGetValue(eType, out var e))
            {
                return;
            }

            var callback = (Action<T>)e - listener;
            if (callback == null)
            {
                EventDictionary.Remove(eType);
            }
            else
            {
                EventDictionary[eType] = callback;
            }

            OnEventRegistrationsChanged?.Invoke();
        }

        /// <summary>
        /// 发布事件
        /// </summary>
        public void InvokeEvent<T>(T args) where T : IEventArgs
        {
            if (EventDictionary.TryGetValue(typeof(T), out var e))
            {
                ((Action<T>)e)?.Invoke(args);
            }
        }

        /// <summary>
        /// 清空所有已注册的事件
        /// </summary>
        public void Clear()
        {
            foreach (var e in EventDictionary.Values)
            {
                (e as IDisposable)?.Dispose();
            }

            EventDictionary.Clear();
            OnEventRegistrationsChanged?.Invoke();
        }
    }
}
