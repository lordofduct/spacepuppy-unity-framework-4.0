using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.UI
{
    public sealed class SPUIVersionDisplay : SPComponent, IMStartOrEnableReceiver
    {

        #region Fields

        [SerializeField, DefaultFromSelf]
        private TextFieldTarget _versionText = new();

        [SerializeField, Tooltip("A format string with {0} = version, {1} = CompanyName, {2} = ProductName. Leave blank to show just version #.")]
        private string _format;

        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            this.Sync();
        }

        #endregion

        #region Properties

        public TextFieldTarget VersionText => _versionText;

        public string Format
        {
            get => _format;
            set
            {
                _format = value;
                if (this.isActiveAndEnabled)
                {
                    this.Sync();
                }
            }
        }

        #endregion

        #region Methods

        public void Sync()
        {
            _versionText.text = string.IsNullOrEmpty(_format) ? Application.version : string.Format(_format, Application.version, Application.companyName, Application.productName);
        }

        #endregion

    }
}
