using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 泛型单例 MonoBehaviour 基类。通过约束自身类型实现类型安全的单例模式，首次访问时自动创建 GameObject。
    /// </summary>
    [DisallowMultipleComponent]
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
    {
        static T _instance;

        /// <summary>
        /// 获取单例实例。若不存在则自动查找或创建。
        /// <para>
        /// 注意：此 getter 依赖 Unity 重载的 <c>==</c> 运算符来检测已销毁的 MonoBehaviour。
        /// 在 Domain Reload 关闭时，<c>_instance == null</c> 对已销毁对象返回 <c>true</c>，
        /// 从而触发重新查找或创建。请勿使用 <c>ReferenceEquals(_instance, null)</c> 检测存活状态。
        /// </para>
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();
                }

                if (_instance == null)
                {
                    _instance = new GameObject().AddComponent<T>();
                }

                return _instance;
            }
        }

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = (T)this;
            gameObject.name = GetGameObjectName();
            DontDestroyOnLoad(gameObject);
            OnAwake();
        }

        void OnDestroy()
        {
            OnDestroyBefore();

            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// 单例初始化完成后调用，子类可覆写以注册自定义事件
        /// </summary>
        protected virtual void OnAwake() { }

        /// <summary>
        /// 单例销毁时调用，子类可覆写以执行自定义清理
        /// </summary>
        protected virtual void OnDestroyBefore() { }

        /// <summary>
        /// 获取单例 GameObject 的显示名称，子类可覆写自定义命名。
        /// </summary>
        protected virtual string GetGameObjectName() => "[" + typeof(T).Name + "]";
    }
}
