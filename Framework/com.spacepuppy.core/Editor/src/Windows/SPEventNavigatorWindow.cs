using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Dynamic;
using com.spacepuppy.Events;
using System.Reflection;
using com.spacepuppy.Utils;
using System.Linq;

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
        private bool _includeInactiveSources;
        private int _maxDepth = 5;

        private SPEventSourceInfo[] _eventSources;
        private TrackedListenerToken<System.Action> _token;

        private Texture _gameobjectIcon;

        private Vector2 _scrollPos;

        private void OnEnable()
        {
            _scrollPos = default;
            _currentTarget = null;
            _eventSources = null;
            _token.Dispose();
            _token = DelegateRef<System.Action>.Create(o => Selection.selectionChanged += o, o => Selection.selectionChanged -= o).AddTrackedListener(Selection_OnSelectionChanged);

            _gameobjectIcon = EditorGUIUtility.IconContent("GameObject Icon").image;
        }

        private void OnDisable()
        {
            _currentTarget = null;
            _eventSources = null;
            _token.Dispose();
        }

        private void OnGUI()
        {
            if (Selection.activeGameObject != _currentTarget)
            {
                _currentTarget = Selection.activeGameObject;
                _eventSources = FindEventSources(_currentTarget, _includeInactiveSources).ToArray();
            }

            EditorGUILayout.ObjectField("Selected Object", _currentTarget, typeof(GameObject), true);
            EditorGUILayout.BeginHorizontal();
            _includeInactiveSources = EditorGUILayout.Toggle("Include Inactive", _includeInactiveSources);
            _maxDepth = EditorGUILayout.IntField("Max Depth", _maxDepth);
            EditorGUILayout.EndHorizontal();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandHeight(true));
            if (_eventSources?.Length > 0)
            {
                for (int i = 0; i < _eventSources.Length; i++)
                {
                    _eventSources[i] = this.DrawEntry(_eventSources[i], 0);
                }
            }
            EditorGUI.indentLevel = 0;
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        SPEventSourceInfo DrawEntry(SPEventSourceInfo entry, int depth)
        {
            var c = EditorHelper.TempContent(entry.description);

            const float MARGIN_FOLDOUT = 15f;
            var h = EditorStyles.label.CalcHeight(c, EditorGUIUtility.currentViewWidth);
            EditorGUI.indentLevel = depth;
            var rect_foldout = EditorGUILayout.GetControlRect(false, h);
            var rect_indent = EditorGUI.IndentedRect(rect_foldout);
            var rect_button = new Rect(rect_indent.xMin + MARGIN_FOLDOUT, rect_indent.yMin, rect_indent.width - MARGIN_FOLDOUT, rect_indent.height);
            //var rect_button = EditorGUILayout.GetControlRect(false, h);

            c.image = _gameobjectIcon;
            if (entry.subinfos == null || entry.subinfos.Length > 0)
            {
                entry.expanded = EditorGUI.Foldout(rect_foldout, entry.expanded, GUIContent.none);
            }
            if (GUI.Button(rect_button, c, EditorStyles.label))
            {
                EditorGUIUtility.PingObject(entry.source);
            }

            if (entry.expanded && entry.source)
            {
                if (depth < 5)
                {
                    if (entry.subinfos == null) entry.subinfos = FindEventSources(entry.source.gameObject, _includeInactiveSources).ToArray();

                    for (int i = 0; i < entry.subinfos.Length; i++)
                    {
                        entry.subinfos[i] = this.DrawEntry(entry.subinfos[i], depth + 1);
                    }
                }
                else
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("...");
                }
            }

            return entry;
        }

        void Selection_OnSelectionChanged()
        {
            _currentTarget = null;
            _eventSources = null;
            this.Repaint();
        }

        static IEnumerable<SPEventSourceInfo> FindEventSources(GameObject target, bool includeInactiveSources)
        {
            if (target == null) yield break;

            var sc = target.scene;
            if (!sc.IsValid()) yield break;

            foreach (var go in sc.GetRootGameObjects())
            {
                foreach (var c in go.GetComponentsInChildren<MonoBehaviour>(includeInactiveSources))
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
                                if (tgo == target)
                                {
                                    var evnm = string.IsNullOrEmpty(spev.ObservableTriggerId) ? "Unnamed SPEvent" : spev.ObservableTriggerId;
                                    yield return new SPEventSourceInfo()
                                    {
                                        source = c,
                                        eventName = evnm,
                                        description = $"{c.name}.{c.GetType().Name}->{evnm}",
                                    };
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
            public bool expanded;
            public SPEventSourceInfo[] subinfos;
        }

        #endregion

    }

}
