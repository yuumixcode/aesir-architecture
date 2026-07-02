using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 非泛型对象定位器。按类型注册、查询与获取任意对象实例。
    /// <remarks>
    /// 纯 C# 实现，可作为独立实例使用，也可通过 <see cref="Global" /> 访问全局单例。
    /// </remarks>
    /// </summary>
    [Serializable]
    public sealed class Locator : ILocator, IDisposable
    {
        static Locator _global;
        readonly Dictionary<Type, object> _registry = new Dictionary<Type, object>();

        /// <summary>
        /// 获取全局定位器实例。首次访问时懒初始化。
        /// </summary>
        public static Locator Global
        {
            get
            {
                _global ??= new Locator();
                return _global;
            }
        }

        /// <summary>
        /// 释放资源，清空所有注册。若当前实例为全局实例，则同时清除全局引用。
        /// </summary>
        public void Dispose()
        {
            Clear();

            if (ReferenceEquals(_global, this))
            {
                _global = null;
            }
        }

        /// <summary>
        /// 注册一个实例。如果类型已存在，则覆盖原有注册。
        /// </summary>
        public void Register<T>(T instance) where T : class
        {
            var key = typeof(T);

            _registry[key] = instance;
        }

        /// <summary>
        /// 按显式指定的类型注册一个实例
        /// </summary>
        public void Register(Type type, object instance)
        {
            if (!type.IsInstanceOfType(instance))
            {
                throw new ArgumentException($"实例类型与 {type.Name} 不匹配", nameof(instance));
            }

            _registry[type] = instance;
        }

        /// <summary>
        /// 获取指定类型的实例。如果不存在，返回 null。
        /// </summary>
        public T Get<T>() where T : class
        {
            if (_registry.TryGetValue(typeof(T), out var value))
            {
                return value as T;
            }

            return null;
        }

        /// <summary>
        /// 尝试获取指定类型的实例
        /// </summary>
        public bool TryGet<T>(out T instance) where T : class
        {
            if (_registry.TryGetValue(typeof(T), out var value))
            {
                instance = value as T;
                return true;
            }

            instance = null;
            return false;
        }

        /// <summary>
        /// 检查是否已注册指定类型的实例
        /// </summary>
        public bool IsRegistered<T>() where T : class => _registry.ContainsKey(typeof(T));

        /// <summary>
        /// 注销指定类型的实例
        /// </summary>
        public void Unregister<T>() where T : class
        {
            var key = typeof(T);

            _registry.Remove(key);
        }

        /// <summary>
        /// 清空所有已注册的实例
        /// </summary>
        public void Clear()
        {
            _registry.Clear();
        }

        /// <summary>
        /// 按 Type 获取实例（非泛型版本）
        /// </summary>
        public object GetByType(Type type) =>
            _registry.GetValueOrDefault(type);

        /// <summary>
        /// 获取所有已注册的实例集合
        /// </summary>
        public IEnumerable<object> GetAll() => _registry.Values;

        /// <summary>
        /// 获取底层注册表的只读副本
        /// </summary>
        public IReadOnlyDictionary<Type, object> GetRegistry() => _registry;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStaticsOnDisableDomainReload()
        {
            _global = null;
        }
    }
}
