using System;
using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    public abstract class AbstractSystem : ISystem
    {
        IContext _context;
        public IContext GetContext() => _context;

        void ICanSetContext.SetContext(IContext context)
        {
            _context = context;
        }

        public void Dispose()
        {
            OnDispose();
            _context = null;
        }

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

        protected abstract void OnInitialize();

        protected abstract void OnDispose();
    }
}
