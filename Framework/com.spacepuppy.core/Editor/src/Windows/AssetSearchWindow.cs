using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

using com.spacepuppy.Utils;
using System.Threading;

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
        private StringBuilder _statusOutput = new StringBuilder();
        private StringBuilder _output = new StringBuilder();
        private List<(string, UnityEngine.Object)> _outputRefs = new List<(string, UnityEngine.Object)>();
        private Vector2 _scrollPos;

        private CancellationTokenSource _cancellationTokenSource;

        private void OnEnable()
        {
            _scrollPos = Vector2.zero;
            _dataPath = Application.dataPath;
            this.titleContent = new GUIContent("Spacepuppy Asset Search...");
        }

        private void OnInspectorUpdate()
        {
            if (_cancellationTokenSource != null)
            {
                this.Repaint();
            }
        }

        private void OnGUI()
        {
            var cache = GUI.enabled;
            GUI.enabled = _cancellationTokenSource == null;
            _target = EditorGUILayout.ObjectField("Target", _target, typeof(UnityEngine.Object), false);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Search for unreferenced scripts", GUILayout.Width(200f)))
            {
                _ = this.SearchForUnusedScripts();
            }

            if (_target != null)
            {
                if (GUILayout.Button("Search Everything For Target", GUILayout.Width(200f)))
                {
                    _ = this.SearchEverythingForTargetAsset(_target != null ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_target)) : null);
                }
            }
            GUI.enabled = cache;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Output:");
            //EditorGUILayout.HelpBox(_output.ToString(), MessageType.None);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            if (_outputRefs.Count > 0)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandHeight(true));
                foreach (var pair in _outputRefs)
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
                EditorGUILayout.SelectableLabel(_output.ToString(), EditorStyles.helpBox, GUILayout.ExpandHeight(true));
            }
            EditorGUILayout.EndScrollView();


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Status:", GUILayout.Width(45f));
            EditorGUILayout.SelectableLabel(_statusOutput.ToString(), EditorStyles.helpBox, GUILayout.ExpandWidth(true), GUILayout.Height(20f));
            if (_cancellationTokenSource != null)
            {
                if (GUILayout.Button("Cancel", GUILayout.Width(50f)))
                {
                    _cancellationTokenSource.Cancel();
                }
            }
            else
            {
                if (GUILayout.Button("Clear", GUILayout.Width(50f)))
                {
                    _output.Clear();
                    _outputRefs.Clear();
                    _statusOutput.Clear();
                    this.Repaint();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        void UpdateStatus(string msg)
        {
            _statusOutput.Clear();
            _statusOutput.Append(msg);
        }

        private async Task SearchForUnusedScripts()
        {
            if (_cancellationTokenSource != null) return;

            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _output.Clear();
                _outputRefs.Clear();

                string[] guids = AssetDatabase.FindAssets("t:script", new string[] { "Assets/Code" });
                foreach (var guid in guids)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested) return;

                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    UpdateStatus("Processing: " + path);

                    await Task.Run(() =>
                    {
                        var fn = Path.GetFileNameWithoutExtension(path);
                        var tp = TypeUtil.FindType(fn);
                        if (tp == null || !TypeUtil.IsType(tp, typeof(MonoBehaviour)))
                        {
                            return;
                        }

                        if (!FindRefs(guid).Any())
                        {
                            _output.AppendLine(path);
                            _outputRefs.Add((path, AssetDatabase.LoadAssetAtPath(path, typeof(MonoBehaviour))));
                        }
                    }, _cancellationTokenSource.Token);
                }
            }
            catch (System.Exception ex)
            {
                UpdateStatus("ERROR: " + ex.Message);
            }
            finally
            {
                _cancellationTokenSource = null;
                UpdateStatus("");
                this.Repaint();
            }
        }

        private async Task SearchEverythingForTargetAsset(string guid)
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
                UpdateStatus("Processing...");

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
                    _outputRefs.Add((apath, AssetDatabase.LoadAssetAtPath(apath, typeof(UnityEngine.Object))));
                }
            }
            catch (System.Exception ex)
            {
                UpdateStatus("ERROR: " + ex.Message);
            }
            finally
            {
                _cancellationTokenSource = null;
                UpdateStatus("");
                this.Repaint();
            }
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

        #endregion

    }
}
