using System;
using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// System 基类。持有上下文引用，通过 <see cref="OnInitialize" /> 和 <see cref="OnDispose" /> 管理生命周期。
    /// </summary>
    public abstract class AbstractSystem : ISystem
    {
        IContext _context;
        /// <summary>
        /// 获取当前持有的上下文
        /// </summary>
        public IContext GetContext() => _context;

        void ICanSetContext.SetContext(IContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 释放资源，触发 <see cref="OnDispose" />
        /// </summary>
        public void Dispose()
        {
            OnDispose();
            _context = null;
        }

        /// <summary>
        /// 是否已初始化（只读）
        /// </summary>
        public bool Initialized { get; private set; }

        void ICanInitialize.Initialize()
        {
            OnInitialize();
            Initialized = true;
        }

        /// <summary>
        /// 获取依赖模块列表，子类可覆写。依赖项必须是 <see cref="IModel" /> 或 <see cref="ISystem" /> 的子类。
        /// </summary>
        public virtual HashSet<Type> GetDependencies() => null;

        /// <summary>
        /// 初始化逻辑，子类必须实现
        /// </summary>
        protected abstract void OnInitialize();

        /// <summary>
        /// 释放时的清理逻辑，子类必须实现
        /// </summary>
        protected abstract void OnDispose();
    }
}
