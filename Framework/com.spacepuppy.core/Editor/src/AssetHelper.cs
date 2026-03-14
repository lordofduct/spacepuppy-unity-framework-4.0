using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using com.spacepuppy.Utils;

namespace com.spacepuppyeditor
{

    public static class AssetHelper
    {

        public static string ProjectPath
        {
            get
            {
                return Path.GetDirectoryName(Application.dataPath);
            }
        }

        public static string GetRelativeResourcePath(string spath)
        {
            if (string.IsNullOrEmpty(spath)) return string.Empty;

            int i = spath.IndexOf("Resources") + 9;
            if (i >= spath.Length) return string.Empty;

            spath = spath.Substring(i);
            spath = Path.Combine(Path.GetDirectoryName(spath), Path.GetFileNameWithoutExtension(spath)).Replace(@"\", "/");
            return spath.EnsureNotStartWith("/");
        }

        public static void MoveFolder(string fromPath, string toPath)
        {
            fromPath = Path.Combine(AssetHelper.ProjectPath, fromPath);
            toPath = Path.Combine(AssetHelper.ProjectPath, toPath);

            if (Directory.Exists(fromPath) && !Directory.Exists(toPath))
            {
                Directory.Move(fromPath, toPath);
                File.Move(fromPath + ".meta", toPath + ".meta");
            }
            else
            {
                throw new DirectoryNotFoundException();
            }
        }

        public static string GetSelectedPath()
        {
            string path = "Assets";

            foreach (var obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
                {
                    path = System.IO.Path.GetDirectoryName(path);
                    break;
                }
            }

            return path;
        }

    }

    #region UnsafeAssetDatabase

    internal class SpaceuppyAssetDatabase : AssetPostprocessor
    {
        static List<AssetInfo> _assets = new();
        static System.Threading.CancellationTokenSource _cancellationSource;

        [InitializeOnLoadMethod]
        static void InitializeDatabase()
        {
            _cancellationSource?.Cancel();
            _cancellationSource = new();

            var token = _cancellationSource.Token;
            _assets.Clear();
            _assets.AddRange(AssetDatabase.FindAssets("a:assets").Select(s =>
            {
                var spath = AssetDatabase.GUIDToAssetPath(s);
                return new AssetInfo() { guid = s, name = System.IO.Path.GetFileNameWithoutExtension(spath), path = spath, type = AssetDatabase.GetMainAssetTypeAtPath(spath) };
            }));
        }
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            InitializeDatabase();
        }

        public static IEnumerable<AssetInfo> GetAssetInfos() => _assets;

    }

    internal class AssetInfo
    {
        public string guid;
        public string name;
        public string path;
        public System.Type type;
        private System.Type[] _alternativeTypes;

        /// <summary>
        /// Returns list of component types of asset if its a Prefab/GameObject.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<System.Type> GetAlternativeTypes() => _alternativeTypes != null ? _alternativeTypes : this.SyncAltTypes();

        public bool SupportsType(System.Type tp)
        {
            if (TypeUtil.IsType(type, tp)) return true;

            if (ComponentUtil.IsComponentType(tp) || tp.IsInterface)
            {
                foreach (var alt in this.GetAlternativeTypes())
                {
                    if (TypeUtil.IsType(alt, tp)) return true;
                }
            }

            return false;
        }

        System.Type[] SyncAltTypes()
        {
            if (type == typeof(GameObject) && (path?.EndsWith(".prefab") ?? false))
            {
                var asset = this.GetAsset() as GameObject;
                _alternativeTypes = asset ? asset.GetComponents().Select(o => o.GetType()).ToArray() : ArrayUtil.Empty<System.Type>();
            }
            else
            {
                _alternativeTypes = ArrayUtil.Empty<System.Type>();
            }
            return _alternativeTypes;
        }

        public UnityEngine.Object GetAsset() => !string.IsNullOrEmpty(path) ? AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) : null;
    }

    #endregion

}
