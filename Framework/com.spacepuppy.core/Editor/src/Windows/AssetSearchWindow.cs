using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using com.spacepuppy.Utils;

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

        [MenuItem("CONTEXT/Component/Copy Reference Id", false, 1000)]
        private static void ThisIsATest(MenuCommand command)
        {
            GUIUtility.systemCopyBuffer = command.context.GetInstanceID().ToString();
        }

        #endregion

        #region Window

        private static AssetSearchWindow _openWindow;
        private static string _dataPath;

        private UnityEngine.Object _target;

        private SearchForMissingScriptsQuery _missingScriptQuery = new SearchForMissingScriptsQuery();
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
            //general unity info
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.SelectableLabel($"Unity {Application.unityVersion}    PID: {System.Diagnostics.Process.GetCurrentProcess().Id}", EditorStyles.helpBox, GUILayout.ExpandWidth(true), GUILayout.Height(18f));
            EditorGUILayout.EndHorizontal();

            //actual search tool
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
            if (GUILayout.Button("Search for missing scripts", GUILayout.Width(200f)))
            {
                _currentQuery = _missingScriptQuery;
                _ = _missingScriptQuery.Search();
            }
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
                if (_target is UnityEditor.DefaultAsset && System.IO.Directory.Exists(AssetDatabase.GetAssetPath(_target)))
                {
                    if (GUILayout.Button("Search Everything For Contents", GUILayout.Width(200f)))
                    {
                        _currentQuery = _assetSearchQuery;
                        _ = _assetSearchQuery.SearchEverythingInFolder(AssetDatabase.GetAssetPath(_target));
                    }
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

        public abstract class BaseQuery : IQuery
        {

            public event System.EventHandler StatusUpdated;
            public event System.EventHandler Completed;

            private CancellationTokenSource _cancellationTokenSource;
            protected StringBuilder _output = new StringBuilder();
            protected List<OutputRefEntry> _outputRefs = new List<OutputRefEntry>();
            private int _invokeTokenStatusUpdated;
            private int _invokeTokenCompleted;

            public bool IsProcessing => _cancellationTokenSource != null;

            public StringBuilder Output => _output;

            public IReadOnlyList<OutputRefEntry> OutputRefs => _outputRefs;

            public string CurrentStatus { get; protected set; } = string.Empty;

            protected System.Threading.CancellationToken CancellationToken => _cancellationTokenSource.Token;
            protected bool IsCancellationRequested => _cancellationTokenSource?.IsCancellationRequested ?? false;

            public virtual void Cancel()
            {
                _cancellationTokenSource?.Cancel();
            }

            public virtual void Clear()
            {
                if (this.IsProcessing) return; //can't clear if running

                _output.Clear();
                _outputRefs.Clear();
                this.CurrentStatus = string.Empty;
            }

            protected void StartProcessing()
            {
                if (_cancellationTokenSource == null)
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                }
            }

            protected void SignalStatusUpdated(string msg)
            {
                this.CurrentStatus = msg;

                if (EditorHelper.InvokeRequired)
                {
                    _invokeTokenStatusUpdated = EditorHelper.InvokePassive(() =>
                    {
                        this.StatusUpdated?.Invoke(this, System.EventArgs.Empty);
                    }, _invokeTokenStatusUpdated);
                }
                else
                {
                    this.StatusUpdated?.Invoke(this, System.EventArgs.Empty);
                }
            }

            protected void SignalCompleted()
            {
                _cancellationTokenSource = null;
                if (EditorHelper.InvokeRequired)
                {
                    _invokeTokenCompleted = EditorHelper.InvokePassive(() =>
                    {
                        this.Completed?.Invoke(this, System.EventArgs.Empty);
                    }, _invokeTokenCompleted);
                }
                else
                {
                    this.Completed?.Invoke(this, System.EventArgs.Empty);
                }
            }

        }

        public class AssetSearchQuery : BaseQuery
        {

            #region Fields/Properties

            public string CodePath { get; set; } = "Code";

            #endregion

            public async Task SearchForScripts(bool inuse)
            {
                if (this.IsProcessing) return;

                try
                {
                    this.StartProcessing();
                    _output.Clear();
                    _outputRefs.Clear();
                    SignalStatusUpdated("Processing...");

                    string[] guids = AssetDatabase.FindAssets("t:script", new string[] { "Assets/" + this.CodePath });
                    foreach (var guid in guids)
                    {
                        if (this.IsCancellationRequested) return;

                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        SignalStatusUpdated("Processing: " + path);

                        bool success = await Task.Run(() =>
                        {
                            var fn = Path.GetFileNameWithoutExtension(path);
                            var tp = TypeUtil.FindType(fn);
                            if (tp == null || !TypeUtil.IsType(tp, typeof(MonoBehaviour)))
                            {
                                return false;
                            }

                            return FindRefs(guid).Any() == inuse;
                        }, this.CancellationToken);
                        if (this.IsCancellationRequested) return;

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
                    SignalStatusUpdated("ERROR: " + ex.Message);
                }
                finally
                {
                    SignalStatusUpdated(string.Empty);
                    this.SignalCompleted();
                }
            }

            public Task SearchEverythingForTargetAsset(UnityEngine.Object target) => SearchEverythingForTargetAsset(target != null ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(target)) : null);

            public Task SearchEverythingForTargetAsset(string guid) => SearchEverythingForTargets(string.IsNullOrEmpty(guid) ? ArrayUtil.Empty<string>() : new string[] { guid });

            public Task SearchEverythingInFolder(string directory)
            {
                if (this.IsProcessing) return Task.CompletedTask;

                if (string.IsNullOrEmpty(directory))
                {
                    _output.Clear();
                    _outputRefs.Clear();
                    _output.AppendLine("Select a target to search for...");
                    return Task.CompletedTask;
                }

                IEnumerable<string> guids = null;
                try
                {
                    guids = Directory.EnumerateFiles(directory, "*.meta", SearchOption.AllDirectories)
                                     .Select(s => s.Substring(0, s.Length - 5))
                                     .Select(s => AssetDatabase.AssetPathToGUID(s));
                }
                catch (System.Exception ex)
                {
                    SignalStatusUpdated("ERROR: " + ex.Message);
                }
                finally
                {
                    SignalStatusUpdated("");
                    this.SignalCompleted();
                }

                return this.SearchEverythingForTargets(guids);
            }

            public async Task SearchEverythingForTargets(IEnumerable<string> guids)
            {
                if (this.IsProcessing) return;

                try
                {
                    this.StartProcessing();
                    _output.Clear();
                    _outputRefs.Clear();
                    SignalStatusUpdated("Processing...");

                    guids = guids?.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                    if (guids == null || guids.Count() == 0)
                    {
                        _output.Clear();
                        _outputRefs.Clear();
                        _output.AppendLine("Select targets to search for...");
                        return;
                    }

                    var validresults = new System.Collections.Concurrent.ConcurrentQueue<OutputRefEntry>();
                    var task = Task.Run(() =>
                    {
                        try
                        {
                            var files = System.IO.Directory.EnumerateFiles("Assets", "*.unity", System.IO.SearchOption.AllDirectories)
                                        .Union(System.IO.Directory.EnumerateFiles("Assets", "*.prefab", System.IO.SearchOption.AllDirectories))
                                        .Union(System.IO.Directory.EnumerateFiles("Assets", "*.asset", System.IO.SearchOption.AllDirectories));

                            files.AsParallel().ForAll(file =>
                            {
                                this.SignalStatusUpdated("Processing: " + file);

                                using (var reader = new StreamReader(file))
                                {
                                    bool skip = false;
                                    string ln;
                                    while (!skip && (ln = reader.ReadLine()) != null)
                                    {
                                        if (this.IsCancellationRequested) return;

                                        foreach (var guid in guids)
                                        {
                                            if (ln.Contains(guid))
                                            {
                                                validresults.Enqueue(new OutputRefEntry()
                                                {
                                                    path = file,
                                                    message = file,
                                                });
                                                skip = true;
                                            }
                                        }
                                    }
                                }
                            });
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }, this.CancellationToken);

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
                    SignalStatusUpdated("ERROR: " + ex.Message);
                }
                finally
                {
                    SignalStatusUpdated("");
                    this.SignalCompleted();
                }
            }

            static IEnumerable<string> FindRefs(string guid)
            {
                if (string.IsNullOrEmpty(guid)) return Enumerable.Empty<string>();

                var files = System.IO.Directory.EnumerateFiles("Assets", "*.unity", System.IO.SearchOption.AllDirectories)
                            .Union(System.IO.Directory.EnumerateFiles("Assets", "*.prefab", System.IO.SearchOption.AllDirectories))
                            .Union(System.IO.Directory.EnumerateFiles("Assets", "*.asset", System.IO.SearchOption.AllDirectories));

                return from file in files.AsParallel().AsOrdered().WithMergeOptions(ParallelMergeOptions.NotBuffered)
                       where File.ReadLines(file).Any(line => line.Contains(guid))
                       select file;
            }

        }

        public class SearchStringQuery : BaseQuery
        {

            #region Fields/Properties

            public string SearchString { get; set; }

            public AssetTypes AssetTypes { get; set; } = AssetTypes.All;

            public bool UseRegex { get; set; } = true;

            #endregion

            public async Task Search()
            {
                if (this.IsProcessing) return;

                try
                {
                    this.StartProcessing();
                    _output.Clear();
                    _outputRefs.Clear();
                    SignalStatusUpdated("Processing...");

                    if (string.IsNullOrEmpty(this.SearchString) || this.AssetTypes == AssetTypes.None)
                    {
                        _output.AppendLine("Nothing to query.");
                        return;
                    }

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
                            this.SignalStatusUpdated("Processing: " + path);

                            using (var reader = new StreamReader(path))
                            {
                                string ln;
                                while ((ln = reader.ReadLine()) != null)
                                {
                                    if (this.IsCancellationRequested) return;

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
                    }, this.CancellationToken);

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
                    SignalStatusUpdated("ERROR: " + ex.Message);
                }
                finally
                {
                    SignalStatusUpdated(string.Empty);
                    this.SignalCompleted();
                }
            }

        }

        public class SearchForMissingScriptsQuery : BaseQuery
        {

            #region Fields/Properties

            public AssetTypes AssetTypes { get; set; } = AssetTypes.All;

            private int _invokeTokenStatusUpdated;

            #endregion

            public async Task Search()
            {
                if (this.IsProcessing) return;

                try
                {
                    this.StartProcessing();
                    _output.Clear();
                    _outputRefs.Clear();
                    SignalStatusUpdated("Processing...");

                    var l_ext = new List<string>(3);
                    if (this.AssetTypes.HasFlagT(AssetTypes.Scenes)) l_ext.Add("*.unity");
                    if (this.AssetTypes.HasFlagT(AssetTypes.Prefabs)) l_ext.Add("*.prefab");
                    if (this.AssetTypes.HasFlagT(AssetTypes.Assets)) l_ext.Add("*.asset");
                    if (l_ext.Count == 0)
                    {
                        _output.AppendLine("Nothing to query.");
                        return;
                    }

                    var files = Directory.EnumerateFiles("Assets", l_ext[0], SearchOption.AllDirectories);
                    for (int i = 1; i < l_ext.Count; i++)
                    {
                        files = files.Union(Directory.EnumerateFiles("Assets", l_ext[i], SearchOption.AllDirectories));
                    }

                    var validresults = new System.Collections.Concurrent.ConcurrentQueue<OutputRefEntry>();
                    var task = Task.Run(() =>
                    {
                        var rxname = new Regex(@"^\s*m_Name: (?<name>(.*))$");
                        var rxscript = new Regex(@"^\s*m_Script: {fileID: (?<fileid>-?\d+), guid: (?<guid>([a-zA-Z0-9]+)), type: 3}\s*$");
                        foreach (var filepath in files)
                        {
                            if (this.IsCancellationRequested) return;

                            this.SignalStatusUpdated("Processing: " + filepath);
                            int state = filepath.EndsWith(".asset") ? -1 : 0; //-1 = in asset, 0 = reset, 1 = reset last line, 2 = in gameobject
                            string currentname = string.Empty;
                            using (var reader = new StreamReader(filepath))
                            {
                                string ln;
                                while ((ln = reader.ReadLine()) != null)
                                {
                                    if (this.IsCancellationRequested) return;

                                    switch (state)
                                    {
                                        case 0:
                                            if (ln.StartsWith("--- !u"))
                                            {
                                                state = 1;
                                            }
                                            break;
                                        case 1:
                                            if (ln.StartsWith("GameObject:"))
                                            {
                                                state = 2;
                                                currentname = string.Empty;
                                            }
                                            else
                                            {
                                                state = 0;
                                            }
                                            break;
                                        case 2:
                                        default:
                                            var mname = rxname.Match(ln);
                                            if (mname.Success)
                                            {
                                                currentname = mname.Groups["name"].Value;
                                            }
                                            break;
                                    }
                                    var m = rxscript.Match(ln);
                                    if (!m.Success) continue;

                                    if (ScriptDatabase.TryGUIDToScriptInfo(m.Groups["guid"].Value, out _)) continue;

                                    validresults.Enqueue(new OutputRefEntry()
                                    {
                                        path = filepath,
                                        message = $"Missing Script: {m.Groups["guid"].Value}\r\n  {filepath}\r\n  {currentname}",
                                    });
                                }
                            }
                        }
                    }, this.CancellationToken);

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
                    SignalStatusUpdated("ERROR: " + ex.Message);
                }
                finally
                {
                    SignalStatusUpdated(string.Empty);
                    this.SignalCompleted();
                }
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
