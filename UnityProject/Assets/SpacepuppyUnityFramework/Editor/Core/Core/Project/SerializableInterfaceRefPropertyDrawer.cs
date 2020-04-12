using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Dynamic;
using com.spacepuppy.Project;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Core.Project
{

    /// <summary>
    /// Deals with both SerializableInterfaceRef and SelfReducingEntityConfigRef.
    /// </summary>
    [CustomPropertyDrawer(typeof(BaseSerializableInterfaceRef), true)]
    public class SerializableInterfaceRefPropertyDrawer : PropertyDrawer
    {

        public const string PROP_OBJ = "_obj";

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var tp = (this.fieldInfo != null) ? this.fieldInfo.FieldType : null;
            var objProp = property.FindPropertyRelative(PROP_OBJ);
            if (tp == null || objProp == null || objProp.propertyType != SerializedPropertyType.ObjectReference)
            {
                this.DrawMalformed(position);
                return;
            }

            var valueType = DynamicUtil.GetReturnType(DynamicUtil.GetMemberFromType(tp, "_value", true));
            if (valueType == null || !(valueType.IsClass || valueType.IsInterface))
            {
                this.DrawMalformed(position);
                return;
            }

            //SelfReducingEntityConfigRef - support
            try
            {
                var interfaceType = typeof(ISelfReducingEntityConfig<>).MakeGenericType(valueType);
                if (interfaceType != null && TypeUtil.IsType(valueType, interfaceType))
                {
                    var childType = typeof(SelfReducingEntityConfigRef<>).MakeGenericType(valueType);
                    if (TypeUtil.IsType(this.fieldInfo.FieldType, childType))
                    {
                        var obj = EditorHelper.GetTargetObjectOfProperty(property);
                        if (obj != null && childType.IsInstanceOfType(obj))
                        {
                            var entity = SPEntity.Pool.GetFromSource(property.serializedObject.targetObject);
                            var source = DynamicUtil.GetValue(obj, "GetSourceType", entity);
                            label.text = string.Format("{0} (Found from: {1})", label.text, source);
                        }
                    }
                }
            }
            catch (System.Exception) { }

            var val = ObjUtil.GetAsFromSource(valueType, EditorGUI.ObjectField(position, label, objProp.objectReferenceValue, typeof(UnityEngine.Object), true));
            if (val != null && !valueType.IsInstanceOfType(val))
            {
                val = null;
            }
            objProp.objectReferenceValue = val as UnityEngine.Object;
        }

        private void DrawMalformed(Rect position)
        {
            EditorGUI.LabelField(position, "Malformed SerializedInterfaceRef.");
            Debug.LogError("Malformed SerializedInterfaceRef - make sure you inherit from 'SerializableInterfaceRef'.");
        }

    }

}
