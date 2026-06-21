using System;
using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirArchitecture.Samples
{
    public class UICounterMvcPanel : AbstractView<CounterContext>
    {
        [SerializeField]
        Text countText;

        [SerializeField]
        Button increaseButton;

        [SerializeField]
        Button decreaseButton;

        [SerializeField]
        Button resetButton;

        ICounterModel _model;
        ICounterController _ctrl;

        void Awake()
        {
            _model = this.GetModel<ICounterModel>();
            _model.Count.Subscribe(UpdateCountText).UnsubscribeWhenGameObjectOnDestroyed(gameObject);
            _ctrl = new CounterController(_model);
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

        public void UpdateCountText(int count)
        {
            if (countText != null)
            {
                countText.text = count.ToString();
            }
        }
    }
}
