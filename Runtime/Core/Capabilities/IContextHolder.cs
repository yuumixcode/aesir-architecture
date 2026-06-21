namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 上下文持有者接口。
    /// </summary>
    public interface IContextHolder
    {
        /// <summary>
        /// 获取持有的模块上下文
        /// </summary>
        IContext GetContext();
    }
}
