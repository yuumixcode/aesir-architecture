namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// Model 获取能力扩展方法
    /// </summary>
    public static class CanGetModelExtensions
    {
        /// <summary>
        /// 获取已注册的 Model
        /// </summary>
        public static T GetModel<T>(this ICanGetModel self) where T : class, IModel =>
            self.Context.GetModel<T>();
    }
}
