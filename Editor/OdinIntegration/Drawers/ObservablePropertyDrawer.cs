// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEditor;
// using Sirenix.OdinInspector;
// using Sirenix.OdinInspector.Editor;
// using Object = UnityEngine.Object;
//
// namespace Runestone.AesirArchitecture.Editor.OdinIntegration
// {
//     /// <summary>
//     /// 为 <see cref="ObservableProperty{T}" /> 提供 IMGUI 自定义绘制。
//     /// <para>简单类型使用标准 EditorGUILayout 字段（蓝色选中边框、数字标签拖拽）。</para>
//     /// <para>UnityEngine.Object 派生类型使用 ObjectField。</para>
//     /// <para>复杂可序列化类型手动绘制 foldout + 子属性，缩进与 Unity 原生一致。</para>
//     /// <para>所有字段 label 前方添加 EyeFill SDF 图标，标识为 ObservableProperty 字段。</para>
//     /// </summary>
//     /// <typeparam name="T">属性值类型</typeparam>
//     public class ObservablePropertyDrawer<T> : OdinValueDrawer<ObservableProperty<T>>
//     {
//         const float IconSize = 14f;
//         const float IndentWidth = 15f;
//         const float FoldoutArrowWidth = 10f;
//         const float ArrowToIconGap = -8f;
//         const float IconToLabelGap = 3f;
//         bool _foldout;
//
//         protected override void DrawPropertyLayout(GUIContent label)
//         {
//             var prop = ValueEntry.SmartValue;
//             if (prop == null)
//             {
//                 EditorGUILayout.LabelField(label ?? GUIContent.none, new GUIContent("Null"));
//                 return;
//             }
//
//             var value = prop.Value;
//
//             if (TypeClassifier.IsSimple(typeof(T)))
//             {
//                 DrawSimpleWithIcon(label, prop, value);
//             }
//             else if (TypeClassifier.IsUnityObject(typeof(T)))
//             {
//                 DrawObjectWithIcon(label, prop, value);
//             }
//             else
//             {
//                 DrawComplexWithIcon(label, prop, value);
//             }
//         }
//
//         void DrawSimpleWithIcon(GUIContent label, ObservableProperty<T> prop, T value)
//         {
//             if (label == null || label == GUIContent.none || string.IsNullOrEmpty(label.text))
//             {
//                 DrawSimple(GUIContent.none, prop, value);
//                 return;
//             }
//
//             var t = typeof(T);
//             // Vector2/Vector3 使用多行布局（label 行 + X/Y 行），空格 padding 会打乱缩进
//             // 且 Odin 内置 Vector2/Vector3 绘制器与 EditorGUILayout.Vector2Field 布局不同
//             // 委托 Odin 的 InspectorProperty.Draw 绘制，确保与 Plain 字段一致
//             if (t == typeof(Vector2) || t == typeof(Vector3))
//             {
//                 // 用空格 padding 为图标预留空间，与单行字段一致
//                 var paddedLabel = PadLabelForIcon(label);
//                 var valueProp = Property.Children.Get(ObservableProperty<T>.PrivateValueFieldName);
//                 if (valueProp != null)
//                 {
//                     // valueProp.Draw 直接修改内部 value 字段，绕过 Value setter
//                     // 用 BeginChangeCheck 检测变更后触发事件
//                     EditorGUI.BeginChangeCheck();
//                     valueProp.Draw(paddedLabel);
//                     if (EditorGUI.EndChangeCheck())
//                     {
//                         prop.Invoke();
//                     }
//                 }
//                 else
//                 {
//                     DrawSimple(paddedLabel, prop, value);
//                 }
//
//                 // 图标叠加到第一行
//                 var fieldRect = GUILayoutUtility.GetLastRect();
//                 var indentOffset = EditorGUI.indentLevel * IndentWidth;
//                 var iconRect = new Rect(fieldRect.x + indentOffset,
//                     fieldRect.y + (EditorGUIUtility.singleLineHeight - IconSize) * 0.5f, IconSize, IconSize);
//                 DrawObservableIcon(iconRect);
//             }
//             else
//             {
//                 // 单行字段：用空格 padding 为图标预留空间
//                 var paddedLabel = PadLabelForIcon(label);
//                 DrawSimple(paddedLabel, prop, value);
//
//                 var fieldRect = GUILayoutUtility.GetLastRect();
//                 var indentOffset = EditorGUI.indentLevel * IndentWidth;
//                 var iconRect = new Rect(fieldRect.x + indentOffset,
//                     fieldRect.y + (fieldRect.height - IconSize) * 0.5f, IconSize, IconSize);
//                 DrawObservableIcon(iconRect);
//             }
//         }
//
//         void DrawObjectWithIcon(GUIContent label, ObservableProperty<T> prop, T value)
//         {
//             if (label == null || label == GUIContent.none || string.IsNullOrEmpty(label.text))
//             {
//                 DrawObject(GUIContent.none, prop, value);
//                 return;
//             }
//
//             var paddedLabel = PadLabelForIcon(label);
//             DrawObject(paddedLabel, prop, value);
//
//             var fieldRect = GUILayoutUtility.GetLastRect();
//             var indentOffset = EditorGUI.indentLevel * IndentWidth;
//             var iconRect = new Rect(fieldRect.x + indentOffset,
//                 fieldRect.y + (fieldRect.height - IconSize) * 0.5f, IconSize, IconSize);
//             DrawObservableIcon(iconRect);
//         }
//
//         void DrawComplexWithIcon(GUIContent label, ObservableProperty<T> prop, T value)
//         {
//             if (ReferenceEquals(value, null))
//             {
//                 EditorGUILayout.LabelField(label ?? new GUIContent(typeof(T).Name), new GUIContent("null"));
//                 return;
//             }
//
//             var headerRect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label,
//                 GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(true));
//
//             var indentOffset = EditorGUI.indentLevel * IndentWidth;
//             var arrowRect = new Rect(headerRect.x + indentOffset, headerRect.y, FoldoutArrowWidth,
//                 headerRect.height);
//
//             var savedIndent = EditorGUI.indentLevel;
//             EditorGUI.indentLevel = 0;
//             _foldout = EditorGUI.Foldout(arrowRect, _foldout, GUIContent.none, false);
//             EditorGUI.indentLevel = savedIndent;
//
//             var iconX = arrowRect.xMax + ArrowToIconGap;
//             var iconY = headerRect.y + (headerRect.height - IconSize) * 0.5f;
//             var iconRect = new Rect(iconX, iconY, IconSize, IconSize);
//             DrawObservableIcon(iconRect);
//
//             var labelX = iconRect.xMax + IconToLabelGap;
//             var labelRect = new Rect(labelX, headerRect.y, headerRect.xMax - labelX, headerRect.height);
//
//             var headerEvent = Event.current;
//             if (headerEvent.type == EventType.MouseDown && headerRect.Contains(headerEvent.mousePosition))
//             {
//                 _foldout = !_foldout;
//                 headerEvent.Use();
//                 GUI.changed = true;
//             }
//
//             GUI.Label(labelRect, label);
//
//             if (_foldout)
//             {
//                 EditorGUI.indentLevel++;
//                 var valueProp = Property.Children.Get(ObservableProperty<T>.PrivateValueFieldName);
//                 if (valueProp != null)
//                 {
//                     // child.Draw 直接修改内部 value 字段，绕过 Value setter
//                     // 用 BeginChangeCheck 检测变更后触发事件
//                     EditorGUI.BeginChangeCheck();
//                     foreach (var child in valueProp.Children)
//                     {
//                         child.Draw();
//                     }
//
//                     if (EditorGUI.EndChangeCheck())
//                     {
//                         prop.Invoke();
//                     }
//                 }
//
//                 EditorGUI.indentLevel--;
//             }
//         }
//
//         //  通用方法
//
//         /// <summary>
//         /// 在 label 文字前插入与图标等宽的空格，为图标预留绘制空间。
//         /// </summary>
//         static GUIContent PadLabelForIcon(GUIContent original)
//         {
//             var iconSpace = IconSize + IconToLabelGap;
//             var spaceWidth = EditorStyles.label.CalcSize(new GUIContent(" ")).x;
//             // 这里 + 2 是人为防御性补充，因为 Unity API 计算出来的宽度通常都是偏小的。
//             var spaceCount = Mathf.CeilToInt(iconSpace / spaceWidth) + 2;
//             var padding = new string(' ', spaceCount);
//             return new GUIContent(padding + original.text, original.image, original.tooltip);
//         }
//
//         static void DrawObservableIcon(Rect rect)
//         {
//             var color = EditorGUIUtility.isProSkin
//                 ? new Color(0.85f, 0.85f, 0.85f)
//                 : new Color(0.3f, 0.3f, 0.3f);
//             SdfIcons.DrawIcon(rect, SdfIconType.EyeFill, color);
//         }
//
//         static void DrawSimple(GUIContent label, ObservableProperty<T> prop, T value)
//         {
//             var t = typeof(T);
//             EditorGUI.BeginChangeCheck();
//             T newValue = default;
//
//             if (t == typeof(int))
//             {
//                 newValue = (T)(object)EditorGUILayout.IntField(label, (int)(object)value);
//             }
//             else if (t == typeof(float))
//             {
//                 newValue = (T)(object)EditorGUILayout.FloatField(label, (float)(object)value);
//             }
//             else if (t == typeof(string))
//             {
//                 newValue = (T)(object)EditorGUILayout.TextField(label, (string)(object)value);
//             }
//             else if (t == typeof(bool))
//             {
//                 newValue = (T)(object)EditorGUILayout.Toggle(label, (bool)(object)value);
//             }
//             else if (t.IsEnum)
//             {
//                 newValue = (T)(object)EditorGUILayout.EnumPopup(label, (Enum)(object)value);
//             }
//             else if (t == typeof(Vector2))
//             {
//                 newValue = (T)(object)EditorGUILayout.Vector2Field(label, (Vector2)(object)value);
//             }
//             else if (t == typeof(Vector3))
//             {
//                 newValue = (T)(object)EditorGUILayout.Vector3Field(label, (Vector3)(object)value);
//             }
//
//             if (EditorGUI.EndChangeCheck())
//             {
//                 prop.Value = newValue;
//             }
//         }
//
//         static void DrawObject(GUIContent label, ObservableProperty<T> prop, T value)
//         {
//             EditorGUI.BeginChangeCheck();
//             var obj = EditorGUILayout.ObjectField(label, value as Object, typeof(T), true);
//             if (EditorGUI.EndChangeCheck())
//             {
//                 prop.Value = (T)(object)obj;
//             }
//         }
//     }
//
//     /// <summary>
//     /// 非泛型类型分类缓存，避免泛型类型中 static 字段的问题。
//     /// </summary>
//     internal static class TypeClassifier
//     {
//         static readonly Dictionary<Type, bool> SimpleCache = new Dictionary<Type, bool>();
//         static readonly Dictionary<Type, bool> UnityObjectCache = new Dictionary<Type, bool>();
//
//         static readonly HashSet<Type> SimpleTypes = new HashSet<Type>
//         {
//             typeof(int), typeof(float), typeof(string), typeof(bool),
//             typeof(Vector2), typeof(Vector3)
//         };
//
//         public static bool IsSimple(Type type)
//         {
//             if (SimpleCache.TryGetValue(type, out var result))
//             {
//                 return result;
//             }
//
//             result = SimpleTypes.Contains(type) || type.IsEnum;
//             SimpleCache[type] = result;
//             return result;
//         }
//
//         public static bool IsUnityObject(Type type)
//         {
//             if (UnityObjectCache.TryGetValue(type, out var result))
//             {
//                 return result;
//             }
//
//             result = typeof(Object).IsAssignableFrom(type);
//             UnityObjectCache[type] = result;
//             return result;
//         }
//     }
// }
