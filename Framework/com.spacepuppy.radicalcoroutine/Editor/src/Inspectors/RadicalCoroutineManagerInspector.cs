using UnityEngine;
using UnityEditor;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Collections;

namespace com.spacepuppyeditor.Core.Inspectors
{

    [CustomEditor(typeof(RadicalCoroutineManager))]
    public class RadicalCoroutineManagerInspector : SPEditor
    {

        private bool _expanded = true;

        protected override void OnSPInspectorGUI()
        {
            var targ = this.target as RadicalCoroutineManager;
            if(targ == null) return;

            using (var routines = TempCollection.GetList<RadicalCoroutine>())
            {
                targ.GetAllCoroutines(routines);
                EditorGUILayout.HelpBox(string.Format("Managing '{0}' RadicalCoroutines.", routines.Count), MessageType.Info);

                _expanded = EditorGUILayout.Foldout(_expanded, EditorHelper.TempContent("Coroutine Breakdown"));
                if (_expanded)
                {
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < routines.Count; i++)
                    {
                        var routine = routines[i];
                        EditorGUILayout.LabelField(string.Format("[{0:00}] Routine {1}", i, RadicalCoroutine.EditorHelper.GetInternalRoutineID(routine)));
                        EditorGUI.indentLevel += 2;
                        EditorGUILayout.LabelField("Component:", (routine.Owner != null) ? routine.Owner.GetType().Name : "UNKNOWN");
                        EditorGUILayout.LabelField("State:", routine.OperatingState.ToString());
                        EditorGUILayout.LabelField("Yield:", RadicalCoroutine.EditorHelper.GetYieldID(routine));
                        EditorGUILayout.LabelField("Derivative:", RadicalCoroutine.EditorHelper.GetDerivativeID(routine));
                        EditorGUI.indentLevel -= 2;
                    }
                    EditorGUI.indentLevel--;
                }
            }
        }

        public override bool RequiresConstantRepaint()
        {
            return Application.isPlaying && _expanded;
        }

    }

}
