using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 表现层 MVP 中介接口。Presenter 层彻底区分 Model 与 View，
    /// 作为两者间的唯一通信桥梁：从 Model 读取数据并格式化后推送给 View，处理 View 事件并转发给 Model。
    /// <para>
    /// 与 <see cref="IController" /> 的区别：Presenter 额外具备 <see cref="ICanInvokeWithContext" /> 能力，
    /// 可主动向 View 推送变更；同时实现 <see cref="IDisposable" /> 支持显式释放。
    /// </para>
    /// <para>
    /// 能力：GetModel, GetSystem, Subscribe, Invoke, ExecuteCommand, ExecuteQuery
    /// </para>
    /// </summary>
    public interface IPresenter : IContextHolder, ICanExecuteCommand, ICanExecuteQuery, ICanGetModel,
        ICanGetSystem, ICanSubscribeWithContext, ICanInvokeWithContext, IDisposable { }

    public interface IPresenter<T> : IPresenter where T : Context<T>, new()
    {
        IContext IContextHolder.GetContext() => Context<T>.Interface;
    }
}
