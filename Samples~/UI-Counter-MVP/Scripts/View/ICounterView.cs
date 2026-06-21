using System;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 计数器被动视图接口。Presenter 通过此接口控制 View 显示，并通过委托接收用户交互通知。
    /// </summary>
    public interface ICounterView
    {
        /// <summary>
        /// 更新计数显示文本
        /// </summary>
        void UpdateCountText(int count);

        /// <summary>
        /// 用户点击增加按钮时触发
        /// </summary>
        Action IncreaseClicked { get; set; }

        /// <summary>
        /// 用户点击减少按钮时触发
        /// </summary>
        Action DecreaseClicked { get; set; }

        /// <summary>
        /// 用户点击重置按钮时触发
        /// </summary>
        Action ResetClicked { get; set; }
    }
}
