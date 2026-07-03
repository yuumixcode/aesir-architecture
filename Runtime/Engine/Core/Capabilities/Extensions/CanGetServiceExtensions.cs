using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// Service 获取能力扩展方法
    /// </summary>
    public static class CanGetServiceExtensions
    {
        /// <summary>
        /// 获取已注册的 Service。若未注册则抛出包含调用者和目标类型信息的异常。
        /// </summary>
        public static T GetService<T>(this ICanGetService self) where T : class, IService
        {
            var service = self.Context.GetService<T>();
            if (service == null)
            {
                throw new InvalidOperationException(
                    $"{AesirArchitectureLog.ErrorTag} [{self.GetType().Name}] 尝试获取 Service [{typeof(T).Name}]，" +
                    $"但该 Service 未在 Context 中注册。请确认已在 Configure() 中调用 RegisterService<{typeof(T).Name}>()。");
            }

            return service;
        }
    }
}
