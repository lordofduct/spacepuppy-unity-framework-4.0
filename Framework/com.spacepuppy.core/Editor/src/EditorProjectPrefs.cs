using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppyeditor
{

    public static class EditorProjectPrefs
    {

        public enum PrefsLocation
        {
            ProjectSettings = 0,
            UserSettings = 1,
            GlobalSettings = 2
        }

        #region Static Interface

        private static string PROJECT_PREFS_PATH => System.IO.Path.Combine(Application.dataPath, @"../ProjectSettings/Spacepuppy.EditorProjectPrefs.xml");
        private static string LOCAL_PREFS_PATH => System.IO.Path.Combine(Application.dataPath, @"../UserSettings/Spacepuppy.EditorProjectPrefs.xml");
        private static string GLOBAL_PREFS_PATH => System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), @"SpacepuppyUnityFramework/Spacepuppy.EditorProjectPrefs.xml");
        private static bool _autoSaveGroupSettingsOnModify = true;
        private static string _projectId;

        private static XmlSettings _project;
        private static XmlSettings _local;
        private static XmlSettings _global;

        static EditorProjectPrefs()
        {
            _project = new XmlSettings(PROJECT_PREFS_PATH, GetOrCreateSharedProjectSettingsXdoc());
            _local = new XmlSettings(PROJECT_PREFS_PATH, GetOrCreateLocalProjectSettingsXdoc());
            _global = new XmlSettings(GLOBAL_PREFS_PATH, GetOrCreateGlobalSettingsXdoc());
        }

        public static ISettings SharedProject => _project;

        public static ISettings LocalProject => _local;

        public static ISettings Global => _global;

        public static bool AutoSaveGroupSettingsOnModify
        {
            get { return _autoSaveGroupSettingsOnModify; }
            set { _autoSaveGroupSettingsOnModify = value; }
        }




        static XDocument GetOrCreateSharedProjectSettingsXdoc()
        {
            string path = PROJECT_PREFS_PATH;
            XDocument xdoc;
            try
            {
                xdoc = XDocument.Load(path);
            }
            catch
            {
                xdoc = null;
            }

            if (xdoc == null)
            {
                _projectId = "SPProj." + System.Guid.NewGuid().ToString();
                xdoc = new XDocument(new XElement("root"));
                xdoc.Root.Add(new XAttribute("projectId", _projectId));

                var sdir = System.IO.Path.GetDirectoryName(path);
                if (!System.IO.Directory.Exists(sdir)) System.IO.Directory.CreateDirectory(sdir);
                xdoc.Save(path);
            }
            else
            {
                var xattrib = xdoc.Root.Attribute("projectId");
                if (xattrib == null)
                {
                    xattrib = new XAttribute("projectId", "SPProj." + System.Guid.NewGuid().ToString());
                    xdoc.Root.Add(xattrib);
                    xdoc.Save(path);
                }
                else if (string.IsNullOrEmpty(xattrib.Value))
                {
                    xattrib.Value = "SPProj." + System.Guid.NewGuid().ToString();
                    xdoc.Save(path);
                }
                _projectId = xattrib.Value;
            }
            return xdoc;
        }

        static XDocument GetOrCreateLocalProjectSettingsXdoc()
        {
            string path = LOCAL_PREFS_PATH;
            XDocument xdoc;
            try
            {
                xdoc = XDocument.Load(path);
            }
            catch
            {
                xdoc = null;
            }

            if (xdoc == null)
            {
                xdoc = new XDocument(new XElement("root"));
                xdoc.Root.Add(new XAttribute("projectId", _projectId));

                var sdir = System.IO.Path.GetDirectoryName(path);
                if (!System.IO.Directory.Exists(sdir)) System.IO.Directory.CreateDirectory(sdir);
                xdoc.Save(path);
            }
            else
            {
                var xattrib = xdoc.Root.Attribute("projectId");
                if (xattrib == null)
                {
                    xattrib = new XAttribute("projectId", "SPProj." + _projectId);
                    xdoc.Root.Add(xattrib);
                    xdoc.Save(path);
                }
                else if (!xattrib.Value?.EndsWith(_projectId) ?? false)
                {
                    xattrib.Value = "SPProj." + _projectId;
                    xdoc.Save(path);
                }
                _projectId = xattrib.Value;
            }
            return xdoc;
        }

        static XDocument GetOrCreateGlobalSettingsXdoc()
        {
            string path = GLOBAL_PREFS_PATH;
            XDocument xdoc;
            try
            {
                xdoc = XDocument.Load(path);
            }
            catch
            {
                xdoc = null;
            }

            if (xdoc == null)
            {
                xdoc = new XDocument(new XElement("root"));

                var sdir = System.IO.Path.GetDirectoryName(path);
                if (!System.IO.Directory.Exists(sdir)) System.IO.Directory.CreateDirectory(sdir);
                xdoc.Save(path);
            }

            return xdoc;
        }

        #endregion

        public interface ISettings
        {
            void DeleteAll();
            void DeleteKey(string key);
            bool HasKey(string key);
            bool GetBool(string key);
            bool GetBool(string key, bool defaultValue);
            int GetInt(string key);
            int GetInt(string key, int defaultValue);
            float GetFloat(string key);
            float GetFloat(string key, float defaultValue);
            string GetString(string key);
            string GetString(string key, string defaultValue);
#if UNITY_2021_3_OR_NEWER
            T GetEnum<T>(string key) where T : struct, System.Enum;
            T GetEnum<T>(string key, T defaultValue) where T : struct, System.Enum;
#else
            T GetEnum<T>(string key) where T : struct, System.IConvertible;
            T GetEnum<T>(string key, T defaultValue) where T : struct, System.IConvertible;
#endif
            void SetBool(string key, bool value);
            void SetInt(string key, int value);
            void SetFloat(string key, float value);
            void SetString(string key, string value);
            void SetEnum<T>(string key, T value) where T : struct, System.IConvertible;
        }

        class XmlSettings : ISettings
        {

            private const string NODE_NAME = "setting";
            private string _path;
            private XDocument _xdoc;

            public XmlSettings(string path, XDocument xdoc)
            {
                _path = path;
                _xdoc = xdoc;
            }

            public void Save()
            {
                _xdoc.Save(_path);
            }


            public void DeleteAll()
            {
                _xdoc.Root.Elements().Remove();
                if (_autoSaveGroupSettingsOnModify) this.Save();
            }

            public void DeleteKey(string key)
            {
                _xdoc.Root.Elements(key).Remove();
                if (_autoSaveGroupSettingsOnModify) this.Save();
            }

            public bool HasKey(string key)
            {
                return _xdoc.Root.Elements(key).Count() > 0;
            }

            public bool GetBool(string key)
            {
                return this.GetBool(key, false);
            }
            public bool GetBool(string key, bool defaultValue)
            {
                var xel = (from x in _xdoc.Root.Elements(NODE_NAME) where x.Attribute("id").Value == key select x).FirstOrDefault();
                if (xel == null) return defaultValue;
                var xattrib = xel.Attribute("value");
                return (xattrib != null) ? ConvertUtil.ToBool(xel.Attribute("value").Value) : defaultValue;
            }

            public int GetInt(string key)
            {
                return this.GetInt(key, 0);
            }
            public int GetInt(string key, int defaultValue)
            {
                var xel = (from x in _xdoc.Root.Elements(NODE_NAME) where x.Attribute("id").Value == key select x).FirstOrDefault();
                if (xel == null) return defaultValue;
                var xattrib = xel.Attribute("value");
                return (xattrib != null) ? ConvertUtil.ToInt(xel.Attribute("value").Value) : defaultValue;
            }

            public float GetFloat(string key)
            {
                return this.GetFloat(key, 0f);
            }
            public float GetFloat(string key, float defaultValue)
            {
                var xel = (from x in _xdoc.Root.Elements(NODE_NAME) where x.Attribute("id").Value == key select x).FirstOrDefault();
                if (xel == null) return defaultValue;
                var xattrib = xel.Attribute("value");
                return (xattrib != null) ? ConvertUtil.ToSingle(xel.Attribute("value").Value) : defaultValue;
            }

            public string GetString(string key)
            {
                return this.GetString(key, string.Empty);
            }
            public string GetString(string key, string defaultValue)
            {
                var xel = (from x in _xdoc.Root.Elements(NODE_NAME) where x.Attribute("id").Value == key select x).FirstOrDefault();
                if (xel == null) return defaultValue;
                var xattrib = xel.Attribute("value");
                return (xattrib != null) ? xel.Attribute("value").Value : defaultValue;
            }

#if UNITY_2021_3_OR_NEWER
            public T GetEnum<T>(string key) where T : struct, System.Enum
#else
            public T GetEnum<T>(string key) where T : struct, System.IConvertible
#endif
            {
                int i = this.GetInt(key);
                return ConvertUtil.ToEnum<T>(i);
            }

#if UNITY_2021_3_OR_NEWER
            public T GetEnum<T>(string key, T defaultValue) where T : struct, System.Enum
#else
            public T GetEnum<T>(string key, T defaultValue) where T : struct, System.IConvertible
#endif
            {
                int i = this.GetInt(key, System.Convert.ToInt32(defaultValue));
                return ConvertUtil.ToEnum<T>(i, defaultValue);
            }


            public void SetBool(string key, bool value)
            {
                this.SetValue(key, value);
            }

            public void SetInt(string key, int value)
            {
                this.SetValue(key, value);
            }

            public void SetFloat(string key, float value)
            {
                this.SetValue(key, value);
            }

            public void SetString(string key, string value)
            {
                this.SetValue(key, value);
            }

            public void SetEnum<T>(string key, T value) where T : struct, System.IConvertible
            {
                this.SetInt(key, System.Convert.ToInt32(value));
            }



            private void SetValue(string key, object value)
            {
                var sval = StringUtil.ToLower(ConvertUtil.ToString(value));

                var xel = (from x in _xdoc.Root.Elements(NODE_NAME) where x.Attribute("id").Value == key select x).FirstOrDefault();
                if (xel == null)
                {
                    xel = new XElement(NODE_NAME, new XAttribute("id", key), new XAttribute("value", sval));
                    _xdoc.Root.Add(xel);
                }
                else
                {
                    var xattrib = xel.Attribute("value");
                    if (xattrib != null)
                        xattrib.Value = sval;
                    else
                        xel.Add(new XAttribute("value", sval));
                }
                if (_autoSaveGroupSettingsOnModify) this.Save();
            }

        }

    }

}