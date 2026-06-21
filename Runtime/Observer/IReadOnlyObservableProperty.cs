using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 只读可观察属性接口。
    /// <para>View 层通过此接口订阅值变更，不能修改值。</para>
    /// </summary>
    /// <typeparam name="T">属性值类型</typeparam>
    public interface IReadOnlyObservableProperty<out T>
    {
        T Value { get; }

        /// <summary>
        /// 订阅值变更。回调参数为新值。
        /// </summary>
        IUnsubscribe Subscribe(Action<T> callback);

        /// <summary>
        /// 取消订阅值变更。
        /// </summary>
        void Unsubscribe(Action<T> callback);

        /// <summary>
        /// 订阅并立即触发一次当前值，用于初始化时同步订阅方状态。
        /// </summary>
        IUnsubscribe SubscribeAndInvoke(Action<T> callback);

        /// <summary>
        /// 触发值变更通知，用于强制刷新订阅方状态。
        /// </summary>
        void Invoke();

        
    }
}
