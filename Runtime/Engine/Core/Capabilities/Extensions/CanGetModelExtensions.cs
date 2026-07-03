using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// Model 获取能力扩展方法
    /// </summary>
    public static class CanGetModelExtensions
    {
        /// <summary>
        /// 获取已注册的 Model。若未注册则抛出包含调用者和目标类型信息的异常。
        /// </summary>
        public static T GetModel<T>(this ICanGetModel self) where T : class, IModel
        {
            var model = self.Context.GetModel<T>();
            if (model == null)
            {
                throw new InvalidOperationException(
                    $"{AesirArchitectureLog.ErrorTag} [{self.GetType().Name}] 尝试获取 Model [{typeof(T).Name}]，" +
                    $"但该 Model 未在 Context 中注册。请确认已在 Configure() 中调用 RegisterModel<{typeof(T).Name}>()。");
            }

            return model;
        }
    }
}
