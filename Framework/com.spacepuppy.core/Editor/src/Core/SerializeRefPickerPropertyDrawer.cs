using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Collections;
using com.spacepuppy.Dynamic;
using com.spacepuppy.Utils;

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
            float h;
            if (EditorHelper.AssertMultiObjectEditingNotSupportedHeight(property, label, out h)) return h;

            bool cache = property.isExpanded;
            if (this.AlwaysExpanded) property.isExpanded = true;

            try
            {
                //float objheight = !string.IsNullOrEmpty(property.managedReferenceFullTypename) ? EditorGUI.GetPropertyHeight(property, label, true) + 4f : EditorGUIUtility.singleLineHeight * 2f;
                float objheight = 0f;
                if (string.IsNullOrEmpty(property.managedReferenceFullTypename)) //this means nothing is referenced currently
                {
                    objheight = EditorGUIUtility.singleLineHeight;
                }
                else if (property.isExpanded)
                {
                    objheight += FindPropertyDrawer(EditorHelper.GetManagedReferenceType(property)).GetPropertyHeight(property, GUIContent.none);
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
            if (EditorHelper.AssertMultiObjectEditingNotSupported(position, property, label)) return;

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
                        FindPropertyDrawer(EditorHelper.GetManagedReferenceType(property)).OnGUI(drawArea, property, GUIContent.none);
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
                    selectorArea = new Rect(position.xMin, position.yMin, position.width, EditorGUIUtility.singleLineHeight);
                    var drawArea = new Rect(position.xMin, position.yMin + SELECTOR_VER_MARGIN + EditorGUIUtility.singleLineHeight, position.width, Mathf.Max(position.height - EditorGUIUtility.singleLineHeight - SELECTOR_VER_MARGIN, 0f));
                    if (this.AlwaysExpanded)
                    {
                        property.isExpanded = true;
                        if (label.HasContent())
                        {
                            selectorArea = EditorGUI.PrefixLabel(selectorArea, label);
                        }
                        FindPropertyDrawer(EditorHelper.GetManagedReferenceType(property)).OnGUI(drawArea, property, GUIContent.none);
                    }
                    else
                    {
                        cache = SPEditorGUI.PrefixFoldoutLabel(ref selectorArea, property.isExpanded, label);
                        if (property.isExpanded)
                        {
                            FindPropertyDrawer(EditorHelper.GetManagedReferenceType(property)).OnGUI(drawArea, property, GUIContent.none);
                        }
                    }

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
            if (EditorHelper.AssertMultiObjectEditingNotSupported(position, property, label)) return false;

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
                    if (tp != null)
                    {
                        var oldobj = property.managedReferenceValue;
                        var newobj = System.Activator.CreateInstance(tp);
                        if (oldobj != null)
                        {
                            var token = StateToken.GetToken();
                            token.CopyFrom(oldobj);
                            token.CopyTo(newobj);
                            StateToken.ReleaseTempToken(token);
                        }
                        property.managedReferenceValue = newobj;
                    }
                    else
                    {
                        property.managedReferenceValue = null;
                    }
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
            if (_availableTypes.TryGetValue(reftp, out info))
            {
                return info;
            }

            var types = TypeUtil.GetTypesAssignableFrom(reftp).Where(o => !o.IsAbstract && !o.IsInterface && (o.IsValueType || o.GetConstructor(System.Type.EmptyTypes) != null) && !TypeUtil.IsType(o, typeof(UnityEngine.Object)) && o.IsSerializable).ToList();
            types.Sort((a, b) =>
            {
                var ap = a.GetCustomAttribute<SerializeRefLabelAttribute>();
                var bp = b.GetCustomAttribute<SerializeRefLabelAttribute>();
                var albl = string.IsNullOrEmpty(ap?.Label) ? a.Name : ap.Label;
                var blbl = string.IsNullOrEmpty(bp?.Label) ? b.Name : bp.Label;
                int aord = ap?.Order ?? 0;
                int bord = bp?.Order ?? 0;
                return aord == bord ? albl.CompareTo(blbl) : aord.CompareTo(bord);
            });

            info = new TypeInfo();
            info.AvailableTypes = types.ToArray();
            info.AvailableTypeNames = info.AvailableTypes.Select(tp =>
            {
                var attrib = tp.GetCustomAttribute<SerializeRefLabelAttribute>();
                if (!string.IsNullOrEmpty(attrib?.Label)) return new GUIContent(attrib.Label);

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

        #region Static PropertryDrawer Cache

        private static readonly Dictionary<System.Type, PropertyDrawer> _cachedPropertyDrawers = new();
        private static System.Type FindPropertyDrawerTypeFor(System.Type tp)
        {
            TempList<System.Type> useForChildTypes = null;
            foreach (var drawerType in TypeUtil.GetTypesAssignableFrom(typeof(PropertyDrawer)))
            {
                if (drawerType.GetConstructor(System.Type.EmptyTypes) == null) continue;

                foreach (var attrib in drawerType.GetCustomAttributes<CustomPropertyDrawer>(false))
                {
                    var atp = DynamicUtil.GetValue(attrib, "m_Type") as System.Type;
                    if (atp == tp)
                    {
                        return drawerType;
                    }
                    else if (System.Convert.ToBoolean(DynamicUtil.GetValue(attrib, "m_UseForChildren")) && TypeUtil.IsType(tp, atp))
                    {
                        (useForChildTypes ??= TempCollection.GetList<System.Type>()).Add(drawerType);
                    }
                }
            }

            if (useForChildTypes != null)
            {
                var result = useForChildTypes.FirstOrDefault();
                useForChildTypes.Dispose();
                return result;
            }
            else
            {
                return null;
            }
        }

        public static PropertyDrawer FindPropertyDrawer(System.Type tp)
        {
            if (tp == null) return SimpleClassDrawer.Default;

            PropertyDrawer result;
            if (_cachedPropertyDrawers.TryGetValue(tp, out result)) return result;

            var drawerType = FindPropertyDrawerTypeFor(tp);
            if (drawerType != null)
            {
                result = System.Activator.CreateInstance(drawerType) as PropertyDrawer;
            }

            if (result == null)
            {
                result = SimpleClassDrawer.Default;
            }

            _cachedPropertyDrawers[tp] = result;
            return result;
        }

        private class SimpleClassDrawer : PropertyDrawer
        {

            internal static readonly SimpleClassDrawer Default = new();

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                float h = 0;
                foreach (var child in property.GetChildren())
                {
                    h += SPEditorGUI.GetPropertyHeight(child, label, true);
                }
                return h;
            }

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                SPEditorGUI.FlatChildPropertyField(position, property);
            }
        }

        #endregion

    }

}
