using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppyeditor;

using com.spacepuppy.Statistics.Events;
using com.spacepuppyeditor.Core;

namespace com.spacepuppyeditor.Statistics.Events
{

    [CustomEditor(typeof(i_AdjustTokenFromVariant))]
    public class i_AdjustTokenFromVariantInspector : SPEditor
    {

        private const string PROP_CATEGORY = "_category";
        private const string PROP_TOKEN = "_token";
        private const string PROP_VALUE = "_value";
        private const string PROP_MODE = "_mode";

        private VariantReferencePropertyDrawer _variantDrawer = new VariantReferencePropertyDrawer();

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.Update();

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);
            this.DrawPropertyField(EditorHelper.PROP_ORDER);
            this.DrawPropertyField(EditorHelper.PROP_ACTIVATEON);
            EditorGUILayout.Space();

            var catprop = this.serializedObject.FindProperty(PROP_CATEGORY);
            var idprop = this.serializedObject.FindProperty(PROP_TOKEN);
            var valueprop = this.serializedObject.FindProperty(PROP_VALUE);
            var modeprop = this.serializedObject.FindProperty(PROP_MODE);

            int selection = Mathf.Max(0, StatisticsTokenLedgerCategories.FindIndexOfCategory(catprop.stringValue));
            selection = EditorGUILayout.Popup("Category", selection, StatisticsTokenLedgerCategories.Categories.Select(o => o.Name).ToArray());
            catprop.stringValue = StatisticsTokenLedgerCategories.IndexInRange(selection) ? StatisticsTokenLedgerCategories.Categories[selection].Name : null;

            if (StatisticsTokenLedgerCategories.IndexInRange(selection))
            {
                var category = StatisticsTokenLedgerCategories.Categories[selection];

                selection = category.Entries.IndexOf(idprop.stringValue);
                selection = EditorGUILayout.Popup("Token", selection, category.Entries);
                idprop.stringValue = selection >= 0 ? category.Entries[selection] : null;

                var valuelabel = EditorHelper.TempContent(valueprop.displayName, valueprop.tooltip);
                if (category.DataStore == typeof(bool))
                {
                    //valueprop.doubleValue = EditorGUILayout.Toggle("True/False", valueprop.doubleValue != 0d) ? 1 : 0;
                    _variantDrawer.RestrictVariantType = true;
                    _variantDrawer.TypeRestrictedTo = typeof(bool);
                    _variantDrawer.OnGUI(EditorGUILayout.GetControlRect(true, _variantDrawer.GetPropertyHeight(valueprop, valuelabel)), valueprop, valuelabel);
                    modeprop.SetEnumValue(i_AdjustToken.SetMode.Set);
                }
                else if (category.DataStore.IsEnum)
                {
                    //int eval = (int)valueprop.doubleValue;
                    //valueprop.doubleValue = ConvertUtil.ToInt(SPEditorGUILayout.EnumPopup("Value", ConvertUtil.ToEnumOfType(category.DataStore, eval)));
                    _variantDrawer.RestrictVariantType = true;
                    _variantDrawer.TypeRestrictedTo = category.DataStore;
                    _variantDrawer.OnGUI(EditorGUILayout.GetControlRect(true, _variantDrawer.GetPropertyHeight(valueprop, valuelabel)), valueprop, valuelabel);
                    modeprop.SetEnumValue(i_AdjustToken.SetMode.Set);
                }
                else if (category.DataStore == typeof(int))
                {
                    //valueprop.doubleValue = EditorGUILayout.IntField("Value", (int)valueprop.doubleValue);
                    _variantDrawer.RestrictVariantType = true;
                    _variantDrawer.TypeRestrictedTo = typeof(int);
                    _variantDrawer.OnGUI(EditorGUILayout.GetControlRect(true, _variantDrawer.GetPropertyHeight(valueprop, valuelabel)), valueprop, valuelabel);
                    SPEditorGUILayout.PropertyField(modeprop);
                }
                else if (ConvertUtil.IsNumericType(category.DataStore))
                {
                    //valueprop.doubleValue = EditorGUILayout.DoubleField("Value", valueprop.doubleValue);
                    _variantDrawer.RestrictVariantType = true;
                    _variantDrawer.TypeRestrictedTo = typeof(double);
                    _variantDrawer.OnGUI(EditorGUILayout.GetControlRect(true, _variantDrawer.GetPropertyHeight(valueprop, valuelabel)), valueprop, valuelabel);
                    SPEditorGUILayout.PropertyField(modeprop);
                }
            }

            EditorGUILayout.Space();
            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, EditorHelper.PROP_ORDER, EditorHelper.PROP_ACTIVATEON, PROP_CATEGORY, PROP_TOKEN, PROP_VALUE, PROP_MODE);

            this.serializedObject.ApplyModifiedProperties();
        }

    }

}