using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.spacepuppy;
using com.spacepuppy.Collections;
using com.spacepuppy.Dynamic;
using com.spacepuppy.Utils;
using UnityEngine.Experimental.GlobalIllumination;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(VariantMember))]
    public class VariantMemberPropertyDrawer : PropertyDrawer
    {

        public const string PROP_TARGET = "_target";
        public const string PROP_MEMBER = "_memberName";

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);
            EditorHelper.SuppressIndentLevel();

            try
            {
                var targProp = property.FindPropertyRelative(PROP_TARGET);
                var memberProp = property.FindPropertyRelative(PROP_MEMBER);

                var r0 = new Rect(position.xMin, position.yMin, position.width * 0.35f, position.height * 0.85f);
                var r1 = new Rect(r0.xMax, r0.yMin, position.width - r0.width, position.height);

                if (SPEditorGUI.XButton(ref r0, "Clear Selected Object", true))
                {
                    targProp.objectReferenceValue = null;
                    memberProp.stringValue = string.Empty;
                    return;
                }
                targProp.objectReferenceValue = EditorGUI.ObjectField(r0, targProp.objectReferenceValue, typeof(UnityEngine.Object), true);

                const System.Reflection.MemberTypes MASK = System.Reflection.MemberTypes.Field | System.Reflection.MemberTypes.Property;
                const DynamicMemberAccess ACCESS = DynamicMemberAccess.ReadWrite;

                var targObj = targProp.objectReferenceValue;
                var memberName = memberProp.stringValue;

                IEnumerable<MemberInfo> GetMembersFromTarget(object o)
                {
                    return o is IDynamic ? DynamicUtil.GetMembers(o, false, MASK) : DynamicUtil.GetMembersFromType((o.IsProxy() ? (o as IProxy).GetTargetType() : o.GetType()), false, MASK);
                }

                var go = GameObjectUtil.GetGameObjectFromSource(targObj);
                if (go != null)
                {
                    using (var lst = TempCollection.GetList<Component>())
                    {
                        go.GetComponents(lst);
                        var members = (from o in lst.Cast<object>().Prepend(go)
                                       from m in GetMembersFromTarget(o).Where(m => DynamicUtil.GetMemberAccessLevel(m).HasFlag(ACCESS) && !m.IsObsolete())
                                       select (o, m)).ToArray();
                        var entries = members.Select(t =>
                        {
                            if (t.o.IsProxy())
                            {
                                return EditorHelper.TempContent(string.Format("{0}/{1} [{2}]", t.o.GetType().Name, t.m.Name, DynamicUtil.GetReturnType(t.m).Name));
                            }
                            else if ((DynamicUtil.GetMemberAccessLevel(t.m) & DynamicMemberAccess.Write) != 0)
                            {
                                return EditorHelper.TempContent(string.Format(OverrideFormatProvider.Default, "{0}/{1} [{2}] -> {3}", t.o.GetType().Name, t.m.Name, DynamicUtil.GetReturnType(t.m).Name, EditorHelper.GetValueWithMemberSafe(t.m, t.o, true)));
                            }
                            else
                            {
                                return EditorHelper.TempContent(string.Format(OverrideFormatProvider.Default, "{0}/{1} (readonly - {2}) -> {3}", t.o.GetType().Name, t.m.Name, DynamicUtil.GetReturnType(t.m).Name, EditorHelper.GetValueWithMemberSafe(t.m, t.o, true)));
                            }
                        }).Prepend(EditorHelper.TempContent(string.Format("{0} --no selection--", go.name))).ToArray();
                        int index = members.IndexOf(t => object.ReferenceEquals(t.o, targObj) && t.m.Name == memberName) + 1;

                        EditorGUI.BeginChangeCheck();
                        index = EditorGUI.Popup(r1, index, entries);
                        if (EditorGUI.EndChangeCheck())
                        {
                            this.PurgeIfPlaying(property);
                            if (index > 0)
                            {
                                index--;
                                targProp.objectReferenceValue = members[index].o as UnityEngine.Object;
                                memberProp.stringValue = members[index].m.Name;
                            }
                            else if (index == 0)
                            {
                                targProp.objectReferenceValue = targObj ?? go;
                                memberProp.stringValue = string.Empty;
                            }
                            else
                            {
                                targProp.objectReferenceValue = null;
                                memberProp.stringValue = string.Empty;
                            }
                        }
                    }
                }
                else if (targObj)
                {
                    var members = GetMembersFromTarget(targObj).Where(m => DynamicUtil.GetMemberAccessLevel(m).HasFlag(ACCESS) && !m.IsObsolete()).ToArray();
                    var entries = members.Select(m =>
                    {
                        if (targObj.IsProxy())
                        {
                            return EditorHelper.TempContent(string.Format("{0}.{1} [{2}]", targObj.GetType().Name, m.Name, DynamicUtil.GetReturnType(m).Name));
                        }
                        else if ((DynamicUtil.GetMemberAccessLevel(m) & DynamicMemberAccess.Write) != 0)
                        {
                            return EditorHelper.TempContent(string.Format(OverrideFormatProvider.Default, "{0}.{1} [{2}] -> {3}", targObj.GetType().Name, m.Name, DynamicUtil.GetReturnType(m).Name, EditorHelper.GetValueWithMemberSafe(m, targObj, true)));
                        }
                        else
                        {
                            return EditorHelper.TempContent(string.Format(OverrideFormatProvider.Default, "{0}.{1} (readonly - {2}) -> {3}", targObj.GetType().Name, m.Name, DynamicUtil.GetReturnType(m).Name, EditorHelper.GetValueWithMemberSafe(m, targObj, true)));
                        }
                    }).Prepend(EditorHelper.TempContent(string.Format("{0} --no selection--", targObj.name))).ToArray();

                    int index = members.IndexOf(m => m.Name == memberName) + 1;

                    EditorGUI.BeginChangeCheck();
                    index = EditorGUI.Popup(r1, index, entries);
                    if (EditorGUI.EndChangeCheck())
                    {
                        this.PurgeIfPlaying(property);
                        if (index > 0)
                        {
                            index--;
                            targProp.objectReferenceValue = targObj;
                            memberProp.stringValue = members[index].Name;
                        }
                        else if (index == 0)
                        {
                            targProp.objectReferenceValue = targObj;
                            memberProp.stringValue = string.Empty;
                        }
                        else
                        {
                            targProp.objectReferenceValue = null;
                            memberProp.stringValue = string.Empty;
                        }
                    }
                }
                else
                {
                    var cache = SPGUI.Disable();
                    EditorGUI.Popup(r1, 0, new GUIContent[] { EditorHelper.TempContent("No Target") });
                    cache.Reset();
                }
            }
            finally
            {
                EditorHelper.ResumeIndentLevel();
            }
        }

        private void PurgeIfPlaying(SerializedProperty property)
        {
            if (Application.isPlaying)
            {
                var obj = EditorHelper.GetTargetObjectOfProperty(property);
                if (obj is VariantMember) (obj as VariantMember).SetDirty();
            }
        }



        private class OverrideFormatProvider : System.IFormatProvider, System.ICustomFormatter
        {
            public static readonly OverrideFormatProvider Default = new OverrideFormatProvider();

            object System.IFormatProvider.GetFormat(System.Type formatType)
            {
                if (formatType == typeof(System.ICustomFormatter))
                    return this;
                else
                    return null;
            }

            string System.ICustomFormatter.Format(string format, object arg, System.IFormatProvider formatProvider)
            {
                if (arg is Matrix4x4 m)
                {
                    return string.Format("[{0:0.0}, {1:0.0}, {2:0.0}, {3:0.0}, ...]", m.m00, m.m01, m.m02, m.m03);
                }
                else if (arg is System.IFormattable f)
                {
                    return f.ToString(format, System.Globalization.CultureInfo.CurrentCulture);
                }
                else if (arg != null)
                {
                    return arg.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }

        }

    }

}
