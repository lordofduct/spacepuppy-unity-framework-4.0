using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

using com.spacepuppy.Mecanim;
using com.spacepuppyeditor.Internal;
using com.spacepuppyeditor.Events;

using System.Threading.Tasks;
using NUnit.Framework;
using com.spacepuppy.Collections;

namespace com.spacepuppyeditor.Mecanim
{

    [CustomEditor(typeof(SPAnimatorOverrideLayers))]
    [CanEditMultipleObjects]
    public class SPAnimatorOverrideLayersInspector : SPEditor
    {

        protected override void OnAfterSPInspectorGUI()
        {
            if (!Application.isPlaying || this.serializedObject.isEditingMultipleObjects) return;

            var targ = this.serializedObject.targetObject as SPAnimatorOverrideLayers;
            if (!targ) return;

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Active Overrides", EditorStyles.boldLabel);

            using (var lst = TempCollection.GetList<KeyValuePair<AnimationClip, AnimationClip>>())
            {
                targ.GetOverrides(lst);
                foreach (var kvp in lst)
                {
                    if (kvp.Value == null) continue;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField(kvp.Key, typeof(AnimationClip), false, GUILayout.ExpandWidth(true));
                    EditorGUILayout.ObjectField(kvp.Value, typeof(AnimationClip), false, GUILayout.ExpandWidth(true));
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

    }
}
