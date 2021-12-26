using UnityEngine;
using UnityEditor;

using com.spacepuppyeditor.Core;
using com.spacepuppy.Dynamic;

namespace com.spacepuppyeditor.Dynamic
{

    [CustomEditor(typeof(StateModifierNode))]
    public class StateModifierNodeInspector : SPEditor
    {
        private const string PROP_TARGETTYPE = "_targetType";
        private const string PROP_RESPECTPROXY = "_respectProxy";
        private const string PROP_SETTINGS = "_settings";

        private VariantCollectionPropertyDrawer _settingsDrawer = new VariantCollectionPropertyDrawer();

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.Update();

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);

            var targTypeProp = this.serializedObject.FindProperty(PROP_TARGETTYPE);
            SPEditorGUILayout.PropertyField(targTypeProp);

            this.DrawPropertyField(PROP_RESPECTPROXY);

            var settingsProp = this.serializedObject.FindProperty(PROP_SETTINGS);
            var targType = TypeReferencePropertyDrawer.GetTypeFromTypeReference(targTypeProp);
            _settingsDrawer.ConfigurePropertyList(targType);
            var lbl = EditorHelper.TempContent(settingsProp.displayName, settingsProp.tooltip);
            var rect = EditorGUILayout.GetControlRect(true, _settingsDrawer.GetPropertyHeight(settingsProp, lbl));
            _settingsDrawer.OnGUI(rect, settingsProp, lbl);

            this.serializedObject.ApplyModifiedProperties();
        }

    }

}
