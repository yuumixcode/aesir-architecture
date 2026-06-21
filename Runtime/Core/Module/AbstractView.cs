using UnityEngine;

namespace Runestone.AesirArchitecture
{
    public abstract class AbstractView<T> : MonoBehaviour, IView where T : Context<T>, new()
    {
        /// <summary>
        /// 获取当前泛型上下文的单例接口实例。
        /// </summary>
        public IContext GetContext() => Context<T>.Interface;
    }
}
