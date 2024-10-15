using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Dynamic;
using com.spacepuppy.Utils;

namespace com.spacepuppy.UI
{

    [System.Serializable]
    public sealed class ButtonRef : IDynamicProperty, System.IDisposable
    {

        private static readonly System.Type[] _supportedTypes = new System.Type[] { typeof(UnityEngine.UI.Button), typeof(SPUIButton) };

        private System.EventHandler _onClick;
        /// <summary>
        /// The sender returned by OnClick is the button itself, not the ButtonRef object.
        /// </summary>
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
        private Selectable _value;

        [System.NonSerialized]
        private System.IDisposable _onClickHook;

        #endregion

        #region CONSTRUCTOR

        public ButtonRef() { }

        public ButtonRef(Selectable btn)
        {
            this.Button = btn;
        }

        #endregion

        #region Properties

        public Selectable Button
        {
            get => _value;
            set
            {
                if (_value == value) return;

                if (_onClick != null)
                {
                    RemoveEventHandlerHook();
                    _value = ObjUtil.GetAsFromSource(_supportedTypes, value) as Selectable;
                    AddEventHandlerHook();
                }
                else
                {
                    _value = ObjUtil.GetAsFromSource(_supportedTypes, value) as Selectable;
                }
            }
        }

        public GameObject gameObject => this.Button ? this.Button.gameObject : null;

        public Transform transform => this.Button ? this.Button.transform : null;

        public bool interactable
        {
            get => _value ? _value.interactable : false;
            set
            {
                if (_value) _value.interactable = value;
            }
        }

        public bool enabled
        {
            get => _value ? _value.enabled : false;
            set
            {
                if (_value) _value.enabled = value;
            }
        }

        #endregion

        #region Methods

        private void AddEventHandlerHook()
        {
            switch (_value)
            {
                case UnityEngine.UI.Button btn:
                    _onClickHook = btn.onClick.AddTrackedListener(this.UnityButtonHandler);
                    break;
                case SPUIButton spbtn:
                    _onClickHook = spbtn.OnClick.AddTrackedListener(this.SPButtonHandler);
                    break;
            }
        }

        private void RemoveEventHandlerHook()
        {
            _onClickHook?.Dispose();
            _onClickHook = null;
        }

        private void UnityButtonHandler()
        {
            _onClick?.Invoke(_value, System.EventArgs.Empty);
        }

        private void SPButtonHandler(object sender, System.EventArgs e)
        {
            _onClick?.Invoke(sender, System.EventArgs.Empty);
        }

        public TrackedListenerToken AddTrackedListener(System.EventHandler handler) => new TrackedListenerToken(this, handler);

        #endregion

        #region IDynamicProperty Interface

        object IDynamicProperty.Get() => this.Button;

        void IDynamicProperty.Set(object value) => this.Button = value as Selectable;

        System.Type IDynamicProperty.GetType() => typeof(Selectable);

        #endregion

        #region IDisposable Interface

        public void Dispose()
        {
            this.RemoveEventHandlerHook();
            _value = null;
        }

        #endregion

        #region Special Types

        public struct TrackedListenerToken : System.IDisposable
        {

            private ButtonRef _target;
            private System.EventHandler _handler;

            public TrackedListenerToken(ButtonRef target, System.EventHandler handler)
            {
                if (target != null && handler != null)
                {
                    _target = target;
                    _handler = handler;
                    _target.OnClick += handler;
                }
                else
                {
                    _target = null;
                    _handler = null;
                }
            }

            public void Dispose()
            {
                if (_target != null && _handler != null)
                {
                    _target.OnClick -= _handler;
                }
                _target = null;
                _handler = null;
            }

        }

        #endregion

    }

}
