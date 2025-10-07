using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Dynamic;
using com.spacepuppy.Events;
using System.Reflection;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Windows
{

    public class SPEventNavigatorWindow : EditorWindow
    {

        #region Menu Entries

        [MenuItem("Spacepuppy/SPEvent Navigator", priority = int.MaxValue - 1)]
        public static void OpenWindow()
        {
            if (_openWindow == null)
            {
                _openWindow = EditorWindow.GetWindow<SPEventNavigatorWindow>();
            }
            else
            {
                GUI.BringWindowToFront(_openWindow.GetInstanceID());
            }
        }

        #endregion

        #region Window

        private static SPEventNavigatorWindow _openWindow;

        private GameObject _currentTarget;
        private List<SPEventSourceInfo> _eventSources = new();
        private TrackedListenerToken<System.Action> _token;

        private Vector2 _scrollPos;

        private void OnEnable()
        {
            _scrollPos = default;
            _currentTarget = null;
            _eventSources.Clear();
            _token.Dispose();
            _token = DelegateRef<System.Action>.Create(o => Selection.selectionChanged += o, o => Selection.selectionChanged -= o).AddTrackedListener(Selection_OnSelectionChanged);
        }

        private void OnDisable()
        {
            _currentTarget = null;
            _eventSources.Clear();
            _token.Dispose();
        }

        private void OnGUI()
        {
            if (Selection.activeGameObject != _currentTarget)
            {
                _currentTarget = Selection.activeGameObject;
                this.RefillEventSources();
            }

            EditorGUILayout.ObjectField("Selected Object", _currentTarget, typeof(GameObject), true);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandHeight(true));
            var icon = EditorGUIUtility.IconContent("GameObject Icon").image;
            foreach (var entry in _eventSources)
            {
                var c = EditorHelper.TempContent(entry.description);

                var h = EditorStyles.label.CalcHeight(c, EditorGUIUtility.currentViewWidth);
                var r = EditorGUILayout.GetControlRect(false, h);

                c.image = icon;
                if (GUI.Button(r, c, EditorStyles.label))
                {
                    EditorGUIUtility.PingObject(entry.source);
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        void Selection_OnSelectionChanged()
        {
            _currentTarget = null;
            _eventSources.Clear();
            this.Repaint();
        }

        void RefillEventSources()
        {
            _eventSources.Clear();
            if (_currentTarget == null) return;

            var sc = _currentTarget.scene;
            if (!sc.IsValid()) return;

            foreach (var go in sc.GetRootGameObjects())
            {
                foreach (var c in go.GetComponentsInChildren<MonoBehaviour>())
                {
                    foreach (var m in DynamicUtil.GetMembersDirect(c, true, System.Reflection.MemberTypes.Field))
                    {
                        var field = m as FieldInfo;

                        if (TypeUtil.IsType(field?.FieldType, typeof(BaseSPEvent)))
                        {
                            var spev = field.GetValue(c) as BaseSPEvent;
                            foreach (var targ in spev.Targets)
                            {
                                var tgo = GameObjectUtil.GetGameObjectFromSource(targ.Target);
                                if (tgo == _currentTarget)
                                {
                                    var evnm = string.IsNullOrEmpty(spev.ObservableTriggerId) ? "Unnamed SPEvent" : spev.ObservableTriggerId;
                                    _eventSources.Add(new SPEventSourceInfo()
                                    {
                                        source = c,
                                        eventName = evnm,
                                        description = $"{c.name}.{c.GetType().Name}->{evnm}",
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Special Types

        struct SPEventSourceInfo
        {
            public MonoBehaviour source;
            public string eventName;
            public string description;
        }

        #endregion

    }

}
