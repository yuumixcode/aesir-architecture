namespace Runestone.AesirArchitecture
{
    public abstract class AesirViewController<T> : AesirMonoBehaviour, IView, IController
        where T : AbstractContext<T>, new()
    {
        public IContext Context { get; } = AbstractContext<T>.Interface;
    }
}
