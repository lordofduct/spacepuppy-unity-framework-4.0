using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;

namespace com.spacepuppy.UI
{

    [System.Serializable]
    public sealed class TextFieldTarget
    {

        #region Fields

        [SerializeField]
#if SP_TMPRO
        [TypeRestriction(typeof(UnityEngine.UI.Text), typeof(UnityEngine.UI.InputField), typeof(TMPro.TMP_Text), typeof(TMPro.TMP_InputField), typeof(IProxy), AllowProxy = true)]
#else
        [TypeRestriction(typeof(UnityEngine.UI.Text), typeof(UnityEngine.UI.InputField), typeof(IProxy), AllowProxy = true)]
#endif
        private UnityEngine.Object _target;

        #endregion

        #region Properties

        public bool HasTarget => _target != null;

        public UnityEngine.Object Target
        {
            get => _target;
            set => _target = StringUtil.GetAsTextBindingTarget(value, true);
        }

        public string text
        {
            get => StringUtil.TryGetText(_target);
            set => StringUtil.TrySetText(_target, value);
        }

        #endregion

        #region Methods

        public void SelectUIElement()
        {
            switch (StringUtil.GetAsTextBindingTarget(_target, false))
            {
                case UnityEngine.UI.Selectable s:
                    s.Select();
                    break;
            }
        }

        #endregion

    }

}