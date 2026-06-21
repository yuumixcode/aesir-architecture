using System;
using System.Collections.Generic;
using System.Linq;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 上下文实例逻辑基类。提取 <see cref="Context{T}" /> 的实例级行为，
    /// 供 <see cref="Context{T}" />（生产环境单例）与 <see cref="MockContext" />（测试隔离）共用。
    /// </summary>
    public abstract class ContextBase : IContext
    {
        readonly Container<IModel> _modelContainer = new Container<IModel>();
        readonly Container<ISystem> _systemContainer = new Container<ISystem>();
        readonly HashSet<IModel> _initializingModels = new HashSet<IModel>();
        readonly HashSet<ISystem> _initializingSystems = new HashSet<ISystem>();

        /// <summary>
        /// 上下文共享的事件总线
        /// </summary>
        public MiniEventHub EventHub { get; } = new MiniEventHub();

        /// <summary>
        /// 是否已初始化（只读）
        /// </summary>
        public bool Initialized { get; private set; }

        /// <summary>
        /// 注册 Model 并绑定上下文。若上下文已初始化则立即初始化该 Model。
        /// </summary>
        public void RegisterModel<TModel>(TModel model) where TModel : class, IModel
        {
            model.SetContext(this);
            _modelContainer.Register(model);

            if (!Initialized)
            {
                return;
            }

            EnsureModelInitialized(model);
        }

        /// <summary>
        /// 获取已注册的 Model。若该 Model 尚未初始化，先递归触发初始化（按需初始化）。
        /// </summary>
        public TModel GetModel<TModel>() where TModel : class, IModel
        {
            var model = _modelContainer.Get<TModel>();

            if (model != null && !model.Initialized)
            {
                EnsureModelInitialized(model);
            }

            return model;
        }

        /// <summary>
        /// 注册 System 并绑定上下文。若上下文已初始化则立即初始化该 System。
        /// </summary>
        public void RegisterSystem<TSystem>(TSystem system) where TSystem : class, ISystem
        {
            system.SetContext(this);
            _systemContainer.Register(system);

            if (!Initialized)
            {
                return;
            }

            EnsureSystemInitialized(system);
        }

        /// <summary>
        /// 获取已注册的 System。若该 System 尚未初始化，先触发初始化（按需初始化）。
        /// </summary>
        public TSystem GetSystem<TSystem>() where TSystem : class, ISystem
        {
            var system = _systemContainer.Get<TSystem>();

            if (system != null && !system.Initialized)
            {
                EnsureSystemInitialized(system);
            }

            return system;
        }

        /// <summary>
        /// 释放资源。逆序销毁 Model 和 System，清空容器。
        /// </summary>
        public virtual void Dispose()
        {
            if (!Initialized)
            {
                return;
            }

            OnDispose();
            DisposeSystems();
            DisposeModels();
            ClearEventBus();
            ClearContainers();
            Initialized = false;
            AesirArchitectureLog.Log("Context", "模块上下文已释放");
        }

        /// <summary>
        /// 初始化上下文。调用 <see cref="Configure" /> 注册模块，随后初始化所有已注册的 Model 和 System。
        /// </summary>
        protected void Initialize()
        {
            if (Initialized)
            {
                return;
            }

            Configure();

            foreach (var model in _modelContainer.GetAll())
            {
                EnsureModelInitialized(model);
            }

            foreach (var system in _systemContainer.GetAll())
            {
                EnsureSystemInitialized(system);
            }

            Initialized = true;
        }

        /// <summary>
        /// 配置上下文模块，子类在此注册 Model 和 System。
        /// </summary>
        protected abstract void Configure();

        /// <summary>
        /// 子类可选覆写，在释放前执行自定义清理
        /// </summary>
        protected virtual void OnDispose() { }

        void EnsureModelInitialized(IModel model)
        {
            if (model.Initialized)
            {
                return;
            }

            if (!_initializingModels.Add(model))
            {
                throw new InvalidOperationException(
                    $"检测到 Model 循环依赖：{model.GetType().Name} 正在初始化中，无法再次初始化。");
            }

            try
            {
                model.Initialize();
            }
            finally
            {
                _initializingModels.Remove(model);
            }
        }

        void EnsureSystemInitialized(ISystem system)
        {
            if (system.Initialized)
            {
                return;
            }

            if (!_initializingSystems.Add(system))
            {
                throw new InvalidOperationException(
                    $"检测到 System 循环依赖：{system.GetType().Name} 正在初始化中，无法再次初始化。");
            }

            try
            {
                system.Initialize();
            }
            finally
            {
                _initializingSystems.Remove(system);
            }
        }

        void DisposeModels()
        {
            foreach (var model in _modelContainer.GetAll().Where(m => m.Initialized))
            {
                model.Dispose();
            }
        }

        void DisposeSystems()
        {
            foreach (var system in _systemContainer.GetAll().Where(s => s.Initialized))
            {
                system.Dispose();
            }
        }

        void ClearEventBus() => EventHub.Clear();

        void ClearContainers()
        {
            _modelContainer.Clear();
            _systemContainer.Clear();
        }
    }
}
