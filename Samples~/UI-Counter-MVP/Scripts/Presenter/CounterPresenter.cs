using System;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 计数器 Presenter。作为 Model 与 View 之间的中介，
    /// <para>订阅 Model 变更以更新 View，处理 View 交互以驱动 Model。</para>
    /// </summary>
    public sealed class CounterPresenter : IPresenter<CounterContext>
    {
        readonly ICounterView _view;
        readonly ICounterModel _model;
        IUnsubscribe _countSubscription;

        public CounterPresenter(ICounterView view)
        {
            _view = view;
            _view.IncreaseClicked += HandleIncrease;
            _view.DecreaseClicked += HandleDecrease;
            _view.ResetClicked += HandleReset;
            _model = this.GetModel<ICounterModel>();
            _countSubscription = _model.Count.SubscribeAndInvoke(UpdateView);
        }

        void HandleIncrease()
        {
            _model.Increase();
            AesirArchitectureLog.Log("Increase Counter");
        }

        void HandleDecrease()
        {
            _model.Decrease();
            AesirArchitectureLog.Log("Decrease Counter");
        }

        void HandleReset()
        {
            _model.Reset();
            AesirArchitectureLog.Log("Reset Counter");
        }

        void UpdateView(int count)
        {
            _view.UpdateCountText(count);
        }

        /// <summary>
        /// 注销所有订阅，释放资源
        /// </summary>
        public void Dispose()
        {
            _view.IncreaseClicked -= HandleIncrease;
            _view.DecreaseClicked -= HandleDecrease;
            _view.ResetClicked -= HandleReset;

            _countSubscription?.Dispose();
            _countSubscription = null;
        }
    }
}
