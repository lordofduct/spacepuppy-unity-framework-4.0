using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;
using System.Runtime.CompilerServices;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(SerializeRefPickerAttribute))]
    public class SerializeRefPickerPropertyDrawer : PropertyDrawer
    {

        private static float TOP_PAD => 2f + EditorGUIUtility.singleLineHeight;
        private const float BOTTOM_PAD = 5f;
        private const float MARGIN = 1f;
        private const float MARGIN_DBL = MARGIN * 2f;
        private const float SELECTOR_MARGIN = MARGIN + 8f;
        private const float SELECTOR_MARGIN_DBL = SELECTOR_MARGIN * 2f;

        private System.Type _refType;
        private bool _allowNull;
        private bool _displayBox;
        private bool _alwaysExpanded;

        #region Properties

        public System.Type RefType
        {
            get => (this.attribute as SerializeRefPickerAttribute)?.RefType ?? _refType;
            set => _refType = value;
        }

        public bool AllowNull
        {
            get => (this.attribute as SerializeRefPickerAttribute)?.AllowNull ?? _allowNull;
            set => _allowNull = value;
        }

        public bool DisplayBox
        {
            get => (this.attribute as SerializeRefPickerAttribute)?.DisplayBox ?? _displayBox;
            set => _displayBox = value;
        }

        public bool AlwaysExpanded
        {
            get => (this.attribute as SerializeRefPickerAttribute)?.AlwaysExpanded ?? _alwaysExpanded;
            set => _alwaysExpanded = value;
        }

        #endregion

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            bool cache = property.isExpanded;
            if (this.AlwaysExpanded) property.isExpanded = true;

            try
            {
                float objheight = !string.IsNullOrEmpty(property.managedReferenceFullTypename) ? EditorGUI.GetPropertyHeight(property, label, true) + 4f : EditorGUIUtility.singleLineHeight * 2f;

                if (this.DisplayBox)
                {
                    return objheight + BOTTOM_PAD + TOP_PAD - EditorGUIUtility.singleLineHeight;
                }
                else
                {
                    return objheight;
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
            if (this.AlwaysExpanded) property.isExpanded = true;

            Rect selectorArea;
            bool drawSelector;
            try
            {
                if (this.DisplayBox)
                {
                    if (!this.AlwaysExpanded) cache = SPEditorGUI.PrefixFoldoutLabel(position, property.isExpanded, GUIContent.none);

                    if (property.isExpanded)
                    {
                        //float h = SPEditorGUI.GetDefaultPropertyHeight(property, label, true) + BOTTOM_PAD + TOP_PAD - EditorGUIUtility.singleLineHeight;
                        //var area = new Rect(position.xMin, position.yMax - h, position.width, h);
                        var area = position;
                        var drawArea = new Rect(area.xMin + MARGIN, area.yMin + TOP_PAD + EditorGUIUtility.singleLineHeight, area.width - MARGIN_DBL, area.height - TOP_PAD - EditorGUIUtility.singleLineHeight);

                        GUI.BeginGroup(area, label, GUI.skin.box);
                        GUI.EndGroup();

                        EditorGUI.indentLevel++;
                        SPEditorGUI.FlatChildPropertyField(drawArea, property);
                        EditorGUI.indentLevel--;

                        selectorArea = new Rect(position.xMin + SELECTOR_MARGIN, position.yMin + TOP_PAD, position.width - SELECTOR_MARGIN_DBL, EditorGUIUtility.singleLineHeight);
                        drawSelector = true;
                    }
                    else
                    {
                        GUI.BeginGroup(position, label, GUI.skin.box);
                        GUI.EndGroup();

                        selectorArea = default(Rect);
                        drawSelector = false;
                    }
                }
                else
                {
                    if (this.AlwaysExpanded)
                    {
                        property.isExpanded = true;
                        EditorGUI.PrefixLabel(new Rect(position.xMin, position.yMin, position.width, EditorGUIUtility.singleLineHeight), label);
                        SPEditorGUI.FlatChildPropertyField(new Rect(position.xMin, position.yMin + EditorGUIUtility.singleLineHeight, position.width, Mathf.Max(position.height - EditorGUIUtility.singleLineHeight, 0f)), property);
                    }
                    else
                    {
                        SPEditorGUI.DefaultPropertyField(position, property, label, true);
                    }

                    selectorArea = new Rect(position.xMin + EditorGUIUtility.labelWidth, position.yMin, Mathf.Max(0f, position.width - EditorGUIUtility.labelWidth), EditorGUIUtility.singleLineHeight);
                    drawSelector = true;
                }
            }
            finally
            {
                property.isExpanded = cache;
            }




            if (!drawSelector || Application.isPlaying) return;

            if (this.RefType != null)
            {
                var info = GetAvailableTypes(this.RefType);
                if (info == null) return;

                var tp = property.GetManagedReferenceType();

                var atypes = this.AllowNull ? info.Value.AvailableTypesWithNull : info.Value.AvailableTypes;
                var anames = this.AllowNull ? info.Value.AvailableTypeNamesWithNull : info.Value.AvailableTypeNames;

                int index = atypes.IndexOf(tp);
                EditorGUI.BeginChangeCheck();
                index = EditorGUI.Popup(selectorArea, GUIContent.none, index, anames);
                if (EditorGUI.EndChangeCheck())
                {
                    if (index >= 0)
                    {
                        tp = atypes[index];
                        property.managedReferenceValue = tp != null ? System.Activator.CreateInstance(tp) : null;
                        com.spacepuppyeditor.Internal.ScriptAttributeUtility.ResetPropertyHandler(property, true);
                    }
                }
            }
        }


        #region Static Entries

        private static Dictionary<System.Type, TypeInfo> _availableTypes = new Dictionary<System.Type, TypeInfo>();

        public static TypeInfo? GetAvailableTypes(System.Type reftp)
        {
            if (reftp == null) return null;

            TypeInfo info;
            if(_availableTypes.TryGetValue(reftp, out info))
            {
                return info;
            }

            info = new TypeInfo();
            info.AvailableTypes = TypeUtil.GetTypesAssignableFrom(reftp).Where(o => !o.IsAbstract && !o.IsInterface && (o.IsValueType || o.GetConstructor(System.Type.EmptyTypes) != null) && !TypeUtil.IsType(o, typeof(UnityEngine.Object)) && o.IsSerializable).OrderBy(o => o.Name).ToArray();
            info.AvailableTypeNames = info.AvailableTypes.Select(tp =>
            {
                if (info.AvailableTypes.Count(o => string.Equals(o.Name, tp.Name)) > 1)
                {
                    return new GUIContent(tp.Name + " : " + tp.FullName);
                }
                else
                {
                    return new GUIContent(tp.Name);
                }
            }).ToArray();

            info.AvailableTypesWithNull = info.AvailableTypes.Append(null).ToArray();
            info.AvailableTypeNamesWithNull = info.AvailableTypeNames.Append(new GUIContent("NULL")).ToArray();

            _availableTypes[reftp] = info;
            return info;
        }

        public struct TypeInfo
        {
            public System.Type[] AvailableTypes;
            public GUIContent[] AvailableTypeNames;

            public System.Type[] AvailableTypesWithNull;
            public GUIContent[] AvailableTypeNamesWithNull;
        }

        #endregion

    }

}
