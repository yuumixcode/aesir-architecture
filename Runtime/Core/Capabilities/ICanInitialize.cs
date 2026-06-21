using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 可初始化接口。提供初始化与初始化状态标记。
    /// <para>
    /// 被 <see cref="IModel" /> 和 <see cref="ISystem" /> 继承。
    /// </para>
    /// </summary>
    public interface ICanInitialize : IDisposable
    {
        /// <summary>
        /// 是否已初始化（只读）
        /// </summary>
        bool Initialized { get; }

        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize();
    }
}
