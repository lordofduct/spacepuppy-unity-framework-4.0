using UnityEngine;
using UnityEditor;

using com.spacepuppy.UI;

namespace com.spacepuppyeditor.UI
{
    [CustomEditor(typeof(Touchable))]
    [CanEditMultipleObjects]
    public class Touchable_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            this.serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.PropertyField(this.serializedObject.FindProperty(EditorHelper.PROP_SCRIPT));

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
