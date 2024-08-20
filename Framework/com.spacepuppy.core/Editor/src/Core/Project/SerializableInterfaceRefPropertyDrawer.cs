using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Dynamic;
using com.spacepuppy.Project;
using com.spacepuppy.Utils;
using com.spacepuppyeditor.Windows;
using com.spacepuppyeditor.Internal;

namespace com.spacepuppyeditor.Core.Project
{

    /// <summary>
    /// Deals with both SerializableInterfaceRef and SelfReducingEntityConfigRef.
    /// </summary>
    [CustomPropertyDrawer(typeof(BaseSerializableInterfaceRef), true)]
    public class SerializableInterfaceRefPropertyDrawer : PropertyDrawer, EditorHelper.ISerializedWrapperHelper
    {

        public const string PROP_OBJ = "_obj";

        private SelectableComponentPropertyDrawer _componentSelectorDrawer = new SelectableComponentPropertyDrawer()
        {
            RestrictionType = typeof(UnityEngine.Object),
            AllowNonComponents = true,
            AllowProxy = true,
            ShowXButton = true,
            XButtonOnRightSide = true,
        };

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h;
            if (EditorHelper.AssertMultiObjectEditingNotSupportedHeight(property, label, out h)) return h;

            var tp = (this.fieldInfo != null) ? this.fieldInfo.FieldType : null;
            var objProp = property.FindPropertyRelative(PROP_OBJ);
            if (tp == null || objProp == null || objProp.propertyType != SerializedPropertyType.ObjectReference)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            if (objProp.objectReferenceValue == null)
            {
                return EditorGUIUtility.singleLineHeight;
            }
            else
            {
                return _componentSelectorDrawer.GetPropertyHeight(objProp, label);
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (EditorHelper.AssertMultiObjectEditingNotSupported(position, property, label)) return;

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

            if (objProp.objectReferenceValue == null)
            {
                object val = SPEditorGUI.AdvancedObjectField(position, label, objProp.objectReferenceValue, valueType, true, true);
                if (val != null && !valueType.IsInstanceOfType(val) && ObjUtil.GetAsFromSource<IProxy>(val) == null)
                {
                    val = null;
                }
                objProp.objectReferenceValue = val as UnityEngine.Object;
            }
            else
            {
                _componentSelectorDrawer.RestrictionType = valueType;
                _componentSelectorDrawer.OnGUI(position, objProp, label);
            }
        }

        private void DrawMalformed(Rect position)
        {
            EditorGUI.LabelField(position, "Malformed SerializedInterfaceRef.");
            Debug.LogError("Malformed SerializedInterfaceRef - make sure you inherit from 'SerializableInterfaceRef'.");
        }


        public static void SetSerializedProperty(SerializedProperty property, UnityEngine.Object obj)
        {
            if (property == null) throw new System.ArgumentNullException(nameof(property));
            var objProp = property.FindPropertyRelative(PROP_OBJ);
            if (objProp != null && objProp.propertyType == SerializedPropertyType.ObjectReference)
            {
                objProp.objectReferenceValue = obj;
            }
        }

        public static UnityEngine.Object GetFromSerializedProperty(SerializedProperty property)
        {
            if (property == null) throw new System.ArgumentNullException(nameof(property));

            return property.FindPropertyRelative(PROP_OBJ)?.objectReferenceValue;
        }

        public static System.Type GetRefTypeFromSerializedProperty(SerializedProperty property)
        {
            if (property == null) throw new System.ArgumentNullException(nameof(property));

            var wrapperType = property.GetTargetType();
            if (TypeUtil.IsType(wrapperType, typeof(SerializableInterfaceRef<>)))
            {
                var valueprop = wrapperType.GetProperty("Value", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                return valueprop?.PropertyType ?? typeof(UnityEngine.Object);
            }

            return typeof(UnityEngine.Object);
        }



        #region EditorHelper.ISerializedWrapperHelper Interface

        object EditorHelper.ISerializedWrapperHelper.GetValue(SerializedProperty property)
        {
            return GetFromSerializedProperty(property);
        }

        bool EditorHelper.ISerializedWrapperHelper.SetValue(SerializedProperty property, object value)
        {
            SetSerializedProperty(property, ObjUtil.GetAsFromSource<UnityEngine.Object>(value));
            return true;
        }

        System.Type EditorHelper.ISerializedWrapperHelper.GetValueType(SerializedProperty property)
        {
            return GetRefTypeFromSerializedProperty(property);
        }

        #endregion

    }

    [CustomPropertyDrawer(typeof(BaseSerializableInterfaceCollection), true)]
    public class BaseSerializableInterfaceCollectionPropertyDrawer : PropertyDrawer
    {

        public const string PROP_ARR = "_arr";

        private SelectableComponentPropertyDrawer _componentSelectorDrawer = new SelectableComponentPropertyDrawer()
        {
            RestrictionType = typeof(UnityEngine.Object),
            AllowNonComponents = true,
            AllowProxy = true,
            ShowXButton = true,
            XButtonOnRightSide = true,
        };
        private CachedReorderableList _lst;
        private GUIContent _label;
        private System.Type _valueType;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h;
            if (EditorHelper.AssertMultiObjectEditingNotSupportedHeight(property, label, out h)) return h;

            var tp = (this.fieldInfo != null) ? this.fieldInfo.FieldType : null;
            var arrprop = property.FindPropertyRelative(PROP_ARR);
            if (tp == null || arrprop == null || !arrprop.isArray)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            _lst = CachedReorderableList.GetListDrawer(property.FindPropertyRelative(PROP_ARR), _maskList_DrawHeader, _maskList_DrawElement);
            _label = label;
            if (arrprop.arraySize == 0)
            {
                _lst.elementHeight = EditorGUIUtility.singleLineHeight;
            }
            else
            {
                _lst.elementHeight = _componentSelectorDrawer.GetPropertyHeight(arrprop.GetArrayElementAtIndex(0), label);
            }
            return _lst.GetHeight();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (EditorHelper.AssertMultiObjectEditingNotSupported(position, property, label)) return;

            var tp = (this.fieldInfo != null) ? this.fieldInfo.FieldType : null;
            var arrprop = property.FindPropertyRelative(PROP_ARR);
            if (tp == null || arrprop == null || !arrprop.isArray)
            {
                this.DrawMalformed(position);
                return;
            }

            _valueType = TypeUtil.GetElementTypeOfListType(DynamicUtil.GetReturnType(DynamicUtil.GetMemberFromType(tp, "_values", true)));
            if (_valueType == null || !(_valueType.IsClass || _valueType.IsInterface))
            {
                this.DrawMalformed(position);
                return;
            }

            _lst = CachedReorderableList.GetListDrawer(property.FindPropertyRelative(PROP_ARR), _maskList_DrawHeader, _maskList_DrawElement);
            _label = label;

            _lst.DoList(position);
        }

        private void DrawMalformed(Rect position)
        {
            EditorGUI.LabelField(position, "Malformed SerializedInterfaceRef.");
            Debug.LogError("Malformed SerializedInterfaceRef - make sure you inherit from 'SerializableInterfaceRef'.");
        }

        #region Text ReorderableList Handlers

        private void _maskList_DrawHeader(Rect area)
        {
            EditorGUI.LabelField(area, _label ?? GUIContent.none);
        }

        private void _maskList_DrawElement(Rect area, int index, bool isActive, bool isFocused)
        {
            var objProp = _lst.serializedProperty.GetArrayElementAtIndex(index);
            var label = EditorHelper.TempContent(string.Format("Element {0:00}", index));

            //SelfReducingEntityConfigRef - support
            try
            {
                var interfaceType = typeof(ISelfReducingEntityConfig<>).MakeGenericType(_valueType);
                if (interfaceType != null && TypeUtil.IsType(_valueType, interfaceType))
                {
                    var childType = typeof(SelfReducingEntityConfigRef<>).MakeGenericType(_valueType);
                    if (TypeUtil.IsType(this.fieldInfo.FieldType, childType))
                    {
                        var obj = EditorHelper.GetTargetObjectOfProperty(objProp);
                        if (obj != null && childType.IsInstanceOfType(obj))
                        {
                            var entity = SPEntity.Pool.GetFromSource(objProp.serializedObject.targetObject);
                            var source = DynamicUtil.GetValue(obj, "GetSourceType", entity);
                            label.text = string.Format("{0} (Found from: {1})", label.text, source);
                        }
                    }
                }
            }
            catch (System.Exception) { }

            if (objProp.objectReferenceValue == null)
            {
                object val = SPEditorGUI.AdvancedObjectField(area, label, objProp.objectReferenceValue, _valueType, true, true);
                if (val != null && !_valueType.IsInstanceOfType(val) && ObjUtil.GetAsFromSource<IProxy>(val) == null)
                {
                    val = null;
                }
                objProp.objectReferenceValue = val as UnityEngine.Object;
            }
            else
            {
                _componentSelectorDrawer.RestrictionType = _valueType;
                _componentSelectorDrawer.OnGUI(area, objProp, label);
            }
        }

        #endregion

    }

}
