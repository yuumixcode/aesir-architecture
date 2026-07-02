using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 事件总线接口。按事件类型注册、移除与发布监听。
    /// </summary>
    public interface IMiniEventBus
    {
        /// <summary>
        /// 当事件注册信息发生变化时触发（例如添加或移除监听器）
        /// </summary>
        event Action OnEventRegistrationsChanged;

        /// <summary>
        /// 注册事件监听，返回可自动移除的监听句柄
        /// </summary>
        AutoRemoveListenerHandle AddListener<T>(Action<T> listener) where T : IEventArgs;

        /// <summary>
        /// 移除事件监听
        /// </summary>
        void RemoveListener<T>(Action<T> listener) where T : IEventArgs;

        /// <summary>
        /// 发布事件
        /// </summary>
        void InvokeEvent<T>(T args) where T : IEventArgs;

        /// <summary>
        /// 清空所有已注册的事件
        /// </summary>
        void Clear();
    }
}
