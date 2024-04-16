#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Project
{

    [System.Serializable]
    public class TextRef : ITextSource
    {
        
        #region Fields

        [SerializeField]
        private string[] _text;
        [SerializeField]
        private UnityEngine.Object _obj;

        #endregion

        #region CONSTRUCTOR

        public TextRef() { }

        public TextRef(string str)
        {
            _text = new string[] { str ?? string.Empty };
            _obj = null;
        }

        public TextRef(IEnumerable<string> strings)
        {
            _text = strings.ToArray();
            _obj = null;
        }

        public TextRef(UnityEngine.Object src)
        {
            _text = null;
            _obj = src;
        }

        #endregion

        #region Properties

        public UnityEngine.Object Source
        {
            get { return _obj; }
        }

        #endregion

        #region ITextSource Interface

        public int Count
        {
            get
            {
                if (_obj is ITextSource)
                    return (_obj as ITextSource).Count;
                else if (!object.ReferenceEquals(_obj, null))
                    return 1;
                else
                    return _text != null ? _text.Length : 0;
            }
        }

        public string text
        {
            get
            {
                if (_obj is ITextSource)
                    return (_obj as ITextSource).text;
                else if (!object.ReferenceEquals(_obj, null))
                    return StringUtil.TryGetText(_obj);
                else
                    return _text != null && _text.Length > 0 ? _text[0] : null;
            }
        }

        public string this[int index]
        {
            get
            {
                if (_obj != null && _obj is ITextSource)
                {
                    return (_obj as ITextSource)[index];
                }
                else if (!object.ReferenceEquals(_obj, null))
                {
                    if (index != 0)
                        throw new System.IndexOutOfRangeException("index");
                    return StringUtil.TryGetText(_obj);
                }
                else
                {
                    if (_text == null || _text.Length == 0 || index < 0 || index >= _text.Length)
                        throw new System.IndexOutOfRangeException("index");
                    return _text[index];
                }
            }
        }

        #endregion

        #region IEnumerable Interface

        public IEnumerator<string> GetEnumerator()
        {
            if (_obj != null && _obj is ITextSource)
                return (_obj as ITextSource).GetEnumerator();
            else if (!object.ReferenceEquals(_obj, null))
                return FromTextAsset(_obj as TextAsset);
            else if (_text != null && _text.Length > 0)
                return (_text as IEnumerable<string>).GetEnumerator();
            else
                return Enumerable.Empty<string>().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }


        private static IEnumerator<string> FromTextAsset(UnityEngine.Object text)
        {
            yield return StringUtil.TryGetText(text);
        }

        #endregion

        #region Special Types

        public class ConfigAttribute : System.Attribute
        {

            public bool DisallowFoldout;

            public ConfigAttribute(bool disallowFoldout)
            {
                this.DisallowFoldout = disallowFoldout;
            }

        }

        #endregion

    }

}
