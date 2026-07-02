using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirArchitecture.Editor.OdinIntegration
{
    /// <summary>
    /// 为 <see cref="ContextBoard" /> 添加 Inspector 展示属性，
    /// 使 Models 和 Services 字典以带标题的折叠形式展示。
    /// </summary>
    public class ContextBoardAttributeProcessor : OdinAttributeProcessor<ContextBoard>
    {
        /// <summary>
        /// 处理子成员特性，配置 Models 和 Services 字典的展示样式
        /// </summary>
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty,
            MemberInfo member,
            List<Attribute> attributes)
        {
            switch (member.Name)
            {
                case nameof(ContextBoard.Models):
                    attributes.Add(new ShowInInspectorAttribute());
                    attributes.Add(new EnableGUIAttribute());
                    attributes.Add(new TitleAttribute("数据模型列表", titleAlignment: TitleAlignments.Centered));
                    attributes.Add(new DictionaryDrawerSettings
                    {
                        DisplayMode = DictionaryDisplayOptions.ExpandedFoldout,
                        IsReadOnly = true,
                        KeyLabel = "所属上下文",
                        ValueLabel = "Model 列表"
                    });
                    break;

                case nameof(ContextBoard.Services):
                    attributes.Add(new ShowInInspectorAttribute());
                    attributes.Add(new EnableGUIAttribute());
                    attributes.Add(new TitleAttribute("服务对象列表", titleAlignment: TitleAlignments.Centered));
                    attributes.Add(new DictionaryDrawerSettings
                    {
                        DisplayMode = DictionaryDisplayOptions.ExpandedFoldout,
                        IsReadOnly = true,
                        KeyLabel = "所属上下文",
                        ValueLabel = "Service 列表"
                    });
                    break;
            }
        }
    }
}
