using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirArchitecture.Editor.OdinIntegration
{
    /// <summary>
    /// 自动为 <see cref="AbstractModel" /> 子类中的 <see cref="ObservableValue{T}" /> 字段添加 <c>[ShowInInspector]</c>，
    /// 使其在 Inspector 面板中可见且可编辑。修改值时通过 <c>OnValueChanged("Invoke")</c> 自动触发通知。
    /// </summary>
    public class AbstractModelAttributeProcessor : OdinAttributeProcessor<AbstractModel>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty,
            MemberInfo member,
            List<Attribute> attributes)
        {
            if (IsObservableValueMember(member))
            {
                attributes.Add(new ShowInInspectorAttribute());
                attributes.Add(new EnableGUIAttribute());
            }
        }

        static bool IsObservableValueMember(MemberInfo member)
        {
            var memberType = member switch
            {
                FieldInfo field => field.FieldType,
                PropertyInfo property => property.PropertyType,
                _ => null
            };

            return memberType != null && memberType.IsGenericType &&
                   memberType.GetGenericTypeDefinition() == typeof(ObservableValue<>);
        }
    }
}
