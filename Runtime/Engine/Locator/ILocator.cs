using System;
using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    public interface ILocator
    {
        /// <summary>
        /// 注册实例，以 <c>typeof(T)</c> 作为键。重复注册将覆盖已有实例。
        /// <remarks>
        /// 注意：注册与查询必须使用相同的类型参数。若以具体类型注册（如 <c>Register&lt;Sword&gt;</c>），
        /// 再以接口类型查询（如 <c>Get&lt;IWeapon&gt;</c>），将返回 <c>null</c>。
        /// </remarks>
        /// </summary>
        void Register<T>(T instance) where T : class;

        /// <summary>
        /// 注册实例，以 <see cref="Type" /> 作为键。重复注册将覆盖已有实例。
        /// </summary>
        void Register(Type type, object instance);

        /// <summary>
        /// 获取已注册的实例，不存在则返回 null。
        /// </summary>
        T Get<T>() where T : class;

        /// <summary>
        /// 尝试获取已注册的实例。返回是否成功找到对应类型的注册。
        /// </summary>
        bool TryGet<T>(out T instance) where T : class;

        /// <summary>
        /// 判断指定类型是否已注册。
        /// </summary>
        bool IsRegistered<T>() where T : class;

        /// <summary>
        /// 注销指定类型的注册。
        /// </summary>
        void Unregister<T>() where T : class;

        /// <summary>
        /// 清空所有已注册的实例。
        /// </summary>
        void Clear();

        /// <summary>
        /// 按 <see cref="Type" /> 获取已注册的实例，不存在则返回 null。
        /// <para>用于依赖项校验等需要运行时 Type 查询的场景。</para>
        /// </summary>
        object GetByType(Type type);

        /// <summary>
        /// 按注册顺序获取所有已注册的实例。
        /// </summary>
        IEnumerable<object> GetAll();

        /// <summary>
        /// 获取注册表字典的只读视图。
        /// </summary>
        IReadOnlyDictionary<Type, object> GetRegistry();
    }
}
