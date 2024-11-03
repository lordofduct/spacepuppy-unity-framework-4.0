using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using com.spacepuppy.Statistics;
using com.spacepuppy;
using System.Linq;

namespace com.spacepuppyeditor.Statistics
{

    [CustomEditor(typeof(SimpleTokenLedgerService), true)]
    public class SimpleTokenLedgerServiceInspector : SPEditor
    {

        protected override void OnAfterSPInspectorGUI()
        {
            if (this.serializedObject.isEditingMultipleObjects) return;

            var ledger = this.serializedObject.targetObject as IStatisticsTokenLedger;
            if (ledger == null) return;

            foreach (var stat in ledger.EnumerateStats().OrderBy(o => o.ToStatId().ToString()))
            {
                string sid = stat.ToStatId().ToString();
                string sval = stat.Value.ToString();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(sid, GUILayout.ExpandWidth(true));
                GUILayout.Label(sval, GUILayout.Width(50f));
                EditorGUILayout.EndHorizontal();
            }
        }

    }

}
