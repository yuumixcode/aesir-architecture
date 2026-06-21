using System;
using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 类型键控实例容器。按类型注册、查询与解析实例，供上下文管理模块生命周期。
    /// </summary>
    public sealed class Container<T> where T : class
    {
        readonly Dictionary<Type, T> _instances = new Dictionary<Type, T>();

        /// <summary>
        /// 注册实例，以类型作为键。
        /// <para>
        /// 注意：注册与查询必须使用相同的类型参数。若以具体类型注册（如 <c>Register&lt;CounterModel&gt;</c>），
        /// 再以接口类型查询（如 <c>Get&lt;ICounterModel&gt;</c>），将返回 <c>null</c>。
        /// </para>
        /// </summary>
        public void Register<TItem>(TItem instance) where TItem : class, T
        {
            var key = typeof(TItem);
            _instances[key] = instance;
        }

        /// <summary>
        /// 获取已注册的实例，不存在则返回 null
        /// </summary>
        public TItem Get<TItem>() where TItem : class, T
        {
            var key = typeof(TItem);

            if (_instances.TryGetValue(key, out var value))
            {
                return value as TItem;
            }

            return null;
        }

        /// <summary>
        /// 获取所有已注册的实例
        /// </summary>
        public IEnumerable<T> GetAll() => _instances.Values;

        /// <summary>
        /// 清空所有已注册实例
        /// </summary>
        public void Clear() => _instances.Clear();
    }
}
