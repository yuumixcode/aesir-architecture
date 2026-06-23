using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 非单例上下文，用于测试隔离。通过回调配置模块，无需子类。
    /// <para>
    /// 与 <see cref="Context{T}" /> 的区别：无静态字段、无域重置注册、可任意创建实例，
    /// 构造时立即初始化，Dispose 后由 GC 回收，不会自动复活。
    /// </para>
    /// </summary>
    public sealed class MockContext : BaseContext
    {
        readonly Action<IContext> _configure;

        /// <param name="configure">配置回调，在回调中注册 Model</param>
        public MockContext(Action<IContext> configure)
        {
            _configure = configure ?? throw new ArgumentNullException(nameof(configure));
            Initialize();
        }

        protected override void Configure() => _configure(this);
    }
}
