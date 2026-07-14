using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.UI;
using com.spacepuppy.Project;
using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    public sealed class MultiTextFieldTarget : SPComponent, ITextField
    {

        [System.Flags]
        public enum OptionFlags
        {
            SyncOnEnable = 1,
            SyncText = 2,
            SyncColor = 4,
        }

        #region Fields

        [SerializeField, ReorderableArray]
        [RespectsIProxy]
#if SP_TMPRO
        [TypeRestriction(typeof(UnityEngine.UI.Text), typeof(UnityEngine.UI.InputField), typeof(TMPro.TMP_Text), typeof(TMPro.TMP_InputField), typeof(IProxy), AllowProxy = true)]
#else
        [TypeRestriction(typeof(UnityEngine.UI.Text), typeof(UnityEngine.UI.InputField), typeof(IProxy), AllowProxy = true)]
#endif
        private UnityEngine.Object[] _targets = ArrayUtil.Empty<UnityEngine.Object>();

        [SerializeField, TextArea]
        private string _text;
        [SerializeField]
        private Color _color = Color.white;
        [SerializeField, EnumFlags]
        private OptionFlags _options = (OptionFlags)(-1);

        #endregion

        #region CONSTRUCTOR

        protected override void Start()
        {
            base.Start();
            if (_options.HasFlagT(OptionFlags.SyncOnEnable))
            {
                this.Sync();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (_options.HasFlagT(OptionFlags.SyncOnEnable) && this.started)
            {
                this.Sync();
            }
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        public void Sync()
        {
            bool synctext = _options.HasFlagT(OptionFlags.SyncText);
            bool synccolor = _options.HasFlagT(OptionFlags.SyncColor);
            foreach (var o in _targets)
            {
                if (synctext) StringUtil.TrySetText(o, _text);
                if (synccolor) StringUtil.TrySetColor(o, _color);
            }
        }

        #endregion

        #region ITextField Interface

        public string text
        {
            get => _text;
            set
            {
                _text = value;
                if (_options.HasFlagT(OptionFlags.SyncText))
                {
                    foreach (var o in _targets)
                    {
                        StringUtil.TrySetText(o, _text);
                    }
                }
            }
        }
        public Color color
        {
            get => _color;
            set
            {
                _color = value;
                if (_options.HasFlagT(OptionFlags.SyncColor))
                {
                    foreach (var o in _targets)
                    {
                        StringUtil.TrySetColor(o, _color);
                    }
                }
            }
        }

        #endregion

#if UNITY_EDITOR
        void OnValidate()
        {
            if (_options.HasFlagT(OptionFlags.SyncOnEnable))
            {
                this.Sync();
            }
        }
#endif

    }

}
