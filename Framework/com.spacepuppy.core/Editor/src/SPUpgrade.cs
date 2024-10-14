using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using StringComparison = System.StringComparison;

using com.spacepuppy;
using com.spacepuppy.Collections;
using com.spacepuppy.Utils;
using com.spacepuppyeditor.Windows;

namespace com.spacepuppyeditor
{

    public static class SPUpgrade
    {

        public const string MENU_NAME_UPGRADE = SPMenu.MENU_NAME_ROOT + "/Upgrade";

        [MenuItem(MENU_NAME_UPGRADE + "/Upgrade Asset YAML", priority = 1000)]
        public static void UpgradeAssetYAML()
        {
            UpgradeAssetYAML(TypeCache.GetMethodsWithAttribute<YamlUpgradeCallbackAttribute>());
        }
        public static void UpgradeAssetYAML(Assembly assembly)
        {
            if (assembly == null) throw new System.ArgumentNullException(nameof(assembly));

#if UNITY_2022_3_OR_NEWER
            UpgradeAssetYAML(TypeCache.GetMethodsWithAttribute<YamlUpgradeCallbackAttribute>(assembly.FullName));
#else
            var methods = TypeCache.GetMethodsWithAttribute<YamlUpgradeCallbackAttribute>();
            UpgradeAssetYAML(methods.Where(m => m.DeclaringType.Assembly == assembly).ToList());
#endif
        }
        static async void UpgradeAssetYAML(IList<MethodInfo> methods)
        {
            var lst = new List<(ScriptInfo, System.Func<string, bool>)>(methods.Count);
            foreach (var m in methods)
            {
                if (!m.IsStatic)
                {
                    Debug.Log($"Malformed YamlUpgradeCallback: {m.DeclaringType.Name}.{m.Name} [must be static]");
                    continue;
                }

                try
                {
                    foreach (var attrib in m.GetCustomAttributes<YamlUpgradeCallbackAttribute>())
                    {
                        var tp = attrib.type;
                        if (tp == null) continue;
                        if (tp != null && ScriptDatabase.TryGetScriptInfo(tp, out ScriptInfo info))
                        {
                            var del = m.CreateDelegate(typeof(System.Func<string, bool>)) as System.Func<string, bool>;
                            if (del != null)
                            {
                                lst.Add((info, del));
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.Log($"Malformed YamlUpgradeCallback: {m.DeclaringType.Name}.{m.Name} [must match bool(string assetpath)]");
                }
            }

            var query = new AssetSearchWindow.SearchStringQuery()
            {
                AssetTypes = AssetSearchWindow.AssetTypes.All ^ AssetSearchWindow.AssetTypes.AssemblyDefinitions,
                UseRegex = false,
            };

            foreach (var entry in lst)
            {
                query.SearchString = entry.Item1.guid.ToString();
                await query.Search();

                foreach (var targ in query.OutputRefs)
                {
                    try
                    {
                        if (entry.Item2(targ.path))
                        {
                            Debug.Log($"Processed YAML Upgrade for {entry.Item1.name}: {targ.path}", targ.obj);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }

            Debug.Log("Upgrade Asset YAML Completed.");
        }



        #region Special Types

        /// <summary>
        /// Mark a static method with shape 'static bool Callback(string assetpath)' to 
        /// process that file for upgrade if it was found to reference 'type'. 
        /// Return true from said method if the file was modified.
        /// </summary>
        [System.AttributeUsage(System.AttributeTargets.Method)]
        public class YamlUpgradeCallbackAttribute : System.Attribute
        {
            /// <summary>
            /// The type for which this method upgrades the asset yaml for.
            /// </summary>
            public System.Type type;

            public YamlUpgradeCallbackAttribute(System.Type type)
            {
                this.type = type;
            }
        }

        #endregion

    }

    /// <summary>
    /// Acts similar to a TextReader for asset files but with bonus helper methods for editing them. 
    /// ReadXXX - behave like ReadLine until condition is satisfied. 
    /// SeekXXX - behaves similar to ReadXXX but leaves the line number just before the line located 
    ///     so you can modify it (calling Delete, Replace, Insert, etc)
    /// </summary>
    public class AssetRawTextReader : ISPDisposable
    {

        const string YAML_OBJECT_HEADER = "--- !u!";

        #region Fields

        private StreamReader _reader;
        private Deque<string> _lines;
        private int _position;
        private bool _reachedEnd;

        #endregion

        #region CONSTRUCTOR

        public AssetRawTextReader(string assetpath)
        {
            //_reader = new StreamReader(assetpath);
            //_lines = new(1024);
            _position = 0;
            _reachedEnd = false;

            //NOTE - at this time for whatever reason I'm having a sharing violation with when disposing the reader above during WriteAllLines.
            //So for now I'm going to forego the reader all together and fix said problem later.
            var arr = File.ReadAllLines(assetpath);
            _lines = new(arr);
            _reachedEnd = true;
        }

        ~AssetRawTextReader()
        {
            this.Dispose(); //just in case some dummy forgot to dispose
        }

        #endregion

        #region Properties

        public string this[int line]
        {
            get
            {
                if (line < 0)
                {
                    throw new System.IndexOutOfRangeException(nameof(line));
                }
                else if (line < _lines.Count)
                {
                    return _lines[line];
                }
                else
                {
                    this.BufferTo(line);
                    return line < _lines.Count ? _lines[line] : null;
                }
            }
            set
            {
                if (line < 0)
                {
                    throw new System.IndexOutOfRangeException(nameof(line));
                }

                this.BufferTo(line);
                if (line < _lines.Count) _lines[line] = value;
            }
        }

        public int LineNumber
        {
            get => _position;
            set
            {
                value = System.Math.Max(0, value);
                this.BufferTo(value);
                _position = System.Math.Min(value, _lines.Count);
            }
        }

        public string CurrentLine => this[_position];

        /// <summary>
        /// This is the current number of lines buffered into memory. 
        /// If 'ReachedToEnd' returns true, this is the actual line count.
        /// </summary>
        public int BufferedLineCount => _lines.Count;

        /// <summary>
        /// Returns true once all of the lines on disk have been read.
        /// </summary>
        public bool ReachedEnd => _reachedEnd;

        public bool AtEnd => _reachedEnd && _position >= _lines.Count;

        #endregion

        #region Standard Read/Write Methods

        public void BufferToEnd()
        {
            if (this.IsDisposed || _reachedEnd) return;

            string ln;
            while ((ln = _reader.ReadLine()) != null)
            {
                _lines.Push(ln);
            }

            _reader?.Dispose();
            _reader = null;
            _reachedEnd = true;
        }

        /// <summary>
        /// Reads upto line number without moving the current position in the reader.
        /// </summary>
        /// <param name="line"></param>
        public void BufferTo(int line)
        {
            if (this.IsDisposed || _reachedEnd) return;

            while (_lines.Count < line)
            {
                var ln = _reader.ReadLine();
                if (ln == null)
                {
                    _reachedEnd = true;
                    return;
                }

                _lines.Push(ln);
            }
        }

        public void ReadToEnd()
        {
            if (this.IsDisposed) return;
            if (_reachedEnd)
            {
                _position = _lines.Count;
                return;
            }

            string ln;
            while ((ln = _reader.ReadLine()) != null)
            {
                _lines.Push(ln);
            }

            _reader?.Dispose();
            _reader = null;
            _reachedEnd = true;
            _position = _lines.Count;
        }

        public string ReadLine()
        {
            if (this.IsDisposed) return null;

            if (_position < _lines.Count)
            {
                return _lines[_position++];
            }
            else if (_reachedEnd)
            {
                return null;
            }
            else
            {
                var ln = _reader.ReadLine();
                if (ln == null)
                {
                    _reachedEnd = true;
                    return null;
                }

                _position = _lines.Count;
                _lines.Push(ln);
                return ln;
            }
        }

        public string SeekBackwards()
        {
            if (this.IsDisposed) return null;

            if (_position > 0)
            {
                _position--;
                return this[_position];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Should call buffer before calling this.
        /// </summary>
        /// <returns></returns>
        bool DeleteLineUnbuffered()
        {
            if (_position >= 0 && _position < _lines.Count)
            {
                _lines.RemoveAt(_position);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool DeleteLine()
        {
            if (this.IsDisposed) return false;

            this.BufferTo(_position);
            if (DeleteLineUnbuffered())
            {
                _position = System.Math.Min(_position, _lines.Count);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Delete the next 'count' lines.
        /// </summary>
        /// <param name="count">Number of lines to delete.</param>
        /// <returns>Returns the number of lines actually deleted.</returns>
        public int DeleteLines(int count)
        {
            if (this.IsDisposed) return 0;

            this.BufferTo(_position + count - 1);
            int cnt = 0;
            while (this.DeleteLineUnbuffered())
            {
                cnt++;
            }
            return cnt;
        }

        /// <summary>
        /// Replace the current line with text and increment forward 1 line.
        /// </summary>
        /// <param name="text"></param>
        public void ReplaceLine(string text)
        {
            if (this.IsDisposed) return;

            this.BufferTo(_position);
            if (_position < _lines.Count)
            {
                _lines[_position] = text ?? string.Empty;
                _position++;
            }
            else
            {
                _lines.Push(text ?? string.Empty);
                _position = _lines.Count;
            }
        }

        public void ReplaceLines(IEnumerable<string> lines)
        {
            if (this.IsDisposed) return;

            foreach (var line in lines)
            {
                this.BufferTo(_position);
                if (_position < _lines.Count)
                {
                    _lines[_position] = line ?? string.Empty;
                    _position++;
                }
                else
                {
                    _lines.Push(line ?? string.Empty);
                    _position = _lines.Count;
                }
            }
        }

        /// <summary>
        /// Inserts a line at the current line number and increments forward.
        /// </summary>
        /// <param name="line"></param>
        public void InsertLine(string line)
        {
            if (this.IsDisposed) return;

            if (_position < _lines.Count)
            {
                _lines.Insert(_position, line ?? string.Empty);
                _position++;
            }
            else
            {
                _lines.Push(line ?? string.Empty);
                _position = _lines.Count;
            }
        }

        public void InsertLines(IEnumerable<string> lines)
        {
            if (this.IsDisposed) return;

            foreach (var line in lines)
            {
                this.InsertLine(line);
            }
        }

        public void WriteAllLines(string path)
        {
            if (this.IsDisposed) return;

            this.BufferToEnd();
            File.WriteAllLines(path, _lines);
        }

        #endregion

        #region Complex Read/Write Methods

        public string SeekUntilStartsWith(string text, StringComparison comparison = default)
        {
            var ln = ReadUntilStartsWith(text, comparison);
            if (ln != null) _position = System.Math.Max(0, _position - 1);
            return ln;
        }
        public string ReadUntilStartsWith(string text, StringComparison comparison = default)
        {
            string ln;
            while ((ln = this.ReadLine()) != null)
            {
                if (ln.StartsWith(text, comparison))
                {
                    return ln;
                }
            }
            return null;
        }


        public string SeekUntilContains(string text, StringComparison comparison = default)
        {
            var ln = ReadUntilContains(text, comparison);
            if (ln != null) _position = System.Math.Max(0, _position - 1);
            return ln;
        }
        public string ReadUntilContains(string text, StringComparison comparison = default)
        {
            string ln;
            while ((ln = this.ReadLine()) != null)
            {
                if (ln.Contains(text, comparison))
                {
                    return ln;
                }
            }
            return null;
        }

        public string SeekUntilEquals(string text, StringComparison comparison = default)
        {
            var ln = ReadUntilEquals(text, comparison);
            if (ln != null) _position = System.Math.Max(0, _position - 1);
            return ln;
        }
        public string ReadUntilEquals(string text, StringComparison comparison = default)
        {
            string ln;
            while ((ln = this.ReadLine()) != null)
            {
                if (ln.Equals(text, comparison))
                {
                    return ln;
                }
            }
            return null;
        }

        public string SeekUntilEquals(string text, IEqualityComparer<string> comparer)
        {
            var ln = ReadUntilEquals(text, comparer);
            if (ln != null) _position = System.Math.Max(0, _position - 1);
            return ln;
        }
        public string ReadUntilEquals(string text, IEqualityComparer<string> comparer)
        {
            if (comparer == null) throw new System.ArgumentNullException(nameof(comparer));

            string ln;
            while ((ln = this.ReadLine()) != null)
            {
                if (comparer.Equals(ln, text))
                {
                    return ln;
                }
            }
            return null;
        }

        public Match SeekUntilMatch(Regex rx)
        {
            var m = ReadUntilMatch(rx);
            if (m != null) _position = System.Math.Max(0, _position - 1);
            return m;
        }
        public Match ReadUntilMatch(Regex rx)
        {
            string ln;
            while ((ln = this.ReadLine()) != null)
            {
                if (rx.IsMatch(ln))
                {
                    return rx.Match(ln);
                }
            }
            return null;
        }

        public string SeekToNextObject()
        {
            return this.ReadUntilStartsWith(YAML_OBJECT_HEADER, StringComparison.Ordinal);
        }

        public string SeekUntilTransform()
        {
            var ln = this.ReadUntilStartsWith("Transform:", StringComparison.Ordinal);
            if (ln != null) _position = System.Math.Max(0, _position - 1);
            return ln;
        }
        public string ReadUntilTransform()
        {
            return this.ReadUntilStartsWith("Transform:", StringComparison.Ordinal);
        }

        public string SeekUntilScript(System.Type tp)
        {
            var ln = this.ReadUntilScript(tp);
            if (ln != null) _position = System.Math.Max(0, _position - 1);
            return ln;
        }
        public string ReadUntilScript(System.Type tp)
        {
            var guid = ScriptDatabase.GetScriptInfo(tp).guid;
            if (guid.Empty())
            {
                this.ReadToEnd();
                return null;
            }

            var sguid = guid.ToString();
            var rx = new Regex(@"^\s*m_Script: {fileID: (?<fileid>-?\d+), guid: " + sguid + @", type: 3}\s*$");
            var m = this.ReadUntilMatch(rx);
            if (m != null)
            {
                return this.SeekToBeginningOfCurrentObject();
            }
            return null;
        }

        public string SeekUntilAnyScript(params System.Type[] types)
        {
            var ln = this.ReadUntilAnyScript(types);
            if (ln != null) _position = System.Math.Max(0, _position - 1);
            return ln;
        }
        public string ReadUntilAnyScript(params System.Type[] types)
        {
            using (var hash = TempCollection.GetSet<string>(System.StringComparer.OrdinalIgnoreCase))
            {
                if (types?.Length > 0)
                {
                    foreach (var tp in types)
                    {
                        var guid = ScriptDatabase.GetScriptInfo(tp).guid;
                        if (!guid.Empty())
                        {
                            hash.Add(guid.ToString());
                        }
                    }
                }
                if (hash.Count == 0)
                {
                    this.ReadToEnd();
                    return null;
                }

                var rx = new Regex(@"^\s*m_Script: {fileID: (?<fileid>-?\d+), guid: (?<guid>([a-zA-Z0-9]+)), type: 3}\s*$");
                Match m;
                while ((m = this.ReadUntilMatch(rx)) != null)
                {
                    if (hash.Contains(m.Groups["guid"].Value))
                    {
                        return this.SeekToBeginningOfCurrentObject();
                    }
                }
                return null;
            }
        }

        public bool SeekUntilSerializedField(string name, out SerailizedFieldResult result, bool stopAtEndOfCurrentObject = false)
        {
            var b = this.ReadUntilSerializedField(name, out result, stopAtEndOfCurrentObject);
            if (!this.AtEnd) _position = System.Math.Max(0, _position - 1);
            return b;
        }
        public bool ReadUntilSerializedField(string name, out SerailizedFieldResult result, bool stopAtEndOfCurrentObject = false)
        {
            var rx = new Regex(@"^\s*" + name + @":(?<data>.*)$");
            string ln;
            while ((ln = this.ReadLine()) != null)
            {
                if (rx.IsMatch(ln))
                {
                    var m = rx.Match(ln);
                    result = new SerailizedFieldResult()
                    {
                        line = m.Value,
                        fieldName = name,
                        fieldData = m.Groups["data"]?.Value ?? string.Empty,
                    };
                    return true;
                }
                if (stopAtEndOfCurrentObject && ln.StartsWith(YAML_OBJECT_HEADER)) break;
            }

            result = default;
            return false;
        }


        public string SeekBackwardsUntilStartsWith(string text, StringComparison comparison = default)
        {
            string ln;
            while ((ln = this.SeekBackwards()) != null)
            {
                if (ln.StartsWith(text, comparison)) return ln;
            }
            return null;
        }

        public string SeekBackwardsUntilContains(string text, StringComparison comparison = default)
        {
            string ln;
            while ((ln = this.SeekBackwards()) != null)
            {
                if (ln.Contains(text, comparison)) return ln;
            }
            return null;
        }

        public string SeekBackwardsUntilEquals(string text, StringComparison comparison = default)
        {
            string ln;
            while ((ln = this.SeekBackwards()) != null)
            {
                if (ln.Equals(text, comparison)) return ln;
            }
            return null;
        }

        public string SeekBackwardsUntilEquals(string text, IEqualityComparer<string> comparer)
        {
            if (comparer == null) throw new System.ArgumentNullException(nameof(comparer));

            string ln;
            while ((ln = this.SeekBackwards()) != null)
            {
                if (comparer.Equals(ln, text)) return ln;
            }
            return null;
        }

        public Match SeekBackwardsUntilMatch(Regex rx)
        {
            string ln;
            while ((ln = this.SeekBackwards()) != null)
            {
                if (rx.IsMatch(ln))
                {
                    return rx.Match(ln);
                }
            }
            return null;
        }

        public string SeekToBeginningOfCurrentObject()
        {
            string ln = this.SeekBackwardsUntilStartsWith(YAML_OBJECT_HEADER, StringComparison.Ordinal);
            if (ln != null) _position++;
            return ln;
        }


        #endregion

        #region IDisposable Interface

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.IsDisposed = true;
            _reader?.Dispose();
            _reader = null;
        }

        #endregion

        #region Special Types

        public struct SerailizedFieldResult
        {
            public string fieldName;
            public string line;
            public string fieldData;
        }

        #endregion

    }

}
