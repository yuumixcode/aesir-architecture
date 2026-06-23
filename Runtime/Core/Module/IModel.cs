using System;
using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 数据层接口。持有状态（通常使用 <see cref="ObservableProperty{T}" />）。
    /// <para>
    /// 能力：GetModel, Invoke
    /// </para>
    /// </summary>
    public interface IModel : IContextHolder, ICanSetContext, ICanGetModel, ICanInvokeWithContext,
        ICanInitialize
    {
        HashSet<Type> GetDependencies();
    }
}
