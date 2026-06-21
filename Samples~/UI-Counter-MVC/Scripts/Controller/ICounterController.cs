namespace Runestone.AesirArchitecture.Samples
{
    public interface ICounterController : IController<CounterContext>
    {
        ICounterModel Model { get; }
        void Increase();
        void Decrease();
        void ResetCounter();
    }
}
