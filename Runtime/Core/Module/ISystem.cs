namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 系统层接口。组织多个 Controller，处理 Controller 之间的交互与通信。
    /// <para>
    /// ISystem 是主动式的——被外部调用执行跨 Controller 的协调逻辑，通过 <see cref="ICanGetModel" /> 访问数据，
    /// 通过 <see cref="ICanExecuteCommand" /> 委托复杂操作，通过 <see cref="ICanInvokeWithContext" /> 发布业务事件。
    /// 不包含 <see cref="ICanSubscribeWithContext" />，因为系统是主动调用的，不被动监听事件。
    /// </para>
    /// <para>
    /// 能力：GetModel, GetSystem, ExecuteCommand, Invoke, ExecuteQuery, Initialize, Dispose
    /// </para>
    /// </summary>
    public interface ISystem : IContextHolder, ICanSetContext, ICanGetModel, ICanGetSystem,
        ICanExecuteCommand, ICanExecuteQuery, ICanInvokeWithContext, ICanInitialize { }
}
