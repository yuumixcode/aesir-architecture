namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// Service 获取能力扩展方法
    /// </summary>
    public static class CanGetServiceExtensions
    {
        /// <summary>
        /// 获取已注册的 Service
        /// </summary>
        public static T GetService<T>(this ICanGetService self) where T : class, IService =>
            self.Context.GetService<T>();
    }
}
