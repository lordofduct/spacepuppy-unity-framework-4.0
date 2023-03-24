using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;

namespace com.spacepuppyeditor.Windows
{

    public static class RecursiveAssetDuplicator
    {

        [MenuItem("Assets/Recursively Duplicate Folder")]
        private static void RecursivelyDuplicateMenuEntry()
        {
            RecursivelyDuplicateFolder(GetRootFolderPath(Selection.objects.FirstOrDefault()));
        }

        [MenuItem("Assets/Recursively Duplicate Folder", true)]
        private static bool RecursivelyDuplicateMenuEntryValidate()
        {
            var path = GetRootFolderPath(Selection.objects.FirstOrDefault());
            return !string.IsNullOrEmpty(path) && Directory.Exists(path);
        }

        private static string GetRootFolderPath(UnityEngine.Object asset)
        {
            if (asset == null) return string.Empty;
            var path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path) || !path.StartsWith("Assets/")) return string.Empty;
            return Path.Combine(Application.dataPath, path.Substring(7));
        }

        public static bool RecursivelyDuplicateFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath)) return false;

            var assetPathInfo = new DirectoryInfo(Application.dataPath);
            var folderPathInfo = new DirectoryInfo(folderPath);
            if (!folderPathInfo.FullName.StartsWith(assetPathInfo.FullName)) return false;

            var assetDBRelativeFolderRoot = "Assets\\" + Path.GetRelativePath(assetPathInfo.FullName, folderPathInfo.FullName);
            var newAssetDBRelativeFolderName = AssetDatabase.GenerateUniqueAssetPath(assetDBRelativeFolderRoot);
            var table = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);

            foreach (var filepath in Directory.EnumerateFiles(folderPathInfo.FullName, "*.*", SearchOption.AllDirectories))
            {
                if (filepath.EndsWith(".meta")) continue;
                if (Directory.Exists(filepath)) continue;

                string oldpath = assetDBRelativeFolderRoot + filepath.Substring(folderPathInfo.FullName.Length);
                string newpath = newAssetDBRelativeFolderName + filepath.Substring(folderPathInfo.FullName.Length);
                string sguid = AssetDatabase.AssetPathToGUID(oldpath);

                var newdir = Path.GetDirectoryName(newpath);
                if (!Directory.Exists(newdir)) Directory.CreateDirectory(newdir);

                if (AssetDatabase.CopyAsset(oldpath, newpath))
                {
                    table[sguid] = AssetDatabase.AssetPathToGUID(newpath);
                }
                else
                {
                    Debug.Log("FAILED TO COPY: " + oldpath);
                }
            }

            var sb = new StringBuilder();
            var rx = new Regex("guid: (?<guid>[a-zA-Z0-9]{32})");
            foreach (var filepath in Directory.EnumerateFiles(newAssetDBRelativeFolderName, "*.*", SearchOption.AllDirectories))
            {
                if (Directory.Exists(filepath)) continue;

                if (filepath.EndsWith(".asset") || filepath.EndsWith(".prefab") || filepath.EndsWith(".unity") || filepath.EndsWith(".mat"))
                {
                    sb.Clear();
                    string ln;
                    bool altered = false;
                    using (var reader = new StreamReader(filepath))
                    {
                        while ((ln = reader.ReadLine()) != null)
                        {
                            foreach (Match m in rx.Matches(ln))
                            {
                                if (!m.Success) continue;
                                var sguid = m.Groups["guid"].Value;
                                if (!table.ContainsKey(sguid)) continue;

                                ln = ln.Substring(0, m.Index) + $"guid: {table[sguid]}" + ln.Substring(m.Index + m.Length);
                                altered = true;
                            }

                            sb.AppendLine(ln);
                        }
                    }

                    if (altered)
                    {
                        File.WriteAllText(filepath, sb.ToString());
                    }
                }
            }

            return true;
        }
        

    }

}
