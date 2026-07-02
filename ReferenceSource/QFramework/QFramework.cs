/****************************************************************************
 * Copyright (c) 2015 ~ 2024 liangxiegame MIT License
 *
 * QFramework v1.0
 *
 * https://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 *
 * Author:
 *  liangxie        https://github.com/liangxie
 *  soso            https://github.com/so-sos-so
 *
 * Contributor
 *  TastSong        https://github.com/TastSong
 *  京产肠饭         https://gitee.com/JingChanChangFan/hk_-unity-tools
 *  猫叔(一只皮皮虾) https://space.bilibili.com/656352/
 *  misakiMeiii     https://github.com/misakiMeiii
 *  New一天
 *  幽飞冷凝雪～冷
 *
 * Community
 *  QQ Group: 623597263
 *
 * Latest Update: 2025.3.18 10:21 add InitArchitecture api
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QFramework
{
    #region Architecture

    public interface IArchitecture
    {
        void RegisterSystem<T>(T system) where T : ISystem;

        void RegisterModel<T>(T model) where T : IModel;

        void RegisterUtility<T>(T utility) where T : IUtility;

        T GetSystem<T>() where T : class, ISystem;

        T GetModel<T>() where T : class, IModel;

        T GetUtility<T>() where T : class, IUtility;

        void SendCommand<T>(T command) where T : ICommand;

        TResult SendCommand<TResult>(ICommand<TResult> command);

        TResult SendQuery<TResult>(IQuery<TResult> query);

        void SendEvent<T>() where T : new();
        void SendEvent<T>(T e);

        IUnRegister RegisterEvent<T>(Action<T> onEvent);
        void UnRegisterEvent<T>(Action<T> onEvent);

        void Deinit();
    }

    public abstract class Architecture<T> : IArchitecture where T : Architecture<T>, new()
    {
        public static Action<T> OnRegisterPatch = architecture => { };

        protected static T mArchitecture;

        readonly IOCContainer mContainer = new IOCContainer();

        readonly TypeEventSystem mTypeEventSystem = new TypeEventSystem();
        bool mInited;

        public static IArchitecture Interface
        {
            get
            {
                if (mArchitecture == null)
                {
                    InitArchitecture();
                }

                return mArchitecture;
            }
        }

        public void Deinit()
        {
            OnDeinit();
            foreach (var system in mContainer.GetInstancesByType<ISystem>().Where(s => s.Initialized))
            {
                system.Deinit();
            }

            foreach (var model in mContainer.GetInstancesByType<IModel>().Where(m => m.Initialized))
            {
                model.Deinit();
            }

            mContainer.Clear();
            mArchitecture = null;
        }

        public void RegisterSystem<TSystem>(TSystem system) where TSystem : ISystem
        {
            system.SetArchitecture(this);
            mContainer.Register(system);

            if (mInited)
            {
                system.Init();
                system.Initialized = true;
            }
        }

        public void RegisterModel<TModel>(TModel model) where TModel : IModel
        {
            model.SetArchitecture(this);
            mContainer.Register(model);

            if (mInited)
            {
                model.Init();
                model.Initialized = true;
            }
        }

        public void RegisterUtility<TUtility>(TUtility utility) where TUtility : IUtility =>
            mContainer.Register(utility);

        public TSystem GetSystem<TSystem>() where TSystem : class, ISystem => mContainer.Get<TSystem>();

        public TModel GetModel<TModel>() where TModel : class, IModel => mContainer.Get<TModel>();

        public TUtility GetUtility<TUtility>() where TUtility : class, IUtility => mContainer.Get<TUtility>();

        public TResult SendCommand<TResult>(ICommand<TResult> command) => ExecuteCommand(command);

        public void SendCommand<TCommand>(TCommand command) where TCommand : ICommand =>
            ExecuteCommand(command);

        public TResult SendQuery<TResult>(IQuery<TResult> query) => DoQuery(query);

        public void SendEvent<TEvent>() where TEvent : new() => mTypeEventSystem.Send<TEvent>();

        public void SendEvent<TEvent>(TEvent e) => mTypeEventSystem.Send(e);

        public IUnRegister RegisterEvent<TEvent>(Action<TEvent> onEvent) =>
            mTypeEventSystem.Register(onEvent);

        public void UnRegisterEvent<TEvent>(Action<TEvent> onEvent) => mTypeEventSystem.UnRegister(onEvent);

        public static void InitArchitecture()
        {
            if (mArchitecture == null)
            {
                mArchitecture = new T();
                mArchitecture.Init();

                OnRegisterPatch?.Invoke(mArchitecture);

                foreach (var model in mArchitecture.mContainer.GetInstancesByType<IModel>()
                             .Where(m => !m.Initialized))
                {
                    model.Init();
                    model.Initialized = true;
                }

                foreach (var system in mArchitecture.mContainer.GetInstancesByType<ISystem>()
                             .Where(m => !m.Initialized))
                {
                    system.Init();
                    system.Initialized = true;
                }

                mArchitecture.mInited = true;
            }
        }

        protected abstract void Init();

        protected virtual void OnDeinit() { }

        protected virtual TResult ExecuteCommand<TResult>(ICommand<TResult> command)
        {
            command.SetArchitecture(this);
            return command.Execute();
        }

        protected virtual void ExecuteCommand(ICommand command)
        {
            command.SetArchitecture(this);
            command.Execute();
        }

        protected virtual TResult DoQuery<TResult>(IQuery<TResult> query)
        {
            query.SetArchitecture(this);
            return query.Do();
        }
    }

    public interface IOnEvent<T>
    {
        void OnEvent(T e);
    }

    public static class OnGlobalEventExtension
    {
        public static IUnRegister RegisterEvent<T>(this IOnEvent<T> self) where T : struct =>
            TypeEventSystem.Global.Register<T>(self.OnEvent);

        public static void UnRegisterEvent<T>(this IOnEvent<T> self) where T : struct =>
            TypeEventSystem.Global.UnRegister<T>(self.OnEvent);
    }

    #endregion

    #region Controller

    public interface IController : IBelongToArchitecture, ICanSendCommand, ICanGetSystem, ICanGetModel,
        ICanRegisterEvent, ICanSendQuery, ICanGetUtility { }

    #endregion

    #region System

    public interface ISystem : IBelongToArchitecture, ICanSetArchitecture, ICanGetModel, ICanGetUtility,
        ICanRegisterEvent, ICanSendEvent, ICanGetSystem, ICanInit { }

    public abstract class AbstractSystem : ISystem
    {
        IArchitecture mArchitecture;

        IArchitecture IBelongToArchitecture.GetArchitecture() => mArchitecture;

        void ICanSetArchitecture.SetArchitecture(IArchitecture architecture) => mArchitecture = architecture;

        public bool Initialized { get; set; }
        void ICanInit.Init() => OnInit();

        public void Deinit() => OnDeinit();

        protected virtual void OnDeinit() { }

        protected abstract void OnInit();
    }

    #endregion

    #region Model

    public interface IModel : IBelongToArchitecture, ICanSetArchitecture, ICanGetUtility, ICanSendEvent,
        ICanInit { }

    public abstract class AbstractModel : IModel
    {
        IArchitecture mArchitecturel;

        IArchitecture IBelongToArchitecture.GetArchitecture() => mArchitecturel;

        void ICanSetArchitecture.SetArchitecture(IArchitecture architecture) => mArchitecturel = architecture;

        public bool Initialized { get; set; }
        void ICanInit.Init() => OnInit();
        public void Deinit() => OnDeinit();

        protected virtual void OnDeinit() { }

        protected abstract void OnInit();
    }

    #endregion

    #region Utility

    public interface IUtility { }

    #endregion

    #region Command

    public interface ICommand : IBelongToArchitecture, ICanSetArchitecture, ICanGetSystem, ICanGetModel,
        ICanGetUtility, ICanSendEvent, ICanSendCommand, ICanSendQuery
    {
        void Execute();
    }

    public interface ICommand<TResult> : IBelongToArchitecture, ICanSetArchitecture, ICanGetSystem,
        ICanGetModel, ICanGetUtility, ICanSendEvent, ICanSendCommand, ICanSendQuery
    {
        TResult Execute();
    }

    public abstract class AbstractCommand : ICommand
    {
        IArchitecture mArchitecture;

        IArchitecture IBelongToArchitecture.GetArchitecture() => mArchitecture;

        void ICanSetArchitecture.SetArchitecture(IArchitecture architecture) => mArchitecture = architecture;

        void ICommand.Execute() => OnExecute();

        protected abstract void OnExecute();
    }

    public abstract class AbstractCommand<TResult> : ICommand<TResult>
    {
        IArchitecture mArchitecture;

        IArchitecture IBelongToArchitecture.GetArchitecture() => mArchitecture;

        void ICanSetArchitecture.SetArchitecture(IArchitecture architecture) => mArchitecture = architecture;

        TResult ICommand<TResult>.Execute() => OnExecute();

        protected abstract TResult OnExecute();
    }

    #endregion

    #region Query

    public interface IQuery<TResult> : IBelongToArchitecture, ICanSetArchitecture, ICanGetModel,
        ICanGetSystem, ICanSendQuery
    {
        TResult Do();
    }

    public abstract class AbstractQuery<T> : IQuery<T>
    {
        IArchitecture mArchitecture;
        public T Do() => OnDo();

        public IArchitecture GetArchitecture() => mArchitecture;

        public void SetArchitecture(IArchitecture architecture) => mArchitecture = architecture;

        protected abstract T OnDo();
    }

    #endregion

    #region Rule

    public interface IBelongToArchitecture
    {
        IArchitecture GetArchitecture();
    }

    public interface ICanSetArchitecture
    {
        void SetArchitecture(IArchitecture architecture);
    }

    public interface ICanGetModel : IBelongToArchitecture { }

    public static class CanGetModelExtension
    {
        public static T GetModel<T>(this ICanGetModel self) where T : class, IModel =>
            self.GetArchitecture().GetModel<T>();
    }

    public interface ICanGetSystem : IBelongToArchitecture { }

    public static class CanGetSystemExtension
    {
        public static T GetSystem<T>(this ICanGetSystem self) where T : class, ISystem =>
            self.GetArchitecture().GetSystem<T>();
    }

    public interface ICanGetUtility : IBelongToArchitecture { }

    public static class CanGetUtilityExtension
    {
        public static T GetUtility<T>(this ICanGetUtility self) where T : class, IUtility =>
            self.GetArchitecture().GetUtility<T>();
    }

    public interface ICanRegisterEvent : IBelongToArchitecture { }

    public static class CanRegisterEventExtension
    {
        public static IUnRegister RegisterEvent<T>(this ICanRegisterEvent self, Action<T> onEvent) =>
            self.GetArchitecture().RegisterEvent(onEvent);

        public static void UnRegisterEvent<T>(this ICanRegisterEvent self, Action<T> onEvent) =>
            self.GetArchitecture().UnRegisterEvent(onEvent);
    }

    public interface ICanSendCommand : IBelongToArchitecture { }

    public static class CanSendCommandExtension
    {
        public static void SendCommand<T>(this ICanSendCommand self) where T : ICommand, new() =>
            self.GetArchitecture().SendCommand(new T());

        public static void SendCommand<T>(this ICanSendCommand self, T command) where T : ICommand =>
            self.GetArchitecture().SendCommand(command);

        public static TResult SendCommand<TResult>(this ICanSendCommand self, ICommand<TResult> command) =>
            self.GetArchitecture().SendCommand(command);
    }

    public interface ICanSendEvent : IBelongToArchitecture { }

    public static class CanSendEventExtension
    {
        public static void SendEvent<T>(this ICanSendEvent self) where T : new() =>
            self.GetArchitecture().SendEvent<T>();

        public static void SendEvent<T>(this ICanSendEvent self, T e) => self.GetArchitecture().SendEvent(e);
    }

    public interface ICanSendQuery : IBelongToArchitecture { }

    public static class CanSendQueryExtension
    {
        public static TResult SendQuery<TResult>(this ICanSendQuery self, IQuery<TResult> query) =>
            self.GetArchitecture().SendQuery(query);
    }

    public interface ICanInit
    {
        bool Initialized { get; set; }
        void Init();
        void Deinit();
    }

    #endregion

    #region TypeEventSystem

    public interface IUnRegister
    {
        void UnRegister();
    }

    public interface IUnRegisterList
    {
        List<IUnRegister> UnregisterList { get; }
    }

    public static class IUnRegisterListExtension
    {
        public static void AddToUnregisterList(this IUnRegister self, IUnRegisterList unRegisterList) =>
            unRegisterList.UnregisterList.Add(self);

        public static void UnRegisterAll(this IUnRegisterList self)
        {
            foreach (var unRegister in self.UnregisterList)
            {
                unRegister.UnRegister();
            }

            self.UnregisterList.Clear();
        }
    }

    public struct CustomUnRegister : IUnRegister
    {
        Action mOnUnRegister { get; set; }
        public CustomUnRegister(Action onUnRegister) => mOnUnRegister = onUnRegister;

        public void UnRegister()
        {
            mOnUnRegister.Invoke();
            mOnUnRegister = null;
        }
    }

#if UNITY_5_6_OR_NEWER
    public abstract class UnRegisterTrigger : MonoBehaviour
    {
        readonly HashSet<IUnRegister> mUnRegisters = new HashSet<IUnRegister>();

        public IUnRegister AddUnRegister(IUnRegister unRegister)
        {
            mUnRegisters.Add(unRegister);
            return unRegister;
        }

        public void RemoveUnRegister(IUnRegister unRegister) => mUnRegisters.Remove(unRegister);

        public void UnRegister()
        {
            foreach (var unRegister in mUnRegisters)
            {
                unRegister.UnRegister();
            }

            mUnRegisters.Clear();
        }
    }

    public class UnRegisterOnDestroyTrigger : UnRegisterTrigger
    {
        void OnDestroy()
        {
            UnRegister();
        }
    }

    public class UnRegisterOnDisableTrigger : UnRegisterTrigger
    {
        void OnDisable()
        {
            UnRegister();
        }
    }

    public class UnRegisterCurrentSceneUnloadedTrigger : UnRegisterTrigger
    {
        static UnRegisterCurrentSceneUnloadedTrigger mDefault;

        public static UnRegisterCurrentSceneUnloadedTrigger Get
        {
            get
            {
                if (!mDefault)
                {
                    mDefault = new GameObject("UnRegisterCurrentSceneUnloadedTrigger")
                        .AddComponent<UnRegisterCurrentSceneUnloadedTrigger>();
                }

                return mDefault;
            }
        }

        void Awake()
        {
            DontDestroyOnLoad(this);
            hideFlags = HideFlags.HideInHierarchy;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        void OnDestroy() => SceneManager.sceneUnloaded -= OnSceneUnloaded;
        void OnSceneUnloaded(Scene scene) => UnRegister();
    }
#endif

    public static class UnRegisterExtension
    {
#if UNITY_5_6_OR_NEWER

        static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            var trigger = gameObject.GetComponent<T>();

            if (!trigger)
            {
                trigger = gameObject.AddComponent<T>();
            }

            return trigger;
        }

        public static IUnRegister UnRegisterWhenGameObjectDestroyed(this IUnRegister unRegister,
            GameObject gameObject) =>
            GetOrAddComponent<UnRegisterOnDestroyTrigger>(gameObject).AddUnRegister(unRegister);

        public static IUnRegister UnRegisterWhenGameObjectDestroyed<T>(this IUnRegister self, T component)
            where T : Component =>
            self.UnRegisterWhenGameObjectDestroyed(component.gameObject);

        public static IUnRegister UnRegisterWhenDisabled<T>(this IUnRegister self, T component)
            where T : Component =>
            self.UnRegisterWhenDisabled(component.gameObject);

        public static IUnRegister UnRegisterWhenDisabled(this IUnRegister unRegister,
            GameObject gameObject) =>
            GetOrAddComponent<UnRegisterOnDisableTrigger>(gameObject).AddUnRegister(unRegister);

        public static IUnRegister UnRegisterWhenCurrentSceneUnloaded(this IUnRegister self) =>
            UnRegisterCurrentSceneUnloadedTrigger.Get.AddUnRegister(self);
#endif

#if GODOT
		public static IUnRegister UnRegisterWhenNodeExitTree(this IUnRegister unRegister, Godot.Node node)
		{
			node.TreeExiting += unRegister.UnRegister;
			return unRegister;
		}
#endif
    }

    public class TypeEventSystem
    {
        public static readonly TypeEventSystem Global = new TypeEventSystem();
        readonly EasyEvents mEvents = new EasyEvents();

        public void Send<T>() where T : new() => mEvents.GetEvent<EasyEvent<T>>()?.Trigger(new T());

        public void Send<T>(T e) => mEvents.GetEvent<EasyEvent<T>>()?.Trigger(e);

        public IUnRegister Register<T>(Action<T> onEvent) =>
            mEvents.GetOrAddEvent<EasyEvent<T>>().Register(onEvent);

        public void UnRegister<T>(Action<T> onEvent)
        {
            var e = mEvents.GetEvent<EasyEvent<T>>();
            e?.UnRegister(onEvent);
        }
    }

    #endregion

    #region IOC

    public class IOCContainer
    {
        readonly Dictionary<Type, object> mInstances = new Dictionary<Type, object>();

        public void Register<T>(T instance)
        {
            var key = typeof(T);

            if (mInstances.ContainsKey(key))
            {
                mInstances[key] = instance;
            }
            else
            {
                mInstances.Add(key, instance);
            }
        }

        public T Get<T>() where T : class
        {
            var key = typeof(T);

            if (mInstances.TryGetValue(key, out var retInstance))
            {
                return retInstance as T;
            }

            return null;
        }

        public IEnumerable<T> GetInstancesByType<T>()
        {
            var type = typeof(T);
            return mInstances.Values.Where(instance => type.IsInstanceOfType(instance)).Cast<T>();
        }

        public void Clear() => mInstances.Clear();
    }

    #endregion

    #region BindableProperty

    public interface IBindableProperty<T> : IReadonlyBindableProperty<T>
    {
        new T Value { get; set; }
        void SetValueWithoutEvent(T newValue);
    }

    public interface IReadonlyBindableProperty<T> : IEasyEvent
    {
        T Value { get; }

        IUnRegister RegisterWithInitValue(Action<T> action);
        void UnRegister(Action<T> onValueChanged);
        IUnRegister Register(Action<T> onValueChanged);
    }

    public class BindableProperty<T> : IBindableProperty<T>
    {
        readonly EasyEvent<T> mOnValueChanged = new EasyEvent<T>();

        protected T mValue;
        public BindableProperty(T defaultValue = default) => mValue = defaultValue;

        public static Func<T, T, bool> Comparer { get; set; } = (a, b) => a.Equals(b);

        public T Value
        {
            get => GetValue();
            set
            {
                if (value == null && mValue == null)
                {
                    return;
                }

                if (value != null && Comparer(value, mValue))
                {
                    return;
                }

                SetValue(value);
                mOnValueChanged.Trigger(value);
            }
        }

        public void SetValueWithoutEvent(T newValue) => mValue = newValue;

        public IUnRegister Register(Action<T> onValueChanged) => mOnValueChanged.Register(onValueChanged);

        public IUnRegister RegisterWithInitValue(Action<T> onValueChanged)
        {
            onValueChanged(mValue);
            return Register(onValueChanged);
        }

        public void UnRegister(Action<T> onValueChanged) => mOnValueChanged.UnRegister(onValueChanged);

        IUnRegister IEasyEvent.Register(Action onEvent)
        {
            return Register(Action);

            void Action(T _)
            {
                onEvent();
            }
        }

        public BindableProperty<T> WithComparer(Func<T, T, bool> comparer)
        {
            Comparer = comparer;
            return this;
        }

        protected virtual void SetValue(T newValue) => mValue = newValue;

        protected virtual T GetValue() => mValue;

        public override string ToString() => Value.ToString();
    }

    internal static class ComparerAutoRegister
    {
#if UNITY_5_6_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void AutoRegister()
        {
            BindableProperty<int>.Comparer = (a, b) => a == b;
            BindableProperty<float>.Comparer = Mathf.Approximately;
            BindableProperty<double>.Comparer = (a, b) => a == b;
            BindableProperty<string>.Comparer = (a, b) => a == b;
            BindableProperty<long>.Comparer = (a, b) => a == b;
            BindableProperty<Vector2>.Comparer = (a, b) => a == b;
            BindableProperty<Vector3>.Comparer = (a, b) => a == b;
            BindableProperty<Vector4>.Comparer = (a, b) => a == b;
            BindableProperty<Color>.Comparer = (a, b) => a == b;
            BindableProperty<Color32>.Comparer = (a, b) =>
                a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
            BindableProperty<Bounds>.Comparer = (a, b) => a == b;
            BindableProperty<Rect>.Comparer = (a, b) => a == b;
            BindableProperty<Quaternion>.Comparer = (a, b) => a == b;
            BindableProperty<Vector2Int>.Comparer = (a, b) => a == b;
            BindableProperty<Vector3Int>.Comparer = (a, b) => a == b;
            BindableProperty<BoundsInt>.Comparer = (a, b) => a == b;
            BindableProperty<RangeInt>.Comparer = (a, b) => a.start == b.start && a.length == b.length;
            BindableProperty<RectInt>.Comparer = (a, b) => a.Equals(b);
        }
#endif
    }

    #endregion

    #region EasyEvent

    public interface IEasyEvent
    {
        IUnRegister Register(Action onEvent);
    }

    public class EasyEvent : IEasyEvent
    {
        Action mOnEvent = () => { };

        public IUnRegister Register(Action onEvent)
        {
            mOnEvent += onEvent;
            return new CustomUnRegister(() =>
            {
                UnRegister(onEvent);
            });
        }

        public IUnRegister RegisterWithACall(Action onEvent)
        {
            onEvent.Invoke();
            return Register(onEvent);
        }

        public void UnRegister(Action onEvent) => mOnEvent -= onEvent;

        public void Trigger() => mOnEvent?.Invoke();
    }

    public class EasyEvent<T> : IEasyEvent
    {
        Action<T> mOnEvent = e => { };

        IUnRegister IEasyEvent.Register(Action onEvent)
        {
            return Register(Action);

            void Action(T _)
            {
                onEvent();
            }
        }

        public IUnRegister Register(Action<T> onEvent)
        {
            mOnEvent += onEvent;
            return new CustomUnRegister(() =>
            {
                UnRegister(onEvent);
            });
        }

        public void UnRegister(Action<T> onEvent) => mOnEvent -= onEvent;

        public void Trigger(T t) => mOnEvent?.Invoke(t);
    }

    public class EasyEvent<T, K> : IEasyEvent
    {
        Action<T, K> mOnEvent = (t, k) => { };

        IUnRegister IEasyEvent.Register(Action onEvent)
        {
            return Register(Action);

            void Action(T _, K __)
            {
                onEvent();
            }
        }

        public IUnRegister Register(Action<T, K> onEvent)
        {
            mOnEvent += onEvent;
            return new CustomUnRegister(() =>
            {
                UnRegister(onEvent);
            });
        }

        public void UnRegister(Action<T, K> onEvent) => mOnEvent -= onEvent;

        public void Trigger(T t, K k) => mOnEvent?.Invoke(t, k);
    }

    public class EasyEvent<T, K, S> : IEasyEvent
    {
        Action<T, K, S> mOnEvent = (t, k, s) => { };

        IUnRegister IEasyEvent.Register(Action onEvent)
        {
            return Register(Action);

            void Action(T _, K __, S ___)
            {
                onEvent();
            }
        }

        public IUnRegister Register(Action<T, K, S> onEvent)
        {
            mOnEvent += onEvent;
            return new CustomUnRegister(() =>
            {
                UnRegister(onEvent);
            });
        }

        public void UnRegister(Action<T, K, S> onEvent) => mOnEvent -= onEvent;

        public void Trigger(T t, K k, S s) => mOnEvent?.Invoke(t, k, s);
    }

    public class EasyEvents
    {
        static readonly EasyEvents mGlobalEvents = new EasyEvents();

        readonly Dictionary<Type, IEasyEvent> mTypeEvents = new Dictionary<Type, IEasyEvent>();

        public static T Get<T>() where T : IEasyEvent => mGlobalEvents.GetEvent<T>();

        public static void Register<T>() where T : IEasyEvent, new() => mGlobalEvents.AddEvent<T>();

        public void AddEvent<T>() where T : IEasyEvent, new() => mTypeEvents.Add(typeof(T), new T());

        public T GetEvent<T>() where T : IEasyEvent =>
            mTypeEvents.TryGetValue(typeof(T), out var e) ? (T)e : default;

        public T GetOrAddEvent<T>() where T : IEasyEvent, new()
        {
            var eType = typeof(T);
            if (mTypeEvents.TryGetValue(eType, out var e))
            {
                return (T)e;
            }

            var t = new T();
            mTypeEvents.Add(eType, t);
            return t;
        }
    }

    #endregion

    #region Event Extension

    public class OrEvent : IUnRegisterList
    {
        Action mOnEvent = () => { };

        public List<IUnRegister> UnregisterList { get; } = new List<IUnRegister>();

        public OrEvent Or(IEasyEvent easyEvent)
        {
            easyEvent.Register(Trigger).AddToUnregisterList(this);
            return this;
        }

        public IUnRegister Register(Action onEvent)
        {
            mOnEvent += onEvent;
            return new CustomUnRegister(() =>
            {
                UnRegister(onEvent);
            });
        }

        public IUnRegister RegisterWithACall(Action onEvent)
        {
            onEvent.Invoke();
            return Register(onEvent);
        }

        public void UnRegister(Action onEvent)
        {
            mOnEvent -= onEvent;
            this.UnRegisterAll();
        }

        void Trigger() => mOnEvent?.Invoke();
    }

    public static class OrEventExtensions
    {
        public static OrEvent Or(this IEasyEvent self, IEasyEvent e) => new OrEvent().Or(self).Or(e);
    }

    #endregion
}
