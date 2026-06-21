using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 在所属 GameObject 销毁时自动注销所有订阅。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UnsubscribeOnDestroyInvoker : UnsubscribeInvoker
    {
        /// <summary>
        /// Unity 在销毁此 MonoBehaviour 所在的 GameObject 时自动调用，执行所有已注册订阅的注销操作。
        /// </summary>
        void OnDestroy() => UnsubscribeAll();
    }
}
