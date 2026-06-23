using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 完整可观察属性接口。
    /// <para>Presenter 层通过此接口读写数据。</para>
    /// </summary>
    /// <typeparam name="T">属性值类型</typeparam>
    public interface IObservableProperty<T> : IReadOnlyObservableProperty<T>
    {
        new T Value { get; set; }

        /// <summary>
        /// 静默设置值，不触发通知。用于反序列化或批量更新后统一触发。
        /// </summary>
        void SetValueSilently(T value);

        /// <summary>
        /// 设置值。语义等价于 <see cref="Value" /> 的 setter，便于以方法形式调用。
        /// </summary>
        void SetValue(T value);

        /// <summary>
        /// 原地修改值并强制触发通知，绕过相等性检查。
        /// <para>适用于引用类型（class）的就地修改场景：获取引用 → 修改字段 → 触发通知。</para>
        /// </summary>
        /// <param name="modifier">对当前值执行的原地修改操作。</param>
        void Modify(Action<T> modifier);
    }
}
