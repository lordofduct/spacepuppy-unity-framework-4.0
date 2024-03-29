﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(EnumPopupExcludingAttribute))]
    public class EnumPopupExcludingPropertyDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var enumType = this.fieldInfo.FieldType;
            if(!enumType.IsEnum)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            System.Enum evalue = property.GetEnumValue(enumType);
            var attrib = this.attribute as EnumPopupExcludingAttribute;
            if(attrib != null && attrib.excludedValues != null && attrib.excludedValues.Length > 0)
            {
                var excludedValues = (from i in attrib.excludedValues select ConvertUtil.ToEnumOfType(enumType, i)).ToArray();
                //property.enumValueIndex = ConvertUtil.ToInt(SPEditorGUI.EnumPopupExcluding(position, label, evalue, excludedValues));
                property.SetEnumValue(SPEditorGUI.EnumPopupExcluding(position, label, evalue, excludedValues));
            }
            else
            {
                //property.enumValueIndex = ConvertUtil.ToInt(EditorGUI.EnumPopup(position, label, evalue));
                property.SetEnumValue(EditorGUI.EnumPopup(position, label, evalue));
            }

        }

    }

}
