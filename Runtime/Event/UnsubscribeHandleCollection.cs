using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 订阅句柄集合。管理 <see cref="AutoUnsubscribeHandle" /> 句柄的添加与批量注销，
    /// 供 <see cref="UnsubscribeInvoker" /> 和 <see cref="UnsubscribeOnSceneUnloadedInvoker" /> 复用。
    /// </summary>
    public sealed class UnsubscribeHandleCollection
    {
        readonly List<AutoUnsubscribeHandle> _handles = new List<AutoUnsubscribeHandle>();

        /// <summary>
        /// 添加订阅句柄，使其在调用条件满足时自动注销
        /// </summary>
        public AutoUnsubscribeHandle Add(AutoUnsubscribeHandle handle)
        {
            _handles.Add(handle);
            return handle;
        }

        /// <summary>
        /// 注销所有已注册的订阅并清空列表
        /// </summary>
        public void UnsubscribeAll()
        {
            foreach (var handle in _handles)
            {
                handle.Dispose();
            }

            _handles.Clear();
        }
    }
}
