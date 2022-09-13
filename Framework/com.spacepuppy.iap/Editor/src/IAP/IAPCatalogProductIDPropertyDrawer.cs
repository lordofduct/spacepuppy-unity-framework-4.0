#if SP_UNITYIAP
using UnityEngine;
using UnityEditor;
using UnityEngine.Purchasing;
using UnityEditor.Purchasing;

using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;

namespace com.spacepuppyeditor.IAP
{

    [CustomPropertyDrawer(typeof(IAPCatalogProductIDAttribute))]
    public class IAPCatalogProductIDPropertyDrawer : PropertyDrawer
    {

        private const string DISPLAY_NOPRODUCT = "<None>";

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h = EditorGUIUtility.singleLineHeight;
            if (property.propertyType == SerializedPropertyType.String) h *= 2f;
            return h;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, "Malformed IAPCatalogProductID.");
                return;
            }

            var r0 = new Rect(position.xMin, position.yMin, position.width, EditorGUIUtility.singleLineHeight);
            var r1 = new Rect(r0.xMin, r0.yMax, r0.width, r0.height);

            var catalog = ProductCatalog.LoadDefaultCatalog();
            var validids = catalog.allProducts.Select(o => o.id).Prepend(DISPLAY_NOPRODUCT).ToArray();

            int currentIndex = string.IsNullOrEmpty(property.stringValue) ? 0 : System.Array.IndexOf(validids, property.stringValue);
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(r0, "Product Id", currentIndex, validids);

            if (EditorGUI.EndChangeCheck())
            {
                if (newIndex > 0 && newIndex < validids.Length)
                {
                    property.stringValue = validids[newIndex];
                }
                else
                {
                    property.stringValue = string.Empty;
                }
            }

            r1 = EditorGUI.PrefixLabel(r1, EditorHelper.TempContent(" "));
            if (GUI.Button(r1, "IAP Catalog..."))
            {
                ProductCatalogEditor.ShowWindow();
            }
        }

    }
}
#endif
