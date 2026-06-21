namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 计数器模型接口
    /// </summary>
    public interface ICounterModel : IModel
    {
        /// <summary>
        /// 当前计数值（只读可观察属性）
        /// </summary>
        IReadOnlyObservableProperty<int> Count { get; }

        /// <summary>
        /// 计数+1
        /// </summary>
        void Increase();

        /// <summary>
        /// 计数-1
        /// </summary>
        void Decrease();

        /// <summary>
        /// 重置为0
        /// </summary>
        void Reset();
    }
}
