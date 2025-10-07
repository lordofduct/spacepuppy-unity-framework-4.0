using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppyeditor;
using com.spacepuppy.Statistics;

namespace com.spacepuppyeditor.Statistics
{

    [CustomEditor(typeof(StatisticsTokenLedgerProxy))]
    public class TokenLedgerProxyInspector : SPEditor
    {

        public const string PROP_CATEGORY = "_category";

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.UpdateIfRequiredOrScript();

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);

            var catprop = this.serializedObject.FindProperty(PROP_CATEGORY);
            int selection = Mathf.Max(0, StatisticsTokenLedgerCategories.FindIndexOfCategory(catprop.stringValue));
            selection = EditorGUILayout.Popup("Category", selection, StatisticsTokenLedgerCategories.Categories.Select(o => o.PopupPath).ToArray());
            catprop.stringValue = StatisticsTokenLedgerCategories.IndexInRange(selection) ? StatisticsTokenLedgerCategories.Categories[selection].Name : null;

            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, PROP_CATEGORY);

            this.serializedObject.ApplyModifiedProperties();
        }

    }
}
