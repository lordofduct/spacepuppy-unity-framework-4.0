using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Events;
using com.spacepuppy.Utils;
using com.spacepuppyeditor.Internal;

namespace com.spacepuppyeditor.Events
{

    [CustomEditor(typeof(TriggerStateMachineProxyLink))]
    public class TriggerStateMachineProxyLinkInspector : SPEditor
    {

        private const string PROP_STATEMACHINE = "_stateMachine";
        private const string PROP_LINKS = "_links";

        private SPReorderableList _lst_drawer;
        private string[] _availableOptions;

        protected override void OnEnable()
        {
            base.OnEnable();

            _lst_drawer = new SPReorderableList(this.serializedObject, this.serializedObject.FindProperty(PROP_LINKS))
            {
                draggable = true,
                elementHeight = EditorGUIUtility.singleLineHeight,
                drawHeaderCallback = _lstDrawer_DrawHeader,
                drawElementCallback = _lstDrawer_DrawElement
            };
        }

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.UpdateIfRequiredOrScript();

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);
            this.DrawPropertyField(PROP_STATEMACHINE);

            this.SyncAvailableStateIds();
            _lst_drawer.DoLayoutList();

            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, PROP_STATEMACHINE, PROP_LINKS);

            this.serializedObject.ApplyModifiedProperties();
        }

        private void SyncAvailableStateIds()
        {
            var statemachine = ObjUtil.GetAsFromSource<i_TriggerStateMachine>(this.serializedObject.targetObject);
            if (this.serializedObject.isEditingMultipleObjects || !statemachine)
            {
                _availableOptions = ArrayUtil.Empty<string>();
                return;
            }

            _availableOptions = statemachine.States.Select(o => o.Id).Where(o => !string.IsNullOrEmpty(o)).ToArray();
        }

        #region List Drawer Methods

        private void _lstDrawer_DrawHeader(Rect area)
        {
            EditorGUI.LabelField(area, "Links");
        }

        private void _lstDrawer_DrawElement(Rect area, int index, bool isActive, bool isFocused)
        {
            var prop_element = _lst_drawer.serializedProperty.GetArrayElementAtIndex(index);
            var prop_id = prop_element.FindPropertyRelative(nameof(TriggerStateMachineProxyLink.ProxyLink.Id));
            var prop_proxy = prop_element.FindPropertyRelative(nameof(TriggerStateMachineProxyLink.ProxyLink.Proxy));

            var r0 = new Rect(area.xMin, area.yMin, Mathf.Floor(area.width * 0.4f), area.height);
            var r1 = new Rect(r0.xMax + 1, area.yMin, area.width - r0.width - 1, area.height);

            prop_id.stringValue = SPEditorGUI.OptionPopupWithCustom(r0, GUIContent.none, prop_id.stringValue, _availableOptions);
            EditorGUI.PropertyField(r1, prop_proxy, GUIContent.none);
        }

        #endregion

    }

}
