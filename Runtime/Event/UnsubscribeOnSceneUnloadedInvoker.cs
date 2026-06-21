using UnityEngine;
using UnityEngine.SceneManagement;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 任意场景卸载时自动注销所有订阅。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class
        UnsubscribeOnSceneUnloadedInvoker : SingletonMonoBehaviour<UnsubscribeOnSceneUnloadedInvoker>
    {
        readonly UnsubscribeHandleCollection _handles = new UnsubscribeHandleCollection();
        protected override string GetGameObjectName() => "[UnsubscribeOnSceneUnloadedInvoker]";

        /// <summary>
        /// 添加订阅句柄，使其在场景卸载时自动注销
        /// </summary>
        public IUnsubscribe AddUnsubscribeHandle(IUnsubscribe handle) => _handles.Add(handle);

        protected override void OnAwake()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        protected override void OnDestroyInternal()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        /// <summary>
        /// 当任意场景卸载时注销所有已注册的订阅。
        /// </summary>
        void OnSceneUnloaded(Scene scene) => _handles.UnsubscribeAll();
    }
}
