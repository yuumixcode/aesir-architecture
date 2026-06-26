namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 事件发布能力扩展方法。提供 <see cref="ICanInvokeWithContext" /> 能力接口的上下文路由扩展。
    /// </summary>
    public static class CanInvokeWithContextExtensions
    {
        /// <summary>
        /// 通过上下文事件总线发布事件
        /// </summary>
        public static void Invoke<T>(this ICanInvokeWithContext self, T e) where T : IEventArgs =>
            self.GetContext().EventBus.Invoke(e);
    }
}
