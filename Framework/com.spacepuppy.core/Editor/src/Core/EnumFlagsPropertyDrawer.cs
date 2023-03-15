using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
    public class EnumFlagsPropertyDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var attrib = this.attribute as EnumFlagsAttribute;
            var tp = (attrib.EnumType != null && attrib.EnumType.IsEnum) ? attrib.EnumType : this.fieldInfo.FieldType;
            if (tp.IsEnum)
            {
                if (attrib.excluded?.Length > 0)
                {
                    var accepted = (from e in EnumUtil.GetUniqueEnumFlags(tp) select System.Convert.ToInt32(e)).Except(attrib.excluded).ToArray();
                    property.intValue = SPEditorGUI.EnumFlagField(position, tp, accepted, label, property.intValue, false);
                }
                else
                {
                    property.intValue = SPEditorGUI.EnumFlagField(position, tp, label, property.intValue);
                }
            }
            else
            {
                EditorGUI.LabelField(position, label);
            }

            EditorGUI.EndProperty();
        }

    }

}
