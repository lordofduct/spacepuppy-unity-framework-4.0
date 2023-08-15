using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

using com.spacepuppy.Mecanim;
using com.spacepuppyeditor.Internal;
using com.spacepuppyeditor.Events;

using System.Threading.Tasks;
using NUnit.Framework;

namespace com.spacepuppyeditor.Mecanim
{

    [CustomEditor(typeof(SPAnimatorEventProcessor))]
    [CanEditMultipleObjects]
    public class SPAnimatorEventProcessorInspector : SPEditor
    {

        public const string PROP_FORCERECEIVE = "_forceReceiverNotRequired";
        public const string PROP_AUTOSIGNAL = "_automaticallySignalStateMachineBehaviours";
        public const string PROP_EVENTCALLBACKS = "_eventCallbacks";

        const string PROP_SUB_NAME = "_name";
        const string PROP_SUB_EVENT = "_event";

        private SPReorderableList _lstDrawer;
        private SPEventPropertyDrawer _speventDrawer = new SPEventPropertyDrawer()
        {
            AlwaysExpanded = true,
            DoNotDrawParensOnLabel = true,
        };
        private HashSet<string> _drawnNames = new HashSet<string>();

        protected override void OnEnable()
        {
            base.OnEnable();

            _lstDrawer = new SPReorderableList(this.serializedObject, this.serializedObject.FindProperty(PROP_EVENTCALLBACKS), false, true, true, true);
            _lstDrawer.drawHeaderCallback += _lstDrawer_DrawHeader;
            _lstDrawer.onAddCallback += _lstDrawer_AddElement;
            _lstDrawer.drawElementCallback += _lstDrawer_DrawElement;
        }

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.UpdateIfRequiredOrScript();

            this.DrawDefaultInspectorExcept(PROP_EVENTCALLBACKS);

            _drawnNames.Clear();
            if (this.serializedObject.isEditingMultipleObjects)
            {
                EditorGUILayout.LabelField("Event Callbacks", "Unsupported Multi-Object Editing");
            }
            else
            {
                if (Application.isPlaying) EditorGUI.BeginDisabledGroup(true);

                _lstDrawer.DoLayoutList();
                if (_lstDrawer.index >= 0 && _lstDrawer.index < _lstDrawer.serializedProperty.arraySize)
                {
                    const float BOTTOM_PAD = 2f;

                    var element = _lstDrawer.serializedProperty.GetArrayElementAtIndex(_lstDrawer.index);
                    string functionName = element.FindPropertyRelative(PROP_SUB_NAME).stringValue;
                    var prop_event = element.FindPropertyRelative(PROP_SUB_EVENT);

                    float h = _speventDrawer.GetPropertyHeight(prop_event, GUIContent.none) + BOTTOM_PAD;
                    var area = EditorGUILayout.GetControlRect(true, h);
                    var drawArea = area;

                    GUI.BeginGroup(area, GUIContent.none, GUI.skin.box);
                    GUI.EndGroup();

                    _speventDrawer.OnGUI(drawArea, prop_event, EditorHelper.TempContent("Callback Name: " + functionName));
                }

                if (Application.isPlaying) EditorGUI.EndDisabledGroup();
            }
            _drawnNames.Clear();

            if (!Application.isPlaying)
            {
                for (int i = 0; i < _lstDrawer.serializedProperty.arraySize; i++)
                {
                    if (!_drawnNames.Add(_lstDrawer.serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative(PROP_SUB_NAME).stringValue))
                    {
                        _lstDrawer.serializedProperty.DeleteArrayElementAtIndex(i);
                    }
                }
            }
            _drawnNames.Clear();

            this.serializedObject.ApplyModifiedProperties();
        }

        #region ReorderableList Draw Callback

        private void _lstDrawer_DrawHeader(Rect area)
        {
            EditorGUI.LabelField(area, "Event Callbacks");
        }

        private void _lstDrawer_AddElement(ReorderableList lst)
        {
            EditorGUI.FocusTextInControl(string.Empty);
            for (int i = 0; i < lst.serializedProperty.arraySize; i++)
            {
                if (string.IsNullOrEmpty(lst.serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative(PROP_SUB_NAME).stringValue)) return;
            }

            lst.serializedProperty.arraySize++;
            lst.serializedProperty.GetArrayElementAtIndex(lst.serializedProperty.arraySize - 1).FindPropertyRelative(PROP_SUB_NAME).stringValue = string.Empty;
            lst.serializedProperty.GetArrayElementAtIndex(lst.serializedProperty.arraySize - 1).FindPropertyRelative(PROP_SUB_EVENT).FindPropertyRelative("_targets").arraySize = 0;
        }

        private void _lstDrawer_DrawElement(Rect area, int index, bool isActive, bool isFocused)
        {
            if (index >= 0 && index < _lstDrawer.serializedProperty.arraySize)
            {
                var prop_name = _lstDrawer.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative(PROP_SUB_NAME);
                if (_drawnNames.Add(prop_name.stringValue))
                {
                    EditorGUI.DelayedTextField(area, prop_name, EditorHelper.TempContent($"Callback Name {index:00}"));
                }
                else
                {
                    EditorGUI.DelayedTextField(area, prop_name, EditorHelper.TempContent($"Callback Name {index:00} (DUPLICATE!)"));
                }
            }
        }

        #endregion

    }

}
