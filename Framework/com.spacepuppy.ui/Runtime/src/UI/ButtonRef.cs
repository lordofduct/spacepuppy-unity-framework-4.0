using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppy.UI
{

    [System.Serializable]
    public sealed class ButtonRef
    {

        private static readonly System.Type[] _supportedTypes = new System.Type[] { typeof(UnityEngine.UI.Button), typeof(SPUIButton) };

        private System.EventHandler _onClick;
        public event System.EventHandler OnClick
        {
            add
            {
                bool check = _onClick == null;
                _onClick += value;
                if(check)
                {
                    this.AddEventHandlerHook();
                }
            }
            remove
            {
                bool check = _onClick != null;
                _onClick -= value;
                if(check && _onClick == null)
                {
                    this.RemoveEventHandlerHook();
                }
            }
        }

        #region Fields

        [SerializeField]
        [TypeRestriction(new System.Type[] { typeof(UnityEngine.UI.Button), typeof(SPUIButton) })]
        private UnityEngine.Object _value;

        #endregion

        #region Properties

        public UnityEngine.Object Value
        {
            get => _value;
            set
            {
                if (_value == value) return;

                if(_value && _onClick != null)
                {
                    RemoveEventHandlerHook();
                }
                _value = ObjUtil.GetAsFromSource(_supportedTypes, value) as UnityEngine.Object;
                if(_onClick != null)
                {
                    AddEventHandlerHook();
                }
            }
        }

        #endregion

        #region Methods

        private void AddEventHandlerHook()
        {
            switch(_value)
            {
                case UnityEngine.UI.Button btn:
                    btn.onClick.AddListener(this.UnityButtonHandler);
                    break;
                case SPUIButton spbtn:
                    spbtn.OnClick.TriggerActivated += this.SPButtonHandler;
                    break;
            }
        }

        private void RemoveEventHandlerHook()
        {
            switch (_value)
            {
                case UnityEngine.UI.Button btn:
                    btn.onClick.RemoveListener(this.UnityButtonHandler);
                    break;
                case SPUIButton spbtn:
                    spbtn.OnClick.TriggerActivated += this.SPButtonHandler;
                    break;
            }
        }

        private void UnityButtonHandler()
        {
            _onClick?.Invoke(_value, System.EventArgs.Empty);
        }

        private void SPButtonHandler(object sender, System.EventArgs e)
        {
            _onClick?.Invoke(sender, System.EventArgs.Empty);
        }

        #endregion

    }

}
