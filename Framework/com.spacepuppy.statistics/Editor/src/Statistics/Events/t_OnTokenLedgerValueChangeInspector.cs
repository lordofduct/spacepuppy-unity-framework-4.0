using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppyeditor;

using com.spacepuppy.Statistics.Events;

namespace com.spacepuppyeditor.Statistics.Events
{

    [CustomEditor(typeof(t_OnTokenLedgerValueChange))]
    public class t_OnTokenLedgerValueChangeInspector : SPEditor
    {

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.UpdateIfRequiredOrScript();

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);
            EditorGUILayout.Space();

            var filterprop = this.serializedObject.FindProperty(t_OnTokenLedgerValueChange.PROP_HITFILTER);
            var catprop = this.serializedObject.FindProperty(t_OnTokenLedgerValueChange.PROP_CATEGORY);
            var idprop = this.serializedObject.FindProperty(t_OnTokenLedgerValueChange.PROP_TOKEN);

            SPEditorGUILayout.PropertyField(filterprop);

            switch(filterprop.GetEnumValue<t_OnTokenLedgerValueChange.HitFilterOptions>())
            {
                case t_OnTokenLedgerValueChange.HitFilterOptions.Any:
                case t_OnTokenLedgerValueChange.HitFilterOptions.MultiOnly:
                    //do nothing
                    break;
                case t_OnTokenLedgerValueChange.HitFilterOptions.Direct:
                case t_OnTokenLedgerValueChange.HitFilterOptions.DirectOrMulti:
                    {
                        int selection = Mathf.Max(0, StatisticsTokenLedgerCategories.FindIndexOfCategory(catprop.stringValue));
                        selection = EditorGUILayout.Popup("Category", selection, StatisticsTokenLedgerCategories.Categories.Select(o => o.Name).ToArray());
                        catprop.stringValue = StatisticsTokenLedgerCategories.IndexInRange(selection) ? StatisticsTokenLedgerCategories.Categories[selection].Name : null;

                        if (StatisticsTokenLedgerCategories.IndexInRange(selection))
                        {
                            var category = StatisticsTokenLedgerCategories.Categories[selection];

                            selection = category.Entries.IndexOf(idprop.stringValue);
                            selection = EditorGUILayout.Popup("Token", selection, category.Entries);
                            idprop.stringValue = selection >= 0 ? category.Entries[selection] : null;
                        }
                    }
                    break;
                case t_OnTokenLedgerValueChange.HitFilterOptions.Category:
                case t_OnTokenLedgerValueChange.HitFilterOptions.CategoryOrMulti:
                    {
                        int selection = Mathf.Max(0, StatisticsTokenLedgerCategories.FindIndexOfCategory(catprop.stringValue));
                        selection = EditorGUILayout.Popup("Category", selection, StatisticsTokenLedgerCategories.Categories.Select(o => o.Name).ToArray());
                        catprop.stringValue = StatisticsTokenLedgerCategories.IndexInRange(selection) ? StatisticsTokenLedgerCategories.Categories[selection].Name : null;
                    }
                    break;
            }

            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, t_OnTokenLedgerValueChange.PROP_HITFILTER, t_OnTokenLedgerValueChange.PROP_CATEGORY, t_OnTokenLedgerValueChange.PROP_TOKEN);

            this.serializedObject.ApplyModifiedProperties();
        }

    }
}
