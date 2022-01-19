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
            _values = System.Enum.GetValues(typeof(EaseStyle)).Cast<EaseStyle>().OrderBy(o =>
            {
                int i = (int)o;
                return (i & 0x80) != 0 ? (float)((i & 0x7f) + 1) * 3f + 0.5f : (float)i;
            }).ToArray();
            //_values = System.Enum.GetValues(typeof(EaseStyle)).Cast<EaseStyle>().OrderBy(o => (int)o & 0x80).ThenBy(o => (int)o & 0x7f).ToArray();
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
            Debug.Log("DRAW");
        }

    }
}
