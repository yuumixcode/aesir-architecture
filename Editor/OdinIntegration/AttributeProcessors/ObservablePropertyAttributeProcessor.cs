using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirArchitecture.Editor
{
    public class ObservablePropertyAttributeProcessor<T> : OdinAttributeProcessor<ObservableProperty<T>>
    {
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add(new HideLabelAttribute());
            attributes.Add(new InlinePropertyAttribute());
        }

        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty,
            MemberInfo member,
            List<Attribute> attributes)
        {
            if (member.MemberType == MemberTypes.Field &&
                member.Name == ObservableProperty<T>.PrivateValueFieldName)
            {
                attributes.Add(new OnValueChangedAttribute(ObservableProperty<T>.InvokeMethodName, true));
                attributes.Add(new LabelTextAttribute("@$property.Parent.Name", true, SdfIconType.EyeFill));
            }
        }
    }
}
