using System.Threading.Tasks;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 命令执行能力扩展方法
    /// </summary>
    public static class CanExecuteCommandExtensions
    {
        /// <summary>
        /// 执行命令
        /// </summary>
        public static void ExecuteCommand<T>(this ICanExecuteCommand self, T command) where T : ICommand
        {
            command.SetContext(self.GetContext());
            command.Execute();
        }

        /// <summary>
        /// 执行无参命令
        /// </summary>
        public static void ExecuteCommand<T>(this ICanExecuteCommand self) where T : ICommand, new()
        {
            var command = new T();
            command.SetContext(self.GetContext());
            command.Execute();
        }

        /// <summary>
        /// 异步执行无参命令
        /// </summary>
        public static async Task ExecuteCommandAsync<T>(this ICanExecuteCommand self) where T : IAsyncCommand, new()
        {
            var command = new T();
            command.SetContext(self.GetContext());
            await command.ExecuteAsync();
        }

        /// <summary>
        /// 异步执行命令
        /// </summary>
        public static async Task ExecuteCommandAsync<T>(this ICanExecuteCommand self, T command) where T : IAsyncCommand
        {
            command.SetContext(self.GetContext());
            await command.ExecuteAsync();
        }
    }
}
