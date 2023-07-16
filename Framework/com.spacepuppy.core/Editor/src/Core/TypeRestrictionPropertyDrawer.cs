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
        private static TypeRestrictionComponentChoiceSelector _multiSelector = new TypeRestrictionComponentChoiceSelector();

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
            System.Type inheritsFromType = (allInheritableTypes.Length == 1 && TypeUtil.IsType(allInheritableTypes[0], typeof(UnityEngine.Object))) ? allInheritableTypes[0] : fieldType;
            bool isNonStandardUnityType = allInheritableTypes.Any(o => o.IsInterface || o.IsGenericType || !TypeUtil.IsType(o, typeof(UnityEngine.Object))); //is a type that unity's ObjectField doesn't support directly

            if (!objIsSimpleComponentSource || (attrib?.HideTypeDropDown ?? false) ||
                (attrib.HideTypeDropDownIfSingle && !SatisfiesMoreThanOneTarget(property.objectReferenceValue, allInheritableTypes)))
            {
                //draw object field
                UnityEngine.Object targ;
                if (allInheritableTypes.Length > 1 || isNonStandardUnityType || (attrib?.AllowProxy ?? false))
                {
                    System.Func<UnityEngine.Object, bool> filter = null;
                    if (attrib?.AllowProxy ?? false)
                    {
                        if (attrib?.RestrictProxyResolvedType ?? false)
                        {
                            filter = o => o && ((TypeUtil.IsType(o.GetType(), allInheritableTypes) || ObjUtil.GetAsFromSource(allInheritableTypes, o) != null)) || (o is IProxy p && TypeUtil.IsType(p.GetTargetType(), allInheritableTypes));
                        }
                        else
                        {
                            filter = o => o && ((TypeUtil.IsType(o.GetType(), allInheritableTypes) || ObjUtil.GetAsFromSource(allInheritableTypes, o) != null)) || (o is IProxy);
                        }
                    }
                    else
                    {
                        filter = o => o && (TypeUtil.IsType(o.GetType(), allInheritableTypes) || ObjUtil.GetAsFromSource(allInheritableTypes, o) != null);
                    }

                    targ = UnityObjectDropDownWindowSelector.ObjectField(position,
                        label,
                        property.objectReferenceValue,
                        (allInheritableTypes.Length == 1) ? allInheritableTypes[0] : fieldType,
                        attrib?.AllowSceneObjects ?? true,
                        attrib?.AllowProxy ?? false,
                        filter);
                }
                else if (fieldIsComponentType)
                {
                    var fieldCompType = (TypeUtil.IsType(fieldType, typeof(Component))) ? fieldType : typeof(Component);
                    targ = SPEditorGUI.ComponentField(position, label, property.objectReferenceValue as Component, inheritsFromType, attrib?.AllowSceneObjects ?? true, fieldCompType);
                }
                else
                {
                    targ = EditorGUI.ObjectField(position, label, property.objectReferenceValue, inheritsFromType, attrib?.AllowSceneObjects ?? true);
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

                //_selectComponentDrawer.RestrictionType = inheritsFromType ?? typeof(UnityEngine.Object);
                _selectComponentDrawer.RestrictionType = (allInheritableTypes.Length == 1) ? allInheritableTypes[0] : inheritsFromType ?? typeof(UnityEngine.Object);
                _selectComponentDrawer.AllowProxy = (attrib?.AllowProxy ?? false);
                _selectComponentDrawer.ShowXButton = true;
                _selectComponentDrawer.AllowNonComponents = true;
                if (allInheritableTypes.Length > 1 || (attrib?.AllowProxy ?? false))
                {
                    _multiSelector.AllowedTypes = allInheritableTypes;
                    _multiSelector.RestrictProxyResolvedType = (attrib?.RestrictProxyResolvedType ?? false);
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

        #region Special Types

        private class TypeRestrictionComponentChoiceSelector : DefaultComponentChoiceSelector
        {

            public System.Type[] AllowedTypes;
            public bool RestrictProxyResolvedType;

            protected override Component[] DoGetComponents()
            {
                return GetComponentsFromSerializedProperty(this.Property, this.AllowedTypes, this.RestrictionType, this.Drawer.ForceOnlySelf, this.Drawer.SearchChildren, this.AllowProxy, this.RestrictProxyResolvedType);
            }

            public static Component[] GetComponentsFromSerializedProperty(SerializedProperty property, System.Type[] allowedTypes, System.Type restrictionType, bool forceSelfOnly, bool searchChildren, bool allowProxy, bool restrictProxyResolvedType)
            {
                if (allowedTypes == null || allowedTypes.Length == 0) return ArrayUtil.Empty<Component>();

                var go = DefaultComponentChoiceSelector.GetGameObjectFromSource(property, forceSelfOnly);
                if (go == null) return ArrayUtil.Empty<Component>();

                using (var set = com.spacepuppy.Collections.TempCollection.GetSet<Component>())
                {
                    if (searchChildren)
                    {
                        foreach (var c in go.GetComponentsInChildren<Component>())
                        {
                            if (!IsTypeLocal(c, restrictionType, allowProxy, restrictProxyResolvedType)) continue;
                            foreach (var tp in allowedTypes)
                            {
                                if (IsTypeLocal(c, tp, allowProxy, restrictProxyResolvedType)) set.Add(c);
                            }
                        }
                    }
                    else
                    {
                        foreach (var c in go.GetComponents<Component>())
                        {
                            if (!IsTypeLocal(c, restrictionType, allowProxy, restrictProxyResolvedType)) continue;
                            foreach (var tp in allowedTypes)
                            {
                                if (IsTypeLocal(c, tp, allowProxy, restrictProxyResolvedType)) set.Add(c);
                            }
                        }
                    }

                    return (from c in set orderby c.GetType().Name select c).ToArray();
                }
            }

            static bool IsTypeLocal(object obj, System.Type tp, bool respectProxy, bool restrictProxyResolvedType)
            {
                if (respectProxy && !restrictProxyResolvedType && obj is IProxy) return true;

                return ObjUtil.IsType(obj, tp, respectProxy);
            }

        }

        #endregion

    }

}
