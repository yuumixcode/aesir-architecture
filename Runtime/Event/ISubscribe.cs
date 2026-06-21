using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 事件订阅方法接口。继承 <see cref="IDisposable" />，
    /// 移除时通过 <see cref="Dispose" /> 清空内部委托引用以释放内存。
    /// </summary>
    public interface ISubscribe : IDisposable
    {
        /// <summary>
        /// 订阅事件，并返回可自动注销的订阅句柄
        /// </summary>
        /// <param name="callback">订阅事件的方法</param>
        /// <returns>取消订阅句柄</returns>
        IUnsubscribe Subscribe(Action callback);
    }
}
