namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 可自动注销的订阅句柄。调用 <see cref="Dispose" /> 注销订阅。
    /// </summary>
    public interface IUnsubscribe
    {
        void Dispose();
    }
}
