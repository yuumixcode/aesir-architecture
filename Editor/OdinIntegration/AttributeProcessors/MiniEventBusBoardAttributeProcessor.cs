using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirArchitecture.Editor.OdinIntegration
{
    /// <summary>
    /// 为 <see cref="MiniEventBusBoard" /> 添加 Inspector 展示属性，
    /// 使 <c>EventRegistrations</c> 以列表形式展示事件类型及监听者信息。
    /// </summary>
    public class MiniEventBusBoardAttributeProcessor : OdinAttributeProcessor<MiniEventBusBoard>
    {
        /// <summary>
        /// 处理子成员特性，为事件注册列表添加 Inspector 展示标签
        /// </summary>
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty,
            MemberInfo member,
            List<Attribute> attributes)
        {
            if (member.Name == nameof(MiniEventBusBoard.EventRegistrations))
            {
                attributes.Add(new TitleAttribute("事件检查列表", titleAlignment: TitleAlignments.Centered));
                attributes.Add(new HideLabelAttribute());
                attributes.Add(new ShowInInspectorAttribute());
                attributes.Add(new EnableGUIAttribute());
            }
        }
    }
}
