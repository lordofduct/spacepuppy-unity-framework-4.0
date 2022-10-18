using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(ServiceComponent<>.ServiceRegistrationOptions))]
    [CustomPropertyDrawer(typeof(ServiceScriptableObject<>.ServiceRegistrationOptions))]
    public class ServiceRegistrationOptionsPropertyDrawer : PropertyDrawer
    {
        private static float TOP_PAD => 2f + EditorGUIUtility.singleLineHeight * 2f;
        private const float BOTTOM_PAD = 2f;
        private const float MARGIN = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3f + BOTTOM_PAD + TOP_PAD;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var drawArea = new Rect(position.xMin, position.yMin + TOP_PAD, position.width - MARGIN, position.height - TOP_PAD);

            var targtp = property.serializedObject.GetTargetType();
            var servicetp = this.GetBaseServiceType(property);
            label.text = string.Format("Service Registration Options\r\n(this service is registered as type '{0}')", servicetp?.Name ?? "-");

            GUI.BeginGroup(position, label, GUI.skin.box);
            GUI.EndGroup();

            EditorGUI.indentLevel++;

            float yMin = drawArea.yMin;
            foreach (var child in property.GetChildren())
            {
                var r = new Rect(drawArea.xMin, yMin, drawArea.width, EditorGUIUtility.singleLineHeight);
                yMin += EditorGUIUtility.singleLineHeight;

                SPEditorGUI.PropertyField(r, child);
            }
            EditorGUI.indentLevel--;
        }

        private System.Type GetBaseServiceType(SerializedProperty property)
        {
            var targtype = property.serializedObject.GetTargetType();
            while(targtype != null && targtype != typeof(object))
            {
                if (targtype.IsGenericType)
                {
                    var gtp = targtype.GetGenericTypeDefinition();
                    if (gtp == typeof(ServiceComponent<>) || gtp == typeof(ServiceScriptableObject<>))
                    {
                        return targtype.GetGenericArguments()[0];
                    }
                }
                targtype = targtype.BaseType;
            }
            return null;
        }

    }

}
