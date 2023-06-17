using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppyeditor.Windows;

namespace com.spacepuppyeditor
{

    public static class SPEditorGUILayout
    {

        public static void OnGUILayout(this PropertyDrawer drawer, SerializedProperty property) => OnGUILayout(drawer, property, EditorHelper.TempContent(property?.displayName ?? string.Empty, property?.tooltip ?? string.Empty));
        public static void OnGUILayout(this PropertyDrawer drawer, SerializedProperty property, string label) => OnGUILayout(drawer, property, EditorHelper.TempContent(label));
        public static void OnGUILayout(this PropertyDrawer drawer, SerializedProperty property, GUIContent label)
        {
            var rect = EditorGUILayout.GetControlRect(com.spacepuppyeditor.Internal.UnityInternalPropertyHandler.LabelHasContent(label), drawer.GetPropertyHeight(property, label ?? GUIContent.none));
            drawer.OnGUI(rect, property, label ?? GUIContent.none);
        }

        #region DefaultPropertyField

        public static bool DefaultPropertyField(SerializedProperty property)
        {
            //return com.spacepuppyeditor.Internal.DefaultPropertyHandler.Instance.OnGUILayout(property, GUIContent.none, false, null);
            return com.spacepuppyeditor.Internal.ScriptAttributeUtility.SharedNullPropertyHandler.OnGUILayout(property, GUIContent.none, false, null);
        }

        public static bool DefaultPropertyField(SerializedProperty property, GUIContent label)
        {
            //return com.spacepuppyeditor.Internal.DefaultPropertyHandler.Instance.OnGUILayout(property, label, false, null);
            return com.spacepuppyeditor.Internal.ScriptAttributeUtility.SharedNullPropertyHandler.OnGUILayout(property, label, false, null);
        }

        public static bool DefaultPropertyField(SerializedProperty property, GUIContent label, bool includeChildren)
        {
            //return com.spacepuppyeditor.Internal.DefaultPropertyHandler.Instance.OnGUILayout(property, label, false, null);
            return com.spacepuppyeditor.Internal.ScriptAttributeUtility.SharedNullPropertyHandler.OnGUILayout(property, label, includeChildren, null);
        }

        public static object DefaultPropertyField(string label, object value, System.Type valueType)
        {
            return SPEditorGUI.DefaultPropertyField(EditorGUILayout.GetControlRect(true, SPEditorGUI.GetDefaultPropertyHeight(value, valueType)), EditorHelper.TempContent(label), value, valueType);
        }

        public static object DefaultPropertyField(GUIContent label, object value, System.Type valueType)
        {
            return SPEditorGUI.DefaultPropertyField(EditorGUILayout.GetControlRect(true, SPEditorGUI.GetDefaultPropertyHeight(value, valueType)), label, value, valueType);
        }

        #endregion

        #region PropertyFields

        public static bool PropertyField(SerializedObject obj, string prop)
        {
            if (obj == null) throw new System.ArgumentNullException("obj");

            var serial = obj.FindProperty(prop);
            if (serial != null)
            {
                EditorGUI.BeginChangeCheck();
                //EditorGUILayout.PropertyField(serial);
                SPEditorGUILayout.PropertyField(serial);
                return EditorGUI.EndChangeCheck();
            }

            return false;
        }

        public static bool PropertyField(SerializedObject obj, string prop, bool includeChildren)
        {
            if (obj == null) throw new System.ArgumentNullException("obj");

            var serial = obj.FindProperty(prop);
            if (serial != null)
            {
                EditorGUI.BeginChangeCheck();
                //EditorGUILayout.PropertyField(serial, includeChildren);
                SPEditorGUILayout.PropertyField(serial, includeChildren);
                return EditorGUI.EndChangeCheck();
            }

            return false;
        }

        public static bool PropertyField(SerializedObject obj, string prop, string label, bool includeChildren)
        {
            return SPEditorGUILayout.PropertyField(obj, prop, EditorHelper.TempContent(label), includeChildren);
        }

        public static bool PropertyField(SerializedObject obj, string prop, GUIContent label, bool includeChildren)
        {
            if (obj == null) throw new System.ArgumentNullException("obj");

            var serial = obj.FindProperty(prop);
            if (serial != null)
            {
                EditorGUI.BeginChangeCheck();
                //EditorGUILayout.PropertyField(serial, label, includeChildren);
                SPEditorGUILayout.PropertyField(serial, label, includeChildren);
                return EditorGUI.EndChangeCheck();
            }

            return false;
        }

        public static bool PropertyField(SerializedProperty property, params GUILayoutOption[] options)
        {
            return SPEditorGUILayout.PropertyField(property, (GUIContent)null, false, options);
        }

        public static bool PropertyField(SerializedProperty property, GUIContent label, params GUILayoutOption[] options)
        {
            return SPEditorGUILayout.PropertyField(property, label, false, options);
        }

        public static bool PropertyField(SerializedProperty property, bool includeChildren, params GUILayoutOption[] options)
        {
            return com.spacepuppyeditor.Internal.ScriptAttributeUtility.GetHandler(property).OnGUILayout(property, null, includeChildren, options);
        }

        public static bool PropertyField(SerializedProperty property, GUIContent label, bool includeChildren, params GUILayoutOption[] options)
        {
            return com.spacepuppyeditor.Internal.ScriptAttributeUtility.GetHandler(property).OnGUILayout(property, label, includeChildren, options);
        }

        #endregion

        #region FlatPropertyField

        /// <summary>
        /// Draws all children of a property
        /// </summary>
        /// <param name="position"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static bool FlatChildPropertyField(SerializedProperty property)
        {
            if (property == null) throw new System.ArgumentNullException("property");

            EditorGUI.BeginChangeCheck();
            var iterator = property.Copy();
            var end = property.GetEndProperty();
            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                if (SerializedProperty.EqualContents(iterator, end))
                    break;

                PropertyField(iterator, EditorHelper.TempContent(iterator.displayName, iterator.tooltip), true);
            }
            return EditorGUI.EndChangeCheck();
        }

        public static bool FlatChildPropertyFieldExcept(SerializedProperty property, params string[] names)
        {
            if (property == null) throw new System.ArgumentNullException("property");

            EditorGUI.BeginChangeCheck();
            var iterator = property.Copy();
            var end = property.GetEndProperty();
            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                if (SerializedProperty.EqualContents(iterator, end))
                    break;

                if (names?.Contains(iterator.name) ?? false) continue;

                PropertyField(iterator, EditorHelper.TempContent(iterator.displayName, iterator.tooltip), true);
            }
            return EditorGUI.EndChangeCheck();
        }

        public static bool FlatChildPropertyFieldExcept(SerializedProperty property, System.Func<SerializedProperty, bool> callback)
        {
            if (property == null) throw new System.ArgumentNullException("property");

            EditorGUI.BeginChangeCheck();
            var iterator = property.Copy();
            var end = property.GetEndProperty();
            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                if (SerializedProperty.EqualContents(iterator, end))
                    break;

                if (callback?.Invoke(iterator) ?? false) continue;

                PropertyField(iterator, EditorHelper.TempContent(iterator.displayName, iterator.tooltip), true);
            }
            return EditorGUI.EndChangeCheck();
        }

        #endregion

        #region ObjectField w/ X-btn

        public static void ObjectFieldX(SerializedProperty property, GUIContent label)
        {
            var position = EditorGUILayout.GetControlRect(true);
            SPEditorGUI.ObjectFieldX(position, property, label);
        }

        public static UnityEngine.Object ObjectFieldX(UnityEngine.Object obj, System.Type objType, bool allowSceneObjects)
        {
            var position = EditorGUILayout.GetControlRect(true);
            return SPEditorGUI.ObjectFieldX(position, obj, objType, allowSceneObjects);
        }

        public static void ObjectFieldX(SerializedProperty property, System.Type objType, GUIContent label)
        {
            var position = EditorGUILayout.GetControlRect(true);
            SPEditorGUI.ObjectFieldX(position, property, objType, label);
        }

        public static UnityEngine.Object ObjectFieldX(GUIContent label, UnityEngine.Object obj, System.Type objType, bool allowSceneObjects)
        {
            var position = EditorGUILayout.GetControlRect(true);
            return SPEditorGUI.ObjectFieldX(position, label, obj, objType, allowSceneObjects);
        }

        public static UnityEngine.Object ObjectFieldX(string label, UnityEngine.Object obj, System.Type objType, bool allowSceneObjects)
        {
            var position = EditorGUILayout.GetControlRect(true);
            return SPEditorGUI.ObjectFieldX(position, label, obj, objType, allowSceneObjects);
        }

        public static UnityEngine.Object ObjectFieldX(UnityEngine.Object obj, System.Predicate<UnityEngine.Object> objFilter, bool allowSceneObjects)
        {
            var position = EditorGUILayout.GetControlRect(false);
            return SPEditorGUI.ObjectFieldX(position, obj, objFilter, allowSceneObjects);
        }

        public static void ObjectFieldX(SerializedProperty property, System.Predicate<UnityEngine.Object> objFilter, GUIContent label, bool allowSceneObjects)
        {
            var position = EditorGUILayout.GetControlRect(true);
            SPEditorGUI.ObjectFieldX(position, property, objFilter, label, allowSceneObjects);
        }

        public static UnityEngine.Object ObjectFieldX(GUIContent label, UnityEngine.Object obj, System.Predicate<UnityEngine.Object> objFilter, bool allowSceneObjects)
        {
            var position = EditorGUILayout.GetControlRect(true);
            return SPEditorGUI.ObjectFieldX(position, label, obj, objFilter, allowSceneObjects);
        }

        public static UnityEngine.Object ObjectFieldX(string label, UnityEngine.Object obj, System.Predicate<UnityEngine.Object> objFilter, bool allowSceneObjects)
        {
            var position = EditorGUILayout.GetControlRect(true);
            return SPEditorGUI.ObjectFieldX(position, label, obj, objFilter, allowSceneObjects);
        }

        #endregion

        #region DateTimeField

        public static System.DateTime DateTimeField(System.DateTime dateTime)
        {
            return SPEditorGUI.DateTimeField(EditorGUILayout.GetControlRect(false), dateTime);
        }

        public static System.DateTime DateTimeField(string label, System.DateTime dateTime)
        {
            return SPEditorGUI.DateTimeField(EditorGUILayout.GetControlRect(true), label, dateTime);
        }

        public static System.DateTime DateTimeField(GUIContent label, System.DateTime dateTime)
        {
            return SPEditorGUI.DateTimeField(EditorGUILayout.GetControlRect(true), label, dateTime);
        }

        #endregion

        #region TimeSpan Field

        public static System.TimeSpan TimeSpanField(System.TimeSpan ts)
        {
            return SPEditorGUI.TimeSpanField(EditorGUILayout.GetControlRect(false), ts);
        }

        public static System.TimeSpan TimeSpanField(string label, System.TimeSpan ts)
        {
            return SPEditorGUI.TimeSpanField(EditorGUILayout.GetControlRect(true), label, ts);
        }

        public static System.TimeSpan TimeSpanField(GUIContent label, System.TimeSpan ts)
        {
            return SPEditorGUI.TimeSpanField(EditorGUILayout.GetControlRect(true), label, ts);
        }

        #endregion

        #region LayerMaskField

        public static LayerMask LayerMaskField(string label, int selectedMask)
        {
            return EditorGUILayout.MaskField(label, selectedMask, LayerUtil.GetAllLayerNames());
        }

        #endregion

        #region EnumPopup Inspector

        public static System.Enum EnumPopup(System.Enum enumValue)
        {
            return SPEditorGUI.EnumPopup(EditorGUILayout.GetControlRect(false), enumValue);
        }

        public static System.Enum EnumPopup(string label, System.Enum enumValue)
        {
            return SPEditorGUI.EnumPopup(EditorGUILayout.GetControlRect(true), EditorHelper.TempContent(label), enumValue);
        }

        public static System.Enum EnumPopup(GUIContent label, System.Enum enumValue)
        {
            return SPEditorGUI.EnumPopup(EditorGUILayout.GetControlRect(label != null && label != GUIContent.none), label, enumValue);
        }

        public static System.Enum EnumPopupExcluding(System.Enum enumValue, params System.Enum[] ignoredValues)
        {
            return SPEditorGUI.EnumPopupExcluding(EditorGUILayout.GetControlRect(false), enumValue, ignoredValues);
        }

        public static System.Enum EnumPopupExcluding(string label, System.Enum enumValue, params System.Enum[] ignoredValues)
        {
            return SPEditorGUI.EnumPopupExcluding(EditorGUILayout.GetControlRect(true), EditorHelper.TempContent(label), enumValue, ignoredValues);
        }

        public static System.Enum EnumPopupExcluding(GUIContent label, System.Enum enumValue, params System.Enum[] ignoredValues)
        {
            return SPEditorGUI.EnumPopupExcluding(EditorGUILayout.GetControlRect(label != null && label != GUIContent.none), label, enumValue, ignoredValues);
        }

        #endregion

        #region Option Popup w/ Custom

        public static string OptionPopupWithCustom(string value, string[] options, GUIContent[] guiOptions = null)
        {
            return SPEditorGUI.OptionPopupWithCustom(EditorGUILayout.GetControlRect(false), GUIContent.none, value, options, guiOptions);
        }

        public static string OptionPopupWithCustom(string label, string value, string[] options, GUIContent[] guiOptions = null)
        {
            return SPEditorGUI.OptionPopupWithCustom(EditorGUILayout.GetControlRect(true), label, value, options, guiOptions);
        }

        public static string OptionPopupWithCustom(GUIContent label, string value, string[] options, GUIContent[] guiOptions = null)
        {
            return SPEditorGUI.OptionPopupWithCustom(EditorGUILayout.GetControlRect(label != null && label != GUIContent.none), label, value, options, guiOptions);
        }

        #endregion

        #region EnumFlag Inspector

        public static int EnumFlagField(System.Type enumType, int value)
        {
            //var names = (from e in EnumUtil.GetUniqueEnumFlags(enumType) select e.ToString()).ToArray();
            //return EditorGUILayout.MaskField(value, names);

            var enums = EnumUtil.GetUniqueEnumFlags(enumType).ToArray();
            var names = (from e in enums select e.ToString()).ToArray();
            int mask = EditorHelper.ConvertEnumMaskToPopupMask(value, enums);
            mask = EditorGUILayout.MaskField(mask, names);
            return EditorHelper.ConvertPopupMaskToEnumMask(mask, enums);
        }

        public static System.Enum EnumFlagField(System.Enum value)
        {
            if (value == null) throw new System.ArgumentException("Enum value must be non-null.", "value");

            var enumType = value.GetType();
            int i = EnumFlagField(enumType, System.Convert.ToInt32(value));
            return System.Enum.ToObject(enumType, i) as System.Enum;
        }

        public static int EnumFlagField(System.Type enumType, GUIContent label, int value)
        {
            //var names = (from e in EnumUtil.GetUniqueEnumFlags(enumType) select e.ToString()).ToArray();
            //return EditorGUILayout.MaskField(label, value, names);

            var enums = EnumUtil.GetUniqueEnumFlags(enumType).ToArray();
            var names = (from e in enums select e.ToString()).ToArray();
            int mask = EditorHelper.ConvertEnumMaskToPopupMask(value, enums);
            mask = EditorGUILayout.MaskField(mask, names);
            return EditorHelper.ConvertPopupMaskToEnumMask(mask, enums);
        }

        public static System.Enum EnumFlagField(GUIContent label, System.Enum value)
        {
            if (value == null) throw new System.ArgumentException("Enum value must be non-null.", "value");

            var enumType = value.GetType();
            int i = EnumFlagField(enumType, label, System.Convert.ToInt32(value));
            return System.Enum.ToObject(enumType, i) as System.Enum;
        }

        public static int EnumFlagField(System.Type enumType, int[] acceptedFlags, GUIContent label, int value, bool allowNegativeOneAsEverything)
        {
            var position = EditorGUILayout.GetControlRect(true);
            return SPEditorGUI.EnumFlagField(position, enumType, acceptedFlags, label, value, allowNegativeOneAsEverything);
        }

        public static WrapMode WrapModeField(string label, WrapMode mode, bool allowDefault = false)
        {
            var position = EditorGUILayout.GetControlRect(true);
            return SPEditorGUI.WrapModeField(position, label, mode, allowDefault);
        }

        public static WrapMode WrapModeField(GUIContent label, WrapMode mode, bool allowDefault = false)
        {
            var position = EditorGUILayout.GetControlRect(label != null && label != GUIContent.none);
            return SPEditorGUI.WrapModeField(position, label, mode, allowDefault);
        }

        #endregion

        #region Type Dropdown

        public static System.Type TypeDropDown(GUIContent label,
                                               System.Type selectedType,
                                               System.Type baseType = null,
                                               bool allowAbstractTypes = false, bool allowInterfaces = false, bool allowGeneric = false,
                                               System.Type defaultType = null, System.Type[] excludedTypes = null,
                                               TypeDropDownListingStyle listType = TypeDropDownListingStyle.Flat,
                                               System.Func<System.Type, string, bool> searchFilter = null,
                                               int maxVisisbleCount = TypeDropDownWindowSelector.DEFAULT_MAXCOUNT)
        {
            var position = EditorGUILayout.GetControlRect(true);
            return SPEditorGUI.TypeDropDown(position, label, selectedType, baseType, allowAbstractTypes, allowInterfaces, allowGeneric, defaultType, excludedTypes, listType, searchFilter, maxVisisbleCount);
        }

        public static System.Type TypeDropDown(GUIContent label,
                                               System.Type selectedType,
                                               IEnumerable<System.Type> typeEnumerator,
                                               System.Type baseType = null, System.Type defaultType = null,
                                               TypeDropDownListingStyle listType = TypeDropDownListingStyle.Flat,
                                               System.Func<System.Type, string, bool> searchFilter = null,
                                               int maxVisisbleCount = TypeDropDownWindowSelector.DEFAULT_MAXCOUNT)
        {
            var position = EditorGUILayout.GetControlRect(true);
            return SPEditorGUI.TypeDropDown(position, label, selectedType, typeEnumerator, baseType, defaultType, listType, searchFilter, maxVisisbleCount);
        }

        public static System.Type TypeDropDown(GUIContent label,
                                               System.Type selectedType,
                                               System.Func<System.Type, bool> enumeratePredicate,
                                               System.Type baseType = null, System.Type defaultType = null,
                                               TypeDropDownListingStyle listType = TypeDropDownListingStyle.Flat,
                                               System.Func<System.Type, string, bool> searchFilter = null,
                                               int maxVisisbleCount = TypeDropDownWindowSelector.DEFAULT_MAXCOUNT)
        {
            var position = EditorGUILayout.GetControlRect(true);
            return SPEditorGUI.TypeDropDown(position, label, selectedType, enumeratePredicate, baseType, defaultType, listType, searchFilter, maxVisisbleCount);
        }

        #endregion

        #region Quaternion Field

        public static Quaternion QuaternionField(GUIContent label, Quaternion value, bool useRadians = false)
        {
            var position = EditorGUILayout.GetControlRect(true);
            return SPEditorGUI.QuaternionField(position, label, value, useRadians);
        }

        #endregion

        #region IComponentField

        public static Component ComponentField(GUIContent label, Component value, System.Type inheritsFromType, bool allowSceneObjects)
        {
            var position = EditorGUILayout.GetControlRect(true);
            return SPEditorGUI.ComponentField(position, label, value, inheritsFromType, allowSceneObjects);
        }

        #endregion

        #region Component Selection From Source

        public static Component SelectComponentFromSourceField(string label, GameObject source, Component selectedComp, System.Predicate<Component> filter = null)
        {
            var position = EditorGUILayout.GetControlRect(true);
            return SPEditorGUI.SelectComponentFromSourceField(position, EditorHelper.TempContent(label), source, selectedComp, filter);
        }

        public static Component SelectComponentFromSourceField(GUIContent label, GameObject source, Component selectedComp, System.Predicate<Component> filter = null)
        {
            var position = EditorGUILayout.GetControlRect(true);
            return SPEditorGUI.SelectComponentFromSourceField(position, label, source, selectedComp, filter);
        }

        public static Component SelectComponentField(string label, Component[] components, Component selectedComp)
        {
            var position = EditorGUILayout.GetControlRect(true);
            return SPEditorGUI.SelectComponentField(position, EditorHelper.TempContent(label), components, selectedComp);
        }

        public static Component SelectComponentField(GUIContent label, Component[] components, Component selectedComp)
        {
            var position = EditorGUILayout.GetControlRect(true);
            return SPEditorGUI.SelectComponentField(position, label, components, selectedComp);
        }

        public static Component SelectComponentField(string label, Component[] components, string[] componentLabels, Component selectedComp)
        {
            var position = EditorGUILayout.GetControlRect(true);
            return SPEditorGUI.SelectComponentField(position, label, components, componentLabels, selectedComp);
        }

        public static Component SelectComponentField(GUIContent label, Component[] components, GUIContent[] componentLabels, Component selectedComp)
        {
            var position = EditorGUILayout.GetControlRect(true);
            return SPEditorGUI.SelectComponentField(position, label, components, componentLabels, selectedComp);
        }

        #endregion

        #region Path Textfields

        public static string FolderPathTextfield(string label, string path, string popupTitle)
        {
            return SPEditorGUI.FolderPathTextfield(EditorGUILayout.GetControlRect(true), label, path, popupTitle);
        }
        public static string FolderPathTextfield(GUIContent label, string path, string popupTitle)
        {
            return SPEditorGUI.FolderPathTextfield(EditorGUILayout.GetControlRect(true), label, path, popupTitle);
        }

        public static string SaveFilePathTextfield(string label, string path, string popupTitle, string extension)
        {
            return SPEditorGUI.SaveFilePathTextfield(EditorGUILayout.GetControlRect(true), label, path, popupTitle, extension);
        }
        public static string SaveFilePathTextfield(GUIContent label, string path, string popupTitle, string extension)
        {
            return SPEditorGUI.SaveFilePathTextfield(EditorGUILayout.GetControlRect(true), label, path, popupTitle, extension);
        }

        #endregion


        #region ReflectedPropertyField

        /// <summary>
        /// Reflects the available properties and shows them in a dropdown
        /// </summary>
        public static string ReflectedPropertyField(GUIContent label, object targObj, string selectedMemberName, com.spacepuppy.Dynamic.DynamicMemberAccess access, out System.Reflection.MemberInfo selectedMember, bool allowSetterMethods = false)
        {
            var position = EditorGUILayout.GetControlRect(label == GUIContent.none);
            return SPEditorGUI.ReflectedPropertyField(position, label, targObj, selectedMemberName, access, out selectedMember, allowSetterMethods);
        }

        public static string ReflectedPropertyField(GUIContent label, object targObj, string selectedMemberName, com.spacepuppy.Dynamic.DynamicMemberAccess access, bool allowSetterMethods = false)
        {
            var position = EditorGUILayout.GetControlRect(label == GUIContent.none);
            System.Reflection.MemberInfo selectedMember;
            return SPEditorGUI.ReflectedPropertyField(position, label, targObj, selectedMemberName, access, out selectedMember, allowSetterMethods);
        }

        /// <summary>
        /// Reflects the available properties and shows them in a dropdown
        /// </summary>
        public static string ReflectedPropertyField(GUIContent label, System.Type targType, string selectedMemberName, out System.Reflection.MemberInfo selectedMember, bool allowSetterMethods = false)
        {
            var position = EditorGUILayout.GetControlRect(label == GUIContent.none);
            return SPEditorGUI.ReflectedPropertyField(position, label, targType, selectedMemberName, out selectedMember, allowSetterMethods);
        }

        public static string ReflectedPropertyField(GUIContent label, System.Type targType, string selectedMemberName, bool allowSetterMethods = false)
        {
            var position = EditorGUILayout.GetControlRect(label == GUIContent.none);
            System.Reflection.MemberInfo selectedMember;
            return SPEditorGUI.ReflectedPropertyField(position, label, targType, selectedMemberName, out selectedMember, allowSetterMethods);
        }

        #endregion


        #region SelectionTabs

        public static int SelectionTabs(int mode, string[] modes, int xCount)
        {
            int yCount = Mathf.CeilToInt((float)modes.Length / (float)xCount);
            int currentRow = Mathf.FloorToInt((float)mode / (float)xCount);

            var gridStyle = new GUIStyle(GUI.skin.window);
            gridStyle.padding.bottom = -20;
            Rect rect = GUILayoutUtility.GetRect(1, 16f * yCount);
            rect.x += 4;
            rect.width -= 7;

            if (currentRow == yCount - 1)
            {
                //if selected row is last row, don't bother remapping
                return GUI.SelectionGrid(rect, mode, modes, 2, gridStyle);
            }
            else
            {
                //remap so that selected row is the last row
                var altModes = modes.Clone() as string[];
                for (int i = 0; i < xCount; i++)
                {
                    altModes[(altModes.Length - xCount) + i] = modes[xCount * currentRow + i]; //move selected row to end
                    altModes[xCount * currentRow + i] = modes[(modes.Length - xCount) + i]; //move last row to selected row
                }
                int altMode = (modes.Length - xCount) + (mode % xCount);
                EditorGUI.BeginChangeCheck();
                altMode = GUI.SelectionGrid(rect, altMode, altModes, 2, gridStyle);
                if (EditorGUI.EndChangeCheck())
                {
                    var newRow = Mathf.FloorToInt((float)altMode / (float)xCount);
                    if (newRow == yCount - 1)
                    {
                        return (currentRow * xCount) + (altMode % xCount);
                    }
                    else if (newRow == currentRow)
                    {
                        return (modes.Length - xCount) + (altMode % xCount);
                    }
                    else
                    {
                        return altMode;
                    }
                }
                else
                {
                    return mode;
                }
            }
        }

        #endregion

    }

}
