using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 任意场景卸载时自动注销该场景注册的订阅。按场景名分桶，场景 A 卸载不会误杀场景 B 的订阅。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class
        UnsubscribeOnSceneUnloadedInvoker : SingletonMonoBehaviour<UnsubscribeOnSceneUnloadedInvoker>
    {
        readonly Dictionary<string, UnsubscribeHandleCollection> _sceneHandles =
            new Dictionary<string, UnsubscribeHandleCollection>();

        protected override string GetGameObjectName() => "[UnsubscribeOnSceneUnloadedInvoker]";

        /// <summary>
        /// 添加订阅句柄，使其在注册时所属场景卸载时自动注销
        /// </summary>
        public void AddUnsubscribeHandle(AutoUnsubscribeHandle handle)
        {
            var sceneName = SceneManager.GetActiveScene().name;
            if (!_sceneHandles.TryGetValue(sceneName, out var collection))
            {
                collection = new UnsubscribeHandleCollection();
                _sceneHandles[sceneName] = collection;
            }

            collection.Add(handle);
        }

        protected override void OnAwake()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        protected override void OnDestroyBefore()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        /// <summary>
        /// 当场景卸载时注销该场景下所有已注册的订阅。
        /// </summary>
        void OnSceneUnloaded(Scene scene)
        {
            if (_sceneHandles.TryGetValue(scene.name, out var collection))
            {
                collection.UnsubscribeAll();
                _sceneHandles.Remove(scene.name);
            }
        }
    }
}
