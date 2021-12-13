using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(DisplayFlatAttribute))]
    public class DisplayFlatPropertyDrawer : PropertyDrawer
    {

        private static float TOP_PAD => 2f + EditorGUIUtility.singleLineHeight;
        private const float BOTTOM_PAD = 2f;
        private const float MARGIN = 2f;

        private bool _displayBox;
        private bool _alwaysExpanded;

        #region Properties

        public bool DisplayBox
        {
            get => (this.attribute as DisplayFlatAttribute)?.DisplayBox ?? _displayBox;
            set => _displayBox = value;
        }

        public bool AlwaysExpanded
        {
            get => (this.attribute as DisplayFlatAttribute)?.AlwaysExpanded ?? _alwaysExpanded;
            set => _alwaysExpanded = value;
        }

        #endregion

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            bool cache = property.isExpanded;
            if (this.AlwaysExpanded)
            {
                property.isExpanded = true;
            }

            try
            {
                if (property.isExpanded)
                {
                    if (property.hasChildren)
                    {
                        if(this.DisplayBox)
                        {
                            return SPEditorGUI.GetDefaultPropertyHeight(property, label, true) + BOTTOM_PAD + TOP_PAD - EditorGUIUtility.singleLineHeight;
                        }
                        else
                        {
                            return Mathf.Max(SPEditorGUI.GetDefaultPropertyHeight(property, label, true) - EditorGUIUtility.singleLineHeight, 0f);
                        }
                    }
                    else
                    {
                        return SPEditorGUI.GetDefaultPropertyHeight(property, label);
                    }
                }
                else if (this.DisplayBox)
                {
                    return EditorGUIUtility.singleLineHeight + BOTTOM_PAD;
                }
                else
                {
                    return EditorGUIUtility.singleLineHeight;
                }
            }
            finally
            {
                property.isExpanded = cache;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool cache = property.isExpanded;
            if (this.AlwaysExpanded)
            {
                property.isExpanded = true;
            }

            try
            {
                if (!property.hasChildren)
                {
                    SPEditorGUI.DefaultPropertyField(position, property, label);
                    return;
                }

                if (!this.AlwaysExpanded) cache = SPEditorGUI.PrefixFoldoutLabel(position, property.isExpanded, GUIContent.none);

                if (property.isExpanded)
                {
                    if(this.DisplayBox)
                    {
                        //float h = SPEditorGUI.GetDefaultPropertyHeight(property, label, true) + BOTTOM_PAD + TOP_PAD - EditorGUIUtility.singleLineHeight;
                        //var area = new Rect(position.xMin, position.yMax - h, position.width, h);
                        var area = position;
                        var drawArea = new Rect(area.xMin, area.yMin + TOP_PAD, area.width - MARGIN, area.height - TOP_PAD);

                        GUI.BeginGroup(area, label, GUI.skin.box);
                        GUI.EndGroup();

                        EditorGUI.indentLevel++;
                        SPEditorGUI.FlatChildPropertyField(drawArea, property);
                        EditorGUI.indentLevel--;
                    }
                    else
                    {
                        SPEditorGUI.FlatChildPropertyField(position, property);
                    }
                }
                else if(this.DisplayBox)
                {
                    GUI.BeginGroup(position, label, GUI.skin.box);
                    GUI.EndGroup();
                }
                else
                {
                    EditorGUI.PrefixLabel(position, label);
                }
            }
            finally
            {
                property.isExpanded = cache;
            }
        }

    }
}
