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

        private TokenLedgerCategorySelectorPropertyDrawer _categoryDrawer = new TokenLedgerCategorySelectorPropertyDrawer();
        private TokenLedgerCategoryEntrySelectorPropertyDrawer _tokenDrawer = new TokenLedgerCategoryEntrySelectorPropertyDrawer();

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.UpdateIfRequiredOrScript();

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);
            this.DrawPropertyField(EditorHelper.PROP_ORDER);
            this.DrawPropertyField(EditorHelper.PROP_ACTIVATEON);
            EditorGUILayout.Space();

            var catprop = this.serializedObject.FindProperty(i_TriggerIndexByTokenValue.PROP_CATEGORY);
            var idprop = this.serializedObject.FindProperty(i_TriggerIndexByTokenValue.PROP_TOKEN);

            _categoryDrawer.OnGUILayout(catprop);

            _tokenDrawer.HideCustom = false;
            _tokenDrawer.CategoryFilter = catprop.stringValue;
            _tokenDrawer.OnGUILayout(idprop);

            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, EditorHelper.PROP_ORDER, EditorHelper.PROP_ACTIVATEON, i_TriggerIndexByTokenValue.PROP_CATEGORY, i_TriggerIndexByTokenValue.PROP_TOKEN);

            this.serializedObject.ApplyModifiedProperties();
        }
    }

}
