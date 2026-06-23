using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 自动注销调用器基类。维护订阅句柄列表，子类在特定生命周期事件中调用 <see cref="UnsubscribeAll" /> 批量注销。
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class UnsubscribeInvoker : MonoBehaviour
    {
        readonly UnsubscribeHandleCollection _handles = new UnsubscribeHandleCollection();

        /// <summary>
        /// 添加订阅句柄，使其在调用条件满足时自动注销
        /// </summary>
        public void AddUnsubscribeHandle(AutoUnsubscribeHandle handle)
        {
            _handles.Add(handle);
        }

        /// <summary>
        /// 注销所有已注册的订阅并清空列表
        /// </summary>
        protected void UnsubscribeAll() => _handles.UnsubscribeAll();
    }
}
