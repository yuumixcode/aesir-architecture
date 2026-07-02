using System.Collections.Generic;
using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 上下文看板组件。通过 <see cref="AddContext" /> 直接注入上下文实例，
    /// 在 Inspector 中以字典形式展示每个 Context 的 Model 和 Service 列表。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ContextBoard : AesirMonoBehaviour
    {
        static ContextBoard _instance;
        /// <summary>
        /// 缓存所有已注册上下文的 Model 列表，按上下文类名分组
        /// </summary>
        public Dictionary<string, List<IModel>> Models = new Dictionary<string, List<IModel>>();

        /// <summary>
        /// 缓存所有已注册上下文的 Service 列表，按上下文类名分组
        /// </summary>
        public Dictionary<string, List<IService>> Services = new Dictionary<string, List<IService>>();

        /// <summary>
        /// 获取或创建全局唯一的上下文看板实例
        /// </summary>
        public static ContextBoard Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                _instance = AesirArchitecture.GetOrAddComponent<ContextBoard>();
                return _instance;
            }
        }

        /// <summary>
        /// 添加一个上下文的 Model 和 Service 到字典中
        /// </summary>
        public void AddContext(IContext context)
        {
            var key = context.GetType().Name;
            Models[key] = new List<IModel>(context.GetAllModels());
            Services[key] = new List<IService>(context.GetAllServices());
        }
    }
}
