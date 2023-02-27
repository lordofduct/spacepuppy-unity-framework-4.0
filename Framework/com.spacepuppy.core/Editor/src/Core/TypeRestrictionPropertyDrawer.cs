using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppyeditor.Windows;
using com.spacepuppy.Collections;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(TypeRestrictionAttribute))]
    public class TypeRestrictionPropertyDrawer : PropertyDrawer
    {

        #region Fields

        private SelectableComponentPropertyDrawer _selectComponentDrawer;
        private static MultiTypeComponentChoiceSelector _multiSelector = new MultiTypeComponentChoiceSelector();

        #endregion

        #region Utils

        private bool ValidateFieldType()
        {
            bool isArray = this.fieldInfo.FieldType.IsListType();
            var fieldType = (isArray) ? this.fieldInfo.FieldType.GetElementTypeOfListType() : this.fieldInfo.FieldType;
            if (!TypeUtil.IsType(fieldType, typeof(UnityEngine.Object)))
                return false;
            //if (!TypeUtil.IsType(fieldType, typeof(Component))) return false;

            var attrib = this.attribute as TypeRestrictionAttribute;
            if (attrib?.InheritsFromTypes != null && attrib?.InheritsFromTypes.Length > 0)
            {
                foreach (var tp in attrib.InheritsFromTypes)
                {
                    if (tp.IsInterface || TypeUtil.IsType(tp, fieldType)) return true;
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion

        #region Drawer Overrides

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var attrib = this.attribute as TypeRestrictionAttribute;
            if (attrib.HideTypeDropDown)
            {
                return EditorGUIUtility.singleLineHeight;
            }
            else
            {
                if (_selectComponentDrawer == null) _selectComponentDrawer = new SelectableComponentPropertyDrawer();
                return _selectComponentDrawer.GetPropertyHeight(property, label);
            }
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!this.ValidateFieldType())
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            //get base type
            var attrib = this.attribute as TypeRestrictionAttribute;

            bool isArray = this.fieldInfo.FieldType.IsListType();
            var fieldType = (isArray) ? this.fieldInfo.FieldType.GetElementTypeOfListType() : this.fieldInfo.FieldType;
            bool fieldIsComponentType = TypeUtil.IsType(fieldType, typeof(Component));
            bool objIsSimpleComponentSource = (property.objectReferenceValue is Component || property.objectReferenceValue is GameObject);

            System.Type[] allInheritableTypes = attrib?.InheritsFromTypes ?? ArrayUtil.Empty<System.Type>();
            System.Type inheritsFromType = (allInheritableTypes.Length == 1) ? allInheritableTypes[0] : fieldType;
            bool isNonStandardUnityType = allInheritableTypes.Any(o => o.IsInterface || o.IsGenericType); //is a type that unity's ObjectField doesn't support directly

            if (attrib.HideTypeDropDown || !objIsSimpleComponentSource || (attrib.HideTypeDropDownIfSingle && !SatisfiesMoreThanOneTarget(property.objectReferenceValue, allInheritableTypes)))
            {
                //draw object field
                UnityEngine.Object targ;
                if (allInheritableTypes.Length > 1 || isNonStandardUnityType)
                {
                    targ = UnityObjectDropDownWindowSelector.ObjectField(position,
                        label,
                        property.objectReferenceValue,
                        (allInheritableTypes.Length == 1) ? allInheritableTypes[0] : fieldType,
                        attrib?.AllowSceneObjects ?? true,
                        attrib?.AllowProxy ?? false,
                        (o) =>
                        {
                            return o && (TypeUtil.IsType(o.GetType(), allInheritableTypes) || ObjUtil.GetAsFromSource(allInheritableTypes, o) != null);
                        });
                }
                else if (fieldIsComponentType)
                {
                    var fieldCompType = (TypeUtil.IsType(fieldType, typeof(Component))) ? fieldType : typeof(Component);
                    targ = SPEditorGUI.ComponentField(position, label, property.objectReferenceValue as Component, inheritsFromType, attrib?.AllowSceneObjects ?? true, fieldCompType);
                }
                else
                {
                    targ = EditorGUI.ObjectField(position, label, property.objectReferenceValue, fieldType, attrib?.AllowSceneObjects ?? true);
                }

                if (targ == null)
                {
                    property.objectReferenceValue = null;
                }
                else
                {
                    var o = (allInheritableTypes.Length > 1 ? ObjUtil.GetAsFromSource(allInheritableTypes, targ) : ObjUtil.GetAsFromSource(inheritsFromType, targ)) as UnityEngine.Object;
                    if (attrib.AllowProxy && o == null)
                    {
                        o = ObjUtil.GetAsFromSource<IProxy>(targ) as UnityEngine.Object;
                    }
                    property.objectReferenceValue = o;
                }
            }
            else
            {
                //draw complex field
                if (_selectComponentDrawer == null)
                {
                    _selectComponentDrawer = new SelectableComponentPropertyDrawer();
                }

                _selectComponentDrawer.RestrictionType = inheritsFromType ?? typeof(UnityEngine.Object);
                _selectComponentDrawer.AllowProxy = attrib.AllowProxy;
                _selectComponentDrawer.ShowXButton = true;
                _selectComponentDrawer.AllowNonComponents = true;
                if (allInheritableTypes.Length > 1)
                {
                    _multiSelector.AllowedTypes = allInheritableTypes;
                    _selectComponentDrawer.ChoiceSelector = _multiSelector;
                }
                else
                {
                    _selectComponentDrawer.ChoiceSelector = DefaultComponentChoiceSelector.Default;
                }

                _selectComponentDrawer.OnGUI(position, property, label);
            }

            EditorGUI.EndProperty();
        }

        static bool SatisfiesMoreThanOneTarget(UnityEngine.Object source, System.Type[] allInheritableTypes)
        {
            var go = GameObjectUtil.GetGameObjectFromSource(source);
            if (!go) return false;

            using (var hash = TempCollection.GetSet<UnityEngine.Object>())
            using (var components = TempCollection.GetList<Component>())
            {
                go.GetComponents(components);
                foreach (var tp in allInheritableTypes)
                {
                    if (tp.IsInstanceOfType(go)) hash.Add(go);
                    foreach (var c in components)
                    {
                        if (tp.IsInstanceOfType(c)) hash.Add(c);
                    }
                }
                return hash.Count > 1;
            }
        }

        #endregion


    }

}
