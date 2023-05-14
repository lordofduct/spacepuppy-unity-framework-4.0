using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Dynamic;

namespace com.spacepuppy.DataBinding
{

    public class TextBinder : ContentBinder
    {

        public enum FormattingModes
        {
            Format = 0,
            Eval = 1,
        }

        #region Fields

        [SerializeField]
#if SP_TMPRO
        [TypeRestriction(typeof(UnityEngine.UI.Text), typeof(UnityEngine.UI.InputField), typeof(TMPro.TMP_Text), typeof(TMPro.TMP_InputField), typeof(IProxy), AllowProxy = true)]
#else
        [TypeRestriction(typeof(UnityEngine.UI.Text), typeof(UnityEngine.UI.InputField), typeof(IProxy), AllowProxy = true)]
#endif
        private UnityEngine.Object _target;

        [SerializeField]
        private FormattingModes _mode;
        [SerializeField]
        private string _formatting;

        #endregion

        #region Properties

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

        #region Methods

        public override void Bind(DataBindingContext context, object source)
        {
            var value = context.GetBoundValue(source, this.Key);
            if (string.IsNullOrEmpty(_formatting))
            {
                StringUtil.TrySetText(_target, value is string s ? s : value?.ToString() ?? string.Empty);
            }
            else
            {
                string stxt;
                switch (_mode)
                {
                    case FormattingModes.Format:
                        stxt = string.Format(_formatting, value);
                        break;
                    case FormattingModes.Eval:
                        stxt = Evaluator.EvalString(_formatting, value);
                        break;
                    default:
                        stxt = value is string s ? s : value?.ToString() ?? string.Empty;
                        break;
                }
                StringUtil.TrySetText(_target, stxt);
            }
        }

        #endregion

    }

}
