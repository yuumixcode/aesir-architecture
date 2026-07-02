namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 计数器 Presenter 实现。
    /// <para>直接调用 Model 方法（而非 MVC 的 Command，也可以增加一层 Command）
    /// 用于对比展示 MVP 与 MVC 的差异。</para>
    /// </summary>
    public sealed class SampleMvpCounterPresenter : ISampleMvpCounterPresenter
    {
        readonly ISampleMvpCounterView _view;
        readonly ISampleMvpCounterModel _model;

        public SampleMvpCounterPresenter(ISampleMvpCounterView view)
        {
            _view = view;
            _view.IncreaseClicked += OnIncreaseClicked;
            _view.DecreaseClicked += OnDecreaseClicked;
            _view.ResetClicked += OnResetClicked;
            _model = this.GetModel<ISampleMvpCounterModel>();
        }

        void OnIncreaseClicked()
        {
            _model.Increase();
            _view.UpdateCount(_model.Count.Value);
            AesirArchitectureLog.Log("Increase Counter");
        }

        void OnDecreaseClicked()
        {
            _model.Decrease();
            _view.UpdateCount(_model.Count.Value);
            AesirArchitectureLog.Log("Decrease Counter");
        }

        void OnResetClicked()
        {
            _model.Reset();
            _view.UpdateCount(_model.Count.Value);
            AesirArchitectureLog.Log("Reset Counter");
        }

        public void Dispose()
        {
            _view.IncreaseClicked -= OnIncreaseClicked;
            _view.DecreaseClicked -= OnDecreaseClicked;
            _view.ResetClicked -= OnResetClicked;
        }
    }
}
