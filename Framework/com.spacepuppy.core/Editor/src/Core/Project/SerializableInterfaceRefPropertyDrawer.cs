using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Dynamic;
using com.spacepuppy.Project;
using com.spacepuppy.Utils;

using com.spacepuppyeditor.Internal;
using System.Drawing.Printing;

namespace com.spacepuppyeditor.Core.Project
{

    /// <summary>
    /// Deals with both SerializableInterfaceRef and SelfReducingEntityConfigRef.
    /// </summary>
    [CustomPropertyDrawer(typeof(BaseSerializableInterfaceRef), true)]
    public class SerializableInterfaceRefPropertyDrawer : PropertyDrawer, EditorHelper.ISerializedWrapperHelper
    {

        public const string PROP_UOBJECT = BaseSerializableInterfaceRef.PROP_UOBJECT;
        public const string PROP_REFOBJECT = BaseSerializableInterfaceRef.PROP_REFOBJECT;

        private SelectableComponentPropertyDrawer _componentSelectorDrawer = new SelectableComponentPropertyDrawer()
        {
            RestrictionType = typeof(UnityEngine.Object),
            AllowNonComponents = true,
            AllowProxy = true,
            ShowXButton = true,
            XButtonOnRightSide = true,
        };
        private SerializeRefPickerPropertyDrawer _refSelectorDrawer = new()
        {
            AllowNull = true,
            DisplayBox = false,
            AlwaysExpanded = true,
        };

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h;
            if (EditorHelper.AssertMultiObjectEditingNotSupportedHeight(property, label, out h)) return h;

            System.Type valueType;
            SerializedProperty prop_obj;
            SerializedProperty prop_ref;
            if (!ValidateRefType(property, out valueType, out prop_obj, out prop_ref))
            {
                return EditorGUIUtility.singleLineHeight;
            }

            _refSelectorDrawer.RefType = valueType;
            _componentSelectorDrawer.RestrictionType = valueType;


            if (prop_ref == null)
            {
                if (prop_obj != null && prop_obj.objectReferenceValue != null)
                {
                    return _componentSelectorDrawer.GetPropertyHeight(prop_obj, label);
                }
                else
                {
                    return EditorGUIUtility.singleLineHeight;
                }
            }
            else if (prop_obj == null)
            {
                return _refSelectorDrawer.GetPropertyHeight(prop_ref, label);
            }
            else
            {
                if (prop_ref.managedReferenceValue != null)
                {
                    return _refSelectorDrawer.GetPropertyHeight(prop_ref, label);
                }
                else if (prop_obj.objectReferenceValue != null)
                {
                    return _componentSelectorDrawer.GetPropertyHeight(prop_obj, label);
                }
                else
                {
                    return EditorGUIUtility.singleLineHeight;
                }
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (EditorHelper.AssertMultiObjectEditingNotSupported(position, property, label)) return;

            System.Type valueType;
            SerializedProperty prop_obj;
            SerializedProperty prop_ref;
            if (!ValidateRefType(property, out valueType, out prop_obj, out prop_ref))
            {
                this.DrawMalformed(position);
                return;
            }

            _refSelectorDrawer.RefType = valueType;
            _componentSelectorDrawer.RestrictionType = valueType;

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

            if (prop_ref == null)
            {
                this.DrawUObjectField(position, prop_obj, GUIContent.none, valueType);
            }
            else if(prop_obj == null)
            {
                _refSelectorDrawer.OnGUI(position, prop_ref, label);
            }
            else
            {
                if (prop_ref.managedReferenceValue == null)
                {
                    const float DROP_ARROW_WIDTH = 20f;

                    var r0 = new Rect(position.xMin, position.yMin, position.width, position.height);
                    r0 = EditorGUI.PrefixLabel(r0, label);
                    var r0_a = new Rect(r0.xMin, r0.yMin, r0.width - DROP_ARROW_WIDTH, r0.height);
                    var r0_b = new Rect(r0_a.xMax, r0.yMin, DROP_ARROW_WIDTH, r0.height);

                    this.DrawUObjectField(r0_a, prop_obj, GUIContent.none, valueType);

                    _refSelectorDrawer.OnGUI(r0_b, prop_ref, GUIContent.none);
                }
                else
                {
                    //const float MARGIN = 2f;//
                    //const float MARGIN_DBL = MARGIN * 2f;

                    //var area = position;
                    //position = new Rect(area.xMin + MARGIN, area.yMin, area.width - MARGIN_DBL, area.height);

                    //GUI.BeginGroup(area, GUIContent.none, GUI.skin.box);
                    //GUI.EndGroup();

                    _refSelectorDrawer.OnGUI(position, prop_ref, label);
                    if (prop_obj != null) prop_obj.objectReferenceValue = null;
                }
            }
        }

        private void DrawUObjectField(Rect position, SerializedProperty prop_obj, GUIContent label, System.Type valueType)
        {
            if (prop_obj.objectReferenceValue == null)
            {
                object val = SPEditorGUI.AdvancedObjectField(position, GUIContent.none, prop_obj.objectReferenceValue, valueType, true, true);
                if (val != null && !valueType.IsInstanceOfType(val) && ObjUtil.GetAsFromSource<IProxy>(val) == null)
                {
                    val = null;
                }
                prop_obj.objectReferenceValue = val as UnityEngine.Object;
            }
            else
            {
                _componentSelectorDrawer.OnGUI(position, prop_obj, label);
            }
        }

        private void DrawMalformed(Rect position)
        {
            EditorGUI.LabelField(position, "Malformed SerializedInterfaceRef.");
            Debug.LogError("Malformed SerializedInterfaceRef - make sure you inherit from 'SerializableInterfaceRef'.");
        }


        public static void SetSerializedProperty(SerializedProperty property, object value)
        {
            if (property == null) throw new System.ArgumentNullException(nameof(property));

            var prop_obj = property.FindPropertyRelative(PROP_UOBJECT);
            if (prop_obj != null && prop_obj.propertyType != SerializedPropertyType.ObjectReference) prop_obj = null;
            var prop_ref = property.FindPropertyRelative(PROP_REFOBJECT);
            if (prop_ref != null && prop_ref.propertyType != SerializedPropertyType.ManagedReference) prop_ref = null;

            if (value is UnityEngine.Object uot)
            {
                if (prop_obj != null) prop_obj.objectReferenceValue = uot;
                if (prop_ref != null) prop_ref.managedReferenceValue = null;
            }
            else if (value == null || value.GetType().IsSerializable)
            {
                if (prop_obj != null) prop_obj.objectReferenceValue = null;
                if (prop_ref != null) prop_ref.managedReferenceValue = value;
            }
        }

        public static object GetFromSerializedProperty(SerializedProperty property)
        {
            if (property == null) throw new System.ArgumentNullException(nameof(property));

            var prop_obj = property.FindPropertyRelative(PROP_UOBJECT);
            if (prop_obj != null && prop_obj.propertyType != SerializedPropertyType.ObjectReference) prop_obj = null;
            var prop_ref = property.FindPropertyRelative(PROP_REFOBJECT);
            if (prop_ref != null && prop_ref.propertyType != SerializedPropertyType.ManagedReference) prop_ref = null;

            if (prop_obj != null && prop_obj.objectReferenceValue != null) return prop_obj.objectReferenceValue;
            if (prop_ref != null) return prop_ref.managedReferenceValue;
            return null;
        }

        public static System.Type GetRefTypeFromSerializedProperty(SerializedProperty property)
        {
            if (ValidateRefType(property, out System.Type valueType, out _, out _))
            {
                return valueType;
            }
            else
            {
                return typeof(object);
            }
        }

        static bool ValidateRefType(SerializedProperty property, out System.Type valueType, out SerializedProperty prop_obj, out SerializedProperty prop_ref)
        {
            if (property == null) throw new System.ArgumentNullException(nameof(property));

            prop_obj = property.FindPropertyRelative(PROP_UOBJECT);
            if (prop_obj != null && prop_obj.propertyType != SerializedPropertyType.ObjectReference) prop_obj = null;
            prop_ref = property.FindPropertyRelative(PROP_REFOBJECT);
            if (prop_ref != null && prop_ref.propertyType != SerializedPropertyType.ManagedReference) prop_ref = null;

            if (prop_ref != null)
            {
                valueType = prop_ref.GetManagedReferenceFieldType();
            }
            else
            {
                var wrapperType = property.GetTargetType();
                var valueprop = wrapperType.GetProperty("Value", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                valueType = valueprop?.PropertyType ?? (prop_obj != null ? typeof(UnityEngine.Object) : typeof(object));
            }

            return valueType != null && (valueType.IsClass || valueType.IsInterface) && (prop_obj != null || prop_ref != null);
        }



        #region EditorHelper.ISerializedWrapperHelper Interface

        object EditorHelper.ISerializedWrapperHelper.GetValue(SerializedProperty property)
        {
            return GetFromSerializedProperty(property);
        }

        bool EditorHelper.ISerializedWrapperHelper.SetValue(SerializedProperty property, object value)
        {
            SetSerializedProperty(property, value);
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
