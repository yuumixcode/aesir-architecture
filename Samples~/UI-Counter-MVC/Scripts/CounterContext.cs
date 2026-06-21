namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 计数器 Demo 游戏上下文
    /// </summary>
    public sealed class CounterContext : Context<CounterContext>
    {
        protected override void Configure()
        {
            RegisterModel<ICounterModel>(new CounterModel());
        }
    }
}
