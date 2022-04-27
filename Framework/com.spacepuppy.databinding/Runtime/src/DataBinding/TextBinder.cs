using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppy.DataBinding
{

    public class TextBinder : ContentBinder
    {

        #region Fields

#if SP_TMPRO
        private static readonly System.Type[] _availableTargTypes = new System.Type[] { typeof(UnityEngine.UI.Text), typeof(TMPro.TMP_Text), typeof(IProxy) };
#else
        private static readonly System.Type[] _availableTargTypes = new System.Type[] { typeof(UnityEngine.UI.Text), typeof(IProxy) };
#endif

        [SerializeField]
#if SP_TMPRO
        [TypeRestriction(typeof(UnityEngine.UI.Text), typeof(TMPro.TMP_Text), typeof(IProxy), AllowProxy = true)]
#else
        [TypeRestriction(typeof(UnityEngine.UI.Text), typeof(IProxy), AllowProxy = true)]
#endif
        private UnityEngine.Object _target;

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
            get => TryGetText(_target);
            set => TrySetText(_target, value);
        }

        #endregion

        #region Methods

        public override void Bind(DataBindingContext context, object source)
        {
            var value = context.GetBoundValue(source, this.Key);
            string stxt = string.IsNullOrEmpty(_formatting) ? value?.ToString() ?? string.Empty : string.Format(_formatting, value);
            TrySetText(_target, stxt);
        }

        #endregion

        #region Static Utils

        public static string TryGetText(object target)
        {
            switch (target)
            {
                case UnityEngine.UI.Text utxt:
                    return utxt.text;
#if SP_TMPRO
                case TMPro.TMP_Text tmp:
                    return tmp.text;
#endif
                case IProxy proxy:
                    {
                        var utxt = ObjUtil.GetAsFromSource<UnityEngine.UI.Text>(proxy, true);
                        if (utxt) return utxt.text;

#if SP_TMPRO
                        var tmp = ObjUtil.GetAsFromSource<TMPro.TMP_Text>(proxy, true);
                        if (tmp) return tmp.text;
#endif

                        return string.Empty;
                    }
            }

            return string.Empty;
        }

        public static bool TrySetText(object target, string stxt)
        {
            switch (target)
            {
                case UnityEngine.UI.Text utxt:
                    utxt.text = stxt;
                    return true;
#if SP_TMPRO
                case TMPro.TMP_Text tmp:
                    tmp.text = stxt;
                    return true;
#endif
                case IProxy proxy:
                    {
                        var utxt = ObjUtil.GetAsFromSource<UnityEngine.UI.Text>(proxy, true);
                        if (utxt)
                        {
                            utxt.text = stxt;
                            return true;
                        }

#if SP_TMPRO
                        var tmp = ObjUtil.GetAsFromSource<TMPro.TMP_Text>(proxy, true);
                        if (tmp)
                        {
                            tmp.text = stxt;
                            return true;
                        }
#endif

                        return false;
                    }
            }

            return false;
        }

        #endregion

    }

}
