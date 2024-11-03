using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using com.spacepuppy;

namespace com.spacepuppyeditor
{

    [InitializeOnLoad()]
    public static class SPPackage
    {

        static readonly GUID VersionFileAssetGuid = new GUID("9cae18b92260f984ea83265e5eb2e57d");
        static readonly Dictionary<GUID, string> GlobalDefinesTable = new()
        {
            //{ new GUID("d8fc4124cad14a249a28dcf3b0ba98cb"), "SP_DATABINDING" },
            //{ new GUID("1504d8b5d14ee4540a06b9a7c3aac4a6"), "SP_UI" },
        };

        static SPPackage()
        {
            if (SpacepuppySettings.AutoSyncGlobalDefines)
            {
                int hash = CalculatePackageHash();
                int lasthash = EditorProjectPrefs.LocalProject.GetInt("AutoSyncPackageHash", 0);
                if (hash != lasthash)
                {
                    SyncGlobalDefineSymbolsForSpacepuppy();
                    EditorProjectPrefs.LocalProject.SetInt("AutoSyncPackageHash", hash);
                }
            }
        }

        public static System.Version GetPackageVersion()
        {
            var versionPath = AssetDatabase.GUIDToAssetPath(VersionFileAssetGuid);
            if (!string.IsNullOrWhiteSpace(versionPath) && System.Version.TryParse(File.ReadAllText(versionPath), out System.Version v))
            {
                return v;
            }

            return new System.Version(4, 0, 0, -1); //-1 signifies a failure to read the version
        }


        [MenuItem(SPMenu.MENU_NAME_SETTINGS + "/Export Unity Package", priority = SPMenu.MENU_PRIORITY_SETTINGS)]
        public static void ExportAsUnityPackage()
        {
            var versionPath = AssetDatabase.GUIDToAssetPath(VersionFileAssetGuid);
            if (string.IsNullOrWhiteSpace(versionPath))
            {
                EditorUtility.DisplayDialog("Spacepuppy Framework Package Exporter", "Failed to locate version.txt for Spacepuppy Framework. Can not export the package.", "OK");
                return;
            }

            var packageFolder = versionPath.Substring(0, versionPath.Length - (Path.GetFileName(versionPath).Length + 1)); //we do it this weird way so that we don't lose the path formatting of unity
                                                                                                                           //var includedAssets = AssetDatabase.GetAllAssetPaths().Where(s => s.StartsWith(packageFolder)).ToArray();

            var outputPath = EditorProjectPrefs.LocalProject.GetString("LastExportPackageFilePath", string.Empty);
            outputPath = EditorUtility.SaveFilePanel("Spacepuppy Framework Package Exporter", 
                                                     (!string.IsNullOrWhiteSpace(outputPath) ? Path.GetDirectoryName(outputPath) : string.Empty), 
                                                     (!string.IsNullOrWhiteSpace(outputPath) ? Path.GetFileName(outputPath) : string.Empty), 
                                                     "unitypackage");
            if (string.IsNullOrEmpty(outputPath))
            {
                return;
            }

            var sversion = File.ReadAllText(versionPath);
            if (System.Version.TryParse(sversion, out System.Version v))
            {
                v = new System.Version(v.Major, v.Minor, v.Build, v.Revision + 1);
            }
            else
            {
                v = new System.Version(4, 0, 0, 1000);
            }
            AssetDatabase.ReleaseCachedFileHandles();
            File.WriteAllText(versionPath, v.ToString());

            EditorProjectPrefs.LocalProject.SetString("LastExportPackageFilePath", outputPath);
            AssetDatabase.ExportPackage(packageFolder, outputPath, ExportPackageOptions.Recurse);
        }

        public static void SyncGlobalDefineSymbolsForSpacepuppy()
        {

            const string RSP_PATH = "Assets/csc.rsp";

            List<string> lines;
            if (File.Exists(RSP_PATH))
            {
                lines = new List<string>(File.ReadAllLines(RSP_PATH));
            }
            else
            {
                lines = new List<string>();
            }

            bool changed = false;
            foreach (var pair in GlobalDefinesTable)
            {
                string sdef = $"-define:{pair.Value}";
                int i = lines.IndexOf(sdef);
                if (string.IsNullOrWhiteSpace(AssetDatabase.GUIDToAssetPath(pair.Key)))
                {
                    //remove from list if it exists
                    if (i >= 0)
                    {
                        changed = true;
                        lines.RemoveAt(i);
                    }
                }
                else if (i < 0)
                {
                    //add to list
                    changed = true;
                    lines.Add(sdef);
                }
            }

            if (changed)
            {
                File.WriteAllLines(RSP_PATH, lines);
            }
        }

        static int CalculatePackageHash()
        {
            int i = 0;
            foreach (var uguid in GlobalDefinesTable.Keys)
            {
                if (!string.IsNullOrWhiteSpace(AssetDatabase.GUIDToAssetPath(uguid)))
                {
                    var guid = (SerializableGuid)uguid.ToGuid();
                    i ^= guid.a; //simple hashing using the high 32-bits of the guids, this ensures that the hash is the same every time and is fast to calculate (as opposed to something like MD5)
                }
            }
            return i;
        }

    }

}
