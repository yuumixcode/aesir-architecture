using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// View 基类。通过泛型上下文获取模块访问能力，仅具备只读权限。
    /// </summary>
    public abstract class AbstractView<T> : MonoBehaviour, IView where T : Context<T>, new()
    {
        /// <summary>
        /// 获取当前泛型上下文的单例接口实例。
        /// </summary>
        public IContext GetContext() => Context<T>.Interface;
    }
}
