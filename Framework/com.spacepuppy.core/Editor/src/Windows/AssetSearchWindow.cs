using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

using com.spacepuppy.Utils;
using System.Threading;
using System.Text.RegularExpressions;
using System;

namespace com.spacepuppyeditor.Windows
{

    public class AssetSearchWindow : EditorWindow
    {

        #region Menu Entries

        [MenuItem("Spacepuppy/Search...", priority = int.MaxValue)]
        public static void OpenWindow()
        {
            if (_openWindow == null)
            {
                _openWindow = EditorWindow.GetWindow<AssetSearchWindow>();
            }
            else
            {
                GUI.BringWindowToFront(_openWindow.GetInstanceID());
            }
        }

        #endregion

        #region Window

        private static AssetSearchWindow _openWindow;
        private static string _dataPath;

        private UnityEngine.Object _target;

        private AssetSearchQuery _assetSearchQuery = new AssetSearchQuery()
        {
            CodePath = "Code",
        };
        private SearchStringQuery _searchStringQuery = new SearchStringQuery();
        private IQuery _currentQuery;

        private Vector2 _scrollPos;

        private void OnEnable()
        {
            _scrollPos = Vector2.zero;
            _dataPath = Application.dataPath;
            this.titleContent = new GUIContent("Spacepuppy Asset Search...");
            _assetSearchQuery.StatusUpdated += (s, e) => this.Repaint();
            _assetSearchQuery.Completed += (s, e) => this.Repaint();
        }

        private void OnInspectorUpdate()
        {
            if (_currentQuery?.IsProcessing ?? false)
            {
                this.Repaint();
            }
        }

        private void OnGUI()
        {
            const string CTRL_SEACHSTRING = "AssetSearchWindow.SearchString";
            string pressedEnterCtrl = null;
            if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return))
            {
                pressedEnterCtrl = GUI.GetNameOfFocusedControl();
            }

            var cache = GUI.enabled;
            GUI.enabled = !(_currentQuery?.IsProcessing ?? false);
            _target = EditorGUILayout.ObjectField("Target", _target, typeof(UnityEngine.Object), false);
            _assetSearchQuery.CodePath = EditorGUILayout.TextField("Code Path", _assetSearchQuery.CodePath);

            EditorGUILayout.BeginHorizontal();

            GUI.SetNextControlName("AssetSearchWindow.SearchString");
            _searchStringQuery.SearchString = EditorGUILayout.TextField("Search String", _searchStringQuery.SearchString, GUILayout.ExpandWidth(true));
            _searchStringQuery.AssetTypes = (AssetTypes)SPEditorGUILayout.EnumFlagField(typeof(AssetTypes), GUIContent.none, (int)_searchStringQuery.AssetTypes, GUILayout.Width(100f));
            _searchStringQuery.UseRegex = EditorGUILayout.ToggleLeft("Regex", _searchStringQuery.UseRegex, GUILayout.Width(60f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Search for unreferenced scripts", GUILayout.Width(200f)))
            {
                _currentQuery = _assetSearchQuery;
                _ = _assetSearchQuery.SearchForScripts(false);
            }
            if (GUILayout.Button("Search for referenced scripts", GUILayout.Width(200f)))
            {
                _currentQuery = _assetSearchQuery;
                _ = _assetSearchQuery.SearchForScripts(true);
            }

            if (_target != null)
            {
                if (GUILayout.Button("Search Everything For Target", GUILayout.Width(200f)))
                {
                    _currentQuery = _assetSearchQuery;
                    _ = _assetSearchQuery.SearchEverythingForTargetAsset(_target);
                }
            }

            if (!string.IsNullOrEmpty(_searchStringQuery.SearchString))
            {
                if (GUILayout.Button("Search For String", GUILayout.Width(200f)) || pressedEnterCtrl == CTRL_SEACHSTRING)
                {
                    _currentQuery = _searchStringQuery;
                    _ = _searchStringQuery.Search();
                }
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Force Reserialize Assets"))
            {
                if (EditorUtility.DisplayDialog("Caution", "This will force all assets to be reserialized, this takes a long time and touches everything. Do not continue unless you are certain this is what you want to do.", "Continue", "Cancel"))
                {
                    AssetDatabase.ForceReserializeAssets();
                }
            }

            GUI.enabled = cache;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Output:");
            //EditorGUILayout.HelpBox(_output.ToString(), MessageType.None);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            if ((_currentQuery?.OutputRefs.Count ?? 0) > 0)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandHeight(true));
                foreach (var entry in _currentQuery.OutputRefs)
                {
                    var c = EditorHelper.TempContent(entry.message);
                    var h = EditorStyles.label.CalcHeight(c, EditorGUIUtility.currentViewWidth);
                    var r = EditorGUILayout.GetControlRect(false, h);
                    if (GUI.Button(r, entry.message, EditorStyles.label))
                    {
                        EditorGUIUtility.PingObject(entry.obj);
                    }
                }
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.SelectableLabel((_currentQuery?.Output.ToString() ?? string.Empty), EditorStyles.helpBox, GUILayout.ExpandHeight(true));
            }
            EditorGUILayout.EndScrollView();


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Status:", GUILayout.Width(45f));
            EditorGUILayout.SelectableLabel(_currentQuery?.CurrentStatus ?? string.Empty, EditorStyles.helpBox, GUILayout.ExpandWidth(true), GUILayout.Height(20f));
            if (_currentQuery?.IsProcessing ?? false)
            {
                if (GUILayout.Button("Cancel", GUILayout.Width(50f)))
                {
                    _currentQuery.Cancel();
                }
            }
            else
            {
                if (GUILayout.Button("Clear", GUILayout.Width(50f)))
                {
                    _currentQuery?.Clear();
                    _currentQuery = null;
                    this.Repaint();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Special Types

        public interface IQuery
        {

            event System.EventHandler StatusUpdated;
            event System.EventHandler Completed;

            bool IsProcessing { get; }
            StringBuilder Output { get; }
            IReadOnlyList<OutputRefEntry> OutputRefs { get; }
            string CurrentStatus { get; }


            void Cancel();
            void Clear();


        }

        public struct OutputRefEntry
        {
            public string path;
            public string message;
            public UnityEngine.Object obj;
        }

        public class AssetSearchQuery : IQuery
        {

            public event System.EventHandler StatusUpdated;
            public event System.EventHandler Completed;

            #region Fields/Properties

            private CancellationTokenSource _cancellationTokenSource;

            private StringBuilder _output = new StringBuilder();
            private List<OutputRefEntry> _outputRefs = new List<OutputRefEntry>();

            public string CodePath { get; set; } = "Code";

            public bool IsProcessing => _cancellationTokenSource != null;

            public string CurrentStatus { get; private set; } = string.Empty;

            public StringBuilder Output => _output;

            public IReadOnlyList<OutputRefEntry> OutputRefs => _outputRefs;

            #endregion

            public void Cancel()
            {
                _cancellationTokenSource?.Cancel();
            }

            public void Clear()
            {
                if (this.IsProcessing) return; //can't clear if running

                _output.Clear();
                _outputRefs.Clear();
                this.CurrentStatus = string.Empty;
            }

            public async Task SearchForScripts(bool inuse)
            {
                if (_cancellationTokenSource != null) return;

                try
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                    _output.Clear();
                    _outputRefs.Clear();

                    string[] guids = AssetDatabase.FindAssets("t:script", new string[] { "Assets/" + this.CodePath });
                    foreach (var guid in guids)
                    {
                        if (_cancellationTokenSource.Token.IsCancellationRequested) return;

                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        OnStatusUpdated("Processing: " + path);

                        bool success = await Task.Run(() =>
                        {
                            var fn = Path.GetFileNameWithoutExtension(path);
                            var tp = TypeUtil.FindType(fn);
                            if (tp == null || !TypeUtil.IsType(tp, typeof(MonoBehaviour)))
                            {
                                return false;
                            }

                            return FindRefs(guid).Any() == inuse;
                        }, _cancellationTokenSource.Token);
                        if (_cancellationTokenSource.Token.IsCancellationRequested) return;

                        if (success)
                        {
                            _output.AppendLine(path);
                            _outputRefs.Add(new OutputRefEntry()
                            {
                                path = path,
                                message = path,
                                obj = AssetDatabase.LoadAssetAtPath(path, typeof(MonoBehaviour))
                            });
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    OnStatusUpdated("ERROR: " + ex.Message);
                }
                finally
                {
                    _cancellationTokenSource = null;
                    OnStatusUpdated(string.Empty);
                    this.OnCompleted();
                }
            }

            public Task SearchEverythingForTargetAsset(UnityEngine.Object target) => SearchEverythingForTargetAsset(target != null ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(target)) : null);

            public async Task SearchEverythingForTargetAsset(string guid)
            {
                if (_cancellationTokenSource != null) return;

                if (string.IsNullOrEmpty(guid))
                {
                    _output.Clear();
                    _outputRefs.Clear();
                    _output.AppendLine("Select a target to search for...");
                    return;
                }

                try
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                    _output.Clear();
                    _outputRefs.Clear();
                    OnStatusUpdated("Processing...");

                    string[] paths = ArrayUtil.Empty<string>();
                    await Task.Run(() =>
                    {
                        paths = FindRefs(guid).ToArray();
                    }, _cancellationTokenSource.Token);

                    if (_cancellationTokenSource.Token.IsCancellationRequested) return;

                    _output.Clear();
                    _outputRefs.Clear();
                    foreach (var path in paths)
                    {
                        _output.AppendLine(path);

                        var apath = path.Substring(_dataPath.Length - 6);
                        _outputRefs.Add(new OutputRefEntry()
                        {
                            path = apath,
                            message = apath,
                            obj = AssetDatabase.LoadAssetAtPath(apath, typeof(UnityEngine.Object))
                        });
                    }
                }
                catch (System.Exception ex)
                {
                    OnStatusUpdated("ERROR: " + ex.Message);
                }
                finally
                {
                    _cancellationTokenSource = null;
                    OnStatusUpdated("");
                    this.OnCompleted();
                }
            }

            void OnStatusUpdated(string msg)
            {
                this.CurrentStatus = msg;
                this.StatusUpdated?.Invoke(this, System.EventArgs.Empty);
            }

            void OnCompleted()
            {
                this.Completed?.Invoke(this, System.EventArgs.Empty);
            }

            static IEnumerable<string> FindRefs(string guid)
            {
                if (string.IsNullOrEmpty(guid)) return Enumerable.Empty<string>();

                var files = System.IO.Directory.EnumerateFiles(_dataPath, "*.unity", System.IO.SearchOption.AllDirectories)
                            .Union(System.IO.Directory.EnumerateFiles(_dataPath, "*.prefab", System.IO.SearchOption.AllDirectories))
                            .Union(System.IO.Directory.EnumerateFiles(_dataPath, "*.asset", System.IO.SearchOption.AllDirectories));

                return from file in files.AsParallel().AsOrdered().WithMergeOptions(ParallelMergeOptions.NotBuffered)
                       where File.ReadLines(file).Any(line => line.Contains(guid))
                       select file;
            }


        }

        public class SearchStringQuery : IQuery
        {

            public event System.EventHandler StatusUpdated;
            public event System.EventHandler Completed;

            #region Fields/Properties

            private CancellationTokenSource _cancellationTokenSource;

            private StringBuilder _output = new StringBuilder();
            private List<OutputRefEntry> _outputRefs = new List<OutputRefEntry>();

            public string SearchString { get; set; }

            public AssetTypes AssetTypes { get; set; } = AssetTypes.All;

            public bool UseRegex { get; set; } = true;

            public bool IsProcessing => _cancellationTokenSource != null;

            public string CurrentStatus { get; private set; } = string.Empty;

            public StringBuilder Output => _output;

            public IReadOnlyList<OutputRefEntry> OutputRefs => _outputRefs;

            #endregion

            public void Cancel()
            {
                _cancellationTokenSource?.Cancel();
            }

            public void Clear()
            {
                if (this.IsProcessing) return; //can't clear if running

                _output.Clear();
                _outputRefs.Clear();
                this.CurrentStatus = string.Empty;
            }

            public async Task Search()
            {
                if (_cancellationTokenSource != null) return;

                try
                {
                    _output.Clear();
                    _outputRefs.Clear();
                    if (string.IsNullOrEmpty(this.SearchString) || this.AssetTypes == AssetTypes.None)
                    {
                        _output.AppendLine("Nothing to query.");
                        return;
                    }

                    _cancellationTokenSource = new CancellationTokenSource();

                    var l_ext = new List<string>(3);
                    if (this.AssetTypes.HasFlagT(AssetTypes.Scenes)) l_ext.Add("*.unity");
                    if (this.AssetTypes.HasFlagT(AssetTypes.Prefabs)) l_ext.Add("*.prefab");
                    if (this.AssetTypes.HasFlagT(AssetTypes.Assets)) l_ext.Add("*.asset");
                    if (l_ext.Count == 0)
                    {
                        _output.AppendLine("Nothing to query.");
                        return;
                    }

                    var rx = this.UseRegex ? new Regex(this.SearchString) : null;
                    var str = this.SearchString;
                    var files = Directory.EnumerateFiles("Assets", l_ext[0], SearchOption.AllDirectories);
                    for (int i = 1; i < l_ext.Count; i++)
                    {
                        files = files.Union(Directory.EnumerateFiles("Assets", l_ext[i], SearchOption.AllDirectories));
                    }

                    var validresults = new System.Collections.Concurrent.ConcurrentQueue<OutputRefEntry>();
                    var task = Task.Run(() =>
                    {
                        var sb = new StringBuilder();
                        string script = string.Empty;
                        bool isMonoBehaviour = false;
                        bool lastLineReset = false;
                        Match m;

                        foreach (var path in files)
                        {
                            int linenum = 0;
                            bool matches = false;
                            sb.Clear();
                            this.OnStatusUpdated("Processing: " + path);

                            using (var reader = new StreamReader(path))
                            {
                                string ln;
                                while ((ln = reader.ReadLine()) != null)
                                {
                                    if (_cancellationTokenSource?.IsCancellationRequested ?? false) return;

                                    if (ln.StartsWith("--- !u"))
                                    {
                                        script = string.Empty;
                                        isMonoBehaviour = false;
                                        lastLineReset = true;
                                    }
                                    else if (lastLineReset)
                                    {
                                        lastLineReset = false;
                                        script = ln.EndsWith(":") ? ln.Substring(0, ln.Length - 1) : ln;
                                        isMonoBehaviour = (script == "MonoBehaviour");
                                    }
                                    else if (isMonoBehaviour &&
                                             (m = Regex.Match(ln, @"^\s*m_Script: {fileID: (?<fileid>-?\d+), guid: (?<guid>([a-zA-Z0-9]+)), type: 3}\s*$")).Success &&
                                             ScriptDatabase.TryGUIDToScriptInfo(m.Groups["guid"].Value, out ScriptInfo info))
                                    {
                                        script = info.name;
                                    }

                                    linenum++;
                                    if (rx != null ? rx.IsMatch(ln) : ln.Contains(str))
                                    {
                                        if (!matches) sb.AppendLine(path);
                                        matches = true;

                                        if (string.IsNullOrEmpty(script))
                                        {
                                            sb.AppendLine($"{linenum:000000}: {ln}");
                                        }
                                        else
                                        {
                                            sb.AppendLine($"{linenum:000000} [{script}]: {ln}");
                                        }
                                    }
                                }
                            }

                            if (matches)
                            {
                                validresults.Enqueue(new OutputRefEntry()
                                {
                                    path = path,
                                    message = sb.ToString(),
                                });
                            }
                        }
                    }, _cancellationTokenSource.Token);

                    while (!task.IsCompleted)
                    {
                        await Task.Yield();
                        if (validresults.Count > 0 && validresults.TryDequeue(out OutputRefEntry entry))
                        {
                            _output.AppendLine(entry.message);
                            entry.obj = AssetDatabase.LoadAssetAtPath(entry.path, typeof(UnityEngine.Object));
                            _outputRefs.Add(entry);
                        }
                    }

                    while (validresults.TryDequeue(out OutputRefEntry entry))
                    {
                        _output.AppendLine(entry.message);
                        entry.obj = AssetDatabase.LoadAssetAtPath(entry.path, typeof(UnityEngine.Object));
                        _outputRefs.Add(entry);
                    }
                }
                catch (System.Exception ex)
                {
                    OnStatusUpdated("ERROR: " + ex.Message);
                }
                finally
                {
                    _cancellationTokenSource = null;
                    OnStatusUpdated(string.Empty);
                    this.OnCompleted();
                }
            }

            void OnStatusUpdated(string msg)
            {
                this.CurrentStatus = msg;
                this.StatusUpdated?.Invoke(this, System.EventArgs.Empty);
            }

            void OnCompleted()
            {
                this.Completed?.Invoke(this, System.EventArgs.Empty);
            }

        }

        [System.Flags]
        public enum AssetTypes
        {
            All = -1,
            None = 0,
            Scenes = 1, //*.unity
            Prefabs = 2, //*.prefab
            Assets = 4, //*.asset
        }

        #endregion

    }

}
