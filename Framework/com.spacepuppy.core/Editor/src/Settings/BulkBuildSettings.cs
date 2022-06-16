#pragma warning disable 0649 // variable declared but not used.
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Threading.Tasks;

using com.spacepuppy;

namespace com.spacepuppyeditor.Settings
{

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

        [SerializeField]
        [EnumFlags]
        private ScriptOptions _postBuildScriptRunOptions;

        [SerializeField]
        [ReorderableArray]
        private List<string> _postBuildScripts;

        #endregion

        #region Properties

        public List<BuildSettings> Builds
        {
            get { return _builds; }
        }

        public ScriptOptions PostBuildScriptRunOptions
        {
            get { return _postBuildScriptRunOptions; }
            set { _postBuildScriptRunOptions = value; }
        }

        public List<string> PostBuildScripts
        {
            get { return _postBuildScripts; }
        }

        #endregion

        #region Methods

        public async Task BuildAsync(BuildSettings.PostBuildOption postBuildOption = BuildSettings.PostBuildOption.Nothing)
        {
            await Task.Delay(10);

            bool failed = false;
            foreach (var settings in _builds)
            {
                if (settings != null)
                {
                    if (!(await settings.BuildAsync(postBuildOption)))
                    {
                        failed = true;
                    }
                    await Task.Delay(1000);
                }
            }

            if ((_postBuildScriptRunOptions & ScriptOptions.Run) == 0) return;
            if ((_postBuildScriptRunOptions & ScriptOptions.CancelIfBuildFails) != 0 && failed) return;

            this.RunScripts();
        }

        private void RunScripts()
        {
            foreach (var str in _postBuildScripts)
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

                        if ((_postBuildScriptRunOptions & ScriptOptions.BlockUntilComplete) != 0)
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

        protected override void OnSPInspectorGUI()
        {
            base.OnSPInspectorGUI();

            if (GUILayout.Button("Build"))
            {
                if (EditorUtility.DisplayDialog("Build?", "Confirm that you want to perform a bulk build.", "Yes", "Cancel"))
                {
                    var settings = this.target as BulkBuildSettings;
                    if (settings == null) return;

                    _ = settings.BuildAsync();
                }
            }
        }

    }

}
