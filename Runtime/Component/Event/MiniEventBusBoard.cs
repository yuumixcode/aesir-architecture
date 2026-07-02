using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 事件总线看板组件。持有 <see cref="MiniEventBus" /> 引用并在 Inspector 中展示事件注册状态，
    /// 底层事件总线本身不依赖 MonoBehaviour。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MiniEventBusBoard : AesirMonoBehaviour
    {
        static MiniEventBusBoard _instance;

        /// <summary>
        /// 获取或创建全局唯一的看板实例
        /// </summary>
        public static MiniEventBusBoard Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                _instance = AesirArchitecture.GetOrAddComponent<MiniEventBusBoard>();
                return _instance;
            }
        }

        /// <summary>
        /// 当前事件注册信息的缓存列表，供 Inspector 展示
        /// </summary>
        public List<EventRegistrationInfo> EventRegistrations { get; private set; }

        void OnEnable()
        {
            MiniEventBus.Global.OnEventRegistrationsChanged += RefreshEventRegistrations;
            RefreshEventRegistrations();
        }

        void OnDisable()
        {
            MiniEventBus.Global.OnEventRegistrationsChanged -= RefreshEventRegistrations;
        }

        void RefreshEventRegistrations()
        {
            EventRegistrations = GetEventRegistrations(MiniEventBus.Global.EventDictionary);
        }

        /// <summary>
        /// 获取当前所有事件注册信息的快照，供 Inspector 展示
        /// </summary>
        static List<EventRegistrationInfo> GetEventRegistrations(Dictionary<Type, object> eventDictionary)
        {
            var result = new List<EventRegistrationInfo>();
            foreach (var kvp in eventDictionary)
            {
                var info = new EventRegistrationInfo(kvp.Key);
                if (kvp.Value is Delegate dlg)
                {
                    foreach (var invocation in dlg.GetInvocationList())
                    {
                        info.AddListener(FormatListener(invocation));
                    }
                }

                result.Add(info);
            }

            return result;
        }

        static string FormatListener(Delegate dlg)
        {
            var method = dlg.Method;
            var isAnonymous = Attribute.IsDefined(method, typeof(CompilerGeneratedAttribute));
            var className = dlg.Target != null ? TryGetReadableTypeName(dlg.Target.GetType()) :
                method.DeclaringType != null ? TryGetReadableTypeName(method.DeclaringType) : "Unknown";
            var methodName = isAnonymous ? "Anonymous" : method.Name;

            return $"{className}.{methodName}";
        }

        static string TryGetReadableTypeName(Type type)
        {
            if (type.Name.StartsWith("<"))
            {
                return type.DeclaringType?.Name ?? "Anonymous";
            }

            return type.Name;
        }
    }
}
