namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 计数器模型实现
    /// </summary>
    public sealed class CounterModel : AbstractModel, ICounterModel
    {
        readonly ObservableProperty<int> _count = new ObservableProperty<int>(0);

        /// <summary>
        /// 当前计数值（只读可观察属性）
        /// </summary>
        public IReadOnlyObservableProperty<int> Count => _count;

        /// <summary>
        /// 计数+1并发布变更事件
        /// </summary>
        public void Increase()
        {
            _count.Value++;
        }

        /// <summary>
        /// 计数-1并发布变更事件
        /// </summary>
        public void Decrease()
        {
            _count.Value--;
        }

        /// <summary>
        /// 重置为0并发布变更事件
        /// </summary>
        public void Reset()
        {
            _count.Value = 0;
        }

        /// <summary>
        /// 初始化逻辑
        /// </summary>
        protected override void OnInitialize() { }
    }
}
