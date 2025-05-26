using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Dynamic;
using com.spacepuppy.Utils;
using System;

namespace com.spacepuppy.UI
{

    [System.Serializable]
    public sealed class TextInputFieldTarget : IDynamicProperty, System.IDisposable
    {

        private System.EventHandler<TempEventArgs> _onSubmit;
        public event System.EventHandler<TempEventArgs> OnSubmit
        {
            add
            {
                bool check = _onSubmit == null;
                _onSubmit += value;
                if (check)
                {
                    this.AddEventHandlerHook();
                }
            }
            remove
            {
                bool check = _onSubmit != null;
                _onSubmit -= value;
                if (check && _onSubmit == null)
                {
                    this.RemoveEventHandlerHook();
                }
            }
        }

        #region Fields

        [SerializeField]
        [RespectsIProxy]
#if SP_TMPRO
        [TypeRestriction(typeof(UnityEngine.UI.InputField), typeof(TMPro.TMP_InputField), typeof(IProxy), AllowProxy = true)]
#else
        [TypeRestriction(typeof(UnityEngine.UI.InputField), typeof(IProxy), AllowProxy = true)]
#endif
        private UnityEngine.Object _target;

        [System.NonSerialized]
        private System.IDisposable _onSendHook;

        #endregion

        #region Properties

        public bool HasTarget => _target != null;

        public UnityEngine.Object Target
        {
            get => _target;
            set
            {
                var targ = StringUtil.GetAsTextInputFieldBindingTarget(value, true);
                if (targ == _target) return;

                this.RemoveEventHandlerHook();
                _target = targ;
                if (_onSubmit != null)
                {
                    AddEventHandlerHook();
                }
            }
        }

        public string text
        {
            get => StringUtil.TryGetText(_target);
            set => StringUtil.TrySetText(_target, value);
        }

        public int caretPosition
        {
            get
            {
                switch (StringUtil.GetAsTextBindingTarget(_target, false))
                {
                    case UnityEngine.UI.InputField uif:
                        return uif.caretPosition;
#if SP_TMPRO
                    case TMPro.TMP_InputField tmpif:
                        return tmpif.caretPosition;
#endif
                    default:
                        return -1;
                }
            }
            set
            {
                switch (StringUtil.GetAsTextBindingTarget(_target, false))
                {
                    case UnityEngine.UI.InputField uif:
                        uif.caretPosition = value;
                        break;
#if SP_TMPRO
                    case TMPro.TMP_InputField tmpif:
                        tmpif.caretPosition = value;
                        break;
#endif
                }
            }
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

        private void AddEventHandlerHook()
        {
            switch (StringUtil.GetAsTextBindingTarget(_target, false))
            {
                case UnityEngine.UI.InputField uif:
                    _onSendHook = uif.onSubmit.AddTrackedListener(Target_OnSubmit);
                    break;
#if SP_TMPRO
                case TMPro.TMP_InputField tmpif:
                    _onSendHook = tmpif.onSubmit.AddTrackedListener(Target_OnSubmit);
                    break;
#endif
            }
        }

        private void RemoveEventHandlerHook()
        {
            _onSendHook?.Dispose();
            _onSendHook = null;
        }

        private void Target_OnSubmit(string value)
        {
            if (_onSubmit != null)
            {
                var te = TempEventArgs.Create(value);
                _onSubmit.Invoke(this, te);
                TempEventArgs.Release(te);
            }
        }

        #endregion

        #region IDynamicProperty Interface

        object IDynamicProperty.Get() => this.Target;

        void IDynamicProperty.Set(object value) => this.Target = value as UnityEngine.Object;

        System.Type IDynamicProperty.GetType() => typeof(UnityEngine.Object);

        #endregion

        #region IDisposable Interface

        public void Dispose()
        {
            this.RemoveEventHandlerHook();
            _target = null;
        }

        #endregion

    }

}
