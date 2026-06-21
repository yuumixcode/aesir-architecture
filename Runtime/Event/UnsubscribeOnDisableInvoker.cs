using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// GameObject 禁用时自动注销所有订阅。挂载此组件后，当 GameObject 被禁用时将执行批量注销操作。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UnsubscribeOnDisableInvoker : UnsubscribeInvoker
    {
        /// <summary>
        /// Unity 在 GameObject 禁用时调用该方法，自动注销所有已注册的订阅。
        /// </summary>
        void OnDisable() => UnsubscribeAll();
    }
}
