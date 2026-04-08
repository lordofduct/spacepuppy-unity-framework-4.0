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
using UnityEditor.SceneManagement;
using com.spacepuppy.Collections;

namespace com.spacepuppyeditor.Windows
{

    public sealed class SPEventNavigatorWindow : EditorWindow
    {

        [System.Flags]
        public enum OptionFlags
        {
            IncludeInactive = 1,
            IncludeAlternateSources = 2,
            IncludeOtherScenesAndPrefabs = 4,
            ShowFullPath = 8,
        }

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

        private UnityEngine.Object _currentTarget;
        private bool _enabled = true;
        private OptionFlags _options;
        private int _maxDepth = 5;

        private List<SPEventSourceInfo> _eventSources;
        private List<SPEventSourceInfo> _alternateSources;
        private List<SPEventSourceInfo> _otherScenesAndPrefabs;
        private TrackedListenerToken<System.Action> _token;
        private EditorCoroutine _routine;
        private int _guiFrame;

        private AssetSearchWindow.AssetSearchQuery _query = new AssetSearchWindow.AssetSearchQuery();

        private Texture _gameobjectIcon;

        private Vector2 _scrollPos;

        private void Awake()
        {
            this.titleContent = new GUIContent("Spacepuppy Event Navigator", "Searches active scenes for anything targeting the current selection via an SPEvent.");
        }

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
            if (_enabled && Selection.activeObject != _currentTarget)
            {
                this.PurgeSources();
                if (Selection.activeGameObject)
                {
                    _currentTarget = Selection.activeGameObject;
                    _routine = EditorCoroutineUtility.StartCoroutine(FirstLevelFindAllSourcesByOptions_Async(_currentTarget, _options), this);
                }
                else if (Selection.activeObject is ScriptableObject so)
                {
                    _currentTarget = so;
                    _routine = EditorCoroutineUtility.StartCoroutine(FirstLevelFindAllSourcesByOptions_Async(_currentTarget, _options), this);
                }
                else if (Selection.activeObject)
                {
                    _currentTarget = Selection.activeObject;
                }
            }

            //line 1
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();
                _enabled = EditorGUILayout.ToggleLeft("Enabled", _enabled);
                if (EditorGUI.EndChangeCheck()) this.PurgeSources();

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Refresh"))
                {
                    this.PurgeSources();
                    if (_currentTarget)
                    {
                        _routine = EditorCoroutineUtility.StartCoroutine(FirstLevelFindAllSourcesByOptions_Async(_currentTarget, _options), this);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            //line 2
            {
                EditorGUILayout.ObjectField("Selected Object", _currentTarget, typeof(GameObject), true);
            }

            //line 3
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();
                _options = (OptionFlags)EditorGUILayout.EnumFlagsField("Options", _options);
                if (EditorGUI.EndChangeCheck()) this.PurgeSources();

                _maxDepth = EditorGUILayout.IntField("Max Depth", _maxDepth);
                EditorGUILayout.EndHorizontal();
            }

            //scrollbox
            {
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
                    EditorGUI.indentLevel = 0;
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
                    EditorGUI.indentLevel = 0;
                }
                if (_otherScenesAndPrefabs?.Count > 0)
                {
                    if (drewEvents) EditorGUILayout.Space(20f);
                    drewEvents = true;

                    EditorGUILayout.LabelField("Other Scenes & Prefabs:");
                    for (int i = 0; i < _otherScenesAndPrefabs.Count; i++)
                    {
                        _otherScenesAndPrefabs[i] = this.DrawAlternateEntry(_otherScenesAndPrefabs[i]);
                    }
                    EditorGUI.indentLevel = 0;
                }

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
        }

        SPEventSourceInfo DrawEntry(SPEventSourceInfo entry, int depth)
        {
            var c = EditorHelper.TempContent(entry.description);
            bool showfoldout = (entry.subinfos == null || entry.subinfos.Length > 0);

            const float MARGIN_FOLDOUT = 15f;
            var h = EditorStyles.label.CalcHeight(c, EditorGUIUtility.currentViewWidth);
            EditorGUI.indentLevel = depth;
            var rect_foldout = EditorGUILayout.GetControlRect(false, h);
            var rect_indent = EditorGUI.IndentedRect(rect_foldout);
            var rect_button = (showfoldout || depth > 0) ? new Rect(rect_indent.xMin + MARGIN_FOLDOUT, rect_indent.yMin, rect_indent.width - MARGIN_FOLDOUT, rect_indent.height) : rect_indent;
            //var rect_button = EditorGUILayout.GetControlRect(false, h);

            c.image = _gameobjectIcon;
            if (showfoldout)
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
                    if (entry.subinfos == null) entry.subinfos = FindEventSourcesInOpenScenes(entry.source, _options).ToArray();

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

        void PurgeSources()
        {
            _currentTarget = null;
            _eventSources = null;
            _alternateSources = null;
            _otherScenesAndPrefabs = null;
            if (_routine != null)
            {
                EditorCoroutineUtility.StopCoroutine(_routine);
                _routine = null;
            }
        }

        System.Collections.IEnumerator FirstLevelFindAllSourcesByOptions_Async(UnityEngine.Object target, OptionFlags options)
        {
            if (target == null) yield break;

            bool showFullPath = options.HasFlagT(OptionFlags.ShowFullPath);
            var events = new List<SPEventSourceInfo>();
            var alts = new List<SPEventSourceInfo>();
            var others = new List<SPEventSourceInfo>();
            //set these here so that way OnGUI draws them as we fill them. But we fill via events/alts in case the user changes selection
            _eventSources = events;
            _alternateSources = alts;
            _otherScenesAndPrefabs = others;

            var interval = System.TimeSpan.FromSeconds(1d / 5d);
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var sc = EditorSceneManager.GetSceneAt(i);

                var allcomponents = sc.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<Component>(options.HasFlagT(OptionFlags.IncludeInactive))).ToArray();

                foreach (Component c in allcomponents) //this is usually very fast so we stick to it first
                {
                    foreach (var s in FindEventSourcesOnComponent(target, c, showFullPath))
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
                if (options.HasFlagT(OptionFlags.IncludeAlternateSources))
                {
                    foreach (Component c in allcomponents) //this is slow, do it last
                    {
                        foreach (var s in FindAlternateSourcesOnComponent(target, c, showFullPath))
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
            }

            if (options.HasFlagT(OptionFlags.IncludeOtherScenesAndPrefabs) && EditorUtility.IsPersistent(target))
            {
                _query.Cancel();
                _query.Clear();
                _ = _query.SearchEverythingForTargetAsset(target);

                int processedCount = 0;
                while (_query.IsProcessing || processedCount < _query.OutputRefs.Count)
                {
                    yield return null;
                    if (processedCount < _query.OutputRefs.Count)
                    {
                        int cnt = _query.OutputRefs.Count;
                        for (int i = processedCount; i < cnt; i++)
                        {
                            var r = _query.OutputRefs[i];
                            others.Add(new SPEventSourceInfo()
                            {
                                source = r.obj,
                                eventName = string.Empty,
                                description = r.path,
                                expanded = false,
                                subinfos = ArrayUtil.Empty<SPEventSourceInfo>()
                            });
                        }
                        processedCount = cnt;
                    }
                }

                _query.Cancel();
                _query.Clear();
            }

            stopwatch.Stop();
            _routine = null;
            this.Repaint();
        }

        static IEnumerable<SPEventSourceInfo> FindEventSourcesInOpenScenes(UnityEngine.Object target, OptionFlags options)
        {
            if (target == null) yield break;

            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var sc = EditorSceneManager.GetSceneAt(i);
                foreach (var go in sc.GetRootGameObjects())
                {
                    foreach (var c in go.GetComponentsInChildren<MonoBehaviour>(options.HasFlagT(OptionFlags.IncludeInactive)))
                    {
                        if (target == c) continue;
                        foreach (var s in FindEventSourcesOnComponent(target, c, options.HasFlagT(OptionFlags.ShowFullPath)))
                        {
                            yield return s;
                        }
                    }
                }
            }
        }
        static IEnumerable<SPEventSourceInfo> FindEventSourcesOnComponent(UnityEngine.Object target, Component source, bool showFullPath)
        {
            var c = source;
            foreach (var m in DynamicUtil.GetMembersDirect(c, true, System.Reflection.MemberTypes.Field))
            {
                var field = m as FieldInfo;

                if (TypeUtil.IsType(field?.FieldType, typeof(BaseSPEvent)))
                {
                    var spev = field.GetValue(c) as BaseSPEvent;
                    foreach (var evtarg in spev.Targets)
                    {
                        if (evtarg.Target && (evtarg.Target == target || GameObjectUtil.IsRelatedGameObjectSource(evtarg.Target, target)))
                        {
                            var evnm = string.IsNullOrEmpty(spev.ObservableTriggerId) ? "Unnamed SPEvent" : spev.ObservableTriggerId;
                            yield return new SPEventSourceInfo()
                            {
                                source = c,
                                eventName = evnm,
                                description = showFullPath ? $"{c.transform.GetFullPathName()}.{c.GetType().Name}->{evnm}" : $"{c.name}.{c.GetType().Name}->{evnm}",
                            };
                        }
                    }
                }
            }
        }

        static IEnumerable<SPEventSourceInfo> FindAlternateSourcesOnComponent(UnityEngine.Object target, Component source, bool showFullPath)
        {
            // Use a SerializedObject to iterate over properties efficiently
            SerializedObject serializedObject = new SerializedObject(source);
            SerializedProperty property = serializedObject.GetIterator();

            while (property.NextVisible(true))
            {
                if (property.propertyType == SerializedPropertyType.ObjectReference)
                {
                    var objref = property.objectReferenceValue;
                    if (objref && (objref == target || GameObjectUtil.IsRelatedGameObjectSource(objref, target)))
                    {
                        yield return new SPEventSourceInfo()
                        {
                            source = source,
                            eventName = property.name,
                            description = showFullPath ? $"{source.transform.GetFullPathName()}.{source.GetType().Name}->{property.propertyPath}" : $"{source.name}.{source.GetType().Name}->{property.propertyPath}",
                        };
                    }
                }
            }
        }

        #endregion

        #region Special Types

        struct SPEventSourceInfo
        {
            public UnityEngine.Object source;
            public string eventName;
            public string description;
            public bool expanded;
            public SPEventSourceInfo[] subinfos;
        }

        #endregion

    }

}
