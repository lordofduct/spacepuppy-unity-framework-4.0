using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Statistics.UI
{

    public sealed class TokenValueTextBinder : SPComponent, IMStartOrEnableReceiver, IStatisticsTokenLedgerChangedGlobalHandler
    {

        public enum DisplayModes
        {
            Nullable = -1,
            Double = 0,
            Int = 1,
            Bool = 2,
        }

        #region Fields

        [SerializeField, DefaultFromSelf]
        [RespectsIProxy]
#if SP_TMPRO
        [TypeRestriction(typeof(UnityEngine.UI.Text), typeof(UnityEngine.UI.InputField), typeof(TMPro.TMP_Text), typeof(TMPro.TMP_InputField), typeof(IProxy), AllowProxy = true)]
#else
        [TypeRestriction(typeof(UnityEngine.UI.Text), typeof(UnityEngine.UI.InputField), typeof(IProxy), AllowProxy = true)]
#endif
        private UnityEngine.Object _target;

        [SerializeField]
        private StatId _statId;

        [SerializeField]
        private DisplayModes _displayMode;
        [SerializeField]
        private string _formatting;

        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            Messaging.RegisterGlobal<IStatisticsTokenLedgerChangedGlobalHandler>(this);
            this.SyncDisplay();
        }

        protected override void OnDisable()
        {
            Messaging.UnregisterGlobal<IStatisticsTokenLedgerChangedGlobalHandler>(this);
            base.OnDisable();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The target text object. This can be a UnityEngine.UI.Text, a TextMeshPro text object, or an IProxy for either.
        /// </summary>
        public UnityEngine.Object Target
        {
            get => _target;
            set => _target = StringUtil.GetAsTextBindingTarget(value, true);
        }

        public StatId StatId
        {
            get => _statId;
            set => _statId = value;
        }

        public DisplayModes DisplayMode
        {
            get => _displayMode;
            set => _displayMode = value;
        }

        /// <summary>
        /// A standard C# format string to apply to the value passed to 'SetValue'. Should be formatted like you're calling string.Format(Formatting, value)
        /// </summary>
        public string Formatting
        {
            get => _formatting;
            set => _formatting = value;
        }

        /// <summary>
        /// Sets the text directly ignoring the 'formatting', use 'SetValue' to format the text.
        /// </summary>
        public string text
        {
            get => StringUtil.TryGetText(_target);
            set => StringUtil.TrySetText(_target, value);
        }

        #endregion

        #region Methods

        public void SyncDisplay()
        {
            if (Services.Get(out IStatisticsTokenLedgerService service))
            {
                string stxt = string.Empty;
                switch (_displayMode)
                {
                    case DisplayModes.Nullable:
                        stxt = string.IsNullOrEmpty(_formatting) ? ConvertUtil.ToString(service.GetStat(_statId)) : string.Format(_formatting, service.GetStat(_statId));
                        break;
                    case DisplayModes.Double:
                        stxt = string.IsNullOrEmpty(_formatting) ? ConvertUtil.ToString(service.GetStatOrDefault(_statId)) : string.Format(_formatting, service.GetStatOrDefault(_statId));
                        break;
                    case DisplayModes.Int:
                        stxt = string.IsNullOrEmpty(_formatting) ? ConvertUtil.ToString((int)(service.GetStatOrDefault(_statId))) : string.Format(_formatting, (int)(service.GetStatOrDefault(_statId)));
                        break;
                    case DisplayModes.Bool:
                        stxt = string.IsNullOrEmpty(_formatting) ? ConvertUtil.ToString(service.GetStatAsBool(_statId)) : string.Format(_formatting, service.GetStatAsBool(_statId));
                        break;
                    default:
                        stxt = string.IsNullOrEmpty(_formatting) ? ConvertUtil.ToString(service.GetStat(_statId)) : string.Format(_formatting, service.GetStat(_statId));
                        break;
                }

                StringUtil.TrySetText(_target, stxt);
            }
            else
            {
                StringUtil.TrySetText(_target, string.Empty);
            }
        }

        #endregion

        #region IStatisticsTokenLedgerChangedGlobalHandler Interface

        void IStatisticsTokenLedgerChangedGlobalHandler.OnChanged(IStatisticsTokenLedgerService ledger, LedgerChangedEventArgs ev)
        {
            if (ev == null || ev.MultipleChanged || ev.StatId == _statId)
            {
                this.SyncDisplay();
            }
        }

        #endregion

    }

}
