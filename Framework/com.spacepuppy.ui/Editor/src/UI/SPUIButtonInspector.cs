using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.UI;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.UI
{

    [CustomEditor(typeof(SPUIButton)), CanEditMultipleObjects]
    public class SPUIButtonInspector : UnityEditor.UI.SelectableEditor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            base.serializedObject.Update();
            SPEditorGUILayout.PropertyField(this.serializedObject.FindProperty("_onClick"));
            SPEditorGUILayout.PropertyField(this.serializedObject.FindProperty("_clickDuration"));
            SPEditorGUILayout.PropertyField(this.serializedObject.FindProperty("_acceptedButtons"));
            base.serializedObject.ApplyModifiedProperties();
        }

        #region Static Menu Methods

        [MenuItem("CONTEXT/Button/Switch to SP UI Button")]
        static void SwitchToSPUIButton()
        {
            UnityEngine.Object script;
            try
            {
                script = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath("260f31c7b251a654ebd9a26b0e23c290"), typeof(UnityEditor.MonoScript));
            }
            catch { return; }
            if (script == null) return;

            foreach (var btn in Selection.objects.Select(o => ObjUtil.GetAsFromSource<UnityEngine.UI.Button>(o)).Where(o => (bool)o))
            {
                var sob = new SerializedObject(btn);
                var tprop = sob.FindProperty(EditorHelper.PROP_SCRIPT);
                tprop.objectReferenceValue = script;
                sob.ApplyModifiedProperties();
            }
        }

        #endregion

    }

}
