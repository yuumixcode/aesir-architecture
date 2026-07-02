using System.Threading.Tasks;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 异步命令基类。持有上下文引用，通过 <see cref="OnExecuteAsync" /> 执行异步命令逻辑。
    /// </summary>
    public abstract class AbstractAsyncCommand : IAsyncCommand
    {
        IContext _context;
        IContext IContextHolder.Context => _context;
        void ICanSetContext.SetContext(IContext context) => _context = context;
        Task IAsyncCommand.ExecuteAsync() => OnExecuteAsync();
        protected abstract Task OnExecuteAsync();
    }
}
