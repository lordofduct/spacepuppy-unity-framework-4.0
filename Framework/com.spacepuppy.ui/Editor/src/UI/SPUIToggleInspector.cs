using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.UI;
using com.spacepuppy.Utils;
using UnityEditor.UI;

namespace com.spacepuppyeditor.UI
{

    [CustomEditor(typeof(SPUIToggle))]
    public class SPUIToggleInspector : ToggleEditor
    {

        [MenuItem("CONTEXT/Toggle/Switch to SP UI Toggle")]
        static void SwitchTOSPUIToggle()
        {
            UnityEngine.Object script;
            try
            {
                script = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath("6f827f28752cb044b9895f90b8701c91"), typeof(UnityEditor.MonoScript));
            }
            catch { return; }
            if (script == null) return;

            foreach (var btn in Selection.objects.Select(o => ObjUtil.GetAsFromSource<UnityEngine.UI.Toggle>(o)).Where(o => (bool)o))
            {
                var sob = new SerializedObject(btn);
                var tprop = sob.FindProperty(EditorHelper.PROP_SCRIPT);
                tprop.objectReferenceValue = script;
                sob.ApplyModifiedProperties();
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SPEditorGUILayout.PropertyField(this.serializedObject, "_onToggleIsOn");
            SPEditorGUILayout.PropertyField(this.serializedObject, "_onToggleIsOff");
            this.serializedObject.ApplyModifiedProperties();
        }

    }

}
