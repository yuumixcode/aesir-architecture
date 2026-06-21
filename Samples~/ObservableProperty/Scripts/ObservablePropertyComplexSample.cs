using Runestone.AesirArchitecture;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 复合类型 ObservableProperty Inspector 演示组件。
    /// <para>struct (WeaponStats) 与 class (CharacterData) 的 ObservableProperty 展示。</para>
    /// <para>进入 PlayMode 后，修改子字段值会触发订阅回调并打印日志。</para>
    /// <para>通过 Button 可用代码修改值，验证事件触发。</para>
    /// </summary>
    public class ObservablePropertyComplexSample : MonoBehaviour
    {
        [Header("struct (WeaponStats) — 对比")]
        public WeaponStats plainWeapon = new WeaponStats("铁剑", 15, 0.1f);

        [SerializeField]
        ObservableProperty<WeaponStats> observableWeapon =
            new ObservableProperty<WeaponStats>(new WeaponStats("铁剑", 15, 0.1f));

        [Header("class (CharacterData) — 对比")]
        public CharacterData plainCharacter = new CharacterData();

        [SerializeField]
        ObservableProperty<CharacterData> observableCharacter =
            new ObservableProperty<CharacterData>(new CharacterData());

        [Title("代码修改测试")]
        [Button("Weapon ATK +5", ButtonSizes.Medium)]
        void AddWeaponAtk() => observableWeapon.Value = new WeaponStats(observableWeapon.Value.WeaponName,
            observableWeapon.Value.AttackPower + 5, observableWeapon.Value.CriticalRate);

        [Button("Character Level +1", ButtonSizes.Medium)]
        void AddCharLevel()
        {
            observableCharacter.Modify(data => data.SetLevel(data.Level + 1));
        }

        public IReadOnlyObservableProperty<WeaponStats> ObservableWeapon => observableWeapon;
        public IReadOnlyObservableProperty<CharacterData> ObservableCharacter => observableCharacter;

        IUnsubscribe _weaponSub, _charSub;

        void OnEnable()
        {
            _weaponSub = observableWeapon.Subscribe(v =>
                Debug.Log($"[Complex] Weapon → {v.WeaponName} (ATK {v.AttackPower})"));
            _charSub = observableCharacter.Subscribe(v =>
                Debug.Log($"[Complex] Character → {v.DisplayName} Lv.{v.Level}"));
        }

        void OnDisable()
        {
            _weaponSub?.Dispose();
            _charSub?.Dispose();
        }
    }
}
