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

        [SerializeField]
        [ReorderableArray]
        [ContentBinderKey]
        private string[] _keys;

        [SerializeField]
#if SP_TMPRO
        [TypeRestriction(typeof(UnityEngine.UI.Text), typeof(UnityEngine.UI.InputField), typeof(TMPro.TMP_Text), typeof(TMPro.TMP_InputField), typeof(IProxy), AllowProxy = true)]
#else
        [TypeRestriction(typeof(UnityEngine.UI.Text), typeof(UnityEngine.UI.InputField), typeof(IProxy), AllowProxy = true)]
#endif
        private UnityEngine.Object _target;

        [SerializeField]
        private string _formatting;

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
            set => _target = StringUtil.GetAsTextBindingTarget(value, true);
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
            get => StringUtil.TryGetText(_target);
            set => StringUtil.TrySetText(_target, value);
        }

        #endregion

        #region IContentBinder Interface

        string IContentBinder.Key => _keys != null && _keys.Length > 0 ? _keys[0] : null;

        public void Bind(DataBindingContext context, object source)
        {
            string stxt = string.Empty;
            if(_keys != null && _keys.Length > 0 && !string.IsNullOrEmpty(_formatting))
            {
                object[] arr = null;
                switch(_keys.Length)
                {
                    case 1:
                        arr = ArrayUtil.Temp(context.GetBoundValue(source, _keys[0]));
                        break;
                    case 2:
                        arr = ArrayUtil.Temp(context.GetBoundValue(source, _keys[0]), 
                                             context.GetBoundValue(source, _keys[1]));
                        break;
                    case 3:
                        arr = ArrayUtil.Temp(context.GetBoundValue(source, _keys[0]),
                                             context.GetBoundValue(source, _keys[1]),
                                             context.GetBoundValue(source, _keys[2]));
                        break;
                    case 4:
                        arr = ArrayUtil.Temp(context.GetBoundValue(source, _keys[0]),
                                             context.GetBoundValue(source, _keys[1]),
                                             context.GetBoundValue(source, _keys[2]),
                                             context.GetBoundValue(source, _keys[3]));
                        break;
                    default:
                        arr = new object[_keys.Length];
                        arr[0] = context.GetBoundValue(source, _keys[0]);
                        for(int i = 1; i < _keys.Length; i++)
                        {
                            arr[i] = context.GetBoundValue(source, _keys[i]);
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

            StringUtil.TrySetText(_target, stxt);
        }

        #endregion

    }

}
