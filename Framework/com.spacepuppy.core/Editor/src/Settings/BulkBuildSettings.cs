#pragma warning disable 0649 // variable declared but not used.
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.spacepuppy;

namespace com.spacepuppyeditor.Settings
{

    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
    public class BulkBuildPreBuildCallbackAttribute : System.Attribute
    {

    }

    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
    public class BulkBuildPostBuildCallbackAttribute : System.Attribute
    {

    }

    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
    public class BulkBuildAuxiliaryButtonCallbackAttribute : System.Attribute
    {
        public string label;
        public int order;

        public BulkBuildAuxiliaryButtonCallbackAttribute(string label, int order = 0)
        {
            this.label = label;
            this.order = order;
        }
    }

    [CreateAssetMenu(fileName = "BulkBuildSettings", menuName = "Spacepuppy Build Pipeline/Bulk Build Settings")]
    public class BulkBuildSettings : ScriptableObject
    {

        [System.Flags]
        public enum ScriptOptions
        {
            Nothing = 0,
            Run = 1,
            CancelIfBuildFails = 2,
            BlockUntilComplete = 4
        }

        #region Fields

        [SerializeField]
        [ReorderableArray]
        private List<BuildSettings> _builds;

        [SerializeField, EnumFlags, UnityEngine.Serialization.FormerlySerializedAs("_postBuildScriptRunOptions")]
        private ScriptOptions _buildScriptRunOptions;

        [SerializeField, ReorderableArray]
        private List<string> _preBuildScripts;

        [SerializeField, ReorderableArray]
        private List<string> _postBuildScripts;

        #endregion

        #region Properties

        public List<BuildSettings> Builds
        {
            get { return _builds; }
        }

        public ScriptOptions PostBuildScriptRunOptions
        {
            get { return _buildScriptRunOptions; }
            set { _buildScriptRunOptions = value; }
        }

        public List<string> PostBuildScripts
        {
            get { return _postBuildScripts; }
        }

        #endregion

        #region Methods

        public void Build(BuildSettings.PostBuildOption postBuildOption = BuildSettings.PostBuildOption.Nothing)
        {
            foreach (var m in TypeCache.GetMethodsWithAttribute<BulkBuildPreBuildCallbackAttribute>())
            {
                try
                {
                    m.Invoke(null, new object[] { this });
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            if ((_buildScriptRunOptions & ScriptOptions.Run) != 0)
            {
                this.RunScripts(_preBuildScripts);
            }

            if (_builds.Count > 0)
            {
                _builds[0].Build(postBuildOption, new BuildSettings.BulkBuildCallback(this, postBuildOption, 0));
            }
        }

        internal async void ContinueBuild(BuildSettings.BulkBuildCallback callback, bool success)
        {
            callback.pass++;
            if (!success) callback.failed = true;

            await System.Threading.Tasks.Task.Delay(1000);

            if (callback.pass < _builds.Count)
            {
                _builds[callback.pass].Build(callback.postBuildOption, callback);
            }
            else
            {
                this.CompleteBuild(callback.failed);
            }
        }

        void CompleteBuild(bool failed)
        {
            foreach (var m in TypeCache.GetMethodsWithAttribute<BulkBuildPostBuildCallbackAttribute>())
            {
                try
                {
                    m.Invoke(null, new object[] { this });
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            if ((_buildScriptRunOptions & ScriptOptions.Run) == 0) return;
            if ((_buildScriptRunOptions & ScriptOptions.CancelIfBuildFails) != 0 && failed) return;

            this.RunScripts(_postBuildScripts);
        }

        private void RunScripts(IList<string> scripts)
        {
            foreach (var str in scripts)
            {
                if (string.IsNullOrEmpty(str)) continue;

                try
                {
                    string path = str;
                    if (path.StartsWith("."))
                    {
                        path = System.IO.Path.Combine(Application.dataPath, str);
                        path = System.IO.Path.GetFullPath(path);
                    }

                    if (System.IO.File.Exists(path))
                    {
                        var proc = new System.Diagnostics.Process();
                        proc.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(path);
                        proc.StartInfo.FileName = System.IO.Path.GetFileName(path);
                        proc.StartInfo.CreateNoWindow = false;
                        proc.Start();

                        if ((_buildScriptRunOptions & ScriptOptions.BlockUntilComplete) != 0)
                        {
                            proc.WaitForExit();
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        #endregion

    }

    [CustomEditor(typeof(BulkBuildSettings))]
    public class BulkBuildSettingsEditor : SPEditor
    {

        public const int BUILD_BUTTON_ORDER = 1000;

        protected override void OnSPInspectorGUI()
        {
            base.OnSPInspectorGUI();

            EditorGUILayout.Space(5f);
            bool needAnotherSpace = false;

            var arr_auxbuttons = TypeCache.GetMethodsWithAttribute<BulkBuildAuxiliaryButtonCallbackAttribute>().Select(o => (o, o.GetCustomAttribute<BulkBuildAuxiliaryButtonCallbackAttribute>())).OrderBy(o => o.Item2.order).ToArray();
            int next_auxbuttonindex = 0;
            if (arr_auxbuttons.Length > 0)
            {
                for (int i = next_auxbuttonindex; i < arr_auxbuttons.Length; i++)
                {
                    var entry = arr_auxbuttons[i];
                    if (entry.Item2.order >= BUILD_BUTTON_ORDER) break;

                    next_auxbuttonindex = i + 1;
                    needAnotherSpace = true;
                    if (GUILayout.Button(entry.Item2.label))
                    {
                        try
                        {
                            entry.Item1.Invoke(null, new object[] { this.target });
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }
            }

            var arr_pre = TypeCache.GetMethodsWithAttribute<BulkBuildPreBuildCallbackAttribute>();
            if (arr_pre.Count > 0)
            {
                needAnotherSpace = true;
                if (GUILayout.Button("Run Only BulkBuildPreBuildCallbacks"))
                {
                    foreach (var m in arr_pre)
                    {
                        try
                        {
                            m.Invoke(null, new object[] { this.target });
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }

                EditorGUILayout.Space(5f);
            }
            var arr_post = TypeCache.GetMethodsWithAttribute<BulkBuildPostBuildCallbackAttribute>();
            if (arr_post.Count > 0)
            {
                needAnotherSpace = true;
                if (GUILayout.Button("Run Only BulkBuildPostBuildCallbacks"))
                {
                    foreach (var m in arr_post)
                    {
                        try
                        {
                            m.Invoke(null, new object[] { this.target });
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }
            }

            if (needAnotherSpace) EditorGUILayout.Space(5f);
            if (GUILayout.Button("Build"))
            {
                if (EditorUtility.DisplayDialog("Build?", "Confirm that you want to perform a bulk build.", "Yes", "Cancel"))
                {
                    var settings = this.target as BulkBuildSettings;
                    if (settings == null) return;

                    settings.Build();
                }
            }

            if (next_auxbuttonindex < arr_auxbuttons.Length)
            {
                if (needAnotherSpace) EditorGUILayout.Space(5f);

                for (int i = next_auxbuttonindex; i < arr_auxbuttons.Length; i++)
                {
                    var entry = arr_auxbuttons[i];

                    next_auxbuttonindex = i + 1;
                    if (GUILayout.Button(entry.Item2.label))
                    {
                        try
                        {
                            entry.Item1.Invoke(null, new object[] { this.target });
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }
            }
        }

    }

}
