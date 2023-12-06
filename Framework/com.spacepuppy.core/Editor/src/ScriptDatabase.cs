using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using com.spacepuppy.Utils;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

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

        static ScriptDatabase()
        {
            _lookupTable.Clear();
            foreach (var sguid in AssetDatabase.FindAssets("t:script"))
            {
                var guid = new GUID(sguid);
                var path = AssetDatabase.GUIDToAssetPath(guid);
                _lookupTable[guid] = new ScriptInfo()
                {
                    guid = guid,
                    path = path,
                };
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
            if (_lookupTable.TryGetValue(guid, out info))
            {
                if (string.IsNullOrEmpty(info.name))
                {
                    info.name = System.IO.Path.GetFileNameWithoutExtension(info.path);
                    info.type = TypeUtil.FindType(info.name, typeof(UnityEngine.Object));
                    _lookupTable[guid] = info;
                }
                return true;
            }
            else
            {
                info = default;
                return false;
            }
        }

    }

}
