namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 同步命令接口。通过 Command 修改 Model 状态，只写无返回值。
    /// <para>
    /// 读操作请使用 <see cref="IQuery{TResult}" />。
    /// </para>
    /// <para>
    /// 能力：GetModel, GetSystem, ExecuteCommand, Invoke, ExecuteQuery
    /// </para>
    /// </summary>
    public interface ICommand : IContextHolder, ICanSetContext, ICanGetModel, ICanGetSystem,
        ICanInvokeWithContext, ICanExecuteCommand, ICanExecuteQuery
    {
        /// <summary>
        /// 执行命令
        /// </summary>
        void Execute();
    }
}
