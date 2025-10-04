using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using com.spacepuppy.Statistics;
using com.spacepuppy;
using System.Linq;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Statistics
{

    [CustomEditor(typeof(SimpleTokenLedgerService), true)]
    public class SimpleTokenLedgerServiceInspector : SPEditor, IStatisticsTokenLedgerChangedGlobalHandler
    {

        private bool _repaint;
        private Dictionary<StatId, System.DateTime> _changed = new();

        protected override void OnEnable()
        {
            base.OnEnable();
            Messaging.RegisterGlobal<IStatisticsTokenLedgerChangedGlobalHandler>(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Messaging.UnregisterGlobal<IStatisticsTokenLedgerChangedGlobalHandler>(this);
        }

        protected override void OnAfterSPInspectorGUI()
        {
            if (this.serializedObject.isEditingMultipleObjects) return;

            var ledger = this.serializedObject.targetObject as IStatisticsTokenLedger;
            if (ledger == null) return;

            var dtnow = System.DateTime.UtcNow;
            foreach (var stat in ledger.EnumerateStats().OrderBy(o => o.ToStatId().ToString()))
            {
                var statid = stat.ToStatId();
                string sid = statid.ToString();
                string sval = stat.Value.ToString();

                EditorGUILayout.BeginHorizontal();
                if (_changed.ContainsKey(statid))
                {
                    GUILayout.Label(sid, EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
                    GUILayout.Label(sval, EditorStyles.boldLabel, GUILayout.Width(50f));
                    if ((dtnow - _changed[statid]).TotalSeconds > 1d)
                    {
                        _repaint = true;
                        _changed.Remove(statid);
                    }
                }
                else
                {
                    GUILayout.Label(sid, GUILayout.ExpandWidth(true));
                    GUILayout.Label(sval, GUILayout.Width(50f));
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        public override bool RequiresConstantRepaint()
        {
            if (_changed.Count > 0 || _repaint)
            {
                _repaint = false;
                return true;
            }
            else
            {
                return base.RequiresConstantRepaint();
            }
        }

        void IStatisticsTokenLedgerChangedGlobalHandler.OnChanged(IStatisticsTokenLedgerService ledger, LedgerChangedEventArgs ev)
        {
            _changed[ev.MultipleChanged ? default : ev.StatId] = System.DateTime.UtcNow;
            _repaint = true;
        }

    }

}
