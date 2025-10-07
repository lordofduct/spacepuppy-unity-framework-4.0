using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Project;
using com.spacepuppy.Utils;
using UnityEditorInternal;
using com.spacepuppy.Collections;

namespace com.spacepuppyeditor.Core.Project
{

    [InitializeOnLoad()]
    [CustomEditor(typeof(TagData))]
    internal class TagDataInspector : SPEditor
    {

        static readonly string[] FIXED_TAGS = new string[]
        {
            SPConstants.TAG_UNTAGGED,
            SPConstants.TAG_RESPAWN,
            SPConstants.TAG_FINISH,
            SPConstants.TAG_EDITORONLY,
            SPConstants.TAG_MAINCAMERA,
            SPConstants.TAG_GAMECONTROLLER,
            SPConstants.TAG_PLAYER,

            SPConstants.TAG_MULTITAG,
            SPConstants.TAG_ROOT,
        };

        #region Static Interface

        /*
        static TagDataInspector()
        {
            EditorApplication.playModeStateChanged -= Touch;
            EditorApplication.playModeStateChanged += Touch;
            EditorHelper.Invoke(() => Touch(PlayModeStateChange.EnteredEditMode));
        }

        public static void Touch(PlayModeStateChange e)
        {
            var asset = TagData.Asset;
            if (asset == null) return;

            if (TagData.Tags != null && TagData.Tags.Except(UnityEditorInternal.InternalEditorUtility.tags).Count() > 0)
            {
                SPMenu.SyncTagData(TagData.Asset);
            }
        }
        */

        #endregion

        #region Inspector

        private static bool _foldout;
        private TagData.EditorHelper _helper;
        private List<string> _editableTags = new();
        private ReorderableList _lstDrawer;

        protected override void OnEnable()
        {
            base.OnEnable();

            _helper = new TagData.EditorHelper(this.target as TagData);
            _lstDrawer = new ReorderableList(_editableTags, typeof(string));
            _lstDrawer.drawHeaderCallback = _lstDrawer_Header;
            _lstDrawer.onAddCallback = _lstDrawer_AddElement;
            _lstDrawer.onRemoveCallback = _lstDrawer_RemoveElement;
            _lstDrawer.drawElementCallback = _lstDrawer_DrawElement;
        }

        protected override void OnSPInspectorGUI()
        {
            EditorGUILayout.HelpBox("This holds a reference to all the available tags for access at runtime. The data is kept in Assets/Resources and should be the ONLY one that exists. Do not delete.\n\nSpacepuppy Framework", MessageType.Info);

            _foldout = EditorGUILayout.Foldout(_foldout, "Fixed Tags");
            if (_foldout)
            {
                EditorGUI.indentLevel++;
                foreach (var tag in FIXED_TAGS)
                {
                    EditorGUILayout.LabelField(tag);
                }
                EditorGUI.indentLevel--;
            }

            GUILayout.Space(10f);

            _editableTags.Clear();
            _editableTags.AddRange(_helper.Tags.Except(FIXED_TAGS));
            EditorGUI.BeginChangeCheck();
            _lstDrawer.DoLayoutList();
            if (EditorGUI.EndChangeCheck())
            {
                using (var lst = TempCollection.GetList<string>())
                {
                    lst.AddRange(FIXED_TAGS);
                    lst.AddRange(_editableTags);
                    _helper.UpdateTags(lst);
                    EditorHelper.CommitDirectChanges(_helper.Target, true);
                    AssetDatabase.SaveAssets();
                }
            }

            bool unsavedChanges = _helper.Tags.Except(UnityEditorInternal.InternalEditorUtility.tags).Count() > 0;
            bool tagManagerHasTagsWeDoNot = UnityEditorInternal.InternalEditorUtility.tags.Except(_helper.Tags).Count() > 0;
            if (unsavedChanges || tagManagerHasTagsWeDoNot)
            {
                GUILayout.Space(10f);
                EditorGUILayout.LabelField("Changes:");

                EditorGUI.indentLevel++;
                if (unsavedChanges)
                {
                    foreach (var stag in _helper.Tags.Except(UnityEditorInternal.InternalEditorUtility.tags))
                    {
                        EditorGUILayout.LabelField("+ " + stag);
                    }
                }
                if (tagManagerHasTagsWeDoNot)
                {
                    foreach (var stag in UnityEditorInternal.InternalEditorUtility.tags.Except(_helper.Tags))
                    {
                        EditorGUILayout.LabelField("- " + stag);
                    }
                }
                EditorGUI.indentLevel--;
            }

            GUILayout.Space(10f);
            if (GUILayout.Button("Sync From TagManager") && EditorUtility.DisplayDialog("Sync From TagManager", "This will overwrite your TagData resource. Are you sure you want to continue?", "Yes", "No"))
            {
                SPMenu.SyncTagData(_helper.Target);
            }
            GUILayout.Space(10f);
            if (GUILayout.Button("Save To TagManager") && EditorUtility.DisplayDialog("Sync From TagManager", "This will overwrite the TagManager. Are you sure you want to continue?", "Yes", "No"))
            {
                SPMenu.SaveTagData(_helper.Tags);
                SPMenu.SyncTagData(_helper.Target);
            }

        }

        private void _lstDrawer_Header(Rect area)
        {
            EditorGUI.LabelField(area, "Editable Tags");
        }

        private void _lstDrawer_AddElement(ReorderableList rlst)
        {
            _editableTags.Add(string.Empty);
            rlst.index = _editableTags.Count - 1;
        }

        private void _lstDrawer_RemoveElement(ReorderableList rlst)
        {
            if (rlst.index >= 0 && rlst.index < _editableTags.Count)
            {
                _editableTags.RemoveAt(rlst.index);
            }
            else if (_editableTags.Count > 0)
            {
                _editableTags.RemoveAt(_editableTags.Count - 1);
            }
        }

        private void _lstDrawer_DrawElement(Rect area, int index, bool isActive, bool isFocused)
        {
            var element = index >= 0 && index < _editableTags.Count ? _editableTags[index] : string.Empty;
            EditorGUI.BeginChangeCheck();
            element = EditorGUI.DelayedTextField(new Rect(area.xMin, area.yMin, area.width, EditorGUIUtility.singleLineHeight), GUIContent.none, element);
            if (EditorGUI.EndChangeCheck() && index >= 0 && index < _editableTags.Count)
            {
                _editableTags[index] = element ?? string.Empty;
            }
        }

        #endregion

        #region Static Utils

        static IEnumerable<string> EnumerateFixedTags()
        {
            yield return SPConstants.TAG_UNTAGGED;
            yield return SPConstants.TAG_RESPAWN;
            yield return SPConstants.TAG_FINISH;
            yield return SPConstants.TAG_EDITORONLY;
            yield return SPConstants.TAG_MAINCAMERA;
            yield return SPConstants.TAG_GAMECONTROLLER;
            yield return SPConstants.TAG_PLAYER;

            yield return SPConstants.TAG_MULTITAG;
            yield return SPConstants.TAG_ROOT;
        }

        #endregion

    }
}
