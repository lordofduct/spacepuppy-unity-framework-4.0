using UnityEngine;
using UnityEditor;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(System.Enum), true)]
    public class EnumPropertyDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var tp = this.fieldInfo.FieldType;
            if (TypeUtil.IsListType(tp)) tp = TypeUtil.GetElementTypeOfListType(tp);
            if (!tp.IsEnum)
            {
                SPEditorGUI.DefaultPropertyField(position, property, label);
                return;
            }

            if (property.hasMultipleDifferentValues)
            {
                SPEditorGUI.DefaultPropertyField(position, property, label);
            }
            else
            {
                System.Enum e = property.GetEnumValue(tp);
                EditorGUI.BeginChangeCheck();
                e = SPEditorGUI.EnumPopup(position, label, e);
                if (EditorGUI.EndChangeCheck())
                {
                    property.SetEnumValue(e);
                }
            }
        }

    }

}
