using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(SerializeRefPickerAttribute))]
    public class SerializeRefPickerPropertyDrawer : PropertyDrawer
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SPEditorGUI.DefaultPropertyField(position, property, label, true);

            if (Application.isPlaying) return;

            var attrib = this.attribute as SerializeRefPickerAttribute;
            if (attrib == null) return;

            var r0 = new Rect(position.xMin + EditorGUIUtility.labelWidth, position.yMin, Mathf.Max(0f, position.width - EditorGUIUtility.labelWidth), EditorGUIUtility.singleLineHeight);
            var info = GetAvailableTypes(attrib.RefType);
            if (info == null) return;

            var tp = property.GetManagedReferenceType();

            var atypes = attrib.AllowNull ? info.Value.AvailableTypesWithNull : info.Value.AvailableTypes;
            var anames = attrib.AllowNull ? info.Value.AvailableTypeNamesWithNull : info.Value.AvailableTypeNames;

            int index = atypes.IndexOf(tp);
            EditorGUI.BeginChangeCheck();
            index = EditorGUI.Popup(r0, GUIContent.none, index, anames);
            if (EditorGUI.EndChangeCheck())
            {
                if (index >= 0)
                {
                    tp = atypes[index];
                    property.managedReferenceValue = tp != null ? System.Activator.CreateInstance(tp) : null;
                }
            }
        }


        #region Static Entries

        private static Dictionary<System.Type, TypeInfo> _availableTypes = new Dictionary<System.Type, TypeInfo>();

        public static TypeInfo? GetAvailableTypes(System.Type reftp)
        {
            if (reftp == null) return null;

            TypeInfo info;
            if(_availableTypes.TryGetValue(reftp, out info))
            {
                return info;
            }

            info = new TypeInfo();
            info.AvailableTypes = TypeUtil.GetTypesAssignableFrom(reftp).Where(o => !o.IsAbstract && !o.IsInterface && (o.IsValueType || o.GetConstructor(System.Type.EmptyTypes) != null) && !TypeUtil.IsType(o, typeof(UnityEngine.Object)) && o.IsSerializable).OrderBy(o => o.Name).ToArray();
            info.AvailableTypeNames = info.AvailableTypes.Select(tp =>
            {
                if (info.AvailableTypes.Count(o => string.Equals(o.Name, tp.Name)) > 1)
                {
                    return new GUIContent(tp.Name + " : " + tp.FullName);
                }
                else
                {
                    return new GUIContent(tp.Name);
                }
            }).ToArray();

            info.AvailableTypesWithNull = info.AvailableTypes.Append(null).ToArray();
            info.AvailableTypeNamesWithNull = info.AvailableTypeNames.Append(new GUIContent("NULL")).ToArray();

            _availableTypes[reftp] = info;
            return info;
        }

        public struct TypeInfo
        {
            public System.Type[] AvailableTypes;
            public GUIContent[] AvailableTypeNames;

            public System.Type[] AvailableTypesWithNull;
            public GUIContent[] AvailableTypeNamesWithNull;
        }

        #endregion

    }

}
