#if SP_UNITYIAP
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using com.spacepuppy.IAP;

namespace com.spacepuppyeditor.IAP
{

    [CustomPropertyDrawer(typeof(ProductInfo))]
    public class ProductInfoPropertyDrawer : PropertyDrawer
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var prop = property.FindPropertyRelative(ProductInfo.PROP_PRODUCTID);
            return prop != null ? SPEditorGUI.GetPropertyHeight(prop) : EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var prop_prodid = property.FindPropertyRelative(ProductInfo.PROP_PRODUCTID);
            if (prop_prodid == null)
            {
                EditorHelper.MalformedProperty(position, label);
                return;
            }

            SPEditorGUI.PropertyField(position, prop_prodid);
        }

    }

}
#endif
