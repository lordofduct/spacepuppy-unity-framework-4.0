using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;

using com.spacepuppyeditor.Core;
using com.spacepuppy.Tween;

namespace com.spacepuppyeditor.Tween
{

    [CustomPropertyDrawer(typeof(EaseStyle))]
    class EaseStylePropertyDrawer : PropertyDrawer
    {

        private static readonly EaseStyle[] _values;
        private static readonly GUIContent[] _displayNames;
        static EaseStylePropertyDrawer()
        {
            //sort so that the new 'OutIn' ease methods get grouped with their related entries
            //basically all OutIn's have the 8th bit flagged, so if that bit is true we multiply 
            //its lower bits by 3 and add 0.5 so that it ends up at 0.5 > the InOut of the same 
            //ease type and 0.5 < the next ease type
            _values = System.Enum.GetValues(typeof(EaseStyle)).Cast<EaseStyle>().OrderBy(o =>
            {
                int i = (int)o;
                return (i & 0x80) != 0 ? (float)((i & 0x7f) + 1) * 3f + 0.5f : (float)i;
            }).ToArray();
            _displayNames = _values.Select(o => new GUIContent(EnumUtil.GetFriendlyName(o))).ToArray();
        }
        

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            int index = System.Array.IndexOf(_values, property.GetEnumValue<EaseStyle>());
            EditorGUI.BeginChangeCheck();
            index = EditorGUI.Popup(position, label, index, _displayNames);
            if(EditorGUI.EndChangeCheck())
            {
                property.SetEnumValue(index >= 0 && index < _values.Length ? _values[index] : EaseStyle.Linear);
            }
        }

    }
}
