using System;
using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 轻量级事件总线。按事件类型注册、移除与发布监听，不依赖 MonoBehaviour。
    /// </summary>
    public sealed class MiniEventBus
    {
        /// <summary>
        /// 全局唯一的事件总线单例
        /// </summary>
        public static MiniEventBus Global { get; private set; } = new MiniEventBus();

        /// <summary>
        /// 当事件注册表发生变化时触发（添加或移除监听）
        /// </summary>
        public event Action OnEventRegistrationsChanged;

        /// <summary>
        /// 存储所有事件类型及其对应监听委托的字典
        /// </summary>
        readonly Dictionary<Type, Delegate> _eventDictionary = new Dictionary<Type, Delegate>();

        /// <summary>
        /// 注册事件监听，返回可自动移除的监听句柄
        /// </summary>
        public AutoRemoveListenerHandle AddListener<T>(Action<T> listener) where T : IEventArgs
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

            OnEventRegistrationsChanged?.Invoke();
            return new AutoRemoveListenerHandle(() => RemoveListener(listener));
        }

        /// <summary>
        /// 移除事件监听
        /// </summary>
        public void RemoveListener<T>(Action<T> listener) where T : IEventArgs
        {
            var eType = typeof(T);
            if (!_eventDictionary.TryGetValue(eType, out var e))
            {
                return;
            }

            var callback = (Action<T>)e - listener;
            if (callback == null)
            {
                _eventDictionary.Remove(eType);
            }
            else
            {
                _eventDictionary[eType] = callback;
            }

            OnEventRegistrationsChanged?.Invoke();
        }

        /// <summary>
        /// 发布事件
        /// </summary>
        public void InvokeEvent<T>(T args) where T : IEventArgs
        {
            if (_eventDictionary.TryGetValue(typeof(T), out var e))
            {
                ((Action<T>)e)?.Invoke(args);
            }
        }

        /// <summary>
        /// 发布无参事件，内部创建默认实例后转发给 <see cref="InvokeEvent{T}(T)" />
        /// </summary>
        public void InvokeEvent<T>() where T : IEventArgs, new() =>
            InvokeEvent(new T());

        /// <summary>
        /// 清空所有已注册的事件
        /// </summary>
        public void Clear()
        {
            _eventDictionary.Clear();
            OnEventRegistrationsChanged?.Invoke();
        }
    }
}
