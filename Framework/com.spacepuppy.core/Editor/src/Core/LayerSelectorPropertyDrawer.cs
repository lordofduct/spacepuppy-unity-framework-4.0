using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(LayerSelectorAttribute))]
    public class LayerSelectorPropertyDrawer : PropertyDrawer
    {

        LayerUtil.LayerInfo[] _layers = null;
        GUIContent[] _options = null;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_layers == null)
            {
                _layers = LayerUtil.GetLayers().ToArray();
                _options = _layers.Select(o => new GUIContent(o.Name)).ToArray();
            }

            EditorGUI.BeginChangeCheck();
            int index = _layers.IndexOf(o => o.Layer == property.intValue);
            index = EditorGUI.Popup(position, label, index, _options);
            if (EditorGUI.EndChangeCheck())
            {
                property.intValue = index >= 0 ? _layers[index].Layer : 0;
            }
        }

    }

}
