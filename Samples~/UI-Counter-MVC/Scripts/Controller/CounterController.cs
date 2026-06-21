namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 计数器 Demo 游戏控制器。MonoBehaviour 适配层，实现 IController 接口。
    /// <para>负责绑定 UI 按钮、订阅事件、驱动 Command/Query。</para>
    /// </summary>
    public class CounterController : ICounterController
    {
        public CounterController(ICounterModel model) => Model = model;

        public ICounterModel Model { get; }

        public void Increase()
        {
            Model.Increase();
            AesirArchitectureLog.Log("Increase Counter");
        }

        public void Decrease()
        {
            Model.Decrease();
            AesirArchitectureLog.Log("Decrease Counter");
        }

        public void ResetCounter()
        {
            Model.Reset();
            AesirArchitectureLog.Log("Reset Counter");
        }
    }
}
