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

        private System.Type _fieldType;
        private System.Type[] _inheritsFromTypes;
        private bool _hideTypeDropDown;
        private bool _hideTypeDropDownIfSingle;
        private bool _allowProxy;
        private bool _restrictProxyResolvedType; //if IProxy is allowed, should we test if the type returned by IProxy.GetReturnedType matches the accepted types
        private bool _allowSceneObjects = true;

        #endregion

        #region Properties

        public System.Type FieldType
        {
            get => this.fieldInfo?.FieldType ?? _fieldType ?? typeof(UnityEngine.Object);
            set => _fieldType = value;
        }

        public System.Type[] InheritsFromTypes
        {
            get => (this.attribute as TypeRestrictionAttribute)?.InheritsFromTypes ?? _inheritsFromTypes ?? ArrayUtil.Empty<System.Type>();
            set => _inheritsFromTypes = value;
        }

        public bool HideTypeDropDown
        {
            get => (this.attribute as TypeRestrictionAttribute)?.HideTypeDropDown ?? _hideTypeDropDown;
            set => _hideTypeDropDown = value;
        }

        public bool HideTypeDropDownIfSingle
        {
            get => (this.attribute as TypeRestrictionAttribute)?.HideTypeDropDownIfSingle ?? _hideTypeDropDownIfSingle;
            set => _hideTypeDropDownIfSingle = value;
        }

        public bool AllowProxy
        {
            get => (this.attribute as TypeRestrictionAttribute)?.AllowProxy ?? _allowProxy;
            set => _allowProxy = value;
        }

        public bool RestrictProxyResolvedType
        {
            get => (this.attribute as TypeRestrictionAttribute)?.RestrictProxyResolvedType ?? _restrictProxyResolvedType;
            set => _restrictProxyResolvedType = value;
        }

        public bool AllowSceneObjects
        {
            get => (this.attribute as TypeRestrictionAttribute)?.AllowSceneObjects ?? _allowSceneObjects;
            set => _allowSceneObjects = value;
        }

        #endregion

        #region Utils

        private bool ValidateFieldType()
        {
            bool isArray = this.FieldType.IsListType();
            var fieldType = (isArray) ? this.FieldType.GetElementTypeOfListType() : this.FieldType;
            if (!TypeUtil.IsType(fieldType, typeof(UnityEngine.Object)))
                return false;
            //if (!TypeUtil.IsType(fieldType, typeof(Component))) return false;

            if (this.InheritsFromTypes.Length > 0)
            {
                foreach (var tp in this.InheritsFromTypes)
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
            float h;
            if (EditorHelper.AssertMultiObjectEditingNotSupportedHeight(property, label, out h)) return h;

            if (this.HideTypeDropDown)
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
            if (EditorHelper.AssertMultiObjectEditingNotSupported(position, property, label)) return;

            if (!this.ValidateFieldType())
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            //get base type
            bool isArray = this.FieldType.IsListType();
            var fieldType = (isArray) ? this.FieldType.GetElementTypeOfListType() : this.FieldType;
            bool fieldIsComponentType = TypeUtil.IsType(fieldType, typeof(Component));
            bool objIsSimpleComponentSource = (property.objectReferenceValue is Component || property.objectReferenceValue is GameObject);

            System.Type[] allInheritableTypes = this.InheritsFromTypes;
            System.Type inheritsFromType = (allInheritableTypes.Length == 1 && TypeUtil.IsType(allInheritableTypes[0], typeof(UnityEngine.Object))) ? allInheritableTypes[0] : fieldType;
            bool isNonStandardUnityType = allInheritableTypes.Any(o => o.IsInterface || o.IsGenericType || !TypeUtil.IsType(o, typeof(UnityEngine.Object))); //is a type that unity's ObjectField doesn't support directly

            if (!objIsSimpleComponentSource || this.HideTypeDropDown ||
                (this.HideTypeDropDownIfSingle && !SatisfiesMoreThanOneTarget(property.objectReferenceValue, allInheritableTypes)))
            {
                //draw object field
                UnityEngine.Object targ;
                if (allInheritableTypes.Length > 1 || isNonStandardUnityType || this.AllowProxy)
                {
                    System.Func<UnityEngine.Object, bool> filter = null;
                    if (this.AllowProxy)
                    {
                        if (this.RestrictProxyResolvedType)
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

                    targ = SPEditorGUI.AdvancedObjectField(position,
                        label,
                        property.objectReferenceValue,
                        (allInheritableTypes.Length == 1) ? allInheritableTypes[0] : fieldType,
                        this.AllowSceneObjects,
                        this.AllowProxy,
                        filter);
                }
                else if (fieldIsComponentType)
                {
                    var fieldCompType = (TypeUtil.IsType(fieldType, typeof(Component))) ? fieldType : typeof(Component);
                    targ = SPEditorGUI.ComponentField(position, label, property.objectReferenceValue as Component, inheritsFromType, this.AllowSceneObjects, fieldCompType);
                    targ = (allInheritableTypes.Length > 1 ? ObjUtil.GetAsFromSource(allInheritableTypes, targ) : ObjUtil.GetAsFromSource(inheritsFromType, targ)) as UnityEngine.Object;
                }
                else
                {
                    targ = EditorGUI.ObjectField(position, label, property.objectReferenceValue, inheritsFromType, this.AllowSceneObjects);
                    targ = (allInheritableTypes.Length > 1 ? ObjUtil.GetAsFromSource(allInheritableTypes, targ) : ObjUtil.GetAsFromSource(inheritsFromType, targ)) as UnityEngine.Object;
                }

                property.objectReferenceValue = targ;
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
                _selectComponentDrawer.AllowProxy = this.AllowProxy;
                _selectComponentDrawer.ShowXButton = true;
                _selectComponentDrawer.AllowNonComponents = true;
                if (allInheritableTypes.Length > 1 || this.AllowProxy)
                {
                    _multiSelector.AllowedTypes = allInheritableTypes;
                    _multiSelector.RestrictProxyResolvedType = this.RestrictProxyResolvedType;
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
