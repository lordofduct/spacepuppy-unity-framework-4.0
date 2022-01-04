using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;

namespace com.spacepuppy.UI
{

    [System.Serializable]
    public class TextFieldTarget
    {

        #region Fields

        private static readonly System.Type[] _availableTargTypes = new System.Type[] { typeof(TextConfigurationDecorator.ITextDecoratorMode), typeof(UnityEngine.UI.Text), typeof(TMPro.TMP_Text), typeof(IProxy) };

        [SerializeField]
        [TypeRestriction(typeof(TextConfigurationDecorator.ITextDecoratorMode), typeof(UnityEngine.UI.Text), typeof(TMPro.TMP_Text), typeof(IProxy), AllowProxy = true)]
        private UnityEngine.Object _target;

        #endregion

        #region Properties

        public UnityEngine.Object Target
        {
            get => _target;
            set => _target = ObjUtil.GetAsFromSource(_availableTargTypes, value) as UnityEngine.Object;
        }

        public string text
        {
            get { return this.TryGetText(); }
            set { this.TrySetText(value); }
        }

        #endregion

        #region Methods

        public string TryGetText()
        {
            try
            {
                return TextConfigurationDecorator.TryGetText(_target);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }

            return null;
        }

        public bool TrySetText(string txt)
        {
            try
            {
                return TextConfigurationDecorator.TrySetText(_target, txt);
            }
            catch(System.Exception ex)
            {
                Debug.LogException(ex);
            }

            return false;
        }

        #endregion

    }

}
