using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using com.spacepuppy.Geom;

namespace com.spacepuppyeditor.Core.Geom
{

    [CustomEditor(typeof(CompoundTrigger), true)]
    public class CompoundTriggerInspector : SPEditor
    {

        private bool _foldout;

        protected override void OnAfterSPInspectorGUI()
        {
            base.OnAfterSPInspectorGUI();
            if (!Application.isPlaying || this.serializedObject.isEditingMultipleObjects) return;

            EditorGUILayout.Space(10f);
            _foldout = EditorGUILayout.Foldout(_foldout, "Active Colliders");
            if (_foldout)
            {
                var target = this.serializedObject.targetObject as CompoundTrigger;
                if (!target) return;

                foreach (var collider in target.GetActiveColliders())
                {
                    EditorGUILayout.ObjectField(collider, typeof(Collider), true);
                }
            }
        }

    }

}
