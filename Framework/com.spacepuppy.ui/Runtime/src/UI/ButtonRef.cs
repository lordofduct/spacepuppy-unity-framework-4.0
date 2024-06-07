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

        #region Properties

        public Selectable Button
        {
            get => _value;
            set
            {
                if (_value == value) return;

                if (_value && _onClick != null)
                {
                    RemoveEventHandlerHook();
                }
                _value = ObjUtil.GetAsFromSource(_supportedTypes, value) as Selectable;
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

    }

}
