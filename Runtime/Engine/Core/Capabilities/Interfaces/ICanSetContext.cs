namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 可设置上下文引用接口
    /// </summary>
    public interface ICanSetContext
    {
        /// <summary>
        /// 设置上下文引用
        /// </summary>
        void SetContext(IContext context);
    }
}
