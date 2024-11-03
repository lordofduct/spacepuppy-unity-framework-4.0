using UnityEngine;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Statistics.UI
{

    public class TokenValueAndNameTextBinder : SPComponent, IMStartOrEnableReceiver, IStatisticsTokenLedgerChangedGlobalHandler
    {

        public enum DisplayModes
        {
            Nullable = -1,
            Double = 0,
            Int = 1,
            Bool = 2,
        }

        #region Fields

        [SerializeField]
        private StatId _statId;

        [SerializeField, DefaultFromSelf]
        [RespectsIProxy]
#if SP_TMPRO
        [TypeRestriction(typeof(UnityEngine.UI.Text), typeof(UnityEngine.UI.InputField), typeof(TMPro.TMP_Text), typeof(TMPro.TMP_InputField), typeof(IProxy), AllowProxy = true)]
#else
        [TypeRestriction(typeof(UnityEngine.UI.Text), typeof(UnityEngine.UI.InputField), typeof(IProxy), AllowProxy = true)]
#endif
        private UnityEngine.Object _valueTarget;

        [SerializeField, DefaultFromSelf]
        [RespectsIProxy]
#if SP_TMPRO
        [TypeRestriction(typeof(UnityEngine.UI.Text), typeof(UnityEngine.UI.InputField), typeof(TMPro.TMP_Text), typeof(TMPro.TMP_InputField), typeof(IProxy), AllowProxy = true)]
#else
        [TypeRestriction(typeof(UnityEngine.UI.Text), typeof(UnityEngine.UI.InputField), typeof(IProxy), AllowProxy = true)]
#endif
        private UnityEngine.Object _nameTarget;

        [SerializeField]
        private DisplayModes _displayMode;

        [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("_valueFormatString")]
        private string _valueFormatting;

        [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("_nameFormatString")]
        private string _nameFormatting;

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

        public StatId StatId
        {
            get => _statId;
            set => _statId = value;
        }

        /// <summary>
        /// The target text object for the value. This can be a UnityEngine.UI.Text, a TextMeshPro text object, or an IProxy for either.
        /// </summary>
        public UnityEngine.Object ValueTarget
        {
            get => _valueTarget;
            set => _valueTarget = StringUtil.GetAsTextBindingTarget(value, true);
        }

        /// <summary>
        /// The target text object for the name. This can be a UnityEngine.UI.Text, a TextMeshPro text object, or an IProxy for either.
        /// </summary>
        public UnityEngine.Object NameTarget
        {
            get => _nameTarget;
            set => _nameTarget = StringUtil.GetAsTextBindingTarget(value, true);
        }

        public DisplayModes DisplayMode
        {
            get => _displayMode;
            set => _displayMode = value;
        }

        public string ValueFormatting
        {
            get => _valueFormatting;
            set => _valueFormatting = value;
        }

        public string NameFormatting
        {
            get => _nameFormatting;
            set => _nameFormatting = value;
        }

        #endregion

        #region Methods

        public void SyncDisplay()
        {
            if (_nameTarget)
            {
                var stxt_name = StringUtil.NicifyVariableName(_statId.Token);
                if (!string.IsNullOrEmpty(_nameFormatting)) stxt_name = string.Format(_nameFormatting, stxt_name);
                StringUtil.TrySetText(_nameTarget, stxt_name);
            }

            if (Services.Get(out IStatisticsTokenLedgerService service))
            {
                string stxt = string.Empty;
                switch (_displayMode)
                {
                    case DisplayModes.Nullable:
                        stxt = string.IsNullOrEmpty(_valueFormatting) ? ConvertUtil.ToString(service.GetStat(_statId)) : string.Format(_valueFormatting, service.GetStat(_statId));
                        break;
                    case DisplayModes.Double:
                        stxt = string.IsNullOrEmpty(_valueFormatting) ? ConvertUtil.ToString(service.GetStatOrDefault(_statId)) : string.Format(_valueFormatting, service.GetStatOrDefault(_statId));
                        break;
                    case DisplayModes.Int:
                        stxt = string.IsNullOrEmpty(_valueFormatting) ? ConvertUtil.ToString((int)(service.GetStatOrDefault(_statId))) : string.Format(_valueFormatting, (int)(service.GetStatOrDefault(_statId)));
                        break;
                    case DisplayModes.Bool:
                        stxt = string.IsNullOrEmpty(_valueFormatting) ? ConvertUtil.ToString(service.GetStatAsBool(_statId)) : string.Format(_valueFormatting, service.GetStatAsBool(_statId));
                        break;
                    default:
                        stxt = string.IsNullOrEmpty(_valueFormatting) ? ConvertUtil.ToString(service.GetStat(_statId)) : string.Format(_valueFormatting, service.GetStat(_statId));
                        break;
                }

                StringUtil.TrySetText(_valueTarget, stxt);
            }
            else
            {
                StringUtil.TrySetText(_valueTarget, string.Empty);
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
