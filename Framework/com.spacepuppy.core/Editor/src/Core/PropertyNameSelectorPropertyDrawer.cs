using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Collections;
using com.spacepuppy.Dynamic;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(PropertyNameSelectorAttribute))]
    public sealed class PropertyNameSelectorPropertyDrawer : PropertyDrawer
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label, EditorHelper.TempContent("PropertyNameSelector applied to field that is not a string."));
                return;
            }

            var attrib = this.attribute as PropertyNameSelectorAttribute;
            if (attrib == null)
            {
                EditorGUI.LabelField(position, label, EditorHelper.TempContent("PropertyNameSelector not configured correctly."));
                return;
            }

            System.Func<MemberInfo, bool> pred = null;
            if (!attrib.AllowReadOnly)
            {
                pred = (m) =>
                {
                    if (m is PropertyInfo p)
                        return p.CanWrite;
                    else if (m is FieldInfo f)
                        return true;
                    else
                        return false;
                };
            }
            if (!string.IsNullOrEmpty(attrib.IgnoreCallback))
            {
                var tp = property.serializedObject.targetObject?.GetType();
                if (tp != null)
                {
                    var method = DynamicUtil.EnumerateAllMembers(tp, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                                            .OfType<MethodInfo>()
                                            .Where(o => o.Name == attrib.IgnoreCallback && o.ReturnType == typeof(bool) && o.GetParameters().Count() == 1 && o.GetParameters().First().ParameterType == typeof(MemberInfo))
                                            .FirstOrDefault();
                    if (method != null)
                    {
                        if (method.IsStatic)
                        {
                            pred = pred.ChainAnd(method.CreateDelegate(typeof(System.Func<MemberInfo, bool>)) as System.Func<MemberInfo, bool>);
                        }
                        else
                        {
                            pred = pred.ChainAnd(method.CreateDelegate(typeof(System.Func<MemberInfo, bool>), property.serializedObject.targetObject) as System.Func<MemberInfo, bool>);
                        }
                    }
                }
            }
            if (attrib.IgnorePropNames != null && attrib.IgnorePropNames.Length > 0)
            {
                pred = pred.ChainAnd(m => !attrib.IgnorePropNames.Contains(m.Name));
            }

            property.stringValue = SPEditorGUI.PropertyNameSelector(position, label, property.stringValue, attrib.TargetTypes ?? ArrayUtil.Empty<System.Type>(), attrib.AllowCustom, pred);
        }

    }

}
