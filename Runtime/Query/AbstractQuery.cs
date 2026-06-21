using System.Threading.Tasks;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 查询基类。持有上下文引用，通过 <see cref="OnExecute" /> 执行查询逻辑并返回结果。
    /// </summary>
    public abstract class AbstractQuery<TResult> : IQuery<TResult>
    {
        IContext _context;

        IContext IContextHolder.GetContext() => _context;

        void ICanSetContext.SetContext(IContext context) => _context = context;

        TResult IQuery<TResult>.Execute() => OnExecute();

        protected abstract TResult OnExecute();
    }

    /// <summary>
    /// 异步查询基类。持有上下文引用，通过 <see cref="OnExecuteAsync" /> 执行异步查询逻辑并返回结果。
    /// </summary>
    public abstract class AbstractAsyncQuery<TResult> : IAsyncQuery<TResult>
    {
        IContext _context;

        IContext IContextHolder.GetContext() => _context;

        void ICanSetContext.SetContext(IContext context) => _context = context;

        Task<TResult> IAsyncQuery<TResult>.ExecuteAsync() => OnExecuteAsync();

        protected abstract Task<TResult> OnExecuteAsync();
    }
}
