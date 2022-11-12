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

    [CustomEditor(typeof(i_TriggerIndexByTokenValue))]
    public class i_TriggerIndexByTokenValueInspector : SPEditor
    {

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.UpdateIfRequiredOrScript();

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);
            this.DrawPropertyField(EditorHelper.PROP_ORDER);
            this.DrawPropertyField(EditorHelper.PROP_ACTIVATEON);
            EditorGUILayout.Space();

            var catprop = this.serializedObject.FindProperty(i_TriggerIndexByTokenValue.PROP_CATEGORY);
            var idprop = this.serializedObject.FindProperty(i_TriggerIndexByTokenValue.PROP_TOKEN);


            int selection = Mathf.Max(0, StatisticsTokenLedgerCategories.FindIndexOfCategory(catprop.stringValue));
            selection = EditorGUILayout.Popup("Category", selection, StatisticsTokenLedgerCategories.Categories.Select(o => o.Name).ToArray());
            catprop.stringValue = StatisticsTokenLedgerCategories.IndexInRange(selection) ? StatisticsTokenLedgerCategories.Categories[selection].Name : null;

            if (StatisticsTokenLedgerCategories.IndexInRange(selection))
            {
                var category = StatisticsTokenLedgerCategories.Categories[selection];

                selection = category.Entries.IndexOf(idprop.stringValue);
                selection = EditorGUILayout.Popup("Id", selection, category.Entries);
                idprop.stringValue = selection >= 0 ? category.Entries[selection] : null;
            }

            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, EditorHelper.PROP_ORDER, EditorHelper.PROP_ACTIVATEON, i_TriggerIndexByTokenValue.PROP_CATEGORY, i_TriggerIndexByTokenValue.PROP_TOKEN);

            this.serializedObject.ApplyModifiedProperties();
        }
    }

}
