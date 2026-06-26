using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 架构单例对象，作为框架入口点自动初始化。
    /// </summary>
    public class AesirArchitecture : SingletonMonoBehaviour<AesirArchitecture>
    {
        protected override string GetGameObjectName() => "[Aesir Architecture]";

        /// <summary>
        /// 在第一个场景的 Awake 触发前，显式访问 Instance，启动架构单例对象。
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Bootstrap()
        {
            _ = Instance;
        }
    }
}
