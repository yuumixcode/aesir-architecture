using System;
using System.Collections.Generic;

using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 可观察属性实现。
    /// <para>Model 层持有可写实例，View 层通过 <see cref="IReadOnlyObservableProperty{T}" /> 只读订阅。</para>
    /// </summary>
    /// <typeparam name="T">属性值类型</typeparam>
    [Serializable]
    public sealed class ObservableProperty<T> : IObservableProperty<T>
    {
        /// <summary>
        /// 私有字段名称，等同于提供 nameof(value) 给外界。
        /// </summary>
        public const string PrivateValueFieldName = nameof(value);

        [SerializeField]
        T value;

        Action<T> _onValueChanged;

        public ObservableProperty() { }

        public ObservableProperty(T initialValue) => value = initialValue;

        public T Value
        {
            get => value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(this.value, value))
                {
                    return;
                }

                this.value = value;
                _onValueChanged?.Invoke(value);
            }
        }

        /// <summary>
        /// 静默设置值，不触发通知。用于反序列化或批量更新后统一触发。
        /// </summary>
        public void SetValueSilently(T v)
        {
            if (EqualityComparer<T>.Default.Equals(value, v))
            {
                return;
            }

            value = v;
        }

        /// <summary>
        /// 设置值。语义等价于 <see cref="Value" /> 的 setter。
        /// </summary>
        public void SetValue(T v)
        {
            Value = v;
        }

        /// <summary>
        /// 订阅值变更。回调参数为新值。
        /// </summary>
        public IUnsubscribe Subscribe(Action<T> callback)
        {
            _onValueChanged += callback;
            return new AutoUnsubscribeHandle(() => Unsubscribe(callback));
        }

        /// <summary>
        /// 取消订阅值变更。
        /// </summary>
        public void Unsubscribe(Action<T> callback) => _onValueChanged -= callback;

        /// <summary>
        /// 订阅并立即触发一次当前值，用于初始化时同步订阅方状态。
        /// </summary>
        public IUnsubscribe SubscribeAndInvoke(Action<T> callback)
        {
            var handler = Subscribe(callback);
            callback?.Invoke(value);
            return handler;
        }

        /// <summary>
        /// 触发值变更通知，用于强制刷新订阅方状态。
        /// </summary>
        public void Invoke()
        {
            _onValueChanged?.Invoke(value);
        }

        /// <summary>
        /// 原地修改值并强制触发通知，绕过相等性检查。
        /// <para>适用于引用类型（class）的就地修改场景：获取引用 → 修改字段 → 触发通知。</para>
        /// </summary>
        /// <param name="modifier">对当前值执行的原地修改操作。</param>
        public void Modify(Action<T> modifier)
        {
            modifier.Invoke(value);
            _onValueChanged?.Invoke(value);
        }

        /// <summary>
        /// 清除所有订阅。
        /// </summary>
        public void Clear()
        {
            _onValueChanged = null;
        }
    }
}
