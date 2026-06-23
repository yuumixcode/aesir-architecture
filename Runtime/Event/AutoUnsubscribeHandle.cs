using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 订阅注销句柄。包装注销回调。
    /// </summary>
    public struct AutoUnsubscribeHandle : IDisposable
    {
        Action _callback;

        /// <summary>
        /// 创建注销句柄，传入注销回调
        /// </summary>
        public AutoUnsubscribeHandle(Action unsubscribeCallback) => _callback = unsubscribeCallback;

        /// <summary>
        /// 执行注销，重复调用安全
        /// </summary>
        public void Dispose()
        {
            _callback?.Invoke();
            _callback = null;
        }
    }
}
