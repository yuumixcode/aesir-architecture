using System;
using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 事件注册信息，用于 Inspector 展示单个事件类型的监听状态
    /// </summary>
    [Serializable]
    public class EventRegistrationInfo
    {
        /// <summary>
        /// 事件类型名称
        /// </summary>
        public string eventName;

        /// <summary>
        /// 监听者数量
        /// </summary>
        public int listenerCount;

        /// <summary>
        /// 监听者列表，格式为 "类名.方法名"
        /// </summary>
        public List<string> listeners = new List<string>();

        public EventRegistrationInfo(Type type) => eventName = type.Name;

        public void AddListener(string listener)
        {
            listeners.Add(listener);
            listenerCount++;
        }

        public void RemoveListener(string listener)
        {
            listeners.Remove(listener);
            listenerCount--;
        }
    }
}
