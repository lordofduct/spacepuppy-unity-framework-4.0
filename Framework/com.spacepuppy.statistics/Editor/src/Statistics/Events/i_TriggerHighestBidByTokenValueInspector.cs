using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppyeditor.Internal;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppyeditor;

using com.spacepuppy.Statistics.Events;

namespace com.spacepuppyeditor.Statistics.Events
{

    [CustomEditor(typeof(i_TriggerHighestBidByTokenValue))]
    public class i_TriggerHighestBidByTokenValueInspector : SPEditor
    {

        private TokenLedgerCategorySelectorPropertyDrawer _categoryDrawer = new TokenLedgerCategorySelectorPropertyDrawer();
        private TokenLedgerCategoryEntrySelectorPropertyDrawer _tokenDrawer = new TokenLedgerCategoryEntrySelectorPropertyDrawer();

        private ReorderableList _lstDrawer;
        protected ReorderableList ListDrawer
        {
            get
            {
                if (_lstDrawer == null)
                {
                    _lstDrawer = new SPReorderableList(this.serializedObject, this.serializedObject.FindProperty(i_TriggerHighestBidByTokenValue.PROP_BIDS))
                    {
                        draggable = true,
                        elementHeight = EditorGUIUtility.singleLineHeight,
                        drawHeaderCallback = _lstDrawer_DrawHeader,
                        drawElementCallback = _lstDrawer_DrawElement
                    };
                }
                return _lstDrawer;
            }
        }

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

            this.ListDrawer.DoLayoutList();

            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, EditorHelper.PROP_ORDER, EditorHelper.PROP_ACTIVATEON, i_TriggerHighestBidByTokenValue.PROP_CATEGORY, i_TriggerHighestBidByTokenValue.PROP_TOKEN, i_TriggerHighestBidByTokenValue.PROP_BIDS);

            this.serializedObject.ApplyModifiedProperties();
        }

        #region List Drawer Methods

        protected virtual void _lstDrawer_DrawHeader(Rect area)
        {
            EditorGUI.LabelField(area, "Bids");
        }

        protected virtual void _lstDrawer_DrawElement(Rect area, int index, bool isActive, bool isFocused)
        {
            var prop = _lstDrawer.serializedProperty.GetArrayElementAtIndex(index);
            var r0 = new Rect(area.xMin, area.yMin, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            var r1 = new Rect(r0.xMax, area.yMin, area.width - r0.width, EditorGUIUtility.singleLineHeight);

            SPEditorGUI.PropertyField(r0, prop.FindPropertyRelative(i_TriggerHighestBidByTokenValue.PROP_BIDS_VALUE), GUIContent.none);
            SPEditorGUI.PropertyField(r1, prop.FindPropertyRelative(i_TriggerHighestBidByTokenValue.PROP_BIDS_TARGET), GUIContent.none);
        }

        #endregion

    }
}
