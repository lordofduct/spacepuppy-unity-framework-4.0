using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Dynamic;
using com.spacepuppy.Events;
using System.Reflection;
using com.spacepuppy.Utils;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using System.Runtime.CompilerServices;

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
        private bool _includeAlternateSources;
        private int _maxDepth = 5;

        private List<SPEventSourceInfo> _eventSources;
        private List<SPEventSourceInfo> _alternateSources;
        private TrackedListenerToken<System.Action> _token;
        private EditorCoroutine _routine;
        private int _guiFrame;

        private Texture _gameobjectIcon;

        private Vector2 _scrollPos;

        private void OnEnable()
        {
            _scrollPos = default;
            this.PurgeSources();
            _token.Dispose();
            _token = DelegateRef<System.Action>.Create(o => Selection.selectionChanged += o, o => Selection.selectionChanged -= o).AddTrackedListener(Selection_OnSelectionChanged);

            _gameobjectIcon = EditorGUIUtility.IconContent("GameObject Icon").image;
        }

        private void OnDisable()
        {
            this.PurgeSources();
            _token.Dispose();
        }

        private void OnGUI()
        {
            _guiFrame++;
            if (Selection.activeGameObject != _currentTarget)
            {
                this.PurgeSources();
                _currentTarget = Selection.activeGameObject;


                //_eventSources = FindEventSources(_currentTarget, _includeInactiveSources).ToList();
                //if (_includeAlternateSources) _alternateSources = FindAlternateSources(_currentTarget, _includeInactiveSources).ToList();
                _routine = EditorCoroutineUtility.StartCoroutine(AsyncFindRootSources(_currentTarget, _includeInactiveSources, _includeAlternateSources), this);
            }

            EditorGUILayout.ObjectField("Selected Object", _currentTarget, typeof(GameObject), true);
            EditorGUILayout.BeginHorizontal();
            _includeInactiveSources = EditorGUILayout.Toggle("Include Inactive", _includeInactiveSources);
            _maxDepth = EditorGUILayout.IntField("Max Depth", _maxDepth);
            _includeAlternateSources = EditorGUILayout.Toggle("Include ALL Refs", _includeAlternateSources);
            EditorGUILayout.EndHorizontal();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandHeight(true));
            bool drewEvents = false;
            if (_eventSources?.Count > 0)
            {
                drewEvents = true;
                for (int i = 0; i < _eventSources.Count; i++)
                {
                    _eventSources[i] = this.DrawEntry(_eventSources[i], 0);
                }
            }
            if (_alternateSources?.Count > 0)
            {
                if (drewEvents) EditorGUILayout.Space(20f);
                drewEvents = true;

                EditorGUILayout.LabelField("Alternate Sources:");
                for (int i = 0; i < _alternateSources.Count; i++)
                {
                    _alternateSources[i] = this.DrawAlternateEntry(_alternateSources[i]);
                }
            }
            EditorGUI.indentLevel = 0;

            if (_routine != null)
            {
                if (drewEvents) EditorGUILayout.Space(10f);
                switch (Mathf.Abs(_guiFrame % 3))
                {
                    case 0:
                        EditorGUILayout.LabelField("Processing.");
                        break;
                    case 1:
                        EditorGUILayout.LabelField("Processing..");
                        break;
                    case 2:
                        EditorGUILayout.LabelField("Processing...");
                        break;
                }
            }

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

        SPEventSourceInfo DrawAlternateEntry(SPEventSourceInfo entry)
        {
            var c = EditorHelper.TempContent(entry.description);

            var h = EditorStyles.label.CalcHeight(c, EditorGUIUtility.currentViewWidth);
            EditorGUI.indentLevel = 0;
            var rect_button = EditorGUILayout.GetControlRect(false, h);

            c.image = _gameobjectIcon;
            if (GUI.Button(rect_button, c, EditorStyles.label))
            {
                EditorGUIUtility.PingObject(entry.source);
            }

            return entry;
        }

        void Selection_OnSelectionChanged()
        {
            this.PurgeSources();
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
                    foreach (var s in FindEventSources(target, c))
                    {
                        yield return s;
                    }
                }
            }
        }
        static IEnumerable<SPEventSourceInfo> FindEventSources(GameObject target, Component source)
        {
            var c = source;
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

        static IEnumerable<SPEventSourceInfo> FindAlternateSources(GameObject target, Component source)
        {
            // Use a SerializedObject to iterate over properties efficiently
            SerializedObject serializedObject = new SerializedObject(source);
            SerializedProperty property = serializedObject.GetIterator();

            while (property.NextVisible(true))
            {
                if (property.propertyType == SerializedPropertyType.ObjectReference)
                {
                    var tgo = GameObjectUtil.GetGameObjectFromSource(property.objectReferenceValue);
                    if (tgo && tgo == target)
                    {
                        yield return new SPEventSourceInfo()
                        {
                            source = source,
                            eventName = property.name,
                            description = property.propertyPath,
                        };
                    }
                }
            }
        }


        System.Collections.IEnumerator AsyncFindRootSources(GameObject target, bool includeInactiveSources, bool includeAlternateSources)
        {
            if (target == null) yield break;

            var sc = target.scene;
            if (!sc.IsValid()) yield break;

            var events = new List<SPEventSourceInfo>();
            var alts = new List<SPEventSourceInfo>();
            //set these here so that way OnGUI draws them as we fill them. But we fill via events/alts in case the user changes selection
            _eventSources = events;
            _alternateSources = alts;

            var allcomponents = sc.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<Component>(includeInactiveSources)).ToArray();

            var interval = System.TimeSpan.FromSeconds(1d / 5d);
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            foreach (Component c in allcomponents) //this is usually very fast so we stick to it first
            {
                foreach (var s in FindEventSources(target, c))
                {
                    events.Add(s);
                }

                if (stopwatch.Elapsed > interval * 5)
                {
                    this.Repaint();
                    yield return null;
                    stopwatch.Restart();
                }
            }
            if (includeAlternateSources)
            {
                foreach (Component c in allcomponents) //this is slow, do it last
                {
                    foreach (var s in FindAlternateSources(target, c))
                    {
                        alts.Add(s);
                    }

                    if (stopwatch.Elapsed > interval)
                    {
                        this.Repaint();
                        yield return null;
                        stopwatch.Restart();
                    }
                }
            }
            stopwatch.Stop();
            _routine = null;
            this.Repaint();
        }

        void PurgeSources()
        {
            _currentTarget = null;
            _eventSources = null;
            _alternateSources = null;
            if (_routine != null)
            {
                EditorCoroutineUtility.StopCoroutine(_routine);
                _routine = null;
            }
        }

        #endregion

        #region Special Types

        struct SPEventSourceInfo
        {
            public Component source;
            public string eventName;
            public string description;
            public bool expanded;
            public SPEventSourceInfo[] subinfos;
        }

        #endregion

    }

}
