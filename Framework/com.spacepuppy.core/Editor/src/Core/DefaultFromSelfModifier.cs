using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.spacepuppy;
using com.spacepuppy.Collections;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(DefaultFromSelfAttribute))]
    public class DefaultFromSelfModifier : PropertyModifier
    {

        private HashSet<int> _handled = new HashSet<int>();

        protected internal override void OnBeforeGUI(SerializedProperty property, ref bool cancelDraw)
        {
            if (property.serializedObject.isEditingMultipleObjects) return;

            int hash = com.spacepuppyeditor.Internal.PropertyHandlerCache.GetIndexRespectingPropertyHash(property);
            if (_handled.Contains(hash)) return;
            if ((this.attribute as DefaultFromSelfAttribute).HandleOnce) _handled.Add(hash);

            var relativity = (this.attribute as DefaultFromSelfAttribute).Relativity;

            if (property.isArray && TypeUtil.IsListType(fieldInfo.FieldType, true))
            {
                //TODO - make list support SerializedInterfaceRef
                var elementType = TypeUtil.GetElementTypeOfListType(this.fieldInfo.FieldType);
                var restrictionType = DefaultFromSelfModifier.GetRestrictedPropertyType(property, this.fieldInfo) ?? elementType;
                ApplyDefaultAsList(property, elementType, restrictionType, relativity);
            }
            else
            {
                ApplyDefaultAsSingle(property, DefaultFromSelfModifier.GetRestrictedPropertyType(property, this.fieldInfo) ?? property.GetPropertyValueType(), relativity);
            }
        }

        public static object GetFromTarget(GameObject targ, System.Type restrictionType, EntityRelativity relativity)
        {
            switch (relativity)
            {
                case EntityRelativity.Entity:
                    {
                        targ = targ.FindRoot();

                        var obj = ObjUtil.GetAsFromSource(restrictionType, targ);
                        if (object.ReferenceEquals(obj, null) && ComponentUtil.IsAcceptableComponentType(restrictionType)) obj = targ.GetComponentInChildren(restrictionType);
                        return obj;
                    }
                case EntityRelativity.Self:
                    {
                        return ObjUtil.GetAsFromSource(restrictionType, targ);
                    }
                case EntityRelativity.SelfAndChildren:
                    {
                        var obj = ObjUtil.GetAsFromSource(restrictionType, targ);
                        if (object.ReferenceEquals(obj, null) && ComponentUtil.IsAcceptableComponentType(restrictionType)) obj = targ.GetComponentInChildren(restrictionType);
                        return obj;
                    }
                case EntityRelativity.SelfAndParents:
                    {
                        var obj = ObjUtil.GetAsFromSource(restrictionType, targ);
                        if (object.ReferenceEquals(obj, null) && ComponentUtil.IsAcceptableComponentType(restrictionType)) obj = targ.GetComponentInParent(restrictionType);
                        return obj;
                    }
                default:
                    return null;
            }
        }

        private static void ApplyDefaultAsSingle(SerializedProperty property, System.Type restrictionType, EntityRelativity relativity)
        {
            object value = property.GetPropertyValue(false);
            if (value != null) return;

            var targ = GameObjectUtil.GetGameObjectFromSource(property.serializedObject.targetObject);
            if (object.ReferenceEquals(targ, null))
            {
                value = ObjUtil.GetAsFromSource(restrictionType, property.serializedObject.targetObject);
            }
            else
            {
                value = GetFromTarget(targ, restrictionType, relativity);
            }

            if (value != null)
            {
                property.SetPropertyValue(value);
            }
        }

        private static void ApplyDefaultAsList(SerializedProperty property, System.Type elementType, System.Type restrictionType, EntityRelativity relativity)
        {
            if (property.arraySize > 1) return;
            if (elementType == null || restrictionType == null) return;

            if (TypeUtil.IsType(elementType, typeof(VariantReference)))
            {
                if (property.arraySize == 1 && EditorHelper.GetTargetObjectOfProperty(property.GetArrayElementAtIndex(0)) is VariantReference vr && vr.Value != null) return;

                var targ = GameObjectUtil.GetGameObjectFromSource(property.serializedObject.targetObject);
                if (object.ReferenceEquals(targ, null))
                {
                    var obj = ObjUtil.GetAsFromSource(restrictionType, property.serializedObject.targetObject);
                    if (obj != null)
                    {
                        property.arraySize = 1;
                        var variant = EditorHelper.GetTargetObjectOfProperty(property.GetArrayElementAtIndex(0)) as VariantReference;
                        if (variant == null) return;
                        variant.Value = obj;
                        property.serializedObject.Update();
                        GUI.changed = true;
                    }
                    else if (property.arraySize > 0)
                    {
                        property.arraySize = 0;
                        GUI.changed = true;
                    }
                    return;
                }

                switch (relativity)
                {
                    case EntityRelativity.Entity:
                        {
                            targ = targ.FindRoot();
                            var arr = ObjUtil.GetAllFromSource(restrictionType, targ, true);

                            property.arraySize = arr.Length;
                            for (int i = 0; i < arr.Length; i++)
                            {
                                var variant = EditorHelper.GetTargetObjectOfProperty(property.GetArrayElementAtIndex(i)) as VariantReference;
                                if (variant != null) variant.Value = arr[i];
                            }
                            property.serializedObject.Update();
                            GUI.changed = true;
                        }
                        break;
                    case EntityRelativity.Self:
                        {
                            var arr = ObjUtil.GetAllFromSource(restrictionType, targ, false);

                            property.arraySize = arr.Length;
                            for (int i = 0; i < arr.Length; i++)
                            {
                                var variant = EditorHelper.GetTargetObjectOfProperty(property.GetArrayElementAtIndex(i)) as VariantReference;
                                if (variant != null) variant.Value = arr[i];
                            }
                            property.serializedObject.Update();
                            GUI.changed = true;
                        }
                        break;
                    case EntityRelativity.SelfAndChildren:
                        {
                            var arr = ObjUtil.GetAllFromSource(restrictionType, targ, true);

                            property.arraySize = arr.Length;
                            for (int i = 0; i < arr.Length; i++)
                            {
                                var variant = EditorHelper.GetTargetObjectOfProperty(property.GetArrayElementAtIndex(i)) as VariantReference;
                                if (variant != null) variant.Value = arr[i];
                            }
                            property.serializedObject.Update();
                            GUI.changed = true;
                        }
                        break;
                    case EntityRelativity.SelfAndParents:
                        {
                            var arr = ComponentUtil.IsAcceptableComponentType(restrictionType) ? targ.GetComponentsInParent(restrictionType, true) : ArrayUtil.Empty<Component>();

                            property.arraySize = arr.Length;
                            for (int i = 0; i < arr.Length; i++)
                            {
                                var variant = EditorHelper.GetTargetObjectOfProperty(property.GetArrayElementAtIndex(i)) as VariantReference;
                                if (variant != null) variant.Value = arr[i];
                            }
                            property.serializedObject.Update();
                            GUI.changed = true;
                        }
                        break;
                }
            }
            else if (TypeUtil.IsType(elementType, typeof(UnityEngine.Object)))
            {
                if (property.arraySize == 1 && (property.GetArrayElementAtIndex(0).objectReferenceValue != null)) return;

                var targ = GameObjectUtil.GetGameObjectFromSource(property.serializedObject.targetObject);
                if (object.ReferenceEquals(targ, null))
                {
                    var obj = ObjUtil.GetAsFromSource(restrictionType, property.serializedObject.targetObject) as UnityEngine.Object;
                    if (obj != null)
                    {
                        property.arraySize = 1;
                        property.GetArrayElementAtIndex(0).objectReferenceValue = obj;
                        GUI.changed = true;
                    }
                    else if (property.arraySize > 0)
                    {
                        property.arraySize = 0;
                        GUI.changed = true;
                    }
                    return;
                }

                switch (relativity)
                {
                    case EntityRelativity.Entity:
                        {
                            targ = targ.FindRoot();
                            var arr = ObjUtil.GetAllFromSource(restrictionType, targ, true);

                            property.arraySize = arr.Length;
                            for (int i = 0; i < arr.Length; i++)
                            {
                                property.GetArrayElementAtIndex(i).objectReferenceValue = arr[i] as UnityEngine.Object;
                            }
                        }
                        break;
                    case EntityRelativity.Self:
                        {
                            var arr = ObjUtil.GetAllFromSource(restrictionType, targ, false);

                            property.arraySize = arr.Length;
                            for (int i = 0; i < arr.Length; i++)
                            {
                                property.GetArrayElementAtIndex(i).objectReferenceValue = arr[i] as UnityEngine.Object;
                            }
                        }
                        break;
                    case EntityRelativity.SelfAndChildren:
                        {
                            var arr = ObjUtil.GetAllFromSource(restrictionType, targ, true);

                            property.arraySize = arr.Length;
                            for (int i = 0; i < arr.Length; i++)
                            {
                                property.GetArrayElementAtIndex(i).objectReferenceValue = arr[i] as UnityEngine.Object;
                            }
                        }
                        break;
                    case EntityRelativity.SelfAndParents:
                        {
                            var arr = ComponentUtil.IsAcceptableComponentType(restrictionType) ? targ.GetComponentsInParent(restrictionType, true) : ArrayUtil.Empty<Component>();

                            property.arraySize = arr.Length;
                            for (int i = 0; i < arr.Length; i++)
                            {
                                property.GetArrayElementAtIndex(i).objectReferenceValue = arr[i] as UnityEngine.Object;
                            }
                        }
                        break;
                }
            }
        }




        public static System.Type GetRestrictedPropertyType(SerializedProperty prop, FieldInfo field)
        {
            if (field == null) return null;

            var a_tpr = field.GetCustomAttribute<TypeRestrictionAttribute>();
            if (a_tpr?.InheritsFromTypes?.Length > 0)
            {
                return a_tpr.InheritsFromTypes[0];
            }

            var a_scr = field.GetCustomAttribute<SelectableComponentAttribute>();
            if (a_scr?.InheritsFromType != null)
            {
                return a_scr.InheritsFromType;
            }

            var tp = EditorHelper.GetPropertyValueType(prop);
            if (TypeUtil.IsListType(tp))
            {
                tp = TypeUtil.GetElementTypeOfListType(tp);
            }
            return tp;
        }

    }

}
