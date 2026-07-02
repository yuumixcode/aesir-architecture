using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 事件监听能力扩展方法
    /// </summary>
    public static class CanAddListenerExtensions
    {
        /// <summary>
        /// 通过上下文事件总线注册事件监听，返回可自动移除的监听句柄
        /// </summary>
        public static AutoRemoveListenerHandle AddListener<T>(this ICanAddListener self, Action<T> onEvent)
            where T : IEventArgs =>
            self.Context.AddListener(onEvent);

        /// <summary>
        /// 通过上下文事件总线移除事件监听
        /// </summary>
        public static void RemoveListener<T>(this ICanAddListener self, Action<T> onEvent)
            where T : IEventArgs =>
            self.Context.RemoveListener(onEvent);
    }
}
