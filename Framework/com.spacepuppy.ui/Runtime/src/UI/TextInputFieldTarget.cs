using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;

namespace com.spacepuppy.UI
{

    [System.Serializable]
    public sealed class TextInputFieldTarget
    {

        #region Fields

        [SerializeField]
        [RespectsIProxy]
#if SP_TMPRO
        [TypeRestriction(typeof(UnityEngine.UI.InputField), typeof(TMPro.TMP_InputField), typeof(IProxy), AllowProxy = true)]
#else
        [TypeRestriction(typeof(UnityEngine.UI.InputField), typeof(IProxy), AllowProxy = true)]
#endif
        private UnityEngine.Object _target;

        #endregion

        #region Properties

        public bool HasTarget => _target != null;

        public UnityEngine.Object Target
        {
            get => _target;
            set => _target = StringUtil.GetAsTextInputFieldBindingTarget(value, true);
        }

        public string text
        {
            get => StringUtil.TryGetText(_target);
            set => StringUtil.TrySetText(_target, value);
        }

        #endregion

        #region Methods

        public UnityEngine.Object ReduceTarget() => StringUtil.GetAsTextBindingTarget(_target, false);

        public void SelectUIElement()
        {
            switch (StringUtil.GetAsTextBindingTarget(_target, false))
            {
                case UnityEngine.UI.Selectable s:
                    s.Select();
                    break;
            }
        }

        public void ActivateInputField()
        {
            switch (StringUtil.GetAsTextBindingTarget(_target, false))
            {
                case UnityEngine.UI.InputField uif:
                    uif.ActivateInputField();
                    break;
#if SP_TMPRO
                case TMPro.TMP_InputField tmpif:
                    tmpif.ActivateInputField();
                    break;
#endif
            }
        }

        public void OnSubmit_AddListener(UnityEngine.Events.UnityAction<string> callback)
        {
            switch (StringUtil.GetAsTextBindingTarget(_target, false))
            {
                case UnityEngine.UI.InputField uif:
                    uif.onSubmit.AddListener(callback);
                    break;
#if SP_TMPRO
                case TMPro.TMP_InputField tmpif:
                    tmpif.onSubmit.AddListener(callback);
                    break;
#endif
            }
        }

        public void OnSubmit_RemoveListener(UnityEngine.Events.UnityAction<string> callback)
        {
            switch (StringUtil.GetAsTextBindingTarget(_target, false))
            {
                case UnityEngine.UI.InputField uif:
                    uif.onSubmit.RemoveListener(callback);
                    break;
#if SP_TMPRO
                case TMPro.TMP_InputField tmpif:
                    tmpif.onSubmit.RemoveListener(callback);
                    break;
#endif
            }
        }

        #endregion

    }

}
