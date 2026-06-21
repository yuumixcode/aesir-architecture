namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// System 获取能力扩展方法
    /// </summary>
    public static class CanGetSystemExtensions
    {
        /// <summary>
        /// 获取已注册的 System
        /// </summary>
        public static T GetSystem<T>(this ICanGetSystem self) where T : class, ISystem =>
            self.GetContext().GetSystem<T>();
    }
}
