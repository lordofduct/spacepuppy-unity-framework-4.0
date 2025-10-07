using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using UnityEditorInternal;

namespace com.spacepuppyeditor.Core
{

    [CustomEditor(typeof(MultiTag)), CanEditMultipleObjects]
    public class MultiTagInspector : SPEditor
    {

        public const string PROP_SEARCHABLEIFINACTIVE = "_searchableIfInactive";
        public const string PROP_TAGS = "_tags";

        #region Properties

        private bool _showTags;
        private ReorderableList _lstDrawer;
        private Rect _lastListRect;

        #endregion

        protected override void OnEnable()
        {
            base.OnEnable();

            _lstDrawer = new ReorderableList(this.serializedObject, this.serializedObject.FindProperty("_tags"));
            _lstDrawer.drawHeaderCallback = _lstDrawer_Header;
            _lstDrawer.onAddCallback = _lstDrawer_AddElement;
            _lstDrawer.onRemoveCallback = _lstDrawer_RemoveElement;
            _lstDrawer.drawElementCallback = _lstDrawer_DrawElement;
        }

        protected override void OnBeforeSPInspectorGUI()
        {
            var go = com.spacepuppy.Utils.GameObjectUtil.GetGameObjectFromSource(this.target);

            if (go != null && !go.CompareTag(SPConstants.TAG_MULTITAG))
            {
                go.tag = SPConstants.TAG_MULTITAG;
                EditorHelper.CommitDirectChanges(go, false);
            }
        }

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.Update();

            //this.DrawPropertyField(PROP_SEARCHABLEIFINACTIVE);
            var searchableProp = this.serializedObject.FindProperty(PROP_SEARCHABLEIFINACTIVE);
            searchableProp.boolValue = EditorGUILayout.ToggleLeft(searchableProp.displayName, searchableProp.boolValue);

            EditorGUILayout.Space(10f);

            _lstDrawer.displayAdd = _lstDrawer.displayRemove = !this.serializedObject.isEditingMultipleObjects || !_lstDrawer.serializedProperty.hasMultipleDifferentValues;
            _lastListRect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, _lstDrawer.GetHeight());
            _lstDrawer.DoList(_lastListRect);

            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, PROP_SEARCHABLEIFINACTIVE, PROP_TAGS);

            this.serializedObject.ApplyModifiedProperties();
        }

        private void _lstDrawer_Header(Rect area)
        {
            if (this.serializedObject.isEditingMultipleObjects)
            {
                EditorGUI.LabelField(area, "Tags (Multi-Editing)");
            }
            else
            {
                EditorGUI.LabelField(area, "Tags");
            }
        }

        private void _lstDrawer_AddElement(ReorderableList rlst)
        {
            if (this.serializedObject.isEditingMultipleObjects && _lstDrawer.serializedProperty.hasMultipleDifferentValues) return;

            void handleItemClicked(object parameter)
            {
                if (parameter is string stag)
                {
                    var knowntags = UnityEditorInternal.InternalEditorUtility.tags;
                    var stags = GetTags(_lstDrawer.serializedProperty).Append(stag).Distinct().Where(s => s != SPConstants.TAG_MULTITAG && knowntags.Contains(s)).ToArray();
                    SetTags(_lstDrawer.serializedProperty, stags);
                    this.serializedObject.ApplyModifiedProperties();
                }
            }

            var rect = new Rect(_lastListRect.xMax - 70f, _lastListRect.yMax, 5f, 0);
            GenericMenu menu = new GenericMenu();
            foreach (var stag in UnityEditorInternal.InternalEditorUtility.tags)
            {
                if (stag == SPConstants.TAG_MULTITAG) continue;
                menu.AddItem(new GUIContent(stag), false, handleItemClicked, stag);
            }
            menu.DropDown(rect);
        }

        private void _lstDrawer_RemoveElement(ReorderableList rlst)
        {
            if (this.serializedObject.isEditingMultipleObjects && _lstDrawer.serializedProperty.hasMultipleDifferentValues) return;

            var stags = GetTags(rlst.serializedProperty).ToList();
            if (rlst.index >= 0 && rlst.index < stags.Count)
            {
                stags.RemoveAt(rlst.index);
            }
            else if (rlst.serializedProperty.arraySize > 0)
            {
                stags.RemoveAt(stags.Count - 1);
            }
            var knowntags = UnityEditorInternal.InternalEditorUtility.tags;
            SetTags(rlst.serializedProperty, stags.Distinct().Where(s => s != SPConstants.TAG_MULTITAG && knowntags.Contains(s)).ToArray());
        }

        private void _lstDrawer_DrawElement(Rect area, int index, bool isActive, bool isFocused)
        {
            var element = _lstDrawer.serializedProperty.GetArrayElementAtIndex(index);
            var r0 = new Rect(area.xMin, area.yMin, area.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(r0, element.stringValue);
        }


        IEnumerable<string> GetTags(SerializedProperty prop)
        {
            for (int i = 0; i < prop.arraySize; i++)
            {
                yield return prop.GetArrayElementAtIndex(i).stringValue;
            }
        }

        private void SetTags(SerializedProperty prop, string[] tags)
        {
            prop.arraySize = tags.Length;
            for (int i = 0; i < tags.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).stringValue = tags[i];
            }
        }

    }

}