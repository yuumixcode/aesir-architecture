using System;
using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 计数器 MVP 被动视图。仅负责 UI 显示与用户输入转发，不直接访问 Model。
    /// <para>所有业务逻辑由 <see cref="CounterPresenter" /> 驱动。</para>
    /// </summary>
    public class UICounterMvpPanel : AbstractView<CounterContext>, ICounterView
    {
        [SerializeField]
        Text countText;

        [SerializeField]
        Button increaseButton;

        [SerializeField]
        Button decreaseButton;

        [SerializeField]
        Button resetButton;

        CounterPresenter _presenter;

        public Action IncreaseClicked { get; set; }
        public Action DecreaseClicked { get; set; }
        public Action ResetClicked { get; set; }

        void Awake()
        {
            _presenter = new CounterPresenter(this);
        }

        void OnEnable()
        {
            increaseButton.onClick.AddListener(OnIncreaseButtonClicked);
            decreaseButton.onClick.AddListener(OnDecreaseButtonClicked);
            resetButton.onClick.AddListener(OnResetButtonClicked);
        }

        void OnDisable()
        {
            increaseButton.onClick.RemoveListener(OnIncreaseButtonClicked);
            decreaseButton.onClick.RemoveListener(OnDecreaseButtonClicked);
            resetButton.onClick.RemoveListener(OnResetButtonClicked);
        }

        void OnDestroy()
        {
            _presenter?.Dispose();
        }

        void OnIncreaseButtonClicked()
        {
            IncreaseClicked?.Invoke();
        }

        void OnDecreaseButtonClicked()
        {
            DecreaseClicked?.Invoke();
        }

        void OnResetButtonClicked()
        {
            ResetClicked?.Invoke();
        }

        /// <summary>
        /// 更新计数显示文本
        /// </summary>
        public void UpdateCountText(int count)
        {
            if (countText != null)
            {
                countText.text = count.ToString();
            }
        }
    }
}
