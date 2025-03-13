using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.spacepuppy;
using com.spacepuppy.Dynamic;
using com.spacepuppy.Project;
using com.spacepuppy.Utils;

using com.spacepuppyeditor.Internal;

namespace com.spacepuppyeditor.Core.Project
{

    /// <summary>
    /// Deals with both SerializableInterfaceRef and SelfReducingEntityConfigRef.
    /// </summary>
    [CustomPropertyDrawer(typeof(BaseSerializableInterfaceRef), true)]
    public class BaseInterfaceRefPropertyDrawer : PropertyDrawer, EditorHelper.ISerializedWrapperHelper
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
        private SerializeRefPickerPropertyDrawer _refPickerDrawer = new()
        {
            AllowNull = true,
            DisplayBox = false,
            AlwaysExpanded = true,
            NullLabel = null,
        };

        public SerializeRefPickerPropertyDrawer RefPickerDrawer => _refPickerDrawer;
        public RefPickerConfigAttribute OverrideConfigAttribute { get; set; }

        void Configure(System.Type valueType)
        {
            var attrib = this.OverrideConfigAttribute ?? this.fieldInfo?.GetCustomAttribute<RefPickerConfigAttribute>();
            _refPickerDrawer.RefType = valueType;
            _refPickerDrawer.AllowNull = attrib?.AllowNull ?? true;
            _refPickerDrawer.DisplayBox = attrib?.DisplayBox ?? false;
            _refPickerDrawer.AlwaysExpanded = attrib?.AlwaysExpanded ?? true;
            _refPickerDrawer.NullLabel = attrib?.NullLabel;

            _componentSelectorDrawer.RestrictionType = valueType;
        }

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

            this.Configure(valueType);

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
                return _refPickerDrawer.GetPropertyHeight(prop_ref, label);
            }
            else
            {
                if (prop_ref.managedReferenceValue != null)
                {
                    return _refPickerDrawer.GetPropertyHeight(prop_ref, label);
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

            this.Configure(valueType);

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
                this.DrawUObjectField(position, prop_obj, label, valueType);
            }
            else if (prop_obj == null)
            {
                _refPickerDrawer.OnGUI(position, prop_ref, label);
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

                    _refPickerDrawer.OnGUI(r0_b, prop_ref, GUIContent.none);
                }
                else
                {
                    //const float MARGIN = 2f;//
                    //const float MARGIN_DBL = MARGIN * 2f;

                    //var area = position;
                    //position = new Rect(area.xMin + MARGIN, area.yMin, area.width - MARGIN_DBL, area.height);

                    //GUI.BeginGroup(area, GUIContent.none, GUI.skin.box);
                    //GUI.EndGroup();

                    _refPickerDrawer.OnGUI(position, prop_ref, label);
                    if (prop_obj != null) prop_obj.objectReferenceValue = null;
                }
            }
        }

        private void DrawUObjectField(Rect position, SerializedProperty prop_obj, GUIContent label, System.Type valueType)
        {
            if (prop_obj.objectReferenceValue == null)
            {
                object val = SPEditorGUI.AdvancedObjectField(position, label, prop_obj.objectReferenceValue, valueType, true, true);
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

        #region Static Utils

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
                //if (prop_ref != null) prop_ref.managedReferenceValue = null;
                if (prop_ref != null) EditorHelper.SetTargetObjectOfProperty(prop_ref, null);
            }
            else if (value == null || value.GetType().IsSerializable)
            {
                if (prop_obj != null) prop_obj.objectReferenceValue = null;
                //if (prop_ref != null) prop_ref.managedReferenceValue = value;
                if (prop_ref != null) EditorHelper.SetTargetObjectOfProperty(prop_ref, value);
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

        internal static bool ValidateRefType(SerializedProperty property, out System.Type valueType, out SerializedProperty prop_obj, out SerializedProperty prop_ref)
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

        #endregion

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

    [CustomPropertyDrawer(typeof(BaseSerializableInterfaceList), true)]
    public class BaseSerializableInterfaceListPropertyDrawer : PropertyDrawer
    {

        public const string PROP_DATA = BaseSerializableInterfaceList.PROP_DATA;
        static readonly RefPickerConfigAttribute DEFAULT_PICKERCONFIG = new RefPickerConfigAttribute()
        {
            AllowNull = true,
            DisplayBox = false,
            AlwaysExpanded = true,
        };

        const float FOOTER_MARGIN_LEFT = 4f;
        const float FOOTER_MARGIN_RIGHT = 4f;
        const float FOOTER_MARGIN_TOP = 10f;
        const float FOOTER_MARGIN_BOTTOM = 4f;

        #region Fields

        private GUIContent _lstLabel;
        private CachedReorderableList _lstDrawer;
        private BaseInterfaceRefPropertyDrawer _elementDrawer = new BaseInterfaceRefPropertyDrawer();

        #endregion

        #region Properties

        public ReorderableArrayPropertyDrawer.FormatElementLabelCallback FormatElementLabel { get; set; }

        public int SelectedIndex => _lstDrawer?.index ?? -1;

        #endregion

        void Configure(SerializedProperty property, GUIContent label)
        {
            _lstLabel = label;
            _lstDrawer = CachedReorderableList.GetListDrawer(property.FindPropertyRelative(PROP_DATA), _lst_DrawHeader, _lst_DrawElement, onAddCallback: _lst_OnAdd);
            _lstDrawer.elementHeight = EditorGUIUtility.singleLineHeight;
            _elementDrawer.RefPickerDrawer.DrawOnlyPicker = true;
            _elementDrawer.OverrideConfigAttribute = this.fieldInfo?.GetCustomAttribute<RefPickerConfigAttribute>() ?? DEFAULT_PICKERCONFIG;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            this.Configure(property, label);

            float h;
            if (EditorHelper.AssertMultiObjectEditingNotSupportedHeight(property, label, out h)) return h;

            h = _lstDrawer.GetHeight();
            int sz = _lstDrawer.serializedProperty.arraySize;
            int index = _lstDrawer.index;
            if (sz > 0 && index >= 0 && index < sz && TryGetManagedRefChildPropertyIfNotNull(_lstDrawer.serializedProperty.GetArrayElementAtIndex(index), out SerializedProperty refprop))
            {
                h += SerializeRefPickerPropertyDrawer.FindPropertyDrawer(EditorHelper.GetManagedReferenceType(refprop)).GetPropertyHeight(refprop, GUIContent.none);
                h += FOOTER_MARGIN_TOP + FOOTER_MARGIN_BOTTOM;
            }
            return h;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            this.Configure(property, label);
            if (EditorHelper.AssertMultiObjectEditingNotSupported(position, property, label)) return;

            GUI.Box(position, GUIContent.none);
            _lstDrawer.DoList(position);

            int sz = _lstDrawer.serializedProperty.arraySize;
            int index = _lstDrawer.index;
            if (sz > 0 && index >= 0 && index < sz && TryGetManagedRefChildPropertyIfNotNull(_lstDrawer.serializedProperty.GetArrayElementAtIndex(index), out SerializedProperty refprop))
            {
                float h = _lstDrawer.GetHeight();
                position = new Rect(position.xMin + FOOTER_MARGIN_LEFT, position.yMin + h + FOOTER_MARGIN_TOP, position.width - FOOTER_MARGIN_LEFT - FOOTER_MARGIN_RIGHT, position.height - h - FOOTER_MARGIN_TOP - FOOTER_MARGIN_BOTTOM);
                var rheader = new Rect(position.xMin + 5f, position.yMin - EditorGUIUtility.singleLineHeight, position.width * 0.5f, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(rheader, this.GetElementLabel(_lstDrawer.serializedProperty.GetArrayElementAtIndex(index), index, true, true) + ":");
                SerializeRefPickerPropertyDrawer.FindPropertyDrawer(EditorHelper.GetManagedReferenceType(refprop)).OnGUI(position, refprop, GUIContent.none);
            }
        }

        private string GetElementLabel(SerializedProperty element, int index, bool isActive, bool isFocused)
        {
            string slbl = this.FormatElementLabel?.Invoke(element, index, isActive, isFocused);
            if (string.IsNullOrEmpty(slbl)) slbl = $"Element {(index + 1):00}";
            return slbl;
        }

        private void _lst_DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, _lstLabel);
        }

        private void _lst_DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (_lstDrawer == null || _lstDrawer.serializedProperty.arraySize == 0) return;

            var property = _lstDrawer.serializedProperty.GetArrayElementAtIndex(index);
            EditorHelper.SuppressIndentLevel();

            try
            {
                _elementDrawer.OnGUI(rect, property, EditorHelper.TempContent(this.GetElementLabel(property, index, true, true)));
            }
            finally
            {
                EditorHelper.ResumeIndentLevel();
            }
        }

        private void _lst_OnAdd(UnityEditorInternal.ReorderableList lst)
        {
            int index = lst.serializedProperty.arraySize;
            lst.serializedProperty.arraySize++;

            var element = lst.serializedProperty.GetArrayElementAtIndex(index);
            var obj_el = element.FindPropertyRelative("_obj");
            if (obj_el != null) obj_el.objectReferenceValue = null;
            var ref_el = element.FindPropertyRelative("_ref");
            //if (ref_el != null) ref_el.managedReferenceValue = null;
            if (ref_el != null) EditorHelper.SetTargetObjectOfProperty(ref_el, null);
        }

        static bool TryGetManagedRefChildPropertyIfNotNull(SerializedProperty arrayElementProperty, out SerializedProperty refprop)
        {
            var prop = arrayElementProperty.FindPropertyRelative(BaseInterfaceRefPropertyDrawer.PROP_REFOBJECT);
            if (prop != null && prop.managedReferenceValue != null)
            {
                refprop = prop;
                return true;
            }
            else
            {
                refprop = null;
                return false;
            }
        }

        #region Static Utils

        public static object GetFromSerializedProperty(SerializedProperty property, int index)
        {
            if (property == null) throw new System.ArgumentNullException(nameof(property));

            var prop_arr = property.FindPropertyRelative(PROP_DATA);
            if (prop_arr == null || !prop_arr.isArray || index < 0 || index >= prop_arr.arraySize) return null;

            return BaseInterfaceRefPropertyDrawer.GetFromSerializedProperty(prop_arr.GetArrayElementAtIndex(index));
        }

        #endregion

    }

    [CustomPropertyDrawer(typeof(BaseObsoleteInterfaceRefCollection), true)]
    public class BaseSerializableInterfaceCollectionPropertyDrawer : PropertyDrawer
    {

        public const string PROP_ARR_OBSOLETE = BaseObsoleteInterfaceRefCollection.PROP_ARR_OBSOLETE;

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
            var arrprop = property.FindPropertyRelative(PROP_ARR_OBSOLETE);
            if (tp == null || arrprop == null || !arrprop.isArray)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            _lst = CachedReorderableList.GetListDrawer(property.FindPropertyRelative(PROP_ARR_OBSOLETE), _maskList_DrawHeader, _maskList_DrawElement, onAddCallback: _lst_OnAdd);
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
            var arrprop = property.FindPropertyRelative(PROP_ARR_OBSOLETE);
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

            _lst = CachedReorderableList.GetListDrawer(property.FindPropertyRelative(PROP_ARR_OBSOLETE), _maskList_DrawHeader, _maskList_DrawElement);
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

        private void _lst_OnAdd(UnityEditorInternal.ReorderableList lst)
        {
            int index = lst.serializedProperty.arraySize;
            lst.serializedProperty.arraySize++;

            var element = lst.serializedProperty.GetArrayElementAtIndex(index);
            var obj_el = element.FindPropertyRelative("_obj");
            if (obj_el != null) obj_el.objectReferenceValue = null;
            var ref_el = element.FindPropertyRelative("_ref");
            //if (ref_el != null) ref_el.managedReferenceValue = null;
            if (ref_el != null) EditorHelper.SetTargetObjectOfProperty(ref_el, null);
        }

        #endregion

    }

}
