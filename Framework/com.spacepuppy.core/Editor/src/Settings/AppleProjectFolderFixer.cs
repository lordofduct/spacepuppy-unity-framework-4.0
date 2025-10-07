#if UNITY_EDITOR_OSX && UNITY_2022_3_OR_NEWER
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace com.spacepuppyeditor.Settings
{

    /// <remarks>
    /// Adapted from code found here: https://discussions.unity.com/t/is-it-possible-to-group-folders-together-at-the-top-on-unity-for-macos-like-on-windows/228968/5
    /// 
    /// This is rather slap-dash in its approach, but it works. I don't care to optimize it at this point as I'm too busy. I may come back and do so at a later point. 
    //  Since that is true, it defaults false/off. Ideas for future, find an event that tells me a project window opened/closed, also maybe mirror your MacOS settings. -dylane 
    /// </remarks>;
    [InitializeOnLoad]
    public class AppleProjectFolderFixer
    {

        private const string SETTING_MACOS_FOLDERSFIRST = "MacOS.FoldersFirst";
        public static bool MacOS_FoldersFirstInProjectWindow
        {
            get => SpacepuppySettings.ProjectPrefs.GetBool(SETTING_MACOS_FOLDERSFIRST, false);
            set => SpacepuppySettings.ProjectPrefs.SetBool(SETTING_MACOS_FOLDERSFIRST, value);
        }

        static AppleProjectFolderFixer instance;

        static AppleProjectFolderFixer()
        {
            SpacepuppySettingsWindow.DrawExtraEditorSettings += () =>
            {
                EditorGUI.BeginChangeCheck();
                bool foldersFirst = EditorGUILayout.ToggleLeft("Show Folders First In Project Windows (MacOS Only)", MacOS_FoldersFirstInProjectWindow);
                if (EditorGUI.EndChangeCheck())
                {
                    MacOS_FoldersFirstInProjectWindow = foldersFirst;
                    if (foldersFirst)
                    {
                        (instance ??= new()).BeginMonitoring();
                    }
                    else
                    {
                        instance?.StopMonitoring();
                    }
                }
            };

            if (MacOS_FoldersFirstInProjectWindow)
            {
                (instance ??= new()).BeginMonitoring();
            }
        }

        private ProjectBrowserFacade facade = new();

        public bool BeginMonitoring()
        {
            if (!facade.IsReflectionSuccessful) return false;

            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
            return true;
        }

        public void StopMonitoring()
        {
            EditorApplication.update -= OnUpdate;
        }

        void OnUpdate()
        {
            //TODO - THIS IS BRUTE FORCE... we should consider refactoring this hot garbage. But it works for now and I need to get to work. - dylane
            try
            {
                foreach (var browser in facade.GetAllProjectBrowsers())
                {
                    facade.SetFolderFirstForProjectWindow(browser);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to set folders first for all project windows.\n{e}");
                this.StopMonitoring();
            }
        }
        
        private class ProjectBrowserFacade
        {
            public IEnumerable<object> GetAllProjectBrowsers() => ((IList)ProjectBrowsersField?.GetValue(ProjectBrowserType)).Cast<object>() ?? Enumerable.Empty<object>();

            public bool SetFolderFirstForProjectWindow(object projectBrowser)
            {
                if (!this.IsReflectionSuccessful) return false;
                if (projectBrowser == null) return false;

                if (SetOneColumnFolderFirst(projectBrowser))
                    return true;

                if (SetTwoColumnFolderFirst(projectBrowser))
                    return true;

                return false;
            }

            private bool SetOneColumnFolderFirst(object projectBrowser)
            {
                var assetTree = AssetTreeField.GetValue(projectBrowser);
                if (assetTree == null)
                {
                    return false;
                }

                var data = AsserTreeDataProperty.GetValue(assetTree);
                if (data == null)
                {
                    return false;
                }

                if (AssetTreeFoldersFirstProperty.GetValue(data) is true)
                    return true;

                AssetTreeFoldersFirstProperty.SetValue(data, true);
                TreeViewDataSourceRefreshRowsField.SetValue(data, true);
                return true;
            }

            private bool SetTwoColumnFolderFirst(object projectBrowser)
            {
                var listArea = ListAreaField.GetValue(projectBrowser);
                if (listArea == null)
                    return false;

                if (ListAreaFoldersFirstProperty.GetValue(listArea) is true)
                    return true;

                ListAreaFoldersFirstProperty.SetValue(listArea, true);

                TopBarSearchSettingsChangedMethod.Invoke(projectBrowser, new object[] { true });

                return true;
            }

            public ProjectBrowserFacade()
            {
                ProjectBrowserType = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.ProjectBrowser");
                TopBarSearchSettingsChangedMethod = ProjectBrowserType?.GetMethod("TopBarSearchSettingsChanged",
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

                ProjectBrowsersField =
                    ProjectBrowserType?.GetField("s_ProjectBrowsers", BindingFlags.Static | BindingFlags.NonPublic);

                AssetTreeField =
                    ProjectBrowserType?.GetField("m_AssetTree", BindingFlags.Instance | BindingFlags.NonPublic);

                AsserTreeDataProperty = AssetTreeField?.FieldType.GetProperty("data");
                AssetTreeFoldersFirstProperty = Assembly.GetAssembly(typeof(Editor))
                                                        .GetType("UnityEditor.AssetsTreeViewDataSource")
                                                        ?.GetProperty("foldersFirst");

                ListAreaField =
                    ProjectBrowserType?.GetField("m_ListArea", BindingFlags.Instance | BindingFlags.NonPublic);

                ListAreaFoldersFirstProperty = ListAreaField?.FieldType.GetProperty("foldersFirst");
                TreeViewDataSourceRefreshRowsField = Assembly.GetAssembly(typeof(Editor))
                                                            .GetType("UnityEditor.IMGUI.Controls.TreeViewDataSource")
                                                            ?.GetField("m_NeedRefreshRows",
                                                                        BindingFlags.Instance | BindingFlags.NonPublic);

                IsReflectionSuccessful = ProjectBrowsersField != null &&
                                        AssetTreeField != null &&
                                        AsserTreeDataProperty != null &&
                                        AssetTreeFoldersFirstProperty != null &&
                                        ListAreaField != null &&
                                        ListAreaFoldersFirstProperty != null &&
                                        TreeViewDataSourceRefreshRowsField != null;

                if (!IsReflectionSuccessful)
                {
                    Debug.LogWarning("ProjectBrowserFacade could not initialize all fields:\n" +
                                    $"ProjectBrowserType: {ProjectBrowserType}\n" +
                                    $"TopBarSearchSettingsChangedMethod: {TopBarSearchSettingsChangedMethod}\n" +
                                    $"ProjectBrowsersField: {ProjectBrowsersField}\n" +
                                    $"AssetTreeField: {AssetTreeField}\n" +
                                    $"AsserTreeDataProperty: {AsserTreeDataProperty}\n" +
                                    $"AssetTreeFoldersFirstProperty: {AssetTreeFoldersFirstProperty}\n" +
                                    $"ListAreaField: {ListAreaField}\n" +
                                    $"ListAreaFoldersFirstProperty: {ListAreaFoldersFirstProperty}\n" +
                                    $"TreeViewDataSourceRefreshRowsField: {TreeViewDataSourceRefreshRowsField}");
                }
            }

            private readonly Type ProjectBrowserType;
            private readonly MethodInfo TopBarSearchSettingsChangedMethod;
            private readonly FieldInfo ProjectBrowsersField;
            private readonly FieldInfo AssetTreeField;
            private readonly FieldInfo ListAreaField;
            private readonly PropertyInfo AsserTreeDataProperty;
            private readonly PropertyInfo AssetTreeFoldersFirstProperty;
            private readonly FieldInfo TreeViewDataSourceRefreshRowsField;
            private readonly PropertyInfo ListAreaFoldersFirstProperty;
            public readonly bool IsReflectionSuccessful;
        }
    }
}
#endif
