namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 表现层只读接口。View 层通过此接口与模块上下文交互。
    /// <para>
    /// 能力：GetModel, AddListener
    /// </para>
    /// </summary>
    public interface IView : IContextHolder, ICanGetModel, ICanGetService, ICanAddListener,
        ICanInvokeEvent { }
}
