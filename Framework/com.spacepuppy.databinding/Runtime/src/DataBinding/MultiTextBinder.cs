using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppy.DataBinding
{

    [Infobox("Allows binding to a ui/text with multiple values formatted into a complex string. 'Formatting' property must not be blank, and must have a matching number of {#} entries for each value.")]
    [RequireComponent(typeof(DataBindingContext))]
    public class MultiTextBinder : MonoBehaviour, IContentBinder
    {

        #region Fields

#if SP_TMPRO
        private static readonly System.Type[] _availableTargTypes = new System.Type[] { typeof(UnityEngine.UI.Text), typeof(TMPro.TMP_Text), typeof(IProxy) };
#else
        private static readonly System.Type[] _availableTargTypes = new System.Type[] { typeof(UnityEngine.UI.Text), typeof(IProxy) };
#endif

        [SerializeField]
        [ReorderableArray]
        [ContentBinderKey]
        private string[] _keys;

        [SerializeField]
#if SP_TMPRO
        [TypeRestriction(typeof(UnityEngine.UI.Text), typeof(TMPro.TMP_Text), typeof(IProxy), AllowProxy = true)]
#else
        [TypeRestriction(typeof(UnityEngine.UI.Text), typeof(IProxy), AllowProxy = true)]
#endif
        private UnityEngine.Object _target;

        [SerializeField]
        private string _formatting;

        [System.NonSerialized]
        private DataBindingContext _context;

        #endregion

        #region Properties

        public string[] Keys
        {
            get => _keys;
            set => _keys = value;
        }

        /// <summary>
        /// The target text object. This can be a UnityEngine.UI.Text, a TextMeshPro text object, or an IProxy for either.
        /// </summary>
        public UnityEngine.Object Target
        {
            get => _target;
            set => _target = ObjUtil.GetAsFromSource(_availableTargTypes, value) as UnityEngine.Object;
        }

        /// <summary>
        /// A standard C# format string to apply to the value passed to 'SetValue'. Should be formatted like you're calling string.Format(Formatting, value)
        /// </summary>
        public string Formatting
        {
            get => _formatting;
            set => _formatting = value;
        }

        /// <summary>
        /// Sets the text directly ignoring the 'formatting', use 'SetValue' to format the text.
        /// </summary>
        public string text
        {
            get => TextBinder.TryGetText(_target);
            set => TextBinder.TrySetText(_target, value);
        }

        #endregion

        #region IContentBinder Interface

        public DataBindingContext Context => _context != null ? _context : (_context = this.GetComponent<DataBindingContext>());

        string IContentBinder.Key => _keys != null && _keys.Length > 0 ? _keys[0] : null;

        public void Bind(object source, object value)
        {
            var context = this.GetComponent<DataBindingContext>();

            string stxt = string.Empty;
            if(_keys != null && _keys.Length > 0 && !string.IsNullOrEmpty(_formatting))
            {
                object[] arr = null;
                switch(_keys.Length)
                {
                    case 1:
                        arr = ArrayUtil.Temp(value);
                        break;
                    case 2:
                        arr = ArrayUtil.Temp(value, 
                                        context?.BindingProtocol?.GetValue(this.Context, source, _keys[1]));
                        break;
                    case 3:
                        arr = ArrayUtil.Temp(value,
                                        context?.BindingProtocol?.GetValue(this.Context, source, _keys[1]),
                                        context?.BindingProtocol?.GetValue(this.Context, source, _keys[2]));
                        break;
                    case 4:
                        arr = ArrayUtil.Temp(value,
                                        context?.BindingProtocol?.GetValue(this.Context, source, _keys[1]),
                                        context?.BindingProtocol?.GetValue(this.Context, source, _keys[2]),
                                        context?.BindingProtocol?.GetValue(this.Context, source, _keys[3]));
                        break;
                    default:
                        arr = new object[_keys.Length];
                        arr[0] = value;
                        for(int i = 1; i < _keys.Length; i++)
                        {
                            arr[i] = context?.BindingProtocol?.GetValue(this.Context, source, _keys[i]);
                        }
                        break;
                }

                try
                {
                    stxt = string.Format(_formatting, arr);
                }
                catch (System.Exception ex)
                {
                    stxt = string.Empty;
                    Debug.LogException(ex);
                }
                ArrayUtil.ReleaseTemp(arr);
            }

            TextBinder.TrySetText(_target, stxt);
        }

        #endregion

    }

}
