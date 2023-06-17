using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using com.spacepuppy;
using com.spacepuppy.Dynamic;
using com.spacepuppy.Project;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor.Core.Project
{

    public class LazyLoadProxyInspector
    {

        private const string MENU_NAME = "Assets/Create/Spacepuppy/Proxy/Create Lazy Reference";

        [MenuItem(MENU_NAME, priority = int.MinValue + 1)]
        private static void CreateLazyLoadProxyRef()
        {
            var arr = Selection.objects;
            if (arr.Length != 1) return;
            if (!CreateLazyLoadProxyRef_Validate(arr[0])) return;

            var localpath = AssetDatabase.GetAssetPath(arr[0]);
            localpath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(localpath), "lazy." + System.IO.Path.GetFileNameWithoutExtension(localpath) + ".asset");
            if (AssetDatabase.LoadAssetAtPath(localpath, typeof(UnityEngine.Object)) != null)
            {
                EditorUtility.DisplayDialog("Exists!", "Lazy Reference already exists.", "Ok");
                return;
            }

            var basetp = typeof(LazyLoadProxy<>).MakeGenericType(arr[0].GetType());
            var tp = TypeUtil.GetTypesAssignableFrom(basetp).Where(t => AssetDatabase.FindAssets(t.Name).Any(s => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(s), typeof(UnityEngine.Object)) is MonoScript)).FirstOrDefault();

            if (tp == null)
            {
                if (EditorUtility.DisplayDialog("Unknown Lazy Ref Type", $"A Lazy Reference Proxy does not exist for the type {arr[0].GetType().Name}, would you like to create one here?", "Yes", "No"))
                {
                    string scriptname = $"LazyLoad{arr[0].GetType().Name}Proxy";
                    localpath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(localpath), scriptname + ".cs");
                    if (AssetDatabase.LoadAssetAtPath(localpath, typeof(UnityEngine.Object)) != null)
                    {
                        EditorUtility.DisplayDialog("Exists!", "A script with this name already exists.", "Ok");
                        return;
                    }

                    var sb = new StringBuilder();
                    sb.AppendLine("using UnityEngine;");
                    sb.AppendLine("using com.spacepuppy.Project;");
                    if ((arr[0] is GameObject || arr[0] is Component) && TypeUtil.FindType("com.spacepuppy.Spawn.ISpawnable", true) != null)
                    {
                        sb.AppendLine("using com.spacepuppy.Spawn;");
                        sb.AppendLine();
                        sb.AppendLine($"public class {scriptname}: LazyLoadProxy<{arr[0].GetType().FullName}>, ISpawnable");
                        sb.AppendLine("{");
                        sb.AppendLine("    bool ISpawnable.Spawn(out GameObject instance, ISpawnPool pool, Vector3 position, Quaternion rotation, Transform parent)");
                        sb.AppendLine("    {");
                        sb.AppendLine("        if (pool == null || !this.Target.isSet || this.Target.isBroken)");
                        sb.AppendLine("        {");
                        sb.AppendLine("            instance = null;");
                        sb.AppendLine("            return false;");
                        sb.AppendLine("        }");
                        if (arr[0] is GameObject)
                        {
                            sb.AppendLine("        instance = pool.Spawn(this.Target.asset, position, rotation, parent);");
                        }
                        else
                        {
                            sb.AppendLine("        instance = pool.Spawn(this.Target.asset.gameObject, position, rotation, parent);");
                        }
                        sb.AppendLine("        return instance != null;");
                        sb.AppendLine("    }");
                        sb.AppendLine("}");
                        sb.AppendLine();
                    }
                    else
                    {
                        sb.AppendLine();
                        sb.AppendLine($"public class {scriptname}: LazyLoadProxy<{arr[0].GetType().FullName}>");
                        sb.AppendLine("{");
                        sb.AppendLine("    ");
                        sb.AppendLine("}");
                        sb.AppendLine();
                    }

                    localpath = System.IO.Path.Combine(Application.dataPath, localpath.Substring(7));
                    System.IO.File.WriteAllText(localpath, sb.ToString());
                    if (TryGetGuid(scriptname, out string sguid))
                    {
                        sb.Clear();
                        sb.AppendLine("fileFormatVersion: 2");
                        sb.AppendLine($"guid: {sguid}");
                        sb.AppendLine("MonoImporter:");
                        sb.AppendLine("  externalObjects: {}");
                        sb.AppendLine("  serializedVersion: 2");
                        sb.AppendLine("  defaultReferences: []");
                        sb.AppendLine("  executionOrder: 0");
                        sb.AppendLine("  icon: {instanceID: 0}");
                        sb.AppendLine("  userData: ");
                        sb.AppendLine("  assetBundleName: ");
                        sb.AppendLine("  assetBundleVariant: ");
                        sb.AppendLine();
                        System.IO.File.WriteAllText(localpath + ".meta", sb.ToString());
                    }
                    AssetDatabase.Refresh();
                }
                return;
            }

            var so = ScriptableObject.CreateInstance(tp);
            DynamicUtil.SetValueDirect(so, "Configure", arr[0]);
            AssetDatabase.CreateAsset(so, localpath);
            AssetDatabase.Refresh();
        }

        [MenuItem(MENU_NAME, validate = true)]
        private static bool CreateLazyLoadProxyRef_Validate()
        {
            var arr = Selection.objects;
            if (arr.Length != 1) return false;
            return CreateLazyLoadProxyRef_Validate(arr[0]);
        }

        static bool CreateLazyLoadProxyRef_Validate(UnityEngine.Object obj)
        {
            if (!obj || !AssetDatabase.Contains(obj)) return false;
            if (TypeUtil.IsType(obj.GetType(), typeof(MonoScript), typeof(DefaultAsset))) return false; //TODO - add more types not accepted if necessary

            return true;
        }

        static bool TryGetGuid(string lazyLoadTypeName, out string guid)
        {
            switch (lazyLoadTypeName)
            {
                case "LazyLoadGameObjectProxy":
                    guid = "958bdb942c4da0a4e9b2b021bf152b4e";
                    return true;
                case "LazyLoadAudioClipProxy":
                    guid = "9b1cd19a946783c42848d4427dcbf840";
                    return true;
                default:
                    guid = null;
                    return false;
            }
        }

    }

}
