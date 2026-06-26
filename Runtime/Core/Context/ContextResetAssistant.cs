using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 非泛型域重置中心。在域加载时自动调用所有已注册的重置回调。
    /// <para>
    /// <see cref="Context{T}" /> 是泛型类型，<see cref="RuntimeInitializeOnLoadMethodAttribute" /> 无法在泛型类型上生效，
    /// 因此需要此非泛型入口点承载 Unity 的域重置回调。
    /// </para>
    /// <para>
    /// 回调列表不在重置后清空，因为关闭 Domain Reload 时静态构造函数不会重新执行，
    /// 回调需保留供下一次进入 Play Mode 时使用。
    /// </para>
    /// </summary>
    internal static class ContextResetAssistant
    {
        static readonly List<Action> ResetCallbacks = new List<Action>();

        public static void Register(Action callback)
        {
            ResetCallbacks.Add(callback);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetAll()
        {
            foreach (var callback in ResetCallbacks)
            {
                callback?.Invoke();
            }
        }
    }
}
