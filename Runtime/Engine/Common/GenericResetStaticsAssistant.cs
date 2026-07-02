using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 静态变量重置助手，用于运行时阶段自动重置静态变量，兼容 Disable Domain Reload。
    /// 回调列表不在重置后清空，因为关闭 Domain Reload 时静态构造函数不会重新执行，
    /// 回调需保留供下一次进入 Play Mode 时使用。
    /// </summary>
    public static class GenericResetStaticsAssistant
    {
        static readonly List<Action> ResetStaticsCallbacks = new List<Action>();

        public static void Register(Action callback)
        {
            ResetStaticsCallbacks.Add(callback);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStaticsAll()
        {
            foreach (var callback in ResetStaticsCallbacks)
            {
                callback?.Invoke();
            }
        }
    }
}
