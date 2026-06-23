using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 模块上下文接口。提供模块注册与获取。
    /// </summary>
    public interface IContext : IDisposable
    {
        /// <summary>
        /// 上下文是否已初始化
        /// </summary>
        bool Initialized { get; }

        /// <summary>
        /// 上下文共享的事件总线。所有 ICanInvokeWithContext / ICanSubscribeWithContext 持有者通过此总线收发事件。
        /// </summary>
        MiniEventBus EventBus { get; }

        /// <summary>
        /// 注册 Model
        /// </summary>
        void RegisterModel<T>(T model) where T : class, IModel;

        /// <summary>
        /// 获取已注册的 Model
        /// </summary>
        T GetModel<T>() where T : class, IModel;

        /// <summary>
        /// 注册 System
        /// </summary>
        void RegisterSystem<T>(T system) where T : class, ISystem;

        /// <summary>
        /// 获取已注册的 System
        /// </summary>
        T GetSystem<T>() where T : class, ISystem;
    }
}
