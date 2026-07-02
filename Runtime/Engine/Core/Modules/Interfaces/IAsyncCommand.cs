using System.Threading.Tasks;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 异步命令接口。通过 Async Command 执行包含网络请求、文件 IO 等异步操作的写逻辑，无返回值。
    /// <para>
    /// 与 <see cref="ICommand" /> 的区别：Command 同步执行，Async Command 返回 Task 可 await。
    /// </para>
    /// <para>
    /// 能力：GetModel, GetService, ExecuteCommand, Invoke
    /// </para>
    /// </summary>
    public interface IAsyncCommand : IContextHolder, ICanSetContext, ICanGetModel, ICanGetService,
        ICanInvokeEvent, ICanExecuteCommand
    {
        /// <summary>
        /// 异步执行命令
        /// </summary>
        Task ExecuteAsync();
    }
}
