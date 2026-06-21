using System.Threading.Tasks;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 查询接口。通过 Query 读取 Model 状态，只读有返回值，不修改状态。
    /// <para>
    /// 与 <see cref="ICommand" /> 的区别：Command 只写无返回值，Query 只读有返回值。
    /// </para>
    /// <para>
    /// 能力：GetModel, GetSystem, ExecuteQuery
    /// </para>
    /// </summary>
    public interface IQuery<TResult> : IContextHolder, ICanSetContext, ICanGetModel, ICanGetSystem,
        ICanExecuteQuery
    {
        /// <summary>
        /// 执行查询并返回结果
        /// </summary>
        TResult Execute();
    }

    /// <summary>
    /// 异步查询接口。通过 Async Query 执行异步读操作并返回结果。
    /// <para>
    /// 与 <see cref="IQuery{TResult}" /> 的区别：Query 同步执行，Async Query 返回 Task 可 await。
    /// </para>
    /// <para>
    /// 能力：GetModel, GetSystem, ExecuteQuery
    /// </para>
    /// </summary>
    public interface IAsyncQuery<TResult> : IContextHolder, ICanSetContext, ICanGetModel, ICanGetSystem,
        ICanExecuteQuery
    {
        /// <summary>
        /// 异步执行查询并返回结果
        /// </summary>
        Task<TResult> ExecuteAsync();
    }
}
