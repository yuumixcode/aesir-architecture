using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 表现层 MVP 中介接口。Presenter 层彻底区分 Model 与 View，
    /// 作为两者间的唯一通信桥梁：从 Model 读取数据并格式化后推送给 View，处理 View 事件并转发给 Model。
    /// <para>
    /// 与 <see cref="IController" /> 的区别：Controller 混合了 Model 和 View 的操作职责，
    /// Presenter 则通过增加 <see cref="ICanInvokeWithContext" /> 能力主动向 View 推送变更，
    /// 同时保留 <see cref="ICanSubscribeWithContext" /> 监听 Model 事件，实现双向中介。
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
