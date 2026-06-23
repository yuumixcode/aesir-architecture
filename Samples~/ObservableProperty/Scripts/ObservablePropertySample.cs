using Runestone.AesirArchitecture;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 简单类型 ObservableProperty Inspector 演示组件。
    /// <para>每种类型同时展示原始字段与 ObservableProperty 字段，方便对比 Inspector 绘制效果。</para>
    /// <para>进入 PlayMode 后，在 Inspector 中拖拽/修改 ObservableProperty 字段值会触发订阅回调并打印日志。</para>
    /// <para>通过 Button 可用代码修改值，验证事件触发。</para>
    /// </summary>
    public class ObservablePropertySimpleSample : MonoBehaviour
    {
        [Header("int — 对比")]
        public int plainHp = 100;
        [SerializeField] ObservableProperty<int> observableHp = new ObservableProperty<int>(100);

        [Header("float — 对比")]
        public float plainSpeed = 5.5f;
        [SerializeField] ObservableProperty<float> observableSpeed = new ObservableProperty<float>(5.5f);

        [Header("string — 对比")]
        public string plainName = "冒险者";
        [SerializeField] ObservableProperty<string> observableName = new ObservableProperty<string>("冒险者");

        [Header("bool — 对比")]
        public bool plainIsAlive = true;
        [SerializeField] ObservableProperty<bool> observableIsAlive = new ObservableProperty<bool>(true);

        [Header("enum — 对比")]
        public SampleAlignment plainAlignment = SampleAlignment.Neutral;
        [SerializeField] ObservableProperty<SampleAlignment> observableAlignment = new ObservableProperty<SampleAlignment>(SampleAlignment.Neutral);

        [Header("Vector2 — 对比")]
        public Vector2 plainPosition = Vector2.zero;
        [SerializeField] ObservableProperty<Vector2> observablePosition = new ObservableProperty<Vector2>(Vector2.zero);

        [Header("Vector3 — 对比")]
        public Vector3 plainVelocity = Vector3.zero;
        [SerializeField] ObservableProperty<Vector3> observableVelocity = new ObservableProperty<Vector3>(Vector3.zero);

        [Title("代码修改测试")]
        [Button("HP +10", ButtonSizes.Medium)]
        void AddHp() => observableHp.Value += 10;

        [Button("Speed +1", ButtonSizes.Medium)]
        void AddSpeed() => observableSpeed.Value += 1f;

        [Button("Name 加后缀", ButtonSizes.Medium)]
        void AppendName() => observableName.Value += "_X";

        [Button("Toggle IsAlive", ButtonSizes.Medium)]
        void ToggleAlive() => observableIsAlive.Value = !observableIsAlive.Value;

        [Button("Alignment 切换", ButtonSizes.Medium)]
        void CycleAlignment() => observableAlignment.Value = (SampleAlignment)(((int)observableAlignment.Value + 1) % 4);

        [Button("Position X+1", ButtonSizes.Medium)]
        void AddPosX() => observablePosition.Value += new Vector2(1, 0);

        [Button("Velocity Y+1", ButtonSizes.Medium)]
        void AddVelY() => observableVelocity.Value += new Vector3(0, 1, 0);

        public IReadOnlyObservableProperty<int> ObservableHp => observableHp;
        public IReadOnlyObservableProperty<float> ObservableSpeed => observableSpeed;
        public IReadOnlyObservableProperty<string> ObservableName => observableName;
        public IReadOnlyObservableProperty<bool> ObservableIsAlive => observableIsAlive;
        public IReadOnlyObservableProperty<SampleAlignment> ObservableAlignment => observableAlignment;
        public IReadOnlyObservableProperty<Vector2> ObservablePosition => observablePosition;
        public IReadOnlyObservableProperty<Vector3> ObservableVelocity => observableVelocity;

        AutoUnsubscribeHandle _hpSub, _speedSub, _nameSub, _aliveSub, _alignSub, _posSub, _velSub;

        void OnEnable()
        {
            _hpSub    = observableHp.Subscribe(v => Debug.Log($"[Simple] HP → {v}"));
            _speedSub = observableSpeed.Subscribe(v => Debug.Log($"[Simple] Speed → {v}"));
            _nameSub  = observableName.Subscribe(v => Debug.Log($"[Simple] Name → {v}"));
            _aliveSub = observableIsAlive.Subscribe(v => Debug.Log($"[Simple] IsAlive → {v}"));
            _alignSub = observableAlignment.Subscribe(v => Debug.Log($"[Simple] Alignment → {v}"));
            _posSub   = observablePosition.Subscribe(v => Debug.Log($"[Simple] Position → {v}"));
            _velSub   = observableVelocity.Subscribe(v => Debug.Log($"[Simple] Velocity → {v}"));
        }

        void OnDisable()
        {
            _hpSub.Dispose();
            _speedSub.Dispose();
            _nameSub.Dispose();
            _aliveSub.Dispose();
            _alignSub.Dispose();
            _posSub.Dispose();
            _velSub.Dispose();
        }
    }

    /// <summary>
    /// 示例用枚举，验证 enum 类型在 Drawer 中的绘制。
    /// </summary>
    public enum SampleAlignment
    {
        Neutral,
        Good,
        Evil,
        Chaotic
    }
}
