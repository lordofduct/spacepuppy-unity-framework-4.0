using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using com.spacepuppy.Utils;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using UnityEngine.Rendering;

namespace com.spacepuppyeditor
{

    public struct ScriptInfo
    {
        public GUID guid;
        public string name;
        public string path;
        public System.Type type;
    }

    /// <summary>
    /// A database of known scripts within the project, this does not include 3rd party scripts or scripts inside dll's.
    /// </summary>
    [InitializeOnLoad]
    public static class ScriptDatabase
    {

        private static Dictionary<GUID, ScriptInfo> _lookupTable = new Dictionary<GUID, ScriptInfo>();
        private static Dictionary<System.Type, GUID> _guidLookupTable = new();

        static ScriptDatabase()
        {
            _lookupTable.Clear();
            foreach (var sguid in AssetDatabase.FindAssets("t:script"))
            {
                var guid = new GUID(sguid);
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                var type = script ? script.GetClass() : null;
                if (type != null)
                {
                    _lookupTable[guid] = new ScriptInfo()
                    {
                        guid = guid,
                        path = path,
                        name = type.Name,
                        type = type,
                    };
                    _guidLookupTable[type] = guid;
                }
            }

        }

        public static ScriptInfo GUIDToScriptInfo(string sguid) => GUIDToScriptInfo(new GUID(sguid));
        public static ScriptInfo GUIDToScriptInfo(GUID guid)
        {
            if (TryGUIDToScriptInfo(guid, out ScriptInfo info))
            {
                return info;
            }
            else
            {
                return default;
            }
        }

        public static bool TryGUIDToScriptInfo(string sguid, out ScriptInfo info) => TryGUIDToScriptInfo(new GUID(sguid), out info);
        public static bool TryGUIDToScriptInfo(GUID guid, out ScriptInfo info)
        {
            return _lookupTable.TryGetValue(guid, out info);
            /*
            if (_lookupTable.TryGetValue(guid, out info))
            {
                if (string.IsNullOrEmpty(info.name))
                {
                    info.name = System.IO.Path.GetFileNameWithoutExtension(info.path);
                    info.type = TypeUtil.FindType(info.name, typeof(object));
                    _lookupTable[guid] = info;
                    if (info.type != typeof(object) && !_guidLookupTable.ContainsKey(info.type))
                    {
                        _guidLookupTable[info.type] = info.guid;
                    }
                }
                return true;
            }
            else
            {
                info = default;
                return false;
            }
            */
        }

        public static ScriptInfo GetScriptInfo(System.Type scriptType)
        {
            if (TryGetScriptInfo(scriptType, out ScriptInfo info))
            {
                return info;
            }
            else
            {
                return default;
            }
        }

        public static bool TryGetScriptInfo(System.Type scriptType, out ScriptInfo info)
        {
            if (scriptType == null) throw new System.ArgumentNullException(nameof(scriptType));
            if (_guidLookupTable.TryGetValue(scriptType, out GUID guid))
            {
                return TryGUIDToScriptInfo(guid, out info);
            }

            info = default;
            return false;
        }

    }

}
