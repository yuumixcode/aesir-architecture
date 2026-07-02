using System;
using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 服务层接口。封装跨模块业务逻辑，协调模块间交互与通信。
    /// <para>
    /// IService 是主动式的——被外部调用执行跨模块的协调逻辑，通过 <see cref="ICanGetModel" /> 访问数据，
    /// 通过 <see cref="ICanExecuteCommand" /> 委托复杂操作，通过 <see cref="ICanInvokeEvent" /> 发布业务事件。
    /// 不包含 <see cref="ICanAddListener" />，因为服务是主动调用的，不被动监听事件。
    /// </para>
    /// <para>
    /// 能力：GetModel, GetService, ExecuteCommand, Invoke, Initialize, Dispose
    /// </para>
    /// </summary>
    public interface IService : IContextHolder, ICanSetContext, ICanGetModel, ICanGetService, ICanInvokeEvent,
        ICanAddListener, ICanInitialize
    {
        /// <summary>
        /// 获取依赖模块列表。依赖项必须是 <see cref="IModel" /> 或 <see cref="IService" /> 的子类。
        /// </summary>
        HashSet<Type> GetDependencies();
    }
}
