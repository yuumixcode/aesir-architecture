using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 订阅注销句柄。包装注销回调，确保重复调用 Dispose 仅执行一次注销。
    /// </summary>
    public struct AutoUnsubscribeHandle : IUnsubscribe
    {
        Action _callback;
        bool _disposed;

        /// <summary>
        /// 创建注销句柄，传入注销回调
        /// </summary>
        public AutoUnsubscribeHandle(Action unsubscribeCallback)
        {
            _callback = unsubscribeCallback;
            _disposed = false;
        }

        /// <summary>
        /// 执行注销，重复调用安全
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _callback?.Invoke();
            _callback = null;
        }
    }
}
