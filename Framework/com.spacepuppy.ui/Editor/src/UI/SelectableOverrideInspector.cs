using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;
using com.spacepuppy.UI;

namespace com.spacepuppyeditor
{

    [CustomEditor(typeof(SelectableOverride), true)]
    public class SelectableOverrideInspector : SPEditor
    {

        const string PROP_SELECTABLE = "_selectable";

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.UpdateIfRequiredOrScript();

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);
            this.DrawPropertyField(PROP_SELECTABLE);

            if (!this.serializedObject.isEditingMultipleObjects && this.target is SelectableOverride over && over.Selectable)
            {
                if (over.Selectable.transition != Selectable.Transition.None)
                {
                    over.Selectable.transition = Selectable.Transition.None;
                    EditorUtility.SetDirty(over.Selectable);
                }

                EditorGUI.BeginChangeCheck();
                var targ = EditorGUILayout.ObjectField("Target Graphic", over.Selectable.targetGraphic, typeof(Graphic), true) as Graphic;
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(over.Selectable, "SelectableOverride Set Target Graphic");
                    over.Selectable.targetGraphic = targ;
                    EditorUtility.SetDirty(over.Selectable);
                }
            }

            this.DrawDefaultInspectorExcept(EditorHelper.PROP_SCRIPT, PROP_SELECTABLE);

            this.serializedObject.ApplyModifiedProperties();
        }

    }

}
