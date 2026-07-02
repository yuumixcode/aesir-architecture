using System;
using System.Collections.Generic;
using System.Linq;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 依赖项校验辅助类。提供 Model 和 Service 的依赖类型检查与初始化状态检查。
    /// </summary>
    internal static class ContextDependencyAssistant
    {
        /// <summary>
        /// 校验 Model 的依赖类型是否合法（必须为 IModel 子类）。
        /// </summary>
        public static void ValidateModelDependencyTypes<TModel>(HashSet<Type> dependencies)
        {
            if (dependencies == null)
            {
                return;
            }

            foreach (var depType in dependencies.Where(depType => !typeof(IModel).IsAssignableFrom(depType)))
            {
                throw new InvalidOperationException(
                    $"{AesirArchitectureLog.ErrorTag} Model [{typeof(TModel).Name}] 声明了非法依赖项 [{depType.Name}]：Model 模块只能依赖 IModel 的子类。");
            }
        }

        /// <summary>
        /// 校验 Service 的依赖类型是否合法（必须为 IModel 或 IService 子类）。
        /// </summary>
        public static void ValidateServiceDependencyTypes<TService>(HashSet<Type> dependencies)
        {
            if (dependencies == null)
            {
                return;
            }

            foreach (var depType in dependencies.Where(depType =>
                         !typeof(IModel).IsAssignableFrom(depType) &&
                         !typeof(IService).IsAssignableFrom(depType)))
            {
                throw new InvalidOperationException(
                    $"{AesirArchitectureLog.ErrorTag} Service [{typeof(TService).Name}] 声明了非法依赖项 [{depType.Name}]：Service 模块只能依赖 IModel 或 IService 的子类。");
            }
        }

        /// <summary>
        /// 检查 Model 的依赖项是否已全部注册且初始化。未注册或未初始化则抛出 InvalidOperationException。
        /// </summary>
        public static void CheckModelDependencies(Type moduleType,
            HashSet<Type> dependencies,
            IGenericLocator<IModel> container)
        {
            if (dependencies == null)
            {
                return;
            }

            foreach (var depType in dependencies)
            {
                var dep = container.GetByType(depType);
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
        /// 检查 Service 的依赖项是否已全部注册且初始化。未注册或未初始化则抛出 InvalidOperationException。
        /// </summary>
        public static void CheckServiceDependencies(Type moduleType,
            HashSet<Type> dependencies,
            IGenericLocator<IModel> modelContainer,
            IGenericLocator<IService> serviceContainer)
        {
            if (dependencies == null)
            {
                return;
            }

            foreach (var depType in dependencies)
            {
                if (typeof(IModel).IsAssignableFrom(depType))
                {
                    var modelDep = modelContainer.GetByType(depType);
                    if (modelDep == null)
                    {
                        throw new InvalidOperationException(
                            $"{AesirArchitectureLog.ErrorTag} Service [{moduleType.Name}] 依赖的 [{depType.Name}] 未在容器中注册。");
                    }

                    if (!modelDep.Initialized)
                    {
                        throw new InvalidOperationException(
                            $"{AesirArchitectureLog.ErrorTag} Service [{moduleType.Name}] 依赖的 [{depType.Name}] 尚未初始化，请调整注册顺序确保被依赖项先注册。");
                    }
                }
                else
                {
                    var serviceDep = serviceContainer.GetByType(depType);
                    if (serviceDep == null)
                    {
                        throw new InvalidOperationException(
                            $"{AesirArchitectureLog.ErrorTag} Service [{moduleType.Name}] 依赖的 [{depType.Name}] 未在容器中注册。");
                    }

                    if (!serviceDep.Initialized)
                    {
                        throw new InvalidOperationException(
                            $"{AesirArchitectureLog.ErrorTag} Service [{moduleType.Name}] 依赖的 [{depType.Name}] 尚未初始化，请调整注册顺序确保被依赖项先注册。");
                    }
                }
            }
        }
    }
}
