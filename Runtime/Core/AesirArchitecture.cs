using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// Aesir Architecture 架构单例物体对象，使用此单例对象接入 Unity 的生命周期。
    /// </summary>
    public class AesirArchitecture : SingletonMonoBehaviour<AesirArchitecture>
    {
        protected override string GetGameObjectName() => "[Aesir Architecture]";

        /// <summary>
        /// 在第一个场景的 Awake 触发前，显示调用 Instance，启动架构单例物体。
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Bootstrap()
        {
            _ = Instance;
        }
    }
}
