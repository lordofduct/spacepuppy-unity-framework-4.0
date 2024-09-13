using System;
using System.Text;
using System.Text.RegularExpressions;

using com.spacepuppy.Collections;

namespace com.spacepuppy.Utils
{

    public static class StringUtil
    {

        public enum Alignment
        {
            Left = 0,
            Right = 1,
            Center = 2
        }

        #region Constants

        public const string RX_OPEN_TO_CLOSE_PARENS = @"\(" +
                                                      @"[^\(\)]*" +
                                                      @"(" +
                                                      @"(" +
                                                      @"(?<Open>\()" +
                                                      @"[^\(\)]*" +
                                                      @")+" +
                                                      @"(" +
                                                      @"(?<Close-Open>\))" +
                                                      @"[^\(\)]*" +
                                                      @")+" +
                                                      @")*" +
                                                      @"(?(Open)(?!))" +
                                                      @"\)";
        public const string RX_OPEN_TO_CLOSE_ANGLES = @"<" +
                                                      @"[^<>]*" +
                                                      @"(" +
                                                      @"(" +
                                                      @"(?<Open><)" +
                                                      @"[^<>]*" +
                                                      @")+" +
                                                      @"(" +
                                                      @"(?<Close-Open>>)" +
                                                      @"[^<>]*" +
                                                      @")+" +
                                                      @")*" +
                                                      @"(?(Open)(?!))" +
                                                      @">";
        public const string RX_OPEN_TO_CLOSE_BRACKETS = @"\[" +
                                                        @"[^\[\]]*" +
                                                        @"(" +
                                                        @"(" +
                                                        @"(?<Open>\[)" +
                                                        @"[^\[\]]*" +
                                                        @")+" +
                                                        @"(" +
                                                        @"(?<Close-Open>\])" +
                                                        @"[^\[\]]*" +
                                                        @")+" +
                                                        @")*" +
                                                        @"(?(Open)(?!))" +
                                                        @"\]";

        public const string RX_UNESCAPED_COMMA = @"(?<!\\),";
        public const string RX_UNESCAPED_COMMA_NOTINPARENS = @"(?<!\\),(?![^()]*\))";

        #endregion


        #region Matching

        public static bool Equals(string valueA, string valueB, bool bIgnoreCase)
        {
            return (bIgnoreCase) ? String.Equals(valueA, valueB) : String.Equals(valueA, valueB, StringComparison.OrdinalIgnoreCase);
        }
        public static bool Equals(string valueA, string valueB)
        {
            return Equals(valueA, valueB, false);
        }
        public static bool Equals(string value, params string[] others)
        {
            if ((others == null || others.Length == 0))
            {
                return String.IsNullOrEmpty(value);
            }

            foreach (var sval in others)
            {
                if (value == sval) return true;
            }

            return false;
        }
        public static bool Equals(string value, string[] others, bool bIgnoreCase)
        {
            if ((others == null || others.Length == 0))
            {
                return String.IsNullOrEmpty(value);
            }

            foreach (var sval in others)
            {
                if (StringUtil.Equals(value, sval, bIgnoreCase)) return true;
            }

            return false;
        }

        public static bool StartsWith(string value, string start)
        {
            return StartsWith(value, start);
        }

        public static bool StartsWith(string value, string start, bool bIgnoreCase)
        {
            return value.StartsWith(start, (bIgnoreCase) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }

        public static bool EndsWith(string value, string end)
        {
            return EndsWith(value, end, false);
        }

        public static bool EndsWith(string value, string end, bool bIgnoreCase)
        {
            return value.EndsWith(end, (bIgnoreCase) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }

        public static bool Contains(string str, params string[] values)
        {
            if (str == null || values == null || values.Length == 0) return false;

            foreach (var sother in values)
            {
                if (str.Contains(sother)) return true;
            }

            return false;
        }

        public static bool Contains(string str, bool ignorCase, string sother)
        {
            if (string.IsNullOrEmpty(str)) return string.IsNullOrEmpty(sother);
            if (sother == null) return false;

            if (ignorCase)
            {
                str = str.ToLower();
                if (str.Contains(sother.ToLower())) return true;
            }
            else
            {
                if (str.Contains(sother)) return true;
            }

            return false;
        }

        public static bool Contains(string str, bool ignorCase, params string[] values)
        {
            if (str == null || values == null || values.Length == 0) return false;

            if (ignorCase)
            {
                str = str.ToLower();
                foreach (var sother in values)
                {
                    if (str.Contains(sother.ToLower())) return true;
                }
            }
            else
            {
                foreach (var sother in values)
                {
                    if (str.Contains(sother)) return true;
                }
            }

            return false;
        }

        #endregion

        #region Morphing

        public static string NicifyVariableName(string nm)
        {
            if (string.IsNullOrEmpty(nm)) return string.Empty;

            int index = 0;
            while (index < nm.Length && char.IsWhiteSpace(nm[index]))
            {
                index++;
            }
            if (index >= nm.Length) return string.Empty;

            if (nm.IndexOf("m_", index) == 0)
            {
                index += 2;
            }
            else if (nm[index] == 'k' && nm.Length > (index + 2) && char.IsUpper(nm[index + 1]))
            {
                index += 1;
            }

            while (index < nm.Length && nm[index] == '_')
            {
                index++;
            }
            if (index >= nm.Length) return string.Empty;


            var sb = GetTempStringBuilder();
            sb.Append(char.ToUpper(nm[index]));
            for (int i = index + 1; i < nm.Length; i++)
            {
                if (char.IsUpper(nm[i]))
                {
                    sb.Append(' ');
                }
                sb.Append(nm[i]);
            }
            return Release(sb);
        }

        public static string ToLower(string value)
        {
            //return (value != null) ? value.ToLower() : null;
            return (value + "").ToLower();
        }

        public static string ToUpper(string value)
        {
            //return (value != null) ? value.ToUpper() : null;
            return (value + "").ToUpper();
        }

        public static string Trim(string value)
        {
            if (value == null) return null;

            return value.Trim();
        }

        public static string TrimAllWhitespace(this string value)
        {
            if (value == null) return null;
            return value.Trim().Trim('\r', '\n');
        }

        public static string[] Split(string value, string delim)
        {
            if (value == null) return null;
            return value.Split(new string[] { delim }, StringSplitOptions.None);
        }

        public static string[] Split(string value, params char[] delim)
        {
            if (value == null) return null;
            return value.Split(delim);
        }

        public static string[] SplitFixedLength(string value, string delim, int len)
        {
            if (value == null) return new string[len];

            string[] arr = value.Split(new string[] { delim }, StringSplitOptions.None);
            if (arr.Length != len) Array.Resize(ref arr, len);
            return arr;
        }

        public static string[] SplitFixedLength(string value, char delim, int len)
        {
            if (value == null) return new string[len];

            string[] arr = value.Split(delim);
            if (arr.Length != len) Array.Resize(ref arr, len);
            return arr;
        }

        public static string[] SplitFixedLength(string value, char[] delims, int len)
        {
            if (value == null) return new string[len];

            string[] arr = value.Split(delims);
            if (arr.Length != len) Array.Resize(ref arr, len);
            return arr;
        }

        public static string EnsureLength(string sval, int len, bool bPadWhiteSpace = false, Alignment eAlign = Alignment.Left)
        {
            if (sval.Length > len) sval = sval.Substring(0, len);

            if (bPadWhiteSpace) sval = PadWithChar(sval, len, eAlign, ' ');

            return sval;
        }

        public static string EnsureLength(string sval, int len, char cPadChar, Alignment eAlign = Alignment.Left)
        {
            if (sval.Length > len) sval = sval.Substring(0, len);

            sval = PadWithChar(sval, len, eAlign, cPadChar);

            return sval;
        }


        public static string PadWithChar(string sString,
                                              int iLength,
                                              Alignment eAlign = 0,
                                              char sChar = ' ')
        {
            if (sChar == '\0') return null;

            switch (eAlign)
            {
                case Alignment.Right:
                    return new String(sChar, (int)Math.Max(0, iLength - sString.Length)) + sString;
                case Alignment.Center:
                    iLength = Math.Max(0, iLength - sString.Length);
                    var sr = new String(sChar, (int)(Math.Ceiling(iLength / 2.0f))); // if odd, pad more on the right
                    var sl = new String(sChar, (int)(Math.Floor(iLength / 2.0f)));
                    return sl + sString + sr;
                case Alignment.Left:
                    return sString + new String(sChar, (int)Math.Max(0, iLength - sString.Length));
            }

            //default trap
            return sString;
        }

        public static string PadWithChar(string sString,
                                          int iLength,
                                          char sAlign,
                                          char sChar = ' ')
        {
            switch (Char.ToUpper(sAlign))
            {
                case 'L':
                    return PadWithChar(sString, iLength, Alignment.Left, sChar);
                case 'C':
                    return PadWithChar(sString, iLength, Alignment.Center, sChar);
                case 'R':
                    return PadWithChar(sString, iLength, Alignment.Right, sChar);

            }

            return null;
        }

        #endregion

        #region Replace Chars

        //####################
        //EnsureNotStartWith

        public static string EnsureNotStartWith(this string value, string start)
        {
            if (value.StartsWith(start)) return value.Substring(start.Length);
            else return value;
        }

        public static string EnsureNotStartWith(this string value, string start, bool ignoreCase)
        {
            if (value.StartsWith(start, StringComparison.OrdinalIgnoreCase)) return value.Substring(start.Length);
            else return value;
        }

        public static string EnsureNotStartWith(this string value, string start, bool ignoreCase, System.Globalization.CultureInfo culture)
        {
            if (value.StartsWith(start, ignoreCase, culture)) return value.Substring(start.Length);
            else return value;
        }

        public static string EnsureNotStartWith(this string value, string start, StringComparison comparison)
        {
            if (value.StartsWith(start, comparison)) return value.Substring(start.Length);
            else return value;
        }


        //####################
        //EnsureNotStartWith

        public static string EnsureNotEndsWith(this string value, string end)
        {
            if (value.EndsWith(end)) return value.Substring(0, value.Length - end.Length);
            else return value;
        }

        public static string EnsureNotEndsWith(this string value, string end, bool ignoreCase)
        {
            if (value.EndsWith(end, StringComparison.OrdinalIgnoreCase)) return value.Substring(0, value.Length - end.Length);
            else return value;
        }

        public static string EnsureNotEndsWith(this string value, string end, bool ignoreCase, System.Globalization.CultureInfo culture)
        {
            if (value.EndsWith(end, ignoreCase, culture)) return value.Substring(0, value.Length - end.Length);
            else return value;
        }

        public static string EnsureNotEndsWith(this string value, string end, StringComparison comparison)
        {
            if (value.EndsWith(end, comparison)) return value.Substring(0, value.Length - end.Length);
            else return value;
        }

        #endregion

        #region StringBuilders

        private static ObjectCachePool<StringBuilder> _pool = new ObjectCachePool<StringBuilder>(10, () => new StringBuilder());

        public static StringBuilder GetTempStringBuilder()
        {
            return _pool.GetInstance();
        }

        public static StringBuilder GetTempStringBuilder(string sval)
        {
            var sb = _pool.GetInstance();
            sb.Append(sval);
            return sb;
        }

        public static string Release(StringBuilder b)
        {
            if (b == null) return null;

            var result = b.ToString();
            b.Length = 0;
            _pool.Release(b);
            return result;
        }

        public static void ReleaseSilently(StringBuilder b)
        {
            if (b == null) return;

            b.Length = 0;
            _pool.Release(b);
        }

        public static string ToStringHax(this StringBuilder sb)
        {
            var info = typeof(StringBuilder).GetField("_str",
                                                        System.Reflection.BindingFlags.NonPublic |
                                                        System.Reflection.BindingFlags.Instance);
            if (info == null)
                return sb.ToString();
            else
                return info.GetValue(sb) as string;

        }

        #endregion

        #region Text Binders

#if SP_TMPRO
        private static readonly System.Type[] _availableTextTargTypes = new System.Type[] { typeof(UnityEngine.TextAsset), typeof(UnityEngine.UI.Text), typeof(UnityEngine.UI.InputField), typeof(TMPro.TMP_Text), typeof(TMPro.TMP_InputField) };
        private static readonly System.Type[] _availableTextInputTargTypes = new System.Type[] { typeof(UnityEngine.UI.InputField), typeof(TMPro.TMP_InputField) };
#else
        private static readonly System.Type[] _availableTextTargTypes = new System.Type[] { typeof(UnityEngine.TextAsset), typeof(UnityEngine.UI.Text), typeof(UnityEngine.UI.InputField) };
        private static readonly System.Type[] _availableTextInputTargTypes = new System.Type[] { typeof(UnityEngine.UI.InputField) };
#endif

#if SP_TMPRO
        private static readonly System.Type[] _availableTextTargTypes_WithProxy = new System.Type[] { typeof(UnityEngine.TextAsset), typeof(UnityEngine.UI.Text), typeof(UnityEngine.UI.InputField), typeof(TMPro.TMP_Text), typeof(TMPro.TMP_InputField), typeof(IProxy) };
        private static readonly System.Type[] _availableTextInputTargTypes_WithProxy = new System.Type[] { typeof(UnityEngine.UI.InputField), typeof(TMPro.TMP_InputField), typeof(IProxy) };
#else
        private static readonly System.Type[] _availableTextTargTypes_WithProxy  = new System.Type[] {typeof(UnityEngine.TextAsset), typeof(UnityEngine.UI.Text), typeof(UnityEngine.UI.InputField), typeof(IProxy) };
        private static readonly System.Type[] _availableTextInputTargTypes_WithProxy  = new System.Type[] { typeof(UnityEngine.UI.InputField), typeof(IProxy) };
#endif

        public static UnityEngine.Object GetAsTextBindingTarget(object obj, bool preserveProxy = false) => preserveProxy ? ObjUtil.GetAsFromSource(_availableTextTargTypes_WithProxy, obj) as UnityEngine.Object : ObjUtil.GetAsFromSource(_availableTextTargTypes, obj, true) as UnityEngine.Object;

        public static UnityEngine.Object GetAsTextInputFieldBindingTarget(object obj, bool preserveProxy = false) => preserveProxy ? ObjUtil.GetAsFromSource(_availableTextInputTargTypes_WithProxy, obj) as UnityEngine.Object : ObjUtil.GetAsFromSource(_availableTextInputTargTypes, obj, true) as UnityEngine.Object;

        /// <summary>
        /// Supports unity.ui.Text, unity.ui.InputField, TMPro.TMP_Text, TMPro.TMP_InputField, and IProxy's of them.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string TryGetText(object target)
        {
            switch (GetAsTextBindingTarget(target, false))
            {
                case UnityEngine.TextAsset ta:
                    return ta.text;
                case UnityEngine.UI.Text utxt:
                    return utxt.text;
                case UnityEngine.UI.InputField uifld:
                    return uifld.text;
#if SP_TMPRO
                case TMPro.TMP_Text tmp:
                    return tmp.text;
                case TMPro.TMP_InputField tmp_i:
                    return tmp_i.text;
#endif
            }

            return string.Empty;
        }

        /// <summary>
        /// Supports unity.ui.Text, TMPro.TMP_Text, and IProxy's of them.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool TrySetText(object target, string stxt)
        {
            switch (GetAsTextBindingTarget(target, false))
            {
                case UnityEngine.TextAsset ta:
                    return false; //TextAsset is readonly
                case UnityEngine.UI.Text utxt:
                    utxt.text = stxt;
                    return true;
                case UnityEngine.UI.InputField uifld:
                    uifld.text = stxt;
                    return true;
#if SP_TMPRO
                case TMPro.TMP_Text tmp:
                    tmp.text = stxt;
                    return true;
                case TMPro.TMP_InputField tmp_i:
                    tmp_i.text = stxt;
                    return true;
#endif
            }

            return false;
        }

        /// <summary>
        /// Supports unity.ui.Text, unity.ui.InputField, TMPro.TMP_Text, TMPro.TMP_InputField, and IProxy's of them.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static UnityEngine.Color TryGetColor(object target)
        {
            switch (StringUtil.GetAsTextBindingTarget(target, false))
            {
                case UnityEngine.UI.Text utxt:
                    return utxt.color;
                case UnityEngine.UI.InputField uifld:
                    return uifld.colors.normalColor;
#if SP_TMPRO
                case TMPro.TMP_Text tmp:
                    return tmp.color;
                case TMPro.TMP_InputField tmp_i:
                    return tmp_i.colors.normalColor;
#endif
            }

            return default;
        }

        /// <summary>
        /// Supports unity.ui.Text, TMPro.TMP_Text, and IProxy's of them.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool TrySetColor(object target, UnityEngine.Color c)
        {
            switch (StringUtil.GetAsTextBindingTarget(target, false))
            {
                case UnityEngine.UI.Text utxt:
                    utxt.color = c;
                    return true;
                case UnityEngine.UI.InputField uifld:
                    {
                        var cs = uifld.colors;
                        cs.normalColor = c;
                        uifld.colors = cs;
                    }
                    return true;
#if SP_TMPRO
                case TMPro.TMP_Text tmp:
                    tmp.color = c;
                    return true;
                case TMPro.TMP_InputField tmp_i:
                    {
                        var cs = tmp_i.colors;
                        cs.normalColor = c;
                        tmp_i.colors = cs;
                    }
                    return true;
#endif
            }

            return false;
        }

        #endregion

    }

}
