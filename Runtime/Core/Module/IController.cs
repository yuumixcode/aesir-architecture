namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 表现层控制器接口。Controller 层可通过此接口执行命令、查询数据、获取 Model 和 System、订阅事件。
    /// <para>
    /// 能力：GetModel, GetSystem, Subscribe, ExecuteCommand, ExecuteQuery
    /// </para>
    /// </summary>
    public interface IController : IContextHolder, ICanExecuteCommand, ICanExecuteQuery, ICanGetModel,
        ICanGetSystem, ICanSubscribeWithContext { }

    public interface IController<T> : IController where T : Context<T>, new()
    {
        IContext IContextHolder.GetContext() => Context<T>.Interface;
    }
}
