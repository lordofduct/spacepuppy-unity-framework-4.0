#pragma warning disable 0649 // variable declared but not used.
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;

using com.spacepuppy;
using com.spacepuppy.Collections;
using com.spacepuppy.Utils;

using com.spacepuppyeditor.Internal;
using UnityEditor.SearchService;

namespace com.spacepuppyeditor.Settings
{

    [CreateAssetMenu(fileName = "BuildSettings", menuName = "Spacepuppy Build Pipeline/Build Settings")]
    public class BuildSettings : ScriptableObject
    {

        [System.Flags]
        public enum PostBuildOption
        {
            Nothing = 0,
            OpenFolder = 1,
            Run = 2,
            OpenFolderAndRun = 3
        }

        #region Fields

        [SerializeField]
        [Tooltip("Leave blank if you want to be asked for a filename every time you build.")]
        public string BuildFileName;
        [SerializeField]
        [Tooltip("Paths can be relative to the 'Assets' folder.\nLeave blank if you want to be asked for a directory every time you build.")]
        public string BuildDirectory;

        [SerializeField]
        public bool PurgeBuildDirectory;

        [SerializeField]
        public VersionInfo Version;

        [SerializeField]
        private SceneAsset _bootScene;

        [SerializeField]
        [ReorderableArray]
        private List<SceneAsset> _scenes;

        [SerializeField]
        private BuildTarget _buildTarget = BuildTarget.StandaloneWindows;

        [SerializeField]
        [EnumFlags]
        private BuildOptions _buildOptions;

        [SerializeField]
        [Tooltip("Leave blank if you want to use default settings found in the Input Settings screen.")]
        private InputSettings _inputSettings;

        [SerializeField]
        private bool _defineSymbols;

        [SerializeField, TextArea(3, 10), Tooltip("Semi-colon delimited symbols.")]
        private string _symbols;

        [SerializeField]
        [ReorderableArray]
        private List<PlayerSettingOverride> _playerSettingOverrides = new List<PlayerSettingOverride>();

        #endregion

        #region Properties

        public SceneAsset BootScene
        {
            get { return _bootScene; }
            set { _bootScene = value; }
        }

        public IList<SceneAsset> Scenes
        {
            get { return _scenes; }
        }

        public BuildTarget BuildTarget
        {
            get { return _buildTarget; }
            set { _buildTarget = value; }
        }

        public BuildOptions BuildOptions
        {
            get { return _buildOptions; }
            set { _buildOptions = value; }
        }

        public InputSettings InputSettings
        {
            get { return _inputSettings; }
            set { _inputSettings = value; }
        }

        public bool DefineSymbols
        {
            get { return _defineSymbols; }
            set { _defineSymbols = value; }
        }

        /// <summary>
        /// Semi-colon delimited symbols.
        /// </summary>
        public string Symbols
        {
            get { return _symbols; }
            set { _symbols = value; }
        }

        public List<PlayerSettingOverride> PlayerSettingsOverrides
        {
            get { return _playerSettingOverrides; }
        }

        #endregion

        #region Methods

        public string GetBuildFileNameWithExtension()
        {
            if (string.IsNullOrEmpty(this.BuildFileName)) return string.Empty;

            string extension = GetExtension(this.BuildTarget);
            string fileName = this.BuildFileName;
            if (!string.IsNullOrEmpty(extension))
            {
                string ext = "." + extension;
                if (!fileName.EndsWith(ext)) fileName += ext;
            }
            return fileName;
        }

        public virtual string[] GetScenePaths()
        {
            using (var lst = TempCollection.GetList<string>())
            {
                if (this.BootScene != null) lst.Add(AssetDatabase.GetAssetPath(this.BootScene));

                foreach (var scene in this.Scenes)
                {
                    lst.Add(AssetDatabase.GetAssetPath(scene));
                }

                return lst.ToArray();
            }
        }

        /// <summary>
        /// Returns true if the pipeline was started, false if it failed to start. 
        /// This does not mean the build succeeded since building may recompile and we don't know if tha that happened or not.
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public virtual bool Build(PostBuildOption option) => Build(option, default);
        internal bool Build(PostBuildOption option, BulkBuildCallback callback)
        {
            string path;
            bool purge = false;
            try
            {
                //get output directory
                var dir = EditorProjectPrefs.LocalProject.GetString("LastBuildDirectory", string.Empty);
                if (string.IsNullOrEmpty(this.BuildFileName))
                {
                    string extension = GetExtension(this.BuildTarget);
                    path = EditorUtility.SaveFilePanel("Build", dir, string.IsNullOrEmpty(extension) ? Application.productName + "." + extension : Application.productName, extension);
                    if (string.IsNullOrEmpty(path))
                    {
                        return false;
                    }
                }
                else
                {
                    string possiblePath = this.BuildDirectory;
                    if (!string.IsNullOrEmpty(possiblePath) && possiblePath.StartsWith(".")) possiblePath = System.IO.Path.Combine(Application.dataPath, possiblePath);
                    if (!string.IsNullOrEmpty(possiblePath) && System.IO.Directory.Exists(possiblePath))
                    {
                        path = System.IO.Path.Combine(possiblePath, this.GetBuildFileNameWithExtension());
                        path = System.IO.Path.GetFullPath(path);
                        purge = this.PurgeBuildDirectory;
                    }
                    else
                    {
                        path = EditorUtility.OpenFolderPanel("Build", dir, string.Empty);
                        if (!string.IsNullOrEmpty(path))
                        {
                            path = System.IO.Path.Combine(path, this.GetBuildFileNameWithExtension());
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }

            //return this.BuildAsync(path, option, purge);

            var command = new BuildCommand()
            {
                path = path,
                postBuildOption = option,
                purgeBuildPath = purge,
                callback = callback,
            };
            command.StartBuild(this);
            return true;
        }

        #endregion

        #region Special Utils

        [System.Serializable]
        public class PlayerSettingOverride : ISerializationCallbackReceiver
        {

            [System.NonSerialized]
            private PropertyInfo _settingInfo;
            public PropertyInfo SettingInfo
            {
                get { return _settingInfo; }
                set
                {
                    if (value == null || BuildSettings.IsValidPropertySettingInfo(value))
                        _settingInfo = value;
                    else
                        throw new System.ArgumentException("PropertyInfo must be for a static property of the 'PlayerSettings' class.");
                }
            }
            [System.NonSerialized]
            public object SettingValue;

            #region Serialization Interface

            [SerializeField]
            private string _propertyName;
            [SerializeField]
            private string _serializedValue;
            [SerializeField]
            private UnityEngine.Object _serializedRef;

            void ISerializationCallbackReceiver.OnAfterDeserialize()
            {
                _settingInfo = null;
                this.SettingValue = null;
                var info = typeof(PlayerSettings).GetProperty(_propertyName, BindingFlags.Static | BindingFlags.Public);
                if (BuildSettings.IsValidPropertySettingInfo(info))
                {
                    var tp = info.PropertyType;
                    if (typeof(UnityEngine.Object).IsAssignableFrom(tp))
                    {
                        _settingInfo = info;
                        this.SettingValue = _serializedRef;
                    }
                    else if (ConvertUtil.IsSupportedType(tp))
                    {
                        _settingInfo = info;
                        this.SettingValue = ConvertUtil.ToPrim(_serializedValue, tp);
                    }
                }
            }

            void ISerializationCallbackReceiver.OnBeforeSerialize()
            {
                if (BuildSettings.IsValidPropertySettingInfo(_settingInfo))
                {
                    _propertyName = _settingInfo.Name;
                    if (typeof(UnityEngine.Object).IsAssignableFrom(_settingInfo.PropertyType))
                    {
                        _serializedRef = this.SettingValue as UnityEngine.Object;
                        _serializedValue = null;
                    }
                    else
                    {
                        _serializedRef = null;
                        _serializedValue = ConvertUtil.ToString(this.SettingValue);
                    }
                }
                else
                {
                    _propertyName = null;
                    _serializedRef = null;
                    _serializedValue = null;
                }
            }

            #endregion

        }

        public static bool IsValidPropertySettingInfo(PropertyInfo info)
        {
            //is read write
            if (info == null || !info.CanRead || !info.CanWrite) return false;
            //is implemented by PlayerSettings
            if (!info.DeclaringType.IsAssignableFrom(typeof(PlayerSettings))) return false;
            //is supported type
            if (!(typeof(UnityEngine.Object).IsAssignableFrom(info.PropertyType) || ConvertUtil.IsSupportedType(info.PropertyType))) return false;

            //getter is public and static
            var getter = info.GetGetMethod();
            if (!getter.IsStatic || !getter.IsPublic) return false;

            //setter is public and static
            var setter = info.GetSetMethod();
            if (!setter.IsStatic || !setter.IsPublic) return false;

            return true;
        }

        public static IEnumerable<PropertyInfo> GetOverridablePlayerSettings()
        {
            return (from info in typeof(PlayerSettings).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                    where info != null && info.CanRead && info.CanWrite &&
                          (typeof(UnityEngine.Object).IsAssignableFrom(info.PropertyType) || ConvertUtil.IsSupportedType(info.PropertyType)) &&
                          info.GetGetMethod().IsPublic && info.GetSetMethod().IsPublic
                    select info);
        }

        public static string GetExtension(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "exe";
                //NOTE - as of 2019.2 only 64-bit linux is supported
                //case BuildTarget.StandaloneLinux:
                //case BuildTarget.StandaloneLinuxUniversal:
                //    return "x86";
                case BuildTarget.StandaloneLinux64:
                    return "x86_64";
                case BuildTarget.StandaloneOSX:
                    return "app";
                default:
                    return string.Empty;
            }
        }

        public static string GetCurrentSymbols(BuildTargetGroup buildGroup)
        {
#if UNITY_2021_2_OR_NEWER
            return PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(buildGroup)) ?? string.Empty;
#else
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(buildGroup) ?? string.Empty;
#endif
        }

        #endregion

        #region BuildCommand

        [System.Serializable]
        class BuildCommand
        {
            internal const string FILENAME_BUILDCOMMAND = "BuildCommand.json";
            const string TEMP_SUFFIX = ".spbuild.bak";
            const string PATH_PROJECTSETTINGS = "ProjectSettings/ProjectSettings.asset";
            const string PATH_INPUTSETTINGS = "ProjectSettings/InputManager.asset";

            enum States
            {
                FailedBuild = -1,
                Initial = 0,
                Building = 1,
                SuccessfulBuild = 2,
            }

            public string path;
            public BuildSettings.PostBuildOption postBuildOption;
            public bool purgeBuildPath;
            public BulkBuildCallback callback;

            [SerializeField]
            string[] scenes;
            [SerializeField]
            BuildTargetGroup buildGroup;
            [SerializeField]
            bool cachedInputSettings;
            [SerializeField]
            bool shouldForceRecompileAtEnd;

            [SerializeField]
            States state;
            [SerializeField]
            private long timestamp;
            [SerializeField]
            private string buildSettingsGuid;


            internal void StartBuild(BuildSettings settings)
            {
                try
                {
                    state = States.Initial;
                    if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(settings, out buildSettingsGuid, out long _)) return;
                    scenes = settings.GetScenePaths();
                    buildGroup = BuildPipeline.GetBuildTargetGroup(settings.BuildTarget);

                    //set version
                    Undo.RecordObject(settings, "Build - Version Increment");
                    settings.Version.Build++;
                    EditorHelper.CommitDirectChanges(settings, true);
                    PlayerSettings.bundleVersion = settings.Version.ToString();
                    AssetDatabase.SaveAssets();

                    //build
                    if (!string.IsNullOrEmpty(path))
                    {
                        //save last build directory
                        EditorProjectPrefs.LocalProject.SetString("LastBuildDirectory", System.IO.Path.GetDirectoryName(path));

                        //do build
                        SPTempFolder.BackupFile(PATH_PROJECTSETTINGS, TEMP_SUFFIX);

                        if (settings.InputSettings != null)
                        {
                            cachedInputSettings = true;

                            SPTempFolder.BackupFile(PATH_INPUTSETTINGS, TEMP_SUFFIX);
                            settings.InputSettings.ApplyToGlobal();
                        }
                        else
                        {
                            cachedInputSettings = false;
                            SPTempFolder.PurgeBackup(PATH_INPUTSETTINGS, TEMP_SUFFIX);
                        }

                        if (settings.DefineSymbols && GetCurrentSymbols(buildGroup) != settings.Symbols)
                        {
                            shouldForceRecompileAtEnd = true;
                            this.PrepForCompilation(States.Building);
                            //EditorUtility.DisplayDialog("Spacepuppy Build Pipeline", "There are distinct custom defines for this build, unity will take a couple seconds to recompile before continuing.", "Ok");
                            Debug.Log("Spacepuppy Build Pipeline - There are distinct custom defines for this build, unity will take a couple seconds to recompile before continuing.");
#if UNITY_2021_2_OR_NEWER
                            PlayerSettings.SetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(buildGroup), settings.Symbols);
                            CompilationPipeline.RequestScriptCompilation();
                            return; //technically this should never get reached
#else
                            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildGroup, settings.Symbols);
                            this.ContinueBuild(settings);
                            return;
#endif
                        }
                        else
                        {
                            shouldForceRecompileAtEnd = false;
                        }

                        this.ContinueBuild(settings);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                    //TODO - callback
                }
            }

            void ContinueBuild(BuildSettings settings)
            {
                state = States.Building;
                try
                {
                    state = States.Building;
                    if (settings._playerSettingOverrides.Count > 0)
                    {
                        foreach (var setting in settings._playerSettingOverrides)
                        {
                            if (setting.SettingInfo != null)
                            {
                                try
                                {
                                    setting.SettingInfo.SetValue(null, setting.SettingValue, null);
                                }
                                catch (System.Exception)
                                { }
                            }
                        }
                    }

                    if (purgeBuildPath)
                    {
                        var dir = System.IO.Directory.GetParent(path);
                        if (dir != null)
                        {
                            foreach (var f in dir.EnumerateFiles()) f.Delete();
                            foreach (var d in dir.EnumerateDirectories()) d.Delete(true);
                        }
                    }

                    var buildPlayerOptions = new BuildPlayerOptions();
                    buildPlayerOptions.scenes = scenes;
                    buildPlayerOptions.locationPathName = path;
                    buildPlayerOptions.target = settings.BuildTarget;
                    buildPlayerOptions.options = settings.BuildOptions;
#if UNITY_2022_3_OR_NEWER
                    switch (buildPlayerOptions.target)
                    {
                        case BuildTarget.StandaloneOSX:
                            //case BuildTarget.StandaloneOSXUniversal:
                            UnityEditor.OSXStandalone.UserBuildSettings.architecture = UnityEditor.Build.OSArchitecture.x64ARM64; //TODO - make this configurable???
                            break;
                        case BuildTarget.StandaloneOSXIntel:
                        case BuildTarget.StandaloneOSXIntel64:
                            Debug.LogWarning("Attempting to build for OSX Intel which is not supported. Instead targeting StandaloneOSX with x64 support.");
                            buildPlayerOptions.target = BuildTarget.StandaloneOSX;
                            UnityEditor.OSXStandalone.UserBuildSettings.architecture = UnityEditor.Build.OSArchitecture.x64;
                            break;
                        default:
                            //do nothing
                            break;
                    }
#endif
                    var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
                    //var report = BuildPipeline.BuildPlayer(scenes, path, settings.BuildTarget, settings.BuildOptions);

                    bool success = (report != null && report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded);

                    if (cachedInputSettings)
                    {
                        SPTempFolder.ResetFromBackupAndPurge(PATH_INPUTSETTINGS, TEMP_SUFFIX);
                    }

                    if (shouldForceRecompileAtEnd)
                    {
                        this.PrepForCompilation(success ? States.SuccessfulBuild : States.FailedBuild);
                        Debug.Log($"Spacepuppy Build Pipeline - resetting project settings and compiler defines.");

                        SPTempFolder.ResetFromBackupAndPurge(PATH_PROJECTSETTINGS, TEMP_SUFFIX);

#if UNITY_2021_2_OR_NEWER
                        Unity.CodeEditor.CodeEditor.CurrentEditor.SyncAll();
                        CompilationPipeline.RequestScriptCompilation();
                        return; //should technically never reach here
#else
                        CompilationPipeline.RequestScriptCompilation();
                        this.CompleteBuild(settings, success);
#endif
                    }
                    else
                    {
                        SPTempFolder.ResetFromBackupAndPurge(PATH_PROJECTSETTINGS, TEMP_SUFFIX);
                        this.CompleteBuild(settings, success);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                    //TODO - callback
                }
            }

            void CompleteBuild(BuildSettings settings, bool success)
            {
                state = success ? States.SuccessfulBuild : States.FailedBuild;
                try
                {
                    if (success)
                    {
                        //save
                        if ((postBuildOption & PostBuildOption.OpenFolder) != 0)
                        {
                            EditorUtility.RevealInFinder(path);
                        }
                        if ((postBuildOption & PostBuildOption.Run) != 0)
                        {
                            var proc = new System.Diagnostics.Process();
                            proc.StartInfo.FileName = path;
                            proc.Start();
                        }

                        this.callback.bulkBuildSettings?.ContinueBuild(this.callback, true);
                    }
                    else
                    {
                        this.callback.bulkBuildSettings?.ContinueBuild(this.callback, false);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            internal bool TryCompleteBuild()
            {
                var settings = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(this.buildSettingsGuid), typeof(BuildSettings)) as BuildSettings;
                if (!settings) return false;

                var ts = new System.DateTime(this.timestamp);
                var delta = System.DateTime.UtcNow - ts;
                if (delta.TotalMinutes > 15d) return false; //this is to make sure we're not reloading stupid late

                switch (this.state)
                {
                    case States.Building:
                        this.ContinueBuild(settings);
                        return true;
                    case States.SuccessfulBuild:
                        this.CompleteBuild(settings, true);
                        return true;
                    case States.FailedBuild:
                        this.CompleteBuild(settings, false);
                        return true;
                    default:
                        return false;
                }
            }

            void PrepForCompilation(States nextstate)
            {
                this.state = nextstate;
                this.timestamp = System.DateTime.UtcNow.Ticks;
                SPTempFolder.WriteAllText(FILENAME_BUILDCOMMAND, JsonUtility.ToJson(this));
            }

        }

        [System.Serializable]
        internal struct BulkBuildCallback : ISerializationCallbackReceiver
        {
            public BulkBuildSettings bulkBuildSettings;
            public PostBuildOption postBuildOption;
            public int pass;
            public bool failed;
            [SerializeField] private string bulkBuildSettingsGuid;

            public BulkBuildCallback(BulkBuildSettings settings, PostBuildOption option, int pass)
            {
                this.bulkBuildSettings = settings;
                this.postBuildOption = option;
                this.pass = pass;
                this.failed = false;
                this.bulkBuildSettingsGuid = null;
            }

            void ISerializationCallbackReceiver.OnAfterDeserialize()
            {
                this.bulkBuildSettings = !string.IsNullOrEmpty(bulkBuildSettingsGuid) ? AssetDatabase.LoadAssetAtPath<BulkBuildSettings>(AssetDatabase.GUIDToAssetPath(this.bulkBuildSettingsGuid)) : null;
            }

            void ISerializationCallbackReceiver.OnBeforeSerialize()
            {
                if (this.bulkBuildSettings)
                {
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(this.bulkBuildSettings, out this.bulkBuildSettingsGuid, out long _);
                }
                else
                {
                    this.bulkBuildSettingsGuid = null;
                }
            }
        }

#if UNITY_2021_2_OR_NEWER
        [InitializeOnLoadMethod]
        static async void InitializeOnLoad()
        {
            try
            {
                var json = SPTempFolder.Exists(BuildCommand.FILENAME_BUILDCOMMAND) ? SPTempFolder.ReadAllText(BuildCommand.FILENAME_BUILDCOMMAND) : null;
                if (!string.IsNullOrEmpty(json))
                {
                    SPTempFolder.Delete(BuildCommand.FILENAME_BUILDCOMMAND);
                    var command = JsonUtility.FromJson<BuildCommand>(json);

                    Debug.Log("Spacepuppy Build Pipeline - continuing build after recompile.");
                    await Task.Delay(1000); //just wait a little
                    if (!command.TryCompleteBuild())
                    {
                        Debug.LogWarning("Spacepuppy Build Pipeline - there was an error attempting to continue build.");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }
#endif

        #endregion

    }

    [CustomEditor(typeof(BuildSettings), true)]
    public class BuildSettingsEditor : SPEditor
    {

        public const string PROP_BUILDFILENAME = "BuildFileName";
        public const string PROP_BUILDDIR = "BuildDirectory";
        public const string PROP_PURGEBUILDDIR = "PurgeBuildDirectory";
        public const string PROP_VERSION = "Version";
        public const string PROP_BOOTSCENE = "_bootScene";
        public const string PROP_SCENES = "_scenes";
        public const string PROP_BUILDTARGET = "_buildTarget";
        public const string PROP_BUILDOPTIONS = "_buildOptions";
        public const string PROP_INPUTSETTINGS = "_inputSettings";
        public const string PROP_DEFINESYMBOLS = "_defineSymbols";
        public const string PROP_SYMBOLS = "_symbols";
        public const string PROP_PLAYERSETTINGSOVERRIDES = "_playerSettingOverrides";

        #region Fields

        private SceneArrayPropertyDrawer _scenesDrawer = new();
        private UnityEditorInternal.ReorderableList _symbolsListDrawer;

        #endregion

        #region Properties

        public com.spacepuppyeditor.Core.ReorderableArrayPropertyDrawer ScenesDrawer
        {
            get { return _scenesDrawer; }
        }

        #endregion

        #region Methods

        protected override void OnEnable()
        {
            base.OnEnable();

            _scenesDrawer.FormatElementLabel = (p, i, b1, b2) =>
            {
                return string.Format("Scene #{0}", i + 1);
            };
        }

        protected override void OnSPInspectorGUI()
        {
            this.serializedObject.Update();

            this.DrawPropertyField(EditorHelper.PROP_SCRIPT);

            var propFileName = this.serializedObject.FindProperty(PROP_BUILDFILENAME);
            EditorGUILayout.PropertyField(propFileName);
            if (!string.IsNullOrEmpty(propFileName.stringValue))
            {
                var propBuildDir = this.serializedObject.FindProperty(PROP_BUILDDIR);
                propBuildDir.stringValue = SPEditorGUILayout.FolderPathTextfield(EditorHelper.TempContent(propBuildDir.displayName, propBuildDir.tooltip), propBuildDir.stringValue, "Build Directory");

                this.DrawPropertyField(PROP_PURGEBUILDDIR);
            }
            this.DrawPropertyField(PROP_VERSION);

            this.DrawScenes();

            this.DrawBuildOptions();

            this.DrawInputSettings();

            this.DrawPlayerSettingOverrides();

            this.serializedObject.ApplyModifiedProperties();

            //build button
            if (this.serializedObject.isEditingMultipleObjects) return;

            EditorGUILayout.Space();

            this.DrawBuildButtons();
        }

        public virtual void DrawScenes()
        {
            //this.DrawPropertyField(PROP_BOOTSCENE);
            //this.DrawPropertyField(PROP_SCENES);

            this.DrawPropertyField(PROP_BOOTSCENE, "Boot Scene #0", false);

            var propScenes = this.serializedObject.FindProperty(PROP_SCENES);
            var lblScenes = EditorHelper.TempContent(propScenes.displayName, propScenes.tooltip);
            var h = _scenesDrawer.GetPropertyHeight(propScenes, lblScenes);
            _scenesDrawer.OnGUI(EditorGUILayout.GetControlRect(true, h), propScenes, lblScenes);
        }

        public virtual void DrawBuildOptions()
        {
            //TODO - upgrade this to more specialized build options gui
            this.DrawPropertyField(PROP_BUILDTARGET);
            this.DrawPropertyField(PROP_BUILDOPTIONS);

            var propDefineSymbols = this.serializedObject.FindProperty(PROP_DEFINESYMBOLS);
            SPEditorGUILayout.PropertyField(propDefineSymbols);
            if (propDefineSymbols.boolValue)
            {
                if (_symbolsListDrawer == null)
                {
                    _symbolsListDrawer = new UnityEditorInternal.ReorderableList(new List<string>(), typeof(string));
                    _symbolsListDrawer.drawHeaderCallback = (r) => EditorGUI.LabelField(r, "Defined Symbols");
                    _symbolsListDrawer.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                    {
                        if (index < 0 || index >= _symbolsListDrawer.list.Count) return;
                        _symbolsListDrawer.list[index] = EditorGUI.TextField(new Rect(rect.xMin, rect.yMin, rect.width, EditorGUIUtility.singleLineHeight), _symbolsListDrawer.list[index] as string);
                    };
                    _symbolsListDrawer.onAddCallback = (listdrawer) =>
                    {
                        (_symbolsListDrawer.list as List<string>).Add(string.Empty);
                    };
                    _symbolsListDrawer.onRemoveCallback = (listdrawer) =>
                    {
                        if (listdrawer.index >= 0 && listdrawer.index < _symbolsListDrawer.list.Count)
                        {
                            (_symbolsListDrawer.list as List<string>).RemoveAt(_symbolsListDrawer.index);
                        }
                        else if (_symbolsListDrawer.list.Count > 0)
                        {
                            (_symbolsListDrawer.list as List<string>).RemoveAt(_symbolsListDrawer.list.Count - 1);
                        }
                    };
                }

                var propSymbols = this.serializedObject.FindProperty(PROP_SYMBOLS);
                var lst = _symbolsListDrawer.list as List<string>;
                lst.Clear();
                lst.AddRange(propSymbols.stringValue.Split(';'));
                EditorGUI.BeginChangeCheck();
                _symbolsListDrawer.DoLayoutList();
                if (EditorGUI.EndChangeCheck())
                {
                    propSymbols.stringValue = string.Join(';', lst);
                }
            }
        }
        void _symbolsListDrawer_Header(Rect area)
        {

        }
        void _symbolsListDrawer_DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {

        }

        public virtual void DrawInputSettings()
        {
            this.DrawPropertyField(PROP_INPUTSETTINGS);
        }

        public virtual void DrawPlayerSettingOverrides()
        {
            this.DrawPropertyField(PROP_PLAYERSETTINGSOVERRIDES);
        }

        public virtual void DrawBuildButtons()
        {
            if (GUILayout.Button("Build"))
            {
                EditorHelper.Invoke(() => this.DoBuild(BuildSettings.PostBuildOption.OpenFolder));
            }
            if (GUILayout.Button("Build & Run"))
            {
                EditorHelper.Invoke(() => this.DoBuild(BuildSettings.PostBuildOption.OpenFolderAndRun));
            }
            if (GUILayout.Button(EditorHelper.TempContent("Sync To Active Build Target", "Copies the symbols, inputsettings, and scenes to the currently active build target found in File->Build Settings.")))
            {
                this.SyncToGlobalBuild();
            }
        }

        protected virtual void DoBuild(BuildSettings.PostBuildOption postBuildOption)
        {
            var settings = this.target as BuildSettings;
            if (settings != null)
            {
                settings.Build(postBuildOption);
            }
        }




        public virtual void SyncToGlobalBuild()
        {
            var settings = this.target as BuildSettings;
            if (settings == null)
            {
                Debug.LogError("Failed to copy build settings to global.");
                return;
            }

            if (settings.DefineSymbols)
            {
#if UNITY_2021_2_OR_NEWER
                var currentBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
                PlayerSettings.SetScriptingDefineSymbols(currentBuildTarget, settings.Symbols);
#else
                var currentBuildTarget = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(currentBuildTarget, settings.Symbols);
#endif
            }

            if (settings.InputSettings != null)
            {
                settings.InputSettings.ApplyToGlobal();
            }

            var lst = new List<EditorBuildSettingsScene>();
            foreach (var sc in settings.GetScenePaths())
            {
                lst.Add(new EditorBuildSettingsScene(sc, true));
            }
            EditorBuildSettings.scenes = lst.ToArray();
        }

        #endregion

        #region Special Types

        class SceneArrayPropertyDrawer : com.spacepuppyeditor.Core.ReorderableArrayPropertyDrawer
        {

            public SceneArrayPropertyDrawer() : base(typeof(SceneAsset))
            {

            }

            protected override CachedReorderableList GetList(SerializedProperty property, GUIContent label)
            {
                var lst = base.GetList(property, label);
                lst.contextMenuFactoryMethod = () =>
                {
                    var menu = lst.CreateCommonContextMenu();
                    menu.AddItem(new GUIContent("Copy Scenes From Global"), false, () =>
                    {
                        var scenes = EditorBuildSettings.scenes;
                        int cnt = scenes?.Length ?? 0;
                        property.arraySize = cnt;
                        for (int i = 0; i < cnt; i++)
                        {
                            property.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenes[i].path);
                        }
                        property.serializedObject.ApplyModifiedProperties();
                        lst.index = -1;
                    });
                    return menu;
                };
                return lst;
            }

        }

        #endregion

    }

    [CustomPropertyDrawer(typeof(BuildSettings.PlayerSettingOverride))]
    internal class PlayerSettingsOverridePropertyDrawer : PropertyDrawer
    {

        public const string PROP_NAME = "_propertyName";
        public const string PROP_VALUE = "_serializedValue";
        public const string PROP_REF = "_serializedRef";

        private static PropertyInfo[] _knownPlayerSettings;
        private static string[] _knownPlayerSettingPropNames;
        private static GUIContent[] _knownPlayerSettingPropNamesPretty;
        static PlayerSettingsOverridePropertyDrawer()
        {
            _knownPlayerSettings = BuildSettings.GetOverridablePlayerSettings().ToArray();
            _knownPlayerSettingPropNames = (from info in _knownPlayerSettings select info.Name).ToArray();
            _knownPlayerSettingPropNamesPretty = (from info in _knownPlayerSettings select new GUIContent(ObjectNames.NicifyVariableName(info.Name))).ToArray();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var r0 = new Rect(position.xMin, position.yMin, position.width / 2f, position.height);
            var r1 = new Rect(r0.xMax, position.yMin, position.width - r0.width, position.height);

            var propName = property.FindPropertyRelative(PROP_NAME);
            var propValue = property.FindPropertyRelative(PROP_VALUE);
            var propRef = property.FindPropertyRelative(PROP_REF);

            int index = System.Array.IndexOf(_knownPlayerSettingPropNames, propName.stringValue);
            EditorGUI.BeginChangeCheck();
            index = EditorGUI.Popup(r0, GUIContent.none, index, _knownPlayerSettingPropNamesPretty);
            if (EditorGUI.EndChangeCheck())
            {
                if (index >= 0 && index < _knownPlayerSettingPropNames.Length)
                    propName.stringValue = _knownPlayerSettingPropNames[index];
                else
                    propName.stringValue = string.Empty;

                propValue.stringValue = string.Empty;
                propRef.objectReferenceValue = null;
            }

            if (index < 0 || index >= _knownPlayerSettings.Length) return;

            var info = _knownPlayerSettings[index];
            if (info.PropertyType.IsEnum)
            {
                int ei = ConvertUtil.ToInt(propValue.stringValue);
                propValue.stringValue = ConvertUtil.ToInt(EditorGUI.EnumPopup(r1, ConvertUtil.ToEnumOfType(info.PropertyType, ei))).ToString();
                propRef.objectReferenceValue = null;
            }
            else
            {
                var etp = VariantReference.GetVariantType(info.PropertyType);
                switch (etp)
                {
                    case VariantType.Null:
                        propValue.stringValue = string.Empty;
                        propRef.objectReferenceValue = null;
                        break;
                    case VariantType.String:
                        propValue.stringValue = EditorGUI.TextField(r1, propValue.stringValue);
                        propRef.objectReferenceValue = null;
                        break;
                    case VariantType.Boolean:
                        propValue.stringValue = EditorGUI.Toggle(r1, GUIContent.none, ConvertUtil.ToBool(propValue.stringValue)).ToString();
                        propRef.objectReferenceValue = null;
                        break;
                    case VariantType.Integer:
                        propValue.stringValue = EditorGUI.IntField(r1, GUIContent.none, ConvertUtil.ToInt(propValue.stringValue)).ToString();
                        propRef.objectReferenceValue = null;
                        break;
                    case VariantType.Float:
                        propValue.stringValue = EditorGUI.FloatField(r1, GUIContent.none, ConvertUtil.ToSingle(propValue.stringValue)).ToString();
                        propRef.objectReferenceValue = null;
                        break;
                    case VariantType.Double:
                        propValue.stringValue = EditorGUI.DoubleField(r1, GUIContent.none, ConvertUtil.ToDouble(propValue.stringValue)).ToString();
                        propRef.objectReferenceValue = null;
                        break;
                    case VariantType.Vector2:
                        propValue.stringValue = VectorUtil.Stringify(EditorGUI.Vector2Field(r1, GUIContent.none, ConvertUtil.ToVector2(propValue.stringValue)));
                        propRef.objectReferenceValue = null;
                        break;
                    case VariantType.Vector3:
                        propValue.stringValue = VectorUtil.Stringify(EditorGUI.Vector3Field(r1, GUIContent.none, ConvertUtil.ToVector3(propValue.stringValue)));
                        propRef.objectReferenceValue = null;
                        break;
                    case VariantType.Vector4:
                        propValue.stringValue = VectorUtil.Stringify(EditorGUI.Vector4Field(r1, (string)null, ConvertUtil.ToVector4(propValue.stringValue)));
                        propRef.objectReferenceValue = null;
                        break;
                    case VariantType.Quaternion:
                        propValue.stringValue = QuaternionUtil.Stringify(SPEditorGUI.QuaternionField(r1, GUIContent.none, ConvertUtil.ToQuaternion(propValue.stringValue)));
                        propRef.objectReferenceValue = null;
                        break;
                    case VariantType.Color:
                        propValue.stringValue = ConvertUtil.ToInt(EditorGUI.ColorField(r1, ConvertUtil.ToColor(propValue.stringValue))).ToString();
                        propRef.objectReferenceValue = null;
                        break;
                    case VariantType.DateTime:
                        //TODO - should never actually occur
                        propValue.stringValue = string.Empty;
                        propRef.objectReferenceValue = null;
                        break;
                    case VariantType.GameObject:
                    case VariantType.Component:
                    case VariantType.Object:
                        propValue.stringValue = string.Empty;
                        propRef.objectReferenceValue = EditorGUI.ObjectField(r1, GUIContent.none, propValue.objectReferenceValue, info.PropertyType, false);
                        break;
                    case VariantType.LayerMask:
                        propValue.stringValue = SPEditorGUI.LayerMaskField(r1, GUIContent.none, ConvertUtil.ToInt(propValue.stringValue)).ToString();
                        propRef.objectReferenceValue = null;
                        break;
                    case VariantType.Rect:
                        //TODO - should never actually occur
                        propValue.stringValue = string.Empty;
                        propRef.objectReferenceValue = null;
                        break;
                    case VariantType.Numeric:

                        break;
                    case VariantType.Ref:

                        break;
                }
            }
        }

    }

}
