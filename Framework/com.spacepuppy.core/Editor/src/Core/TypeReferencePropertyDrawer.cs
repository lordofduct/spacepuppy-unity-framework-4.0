using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppyeditor.Windows;

namespace com.spacepuppyeditor.Core
{


    [CustomPropertyDrawer(typeof(TypeReference))]
    public class TypeReferencePropertyDrawer : PropertyDrawer
    {

        public const string PROP_TYPEHASH = "_typeHash";

        #region Properties

        private System.Reflection.FieldInfo _currentField;
        private TypeReference.ConfigAttribute _currentAttrib;

        public System.Type[] InheritsFromTypes { get; set; }

        public System.Type DefaultType { get; set; }

        public TypeDropDownListingStyle DropDownStyle { get; set; } = TypeDropDownListingStyle.Flat;

        public System.Predicate<System.Type> EnumeratePredicate { get; set; }

        public System.Func<System.Type, string, bool> SearchFilter { get; set; }

        public int MaxVisibleCount { get; set; } = TypeDropDownWindowSelector.DEFAULT_MAXCOUNT;

        #endregion

        public void ConfigureSimple(System.Type inheritsFromType, bool allowAbstract = false, bool allowInterfaces = false, bool allowGeneric = false, System.Type[] excludedTypes = null)
        {
            this.InheritsFromTypes = new System.Type[] { inheritsFromType ?? typeof(object) };
            this.EnumeratePredicate = TypeDropDownWindowSelector.CreateEnumeratePredicate(inheritsFromType, allowAbstract, allowInterfaces, allowGeneric, excludedTypes);
        }

        private void Init()
        {
            if(this.fieldInfo != null && _currentField != this.fieldInfo)
            {
                _currentField = this.fieldInfo;
                _currentAttrib = this.fieldInfo.GetCustomAttributes(typeof(TypeReference.ConfigAttribute), true).FirstOrDefault() as TypeReference.ConfigAttribute;
                if (_currentAttrib != null)
                {
                    this.InheritsFromTypes = _currentAttrib.inheritsFromTypes;
                    this.DefaultType = _currentAttrib.defaultType;
                    this.DropDownStyle = _currentAttrib.dropDownStyle;
                    this.EnumeratePredicate = CreateEnumeratePredicate(_currentAttrib);
                    this.MaxVisibleCount = _currentAttrib.MaxVisibleCount > int.MinValue ? _currentAttrib.MaxVisibleCount : TypeDropDownWindowSelector.DEFAULT_MAXCOUNT;
                }
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            this.Init();

            EditorGUI.BeginProperty(position, label, property);

            var tp = GetTypeFromTypeReference(property);
            EditorGUI.BeginChangeCheck();
            tp = SPEditorGUI.TypeDropDown(position, label, tp, this.EnumeratePredicate, this.InheritsFromTypes?.Length == 1 ? this.InheritsFromTypes[0] : null, this.DefaultType, this.DropDownStyle, this.SearchFilter, this.MaxVisibleCount);
            if (EditorGUI.EndChangeCheck())
            {
                SetTypeToTypeReference(property, tp);
            }

            EditorGUI.EndProperty();
        }


        public static System.Type GetTypeFromTypeReference(SerializedProperty property)
        {
            if (property == null) return null;
            var hashProp = property.FindPropertyRelative(PROP_TYPEHASH);
            if (hashProp == null) return null;
            return TypeReference.UnHashType(hashProp.stringValue);
        }

        public static void SetTypeToTypeReference(SerializedProperty property, System.Type tp)
        {
            if (property == null) return;

            var hashProp = property.FindPropertyRelative(PROP_TYPEHASH);
            if (hashProp == null) return;

            hashProp.stringValue = TypeReference.HashType(tp);

            if(Application.isPlaying)
            {
                if (EditorHelper.GetTargetObjectOfProperty(property) is TypeReference tpref)
                {
                    tpref.Type = tp;
                }
            }
        }


        public static System.Predicate<System.Type> CreateEnumeratePredicate(TypeReference.ConfigAttribute attrib)
        {
            if(attrib.inheritsFromTypes?.Length > 1)
            {
                return TypeDropDownWindowSelector.CreateEnumeratePredicate(attrib.inheritsFromTypes, attrib.allowAbstractClasses, attrib.allowInterfaces, attrib.allowGeneric, attrib.excludedTypes);
            }
            else
            {
                return TypeDropDownWindowSelector.CreateEnumeratePredicate(attrib.inheritsFromTypes?.FirstOrDefault() ?? typeof(object), attrib.allowAbstractClasses, attrib.allowInterfaces, attrib.allowGeneric, attrib.excludedTypes);
            }
        }


    }

}
