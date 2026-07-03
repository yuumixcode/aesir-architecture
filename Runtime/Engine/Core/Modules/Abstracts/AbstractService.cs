using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// Service 基类。持有上下文引用，通过 <see cref="OnInitialize" /> 和 <see cref="OnDispose" /> 管理生命周期。
    /// </summary>
    [Serializable]
    public abstract class AbstractService : IService
    {
        IContext _context;
        IContext IContextHolder.Context => _context;
        void ICanSetContext.SetContext(IContext context) => _context = context;

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
        /// 释放资源，触发 <see cref="OnDispose" />
        /// </summary>
        public void Dispose()
        {
            OnDispose();
            _context = null;
        }

        /// <summary>
        /// 初始化逻辑，子类必须实现
        /// </summary>
        protected abstract void OnInitialize();

        /// <summary>
        /// 释放时的清理逻辑，子类可覆写
        /// </summary>
        protected virtual void OnDispose() { }
    }
}
