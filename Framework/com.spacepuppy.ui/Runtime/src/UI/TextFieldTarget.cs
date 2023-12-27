using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;
using com.spacepuppy.Dynamic;

namespace com.spacepuppy.UI
{

    [System.Serializable]
    public sealed class TextFieldTarget : IDynamicProperty
    {

        #region Fields

        [SerializeField]
        [RespectsIProxy]
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

        public Color color
        {
            get => StringUtil.TryGetColor(_target);
            set => StringUtil.TrySetColor(_target, value);
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

        #region IDynamicProperty Interface

        void IDynamicProperty.Set(object value) => this.Target = value as UnityEngine.Object;
        object IDynamicProperty.Get() => this.Target;
        System.Type IDynamicProperty.GetType() => typeof(UnityEngine.Object);

        #endregion

    }

}
