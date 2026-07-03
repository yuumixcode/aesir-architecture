namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 服务层接口。封装跨模块业务逻辑，协调模块间交互与通信。
    /// <para>
    /// IService 是主动式的——被外部调用执行跨模块的协调逻辑，通过 <see cref="ICanGetModel" /> 访问数据，
    /// 通过 <see cref="ICanInvokeEvent" /> 发布业务事件。
    /// </para>
    /// <para>
    /// 能力：GetModel, GetService, Invoke, AddListener, Initialize, Dispose
    /// </para>
    /// </summary>
    public interface IService : IContextHolder, ICanSetContext, ICanGetModel, ICanGetService, ICanInvokeEvent,
        ICanAddListener, ICanInitialize
    {
    }
}
