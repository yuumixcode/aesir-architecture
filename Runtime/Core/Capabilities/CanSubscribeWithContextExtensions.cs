using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 事件订阅能力扩展方法
    /// </summary>
    public static class CanSubscribeWithContextExtensions
    {
        /// <summary>
        /// 通过上下文事件总线注册事件监听，返回可自动注销的订阅句柄
        /// </summary>
        public static AutoUnsubscribeHandle Subscribe<T>(this ICanSubscribeWithContext self,
            Action<T> onEvent) where T : IEventArgs =>
            self.GetContext().EventBus.Subscribe(onEvent);

        /// <summary>
        /// 通过上下文事件总线注销事件监听
        /// </summary>
        public static void Unsubscribe<T>(this ICanSubscribeWithContext self, Action<T> onEvent)
            where T : IEventArgs =>
            self.GetContext().EventBus.Unsubscribe(onEvent);
    }
}
