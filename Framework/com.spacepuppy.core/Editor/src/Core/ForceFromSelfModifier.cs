using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Collections;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(ForceFromSelfAttribute))]
    public class ForceFromSelfModifier : PropertyModifier
    {

        protected internal override void OnBeforeGUI(SerializedProperty property, ref bool cancelDraw)
        {
            var relativity = (this.attribute as ForceFromSelfAttribute).Relativity;

            if (property.isArray && TypeUtil.IsListType(fieldInfo.FieldType, true))
            {
                var elementType = TypeUtil.GetElementTypeOfListType(this.fieldInfo.FieldType);
                var restrictedType = EditorHelper.GetRestrictedFieldType(this.fieldInfo, true) ?? elementType;
                ApplyDefaultAsList(property, elementType, restrictedType, relativity);
            }
            else
            {
                ApplyDefaultAsSingle(property, EditorHelper.GetRestrictedFieldType(this.fieldInfo, true) ?? property.GetPropertyValueType(), relativity);
            }
        }

        private static void ApplyDefaultAsSingle(SerializedProperty property, System.Type restrictionType, EntityRelativity relativity)
        {
            if (restrictionType == null) return;

            var targ = GameObjectUtil.GetGameObjectFromSource(property.serializedObject.targetObject);
            if (targ == null)
            {
                property.SetPropertyValue(ObjUtil.GetAsFromSource(restrictionType, property.serializedObject.targetObject) as UnityEngine.Object);
                return;
            }

            var currentValue = property.GetPropertyValue() as UnityEngine.Object;
            UnityEngine.Object obj = null;
            switch (relativity)
            {
                case EntityRelativity.Entity:
                    {
                        targ = targ.FindRoot();
                        if (ObjUtil.IsRelatedTo(targ, currentValue)) return;

                        obj = ObjUtil.GetAsFromSource(restrictionType, targ) as UnityEngine.Object;
                        if (obj == null && ComponentUtil.IsAcceptableComponentType(restrictionType)) obj = targ.GetComponentInChildren(restrictionType);
                    }
                    break;
                case EntityRelativity.Self:
                    {
                        if (ObjUtil.IsRelatedTo(targ, currentValue)) return;

                        obj = ObjUtil.GetAsFromSource(restrictionType, targ) as UnityEngine.Object;
                    }
                    break;
                case EntityRelativity.SelfAndChildren:
                    {
                        if (ObjUtil.IsRelatedTo(targ, property.objectReferenceValue)) return;

                        obj = ObjUtil.GetAsFromSource(restrictionType, targ) as UnityEngine.Object;
                        if (obj == null && ComponentUtil.IsAcceptableComponentType(restrictionType)) obj = targ.GetComponentInChildren(restrictionType);
                    }
                    break;
                case EntityRelativity.SelfAndParents:
                    {
                        if (ObjUtil.IsRelatedTo(targ, property.objectReferenceValue)) return;

                        obj = ObjUtil.GetAsFromSource(restrictionType, targ) as UnityEngine.Object;
                        if (obj == null && ComponentUtil.IsAcceptableComponentType(restrictionType)) obj = targ.GetComponentInParent(restrictionType);
                    }
                    break;
            }

            if (obj != null)
            {
                property.SetPropertyValue(obj);
                GUI.changed = true;
            }
            else if (currentValue != null)
            {
                property.SetPropertyValue(null);
                GUI.changed = true;
            }
        }


        private static void ApplyDefaultAsList(SerializedProperty property, System.Type elementType, System.Type restrictionType, EntityRelativity relativity)
        {
            if (elementType == null || restrictionType == null) return;

            if (TypeUtil.IsType(elementType, typeof(VariantReference)))
            {
                var targ = GameObjectUtil.GetGameObjectFromSource(property.serializedObject.targetObject);
                if (targ == null)
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
                            if (ValidateSerializedPropertyArray(property, arr, true)) return;

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
                            if (ValidateSerializedPropertyArray(property, arr, true)) return;

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
                            if (ValidateSerializedPropertyArray(property, arr, true)) return;

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
                            var arr = ComponentUtil.IsAcceptableComponentType(restrictionType) ? targ.GetComponentsInParent(restrictionType) : ArrayUtil.Empty<Component>();
                            if (ValidateSerializedPropertyArray(property, arr, true)) return;

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
                var targ = GameObjectUtil.GetGameObjectFromSource(property.serializedObject.targetObject);
                if (targ == null)
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
                            if (ValidateSerializedPropertyArray(property, arr, false)) return;

                            property.arraySize = arr.Length;
                            for (int i = 0; i < arr.Length; i++)
                            {
                                property.GetArrayElementAtIndex(i).objectReferenceValue = arr[i] as UnityEngine.Object;
                            }
                            property.serializedObject.Update();
                            GUI.changed = true;
                        }
                        break;
                    case EntityRelativity.Self:
                        {
                            var arr = ObjUtil.GetAllFromSource(restrictionType, targ, false);
                            if (ValidateSerializedPropertyArray(property, arr, false)) return;

                            property.arraySize = arr.Length;
                            for (int i = 0; i < arr.Length; i++)
                            {
                                property.GetArrayElementAtIndex(i).objectReferenceValue = arr[i] as UnityEngine.Object;
                            }
                            property.serializedObject.Update();
                            GUI.changed = true;
                        }
                        break;
                    case EntityRelativity.SelfAndChildren:
                        {
                            var arr = ObjUtil.GetAllFromSource(restrictionType, targ, true);
                            if (ValidateSerializedPropertyArray(property, arr, false)) return;

                            property.arraySize = arr.Length;
                            for (int i = 0; i < arr.Length; i++)
                            {
                                property.GetArrayElementAtIndex(i).objectReferenceValue = arr[i] as UnityEngine.Object;
                            }
                            property.serializedObject.Update();
                            GUI.changed = true;
                        }
                        break;
                    case EntityRelativity.SelfAndParents:
                        {
                            var arr = ComponentUtil.IsAcceptableComponentType(restrictionType) ? targ.GetComponentsInParent(restrictionType) : ArrayUtil.Empty<Component>();
                            if (ValidateSerializedPropertyArray(property, arr, false)) return;

                            property.arraySize = arr.Length;
                            for (int i = 0; i < arr.Length; i++)
                            {
                                property.GetArrayElementAtIndex(i).objectReferenceValue = arr[i] as UnityEngine.Object;
                            }
                            property.serializedObject.Update();
                            GUI.changed = true;
                        }
                        break;
                }
            }
        }

        private static bool ValidateSerializedPropertyArray(SerializedProperty property, object[] arr, bool isVariant)
        {
            if (arr == null && property.arraySize > 0) return false;
            if (property.arraySize != arr.Length) return false;
            if (property.arraySize == 0) return true;

            using (var lst = TempCollection.GetList<object>())
            {
                for (int i = 0; i < property.arraySize; i++)
                {
                    var el = property.GetArrayElementAtIndex(i);
                    if (isVariant)
                    {
                        var variant = EditorHelper.GetTargetObjectOfProperty(el) as VariantReference;
                        if (variant == null)
                            lst.Add(null);
                        else
                            lst.Add(variant.Value);
                    }
                    else if (el.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        lst.Add(el.objectReferenceValue);
                    }
                    else
                    {
                        lst.Add(null);
                    }
                }

                return lst.SimilarTo(arr);
            }
        }

    }

}
