using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace com.spacepuppyeditor.Core
{

    [CustomPropertyDrawer(typeof(SerializeRefPickerAttribute))]
    public class SerializeRefPickerPropertyDrawer : PropertyDrawer
    {

        private static float TOP_PAD => 2f + EditorGUIUtility.singleLineHeight;
        private const float BOTTOM_PAD = 5f;
        private const float MARGIN = 1f;
        private const float MARGIN_DBL = MARGIN * 2f;
        private const float SELECTOR_HOR_MARGIN = MARGIN + 8f;
        private const float SELECTOR_HOR_MARGIN_DBL = SELECTOR_HOR_MARGIN * 2f;
        private const float SELECTOR_VER_MARGIN = 2f;

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

        public string NullLabel
        {
            get;
            set;
        }

        #endregion

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            bool cache = property.isExpanded;
            if (this.AlwaysExpanded) property.isExpanded = true;

            try
            {
                //float objheight = !string.IsNullOrEmpty(property.managedReferenceFullTypename) ? EditorGUI.GetPropertyHeight(property, label, true) + 4f : EditorGUIUtility.singleLineHeight * 2f;
                float objheight = 0f;
                if(string.IsNullOrEmpty(property.managedReferenceFieldTypename))
                {
                    objheight = EditorGUIUtility.singleLineHeight;
                }
                else if(property.isExpanded)
                {
                    foreach (var child in property.GetChildren())
                    {
                        objheight += SPEditorGUI.GetPropertyHeight(child);
                    }
                }

                objheight += EditorGUIUtility.singleLineHeight + SELECTOR_VER_MARGIN;

                if (this.DisplayBox)
                {
                    return objheight + BOTTOM_PAD + TOP_PAD;
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
                        var drawArea = new Rect(area.xMin + MARGIN, area.yMin + TOP_PAD + SELECTOR_VER_MARGIN + EditorGUIUtility.singleLineHeight, area.width - MARGIN_DBL, area.height - TOP_PAD - EditorGUIUtility.singleLineHeight);

                        GUI.BeginGroup(area, label, GUI.skin.box);
                        GUI.EndGroup();

                        EditorGUI.indentLevel++;
                        SPEditorGUI.FlatChildPropertyField(drawArea, property);
                        EditorGUI.indentLevel--;

                        selectorArea = new Rect(position.xMin + SELECTOR_HOR_MARGIN, position.yMin + TOP_PAD, position.width - SELECTOR_HOR_MARGIN_DBL, EditorGUIUtility.singleLineHeight);
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
                    var drawArea = new Rect(position.xMin, position.yMin + SELECTOR_VER_MARGIN + EditorGUIUtility.singleLineHeight, position.width, Mathf.Max(position.height - EditorGUIUtility.singleLineHeight - SELECTOR_VER_MARGIN, 0f));
                    if (this.AlwaysExpanded)
                    {
                        property.isExpanded = true;
                        EditorGUI.PrefixLabel(new Rect(position.xMin, position.yMin, position.width, EditorGUIUtility.singleLineHeight), label);
                        SPEditorGUI.FlatChildPropertyField(drawArea, property);
                    }
                    else
                    {
                        cache = SPEditorGUI.PrefixFoldoutLabel(new Rect(position.xMin, position.yMin, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, label);
                        if (property.isExpanded)
                        {
                            SPEditorGUI.FlatChildPropertyField(drawArea, property);
                        }

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
                DrawRefPicker(selectorArea, property, GUIContent.none, this.RefType, this.AllowNull, this.NullLabel);
            }
        }


        #region Static Entries

        private static Dictionary<System.Type, TypeInfo> _availableTypes = new Dictionary<System.Type, TypeInfo>();


        public static bool DrawRefPicker(Rect position, SerializedProperty property, GUIContent label, System.Type reftp, bool allownull, string nulllabel = null)
        {
            if (property == null) throw new System.ArgumentNullException(nameof(property));
            if (reftp == null) throw new System.ArgumentNullException(nameof(reftp));

            var info = GetAvailableTypes(reftp, nulllabel);
            if (info == null) return false;

            var tp = property.GetManagedReferenceType();

            var atypes = allownull ? info.Value.AvailableTypesWithNull : info.Value.AvailableTypes;
            var anames = allownull ? info.Value.AvailableTypeNamesWithNull : info.Value.AvailableTypeNames;

            int index = atypes.IndexOf(tp);
            EditorGUI.BeginChangeCheck();
            index = EditorGUI.Popup(position, label ?? GUIContent.none, index, anames);
            if (EditorGUI.EndChangeCheck())
            {
                if (index >= 0 && atypes[index] != tp)
                {
                    tp = atypes[index];
                    property.managedReferenceValue = tp != null ? System.Activator.CreateInstance(tp) : null;
                    com.spacepuppyeditor.Internal.ScriptAttributeUtility.ResetPropertyHandler(property, true);
                    return true;
                }
            }

            return false;
        }

        public static TypeInfo? GetAvailableTypes(System.Type reftp, string nulllabel = null)
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
                var attrib = tp.GetCustomAttribute<SerializeRefLabelAttribute>();
                if (attrib != null) return new GUIContent(attrib.Label ?? string.Empty);

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
            info.AvailableTypeNamesWithNull = info.AvailableTypeNames.Append(new GUIContent(nulllabel ?? "NULL")).ToArray();

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
