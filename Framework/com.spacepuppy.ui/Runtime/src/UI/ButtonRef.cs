using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Project;

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
                if (check)
                {
                    this.AddEventHandlerHook();
                }
            }
            remove
            {
                bool check = _onClick != null;
                _onClick -= value;
                if (check && _onClick == null)
                {
                    this.RemoveEventHandlerHook();
                }
            }
        }

        #region Fields

        [SerializeField]
        [TypeRestriction(new System.Type[] { typeof(UnityEngine.UI.Button), typeof(SPUIButton) })]
        private Component _value;

        #endregion

        #region Properties

        public Component Button
        {
            get => _value;
            set
            {
                if (_value == value) return;

                if (_value && _onClick != null)
                {
                    RemoveEventHandlerHook();
                }
                _value = ObjUtil.GetAsFromSource(_supportedTypes, value) as Component;
                if (_onClick != null)
                {
                    AddEventHandlerHook();
                }
            }
        }

        public GameObject gameObject => this.Button ? this.Button.gameObject : null;

        public Transform transform => this.Button ? this.Button.transform : null;

        #endregion

        #region Methods

        private void AddEventHandlerHook()
        {
            switch (_value)
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
