namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 事件发布能力扩展方法。提供 <see cref="MiniEventHub" /> 上的便捷操作，
    /// 以及 <see cref="ICanInvokeWithContext" /> / <see cref="ICanSubscribeWithContext" /> 能力接口的上下文路由扩展。
    /// </summary>
    public static class CanInvokeWithContextExtensions
    {
        /// <summary>
        /// 通过上下文事件总线发布无参事件
        /// </summary>
        public static void Invoke<T>(this ICanInvokeWithContext self) where T : IEventArgs, new() =>
            self.GetContext().EventHub.Invoke<T>();

        /// <summary>
        /// 通过上下文事件总线发布事件
        /// </summary>
        public static void Invoke<T>(this ICanInvokeWithContext self, T e) where T : IEventArgs =>
            self.GetContext().EventHub.Invoke(e);
    }
}
