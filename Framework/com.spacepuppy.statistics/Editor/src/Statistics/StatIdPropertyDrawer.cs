using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Statistics;
using com.spacepuppyeditor.Statistics;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Statistics
{

    [CustomPropertyDrawer(typeof(StatId))]
    public class StatIdPropertyDrawer : PropertyDrawer
    {

        public const string PROP_CATEGORY = nameof(StatId.Category);
        public const string PROP_TOKEN = nameof(StatId.Token);

        private bool _hideCustom;
        public virtual bool HideCustom
        {
            get => _hideCustom;
            set => _hideCustom = value;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h;
            if (EditorHelper.AssertMultiObjectEditingNotSupportedHeight(property, label, out h)) return h;

            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (EditorHelper.AssertMultiObjectEditingNotSupported(position, property, label)) return;

            if (property.propertyType == SerializedPropertyType.String)
            {
                DrawAsString(position, property, label);
            }
            else if (property.propertyType == SerializedPropertyType.Generic && property.type == nameof(StatId))
            {
                DrawAsStatId(position, property, label);
            }
            else
            {
                EditorGUI.LabelField(position, label, EditorHelper.TempContent("Mismatched PropertyDrawer..."));
            }
        }

        void DrawAsStatId(Rect position, SerializedProperty property, GUIContent label)
        {
            var catprop = property.FindPropertyRelative(PROP_CATEGORY);
            var tokenprop = property.FindPropertyRelative(PROP_TOKEN);

            position = SPEditorGUI.SafePrefixLabel(position, label);
            var r0 = new Rect(position.xMin, position.yMin, Mathf.FloorToInt(position.width / 2f), position.height);
            var r1 = new Rect(r0.xMax + 1, position.yMin, position.width - r0.width - 1, position.height);

            EditorGUI.BeginChangeCheck();
            string selection = catprop.stringValue;
            if (this.HideCustom)
            {
                int index = Mathf.Max(0, StatisticsTokenLedgerCategories.FindIndexOfCategory(catprop.stringValue));
                index = EditorGUI.Popup(r0, index, StatisticsTokenLedgerCategories.Categories.Select(o => o.PopupPath).ToArray());
                selection = StatisticsTokenLedgerCategories.IndexInRange(index) ? StatisticsTokenLedgerCategories.Categories[index].Name : null;
            }
            else
            {
                var options = StatisticsTokenLedgerCategories.Categories.Select(o => o.Name).ToArray();
                var guioptions = StatisticsTokenLedgerCategories.Categories.Select(o => EditorHelper.TempContent(o.PopupPath)).ToArray();
                selection = SPEditorGUI.OptionPopupWithCustom(r0, GUIContent.none, catprop.stringValue, options, guioptions);
            }
            if (EditorGUI.EndChangeCheck())
            {
                catprop.stringValue = selection;
            }

            if (!string.IsNullOrEmpty(catprop.stringValue))
            {
                int catindex = Mathf.Max(0, StatisticsTokenLedgerCategories.FindIndexOfCategory(catprop.stringValue));
                if (StatisticsTokenLedgerCategories.IndexInRange(catindex))
                {
                    var category = StatisticsTokenLedgerCategories.Categories[catindex];
                    EditorGUI.BeginChangeCheck();
                    selection = tokenprop.stringValue;
                    if (this.HideCustom)
                    {
                        int index = category.EntriesArray.IndexOf(tokenprop.stringValue);
                        index = EditorGUI.Popup(r1, index, category.EntriesArray);
                        selection = index >= 0 ? category.EntriesArray[index] : null;
                    }
                    else
                    {
                        selection = SPEditorGUI.OptionPopupWithCustom(r1, GUIContent.none, tokenprop.stringValue, category.EntriesArray);
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        tokenprop.stringValue = selection;
                    }
                }
                else if (this.HideCustom)
                {
                    EditorGUI.Popup(position, -1, ArrayUtil.Empty<GUIContent>());
                    property.stringValue = string.Empty;
                }
                else
                {
                    property.stringValue = EditorGUI.TextField(position, property.stringValue);
                }
            }
            else
            {
                tokenprop.stringValue = string.Empty;
            }
        }

        void DrawAsString(Rect position, SerializedProperty property, GUIContent label)
        {
            position = SPEditorGUI.SafePrefixLabel(position, label);

            EditorGUI.BeginChangeCheck();
            string selection;
            if (this.HideCustom)
            {
                int index = Mathf.Max(0, StatisticsTokenLedgerCategories.FindIndexOfCategory(property.stringValue));
                index = EditorGUI.Popup(position, index, StatisticsTokenLedgerCategories.Categories.Select(o => o.PopupPath).ToArray());
                selection = StatisticsTokenLedgerCategories.IndexInRange(index) ? StatisticsTokenLedgerCategories.Categories[index].Name : null;
            }
            else
            {
                var options = StatisticsTokenLedgerCategories.Categories.Select(o => o.Name).ToArray();
                var guioptions = StatisticsTokenLedgerCategories.Categories.Select(o => EditorHelper.TempContent(o.PopupPath)).ToArray();
                selection = SPEditorGUI.OptionPopupWithCustom(position, GUIContent.none, property.stringValue, options, guioptions);
            }
            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = selection;
            }
        }

    }

    [CustomPropertyDrawer(typeof(TokenLedgerCategorySelectorAttribute))]
    public class TokenLedgerCategorySelectorPropertyDrawer : StatIdPropertyDrawer
    {
        public override bool HideCustom
        {
            get => (this.attribute as TokenLedgerCategorySelectorAttribute)?.HideCustom ?? base.HideCustom;
            set => base.HideCustom = value;
        }
    }

    [CustomPropertyDrawer(typeof(TokenLedgerCategoryEntrySelectorAttribute))]
    public class TokenLedgerCategoryEntrySelectorPropertyDrawer : PropertyDrawer
    {


        private bool _hideCustom;
        public bool HideCustom
        {
            get => (this.attribute as TokenLedgerCategoryEntrySelectorAttribute)?.HideCustom ?? _hideCustom;
            set => _hideCustom = value;
        }

        private string _categoryFilter;
        public string CategoryFilter
        {
            get => (this.attribute as TokenLedgerCategoryEntrySelectorAttribute)?.CategoryFilter ?? _categoryFilter;
            set => _categoryFilter = value;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label, EditorHelper.TempContent("Mismatched PropertyDrawer..."));
                return;
            }

            position = SPEditorGUI.SafePrefixLabel(position, label);
            string scategory = null;
            var sfilter = this.CategoryFilter;
            if (!string.IsNullOrEmpty(sfilter))
            {
                //TODO - add support for other filters
                //if (scategory.StartsWith("call:"))
                //{

                //}
                //else if (sfilter.StartsWith("sibling:"))
                //{

                //}
                //else if
                if (StatisticsTokenLedgerCategories.FindIndexOfCategory(sfilter) >= 0)
                {
                    scategory = sfilter;
                }
            }

            int catindex = Mathf.Max(0, StatisticsTokenLedgerCategories.FindIndexOfCategory(scategory));
            if (StatisticsTokenLedgerCategories.IndexInRange(catindex))
            {
                var category = StatisticsTokenLedgerCategories.Categories[catindex];
                EditorGUI.BeginChangeCheck();
                string selection = property.stringValue;
                if (this.HideCustom)
                {
                    int index = category.EntriesArray.IndexOf(property.stringValue);
                    index = EditorGUI.Popup(position, index, category.EntriesArray);
                    selection = index >= 0 ? category.EntriesArray[index] : null;
                }
                else
                {
                    selection = SPEditorGUI.OptionPopupWithCustom(position, GUIContent.none, property.stringValue, category.EntriesArray);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    property.stringValue = selection;
                }
            }
            else if (this.HideCustom)
            {
                EditorGUI.Popup(position, -1, ArrayUtil.Empty<GUIContent>());
                property.stringValue = string.Empty;
            }
            else
            {
                property.stringValue = EditorGUI.TextField(position, property.stringValue);
            }
        }


    }

}
