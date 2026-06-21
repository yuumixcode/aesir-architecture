using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 模块上下文基类。采用泛型静态单例模式，每个子类对应一个全局上下文实例。
    /// <para>
    /// 继承此抽象类并在 <see cref="Configure" /> 中注册 Model。
    /// 调用 <see cref="Interface" /> 自动触发初始化流程，调用 <see cref="Dispose" /> 释放资源。
    /// </para>
    /// <para>
    /// 实例级逻辑由 <see cref="ContextBase" /> 提供，本类仅负责单例管理与域重置。
    /// </para>
    /// </summary>
    public abstract class Context<T> : ContextBase where T : Context<T>, new()
    {
        static T _context;

        static Context()
        {
            ContextResetAssistant.Register(ResetForDomainReload);
        }

        /// <summary>
        /// 获取上下文接口实例。首次访问时自动初始化。
        /// </summary>
        public static IContext Interface
        {
            get
            {
                if (_context != null)
                {
                    return _context;
                }

                _context = new T();
                _context.Initialize();
                return _context;
            }
        }

        /// <summary>
        /// 释放资源并清除静态引用。
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            _context = null;
        }

        static void ResetForDomainReload()
        {
            _context?.Dispose();
        }
    }

    /// <summary>
    /// 非泛型域重置中心。在域加载时自动调用所有已注册的重置回调。
    /// <para>
    /// <see cref="Context{T}" /> 是泛型类型，<see cref="RuntimeInitializeOnLoadMethod" /> 无法在泛型类型上生效，
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
