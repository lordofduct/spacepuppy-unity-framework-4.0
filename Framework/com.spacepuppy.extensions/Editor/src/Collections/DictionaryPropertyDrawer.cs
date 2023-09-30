using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Collections;
using com.spacepuppy.Utils;
using com.spacepuppyeditor.Internal;
using System.Reflection;

namespace com.spacepuppyeditor.Collections
{

    [CustomPropertyDrawer(typeof(DrawableDictionary), true)]
    public class DictionaryPropertyDrawer : PropertyDrawer
    {

        private ReorderableList _drawer;
        private GUIContent _currentLabel;
        private SerializedProperty _prop_keys;
        private SerializedProperty _prop_values;
        private object[] _existingKeys;
        private System.Type _keyType;
        private bool _isFlaggingEnum;

        void Initialize(SerializedProperty property, GUIContent label, bool isdraw)
        {
            _currentLabel = label;
            _prop_keys = property.FindPropertyRelative(DrawableDictionary.PROP_KEYS);
            _prop_values = property.FindPropertyRelative(DrawableDictionary.PROP_VALUES);
            _drawer = CachedReorderableList.GetListDrawer(_prop_keys,
                                                          _drawer_DrawHeader,
                                                          _drawer_DrawElement,
                                                          _drawer_OnAdded,
                                                          _drawer_OnRemoved);

            if (!isdraw) return;

            if (_prop_values.arraySize != _prop_keys.arraySize)
            {
                _prop_values.arraySize = _prop_keys.arraySize;
            }

            _keyType = TypeUtil.GetElementTypeOfListType(_prop_keys.GetPropertyValueType(true));
            bool isEnum = _keyType?.IsEnum ?? false;
            _isFlaggingEnum = isEnum && _keyType.GetCustomAttribute<System.FlagsAttribute>() != null;
            using (var hash = TempCollection.GetSet<object>())
            {
                for (int i = 0; i < _prop_keys.arraySize; i++)
                {
                    var val = _prop_keys.GetArrayElementAtIndex(i).GetPropertyValue(true);
                    if (val != null)
                    {
                        if (isEnum) val = ConvertUtil.ToEnumOfType(_keyType, val);
                        hash.Add(val);
                    }
                }
                _existingKeys = isEnum ? hash.Cast<System.Enum>().ToArray() : hash.ToArray();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h;
            if (EditorHelper.AssertMultiObjectEditingNotSupportedHeight(property, label, out h)) return h;

            try
            {
                this.Initialize(property, label, false);
                return _drawer.GetHeight();
            }
            finally
            {
                _drawer = null;
                _currentLabel = null;
                _prop_keys = null;
                _prop_values = null;
                _existingKeys = null;
                _keyType = null;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (EditorHelper.AssertMultiObjectEditingNotSupported(position, property, label)) return;

            try
            {
                this.Initialize(property, label, true);
                _drawer.DoList(position);
            }
            finally
            {
                _drawer = null;
                _currentLabel = null;
                _prop_keys = null;
                _prop_values = null;
                _existingKeys = null;
                _keyType = null;
            }
        }

        private void _drawer_DrawHeader(Rect area)
        {
            EditorGUI.LabelField(area, _currentLabel);
        }

        private void _drawer_DrawElement(Rect area, int index, bool isActive, bool isFocused)
        {
            var prop_key = _prop_keys.GetArrayElementAtIndex(index);
            var prop_values = _prop_values.GetArrayElementAtIndex(index);

            var r0 = new Rect(area.xMin, area.yMin, Mathf.FloorToInt(area.width * 0.3f) - 1, area.height);
            var r1 = new Rect(r0.xMax + 1, area.yMin, area.width - r0.width - 1, area.height);

            if ((_keyType?.IsEnum ?? false) && !_isFlaggingEnum)
            {
                var val = prop_key.GetEnumValue(_keyType);
                SPEditorGUI.EnumPopupExcluding(r0, val, _existingKeys.Cast<System.Enum>().Except(val).ToArray());
            }
            else
            {
                SPEditorGUI.PropertyField(r0, prop_key, GUIContent.none, false);
            }
            SPEditorGUI.PropertyField(r1, prop_values, GUIContent.none, false);
        }

        private void _drawer_OnAdded(ReorderableList lst)
        {
            AddKeyElement(_prop_keys);
            _prop_values.arraySize = _prop_keys.arraySize;
        }

        private void _drawer_OnRemoved(ReorderableList lst)
        {
            int index = lst.index;
            if (index < 0 || index >= _prop_keys.arraySize) index = _prop_keys.arraySize;

            _prop_keys.DeleteArrayElementAtIndex(index);
            _prop_values.DeleteArrayElementAtIndex(index);
        }


        private static void AddKeyElement(SerializedProperty keysProp)
        {
            keysProp.arraySize++;
            var prop = keysProp.GetArrayElementAtIndex(keysProp.arraySize - 1);

            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    {
                        int value = 0;
                        for (int i = 0; i < keysProp.arraySize - 1; i++)
                        {
                            if (keysProp.GetArrayElementAtIndex(i).intValue == value)
                            {
                                value++;
                                if (value == int.MaxValue)
                                    break;
                                else
                                    i = -1;
                            }
                        }
                        prop.intValue = value;
                    }
                    break;
                case SerializedPropertyType.Boolean:
                    {
                        bool value = false;
                        for (int i = 0; i < keysProp.arraySize - 1; i++)
                        {
                            if (keysProp.GetArrayElementAtIndex(i).boolValue == value)
                            {
                                value = true;
                                break;
                            }
                        }
                        prop.boolValue = value;
                    }
                    break;
                case SerializedPropertyType.Float:
                    {
                        float value = 0f;
                        for (int i = 0; i < keysProp.arraySize - 1; i++)
                        {
                            if (keysProp.GetArrayElementAtIndex(i).intValue == value)
                            {
                                value++;
                                if (value == int.MaxValue)
                                    break;
                                else
                                    i = -1;
                            }
                        }
                        prop.floatValue = value;
                    }
                    break;
                case SerializedPropertyType.String:
                    {
                        prop.stringValue = string.Empty;
                    }
                    break;
                case SerializedPropertyType.Color:
                    {
                        Color value = Color.black;
                        for (int i = 0; i < keysProp.arraySize - 1; i++)
                        {
                            if (keysProp.GetArrayElementAtIndex(i).colorValue == value)
                            {
                                value = ConvertUtil.ToColor(ConvertUtil.ToInt(value) + 1);
                                if (value == Color.white)
                                    break;
                                else
                                    i = -1;
                            }
                        }
                        prop.colorValue = value;
                    }
                    break;
                case SerializedPropertyType.ObjectReference:
                    {
                        prop.objectReferenceValue = null;
                    }
                    break;
                case SerializedPropertyType.LayerMask:
                    {
                        int value = -1;
                        for (int i = 0; i < keysProp.arraySize - 1; i++)
                        {
                            if (keysProp.GetArrayElementAtIndex(i).intValue == value)
                            {
                                value++;
                                if (value == int.MaxValue)
                                    break;
                                else
                                    i = -1;
                            }
                        }
                        prop.intValue = value;
                    }
                    break;
                case SerializedPropertyType.Enum:
                    {
                        int value = 0;
                        if (keysProp.arraySize > 1)
                        {
                            var first = keysProp.GetArrayElementAtIndex(0);
                            int max = first.enumNames.Length - 1;

                            for (int i = 0; i < keysProp.arraySize - 1; i++)
                            {
                                if (keysProp.GetArrayElementAtIndex(i).enumValueIndex == value)
                                {
                                    value++;
                                    if (value >= max)
                                        break;
                                    else
                                        i = -1;
                                }
                            }
                        }
                        prop.enumValueIndex = value;
                    }
                    break;
                case SerializedPropertyType.Vector2:
                    {
                        Vector2 value = Vector2.zero;
                        for (int i = 0; i < keysProp.arraySize - 1; i++)
                        {
                            if (keysProp.GetArrayElementAtIndex(i).vector2Value == value)
                            {
                                value.x++;
                                if (value.x == int.MaxValue)
                                    break;
                                else
                                    i = -1;
                            }
                        }
                        prop.vector2Value = value;
                    }
                    break;
                case SerializedPropertyType.Vector3:
                    {
                        Vector3 value = Vector3.zero;
                        for (int i = 0; i < keysProp.arraySize - 1; i++)
                        {
                            if (keysProp.GetArrayElementAtIndex(i).vector3Value == value)
                            {
                                value.x++;
                                if (value.x == int.MaxValue)
                                    break;
                                else
                                    i = -1;
                            }
                        }
                        prop.vector3Value = value;
                    }
                    break;
                case SerializedPropertyType.Vector4:
                    {
                        Vector4 value = Vector4.zero;
                        for (int i = 0; i < keysProp.arraySize - 1; i++)
                        {
                            if (keysProp.GetArrayElementAtIndex(i).vector4Value == value)
                            {
                                value.x++;
                                if (value.x == int.MaxValue)
                                    break;
                                else
                                    i = -1;
                            }
                        }
                        prop.vector4Value = value;
                    }
                    break;
                case SerializedPropertyType.Rect:
                    {
                        prop.rectValue = Rect.zero;
                    }
                    break;
                case SerializedPropertyType.ArraySize:
                    {
                        int value = 0;
                        for (int i = 0; i < keysProp.arraySize - 1; i++)
                        {
                            if (keysProp.GetArrayElementAtIndex(i).arraySize == value)
                            {
                                value++;
                                if (value == int.MaxValue)
                                    break;
                                else
                                    i = -1;
                            }
                        }
                        prop.arraySize = value;
                    }
                    break;
                case SerializedPropertyType.Character:
                    {
                        int value = 0;
                        for (int i = 0; i < keysProp.arraySize - 1; i++)
                        {
                            if (keysProp.GetArrayElementAtIndex(i).intValue == value)
                            {
                                value++;
                                if (value == char.MaxValue)
                                    break;
                                else
                                    i = -1;
                            }
                        }
                        prop.intValue = value;
                    }
                    break;
                case SerializedPropertyType.AnimationCurve:
                    {
                        prop.animationCurveValue = null;
                    }
                    break;
                case SerializedPropertyType.Bounds:
                    {
                        prop.boundsValue = default(Bounds);
                    }
                    break;
                default:
                    throw new System.InvalidOperationException("Can not handle Type as key.");
            }
        }

        private static void SetPropertyDefault(SerializedProperty prop)
        {
            if (prop == null) throw new System.ArgumentNullException("prop");

            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    prop.intValue = 0;
                    break;
                case SerializedPropertyType.Boolean:
                    prop.boolValue = false;
                    break;
                case SerializedPropertyType.Float:
                    prop.floatValue = 0f;
                    break;
                case SerializedPropertyType.String:
                    prop.stringValue = string.Empty;
                    break;
                case SerializedPropertyType.Color:
                    prop.colorValue = Color.black;
                    break;
                case SerializedPropertyType.ObjectReference:
                    prop.objectReferenceValue = null;
                    break;
                case SerializedPropertyType.LayerMask:
                    prop.intValue = -1;
                    break;
                case SerializedPropertyType.Enum:
                    prop.enumValueIndex = 0;
                    break;
                case SerializedPropertyType.Vector2:
                    prop.vector2Value = Vector2.zero;
                    break;
                case SerializedPropertyType.Vector3:
                    prop.vector3Value = Vector3.zero;
                    break;
                case SerializedPropertyType.Vector4:
                    prop.vector4Value = Vector4.zero;
                    break;
                case SerializedPropertyType.Rect:
                    prop.rectValue = Rect.zero;
                    break;
                case SerializedPropertyType.ArraySize:
                    prop.arraySize = 0;
                    break;
                case SerializedPropertyType.Character:
                    prop.intValue = 0;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    prop.animationCurveValue = null;
                    break;
                case SerializedPropertyType.Bounds:
                    prop.boundsValue = default(Bounds);
                    break;
                case SerializedPropertyType.Gradient:
                    throw new System.InvalidOperationException("Can not handle Gradient types.");
            }
        }

    }
}
