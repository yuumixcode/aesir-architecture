namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 模块上下文基类。采用泛型静态单例模式，每个子类对应一个全局上下文实例。
    /// <para>
    /// 继承此抽象类并在 <see cref="Configure" /> 中注册 Model 和 System。
    /// 访问 <see cref="Instance" /> 自动触发初始化流程，调用 <see cref="Dispose" /> 释放资源。
    /// </para>
    /// <para>
    /// 实例级逻辑由 <see cref="BaseContext" /> 提供，本类仅负责单例管理与域重置。
    /// </para>
    /// </summary>
    public abstract class Context<T> : BaseContext where T : Context<T>, new()
    {
        static T _context;

        static Context()
        {
            ContextResetAssistant.Register(ResetForDomainReload);
        }

        /// <summary>
        /// 获取上下文接口实例。首次访问时自动初始化。
        /// </summary>
        public static T Instance
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

        public static IContext Interface => Instance;

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
}
