namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 事件发布能力扩展方法。提供 <see cref="ICanInvokeEvent" /> 能力接口的上下文事件总线扩展。
    /// </summary>
    public static class CanInvokeExtensions
    {
        /// <summary>
        /// 通过上下文事件总线发布事件
        /// </summary>
        public static void InvokeEvent<T>(this ICanInvokeEvent self, T e) where T : IEventArgs =>
            self.Context.InvokeEvent(e);
    }
}
