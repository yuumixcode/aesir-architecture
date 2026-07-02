using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 示例计数器主面板视图。
    /// <para>通过 MonoView 获取上下文，并在 Awake 中初始化 Model 监听和 Controller 实例。</para>
    /// </summary>
    public class SampleMvcCounterMainPanel : MonoView<SampleMvcCounterContext>
    {
        /// <summary>
        /// 显示计数值的文本组件
        /// </summary>
        [SerializeField]
        Text countText;

        /// <summary>
        /// 增加计数的按钮
        /// </summary>
        [SerializeField]
        Button increaseButton;

        /// <summary>
        /// 减少计数的按钮
        /// </summary>
        [SerializeField]
        Button decreaseButton;

        /// <summary>
        /// 重置计数的按钮
        /// </summary>
        [SerializeField]
        Button resetButton;

        ISampleMvcCounterController _ctrl;
        ISampleMvcCounterModel _model;

        void Awake()
        {
            _model = this.GetModel<ISampleMvcCounterModel>();
            _model.Count.AddListener(UpdateCountText).RemoveListenerWhenGameObjectOnDestroyed(gameObject);
            _ctrl = new SampleMvcCounterController();
        }

        void OnEnable()
        {
            increaseButton.onClick.AddListener(_ctrl.Increase);
            decreaseButton.onClick.AddListener(_ctrl.Decrease);
            resetButton.onClick.AddListener(_ctrl.ResetCounter);
        }

        void OnDisable()
        {
            increaseButton.onClick.RemoveAllListeners();
            decreaseButton.onClick.RemoveAllListeners();
            resetButton.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// 根据当前计数值更新 UI 文本显示
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
