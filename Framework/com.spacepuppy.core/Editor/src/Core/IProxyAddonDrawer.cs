using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;
using System.Threading;
using System.Threading.Tasks;
using com.spacepuppyeditor.Windows;

namespace com.spacepuppyeditor.Core
{

    [CustomAddonDrawer(typeof(IProxy), displayAsFooter = true)]
    public class IProxyAddonDrawer : SPEditorAddonDrawer
    {

        private bool _isExpanded = false;

        private IProxy _target = null;
        private AssetSearchWindow.AssetSearchQuery _query = new AssetSearchWindow.AssetSearchQuery();
        private System.DateTime _lastScan;
        private Vector2 _scrollPos;

        protected override void OnEnable()
        {
            base.OnEnable();

            _query.StatusUpdated += _query_Repaint;
            _query.Completed += _query_Repaint;
        }

        protected internal override void OnDisable()
        {
            _query.Cancel();
            _query.Clear();
            _query.StatusUpdated -= _query_Repaint;
            _query.Completed -= _query_Repaint;
        }

        public override void OnInspectorGUI()
        {
            if (this.SerializedObject.isEditingMultipleObjects) return;

            var targ = this.SerializedObject.targetObject as IProxy;
            if (targ == null) return;

            EditorGUI.BeginChangeCheck();
            _isExpanded = EditorGUILayout.Foldout(_isExpanded, "References");
            if (!EditorGUI.EndChangeCheck() && MouseUtil.GuiClicked(Event.current, 0, GUILayoutUtility.GetLastRect())) _isExpanded = !_isExpanded;

            if (!_isExpanded) return;

            SyncReferences(targ);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            if (_query.IsProcessing)
            {
                EditorGUILayout.SelectableLabel(_query.CurrentStatus, EditorStyles.helpBox, GUILayout.ExpandHeight(true));
            }
            else if (_query.OutputRefs.Count > 0)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandHeight(true));
                foreach (var pair in _query.OutputRefs)
                {
                    //EditorGUILayout.ObjectField(pair.Item2, typeof(UnityEngine.Object), false);
                    if (GUILayout.Button(pair.Item1, EditorStyles.label))
                    {
                        EditorGUIUtility.PingObject(pair.Item2);
                    }
                }
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.SelectableLabel(_query.Output.ToString(), EditorStyles.helpBox, GUILayout.ExpandHeight(true));
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Refresh"))
            {
                _target = null;
            }

            EditorGUILayout.LabelField($"Last Scan: {_lastScan:MM/dd/yy hh:mm:ss}");
        }

        void SyncReferences(IProxy targ)
        {
            if (object.ReferenceEquals(targ, _target) || _query.IsProcessing) return;

            _target = targ;
            _lastScan = System.DateTime.Now;
            _query.Clear();

            var uobj = targ as UnityEngine.Object;
            if (uobj == null) return;

            _ = _query.SearchEverythingForTargetAsset(uobj);
        }

        private void _query_Repaint(object sender, System.EventArgs e)
        {
            if (this.Editor) this.Editor.Repaint();
        }

    }

}
