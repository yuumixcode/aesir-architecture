using System.Threading.Tasks;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 查询执行能力扩展方法
    /// </summary>
    public static class CanExecuteQueryExtensions
    {
        /// <summary>
        /// 执行无参查询
        /// </summary>
        public static TResult ExecuteQuery<T, TResult>(this ICanExecuteQuery self) where T : IQuery<TResult>, new()
        {
            var query = new T();
            query.SetContext(self.GetContext());
            return query.Execute();
        }

        /// <summary>
        /// 执行带参查询
        /// </summary>
        public static TResult ExecuteQuery<TResult>(this ICanExecuteQuery self, IQuery<TResult> query)
        {
            query.SetContext(self.GetContext());
            return query.Execute();
        }

        /// <summary>
        /// 异步执行无参查询
        /// </summary>
        public static async Task<TResult> ExecuteQueryAsync<T, TResult>(this ICanExecuteQuery self)
            where T : IAsyncQuery<TResult>, new()
        {
            var query = new T();
            query.SetContext(self.GetContext());
            return await query.ExecuteAsync();
        }

        /// <summary>
        /// 异步执行带参查询
        /// </summary>
        public static async Task<TResult> ExecuteQueryAsync<TResult>(this ICanExecuteQuery self,
            IAsyncQuery<TResult> query)
        {
            query.SetContext(self.GetContext());
            return await query.ExecuteAsync();
        }
    }
}
