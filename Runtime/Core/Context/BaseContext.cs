using System;
using System.Collections.Generic;
using System.Linq;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 上下文实例逻辑基类。提取 <see cref="Context{T}" /> 的实例级行为，
    /// 供 <see cref="Context{T}" />（生产环境单例）与 <see cref="MockContext" />（测试隔离）共用。
    /// </summary>
    public abstract class BaseContext : IContext
    {
        readonly Container<IModel> _modelContainer = new Container<IModel>();
        readonly Container<ISystem> _systemContainer = new Container<ISystem>();

        /// <summary>
        /// 上下文共享的事件总线
        /// </summary>
        public MiniEventBus EventBus { get; } = new MiniEventBus();

        /// <summary>
        /// 是否已初始化（只读）
        /// </summary>
        public bool Initialized { get; private set; }

        /// <summary>
        /// 注册 Model 并绑定上下文。
        /// </summary>
        public void RegisterModel<TModel>(TModel model) where TModel : class, IModel
        {
            var dependencies = model.GetDependencies();
            if (dependencies != null)
            {
                foreach (var depType in dependencies.Where(depType =>
                             !typeof(IModel).IsAssignableFrom(depType)))
                {
                    throw new InvalidOperationException(
                        $"{AesirArchitectureLog.ErrorTag} Model [{typeof(TModel).Name}] 声明了非法依赖项 [{depType.Name}]：Model 模块只能依赖 IModel 的子类。");
                }
            }

            model.SetContext(this);
            _modelContainer.Register(model);

            if (!Initialized)
            {
                return;
            }

            CheckModelDependenciesInitialized(typeof(TModel), model.GetDependencies());
            model.Initialize();
        }

        /// <summary>
        /// 获取已注册的 Model。
        /// </summary>
        public TModel GetModel<TModel>() where TModel : class, IModel => _modelContainer.Get<TModel>();

        /// <summary>
        /// 注册 System 并绑定上下文。
        /// <para>校验依赖集合（仅允许 IModel 和 ISystem）后设置上下文并注册。若上下文已完成统一初始化，检查依赖项是否全部已初始化并立即初始化。</para>
        /// </summary>
        public void RegisterSystem<TSystem>(TSystem system) where TSystem : class, ISystem
        {
            var dependencies = system.GetDependencies();
            if (dependencies != null)
            {
                foreach (var depType in dependencies.Where(depType =>
                             !typeof(IModel).IsAssignableFrom(depType) &&
                             !typeof(ISystem).IsAssignableFrom(depType)))
                {
                    throw new InvalidOperationException(
                        $"{AesirArchitectureLog.ErrorTag} System [{typeof(TSystem).Name}] 声明了非法依赖项 [{depType.Name}]：System 模块只能依赖 IModel 或 ISystem 的子类。");
                }
            }

            system.SetContext(this);
            _systemContainer.Register(system);

            if (!Initialized)
            {
                return;
            }

            CheckSystemDependenciesInitialized(typeof(TSystem), system.GetDependencies());
            system.Initialize();
        }

        /// <summary>
        /// 获取已注册的 System。
        /// </summary>
        public TSystem GetSystem<TSystem>() where TSystem : class, ISystem => _systemContainer.Get<TSystem>();

        /// <summary>
        /// 释放资源。逆序销毁 System 和 Model，清空容器与事件总线。
        /// </summary>
        public virtual void Dispose()
        {
            if (!Initialized)
            {
                return;
            }

            OnDispose();

            foreach (var system in _systemContainer.GetAll())
            {
                system.Dispose();
            }

            foreach (var model in _modelContainer.GetAll())
            {
                model.Dispose();
            }

            EventBus.Dispose();
            _modelContainer.Clear();
            _systemContainer.Clear();
            Initialized = false;
        }

        /// <summary>
        /// 统一初始化。调用 <see cref="Configure" /> 注册模块后，按注册顺序依次初始化 Model 和 System。
        /// <para>开发者需保证注册顺序满足依赖关系——被依赖的模块先注册。初始化每个模块前检查其依赖项是否已初始化，未初始化则直接报错。</para>
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
                CheckModelDependenciesInitialized(model.GetType(), model.GetDependencies());
                model.Initialize();
            }

            foreach (var system in _systemContainer.GetAll())
            {
                CheckSystemDependenciesInitialized(system.GetType(), system.GetDependencies());
                system.Initialize();
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

        /// <summary>
        /// 检查 Model 的依赖项是否已全部注册且初始化。未注册或未初始化则抛出 InvalidOperationException。
        /// </summary>
        void CheckModelDependenciesInitialized(Type moduleType, HashSet<Type> dependencies)
        {
            if (dependencies == null)
            {
                return;
            }

            foreach (var depType in dependencies)
            {
                var dep = _modelContainer.GetByType(depType);
                if (dep == null)
                {
                    throw new InvalidOperationException(
                        $"{AesirArchitectureLog.ErrorTag} Model [{moduleType.Name}] 依赖的 [{depType.Name}] 未在容器中注册。");
                }

                if (!dep.Initialized)
                {
                    throw new InvalidOperationException(
                        $"{AesirArchitectureLog.ErrorTag} Model [{moduleType.Name}] 依赖的 [{depType.Name}] 尚未初始化，请调整注册顺序确保被依赖项先注册。");
                }
            }
        }

        /// <summary>
        /// 检查 System 的依赖项是否已全部注册且初始化。未注册或未初始化则抛出 InvalidOperationException。
        /// </summary>
        void CheckSystemDependenciesInitialized(Type moduleType, HashSet<Type> dependencies)
        {
            if (dependencies == null)
            {
                return;
            }

            foreach (var depType in dependencies)
            {
                if (typeof(IModel).IsAssignableFrom(depType))
                {
                    var modelDep = _modelContainer.GetByType(depType);
                    if (modelDep == null)
                    {
                        throw new InvalidOperationException(
                            $"{AesirArchitectureLog.ErrorTag} System [{moduleType.Name}] 依赖的 [{depType.Name}] 未在容器中注册。");
                    }

                    if (!modelDep.Initialized)
                    {
                        throw new InvalidOperationException(
                            $"{AesirArchitectureLog.ErrorTag} System [{moduleType.Name}] 依赖的 [{depType.Name}] 尚未初始化，请调整注册顺序确保被依赖项先注册。");
                    }
                }
                else
                {
                    var systemDep = _systemContainer.GetByType(depType);
                    if (systemDep == null)
                    {
                        throw new InvalidOperationException(
                            $"{AesirArchitectureLog.ErrorTag} System [{moduleType.Name}] 依赖的 [{depType.Name}] 未在容器中注册。");
                    }

                    if (!systemDep.Initialized)
                    {
                        throw new InvalidOperationException(
                            $"{AesirArchitectureLog.ErrorTag} System [{moduleType.Name}] 依赖的 [{depType.Name}] 尚未初始化，请调整注册顺序确保被依赖项先注册。");
                    }
                }
            }
        }
    }
}
