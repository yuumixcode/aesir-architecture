using System;
using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 类型键控实例容器。按类型注册、查询与解析实例，供上下文管理模块生命周期。
    /// <para>使用 <see cref="Dictionary{TKey, TValue}" /> 保证 O(1) 查询，使用 <see cref="List{T}" /> 保证注册顺序遍历。</para>
    /// </summary>
    public sealed class Container<T> where T : class
    {
        readonly Dictionary<Type, T> _instances = new Dictionary<Type, T>();
        readonly List<T> _orderedInstances = new List<T>();

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
            _orderedInstances.Add(instance);
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
        /// 按 <see cref="Type" /> 获取已注册的实例，不存在则返回 null。
        /// 用于依赖项校验等需要运行时 Type 查询的场景。
        /// </summary>
        public T GetByType(Type type) => _instances.GetValueOrDefault(type);

        /// <summary>
        /// 按注册顺序获取所有已注册的实例
        /// </summary>
        public IEnumerable<T> GetAll() => _orderedInstances;

        /// <summary>
        /// 清空所有已注册实例
        /// </summary>
        public void Clear()
        {
            _instances.Clear();
            _orderedInstances.Clear();
        }
    }
}
