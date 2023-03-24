using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Statistics.Events
{

    public class i_TriggerIndexByTokenValue : AutoTriggerable, IObservableTrigger
    {

#if UNITY_EDITOR
        public const string PROP_CATEGORY = nameof(_category);
        public const string PROP_TOKEN = nameof(_token);
        public const string PROP_WRAPMODE = nameof(_wrapMode);
        public const string PROP_ONEVALBYINDEX = nameof(_onEvalByIndex);
#endif

        #region Fields

        [SerializeField]
        private string _category;
        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("_id")]
        private string _token;

        [SerializeField]
        private MathUtil.WrapMode _wrapMode;
        [SerializeField]
        private SPEvent _onEvalByIndex = new SPEvent("OnEvalByIndex");

        #endregion

        #region Properties

        public string Category { get { return _category; } set { _category = value; } }
        public string Token { get { return _token; } set { _token = value; } }

        public MathUtil.WrapMode WrapMode
        {
            get => _wrapMode;
            set => _wrapMode = value;
        }

        public SPEvent OnEvalByIndex { get { return _onEvalByIndex; } }

        #endregion

        #region Triggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var service = Services.Get<IStatisticsTokenLedgerService>();
            if (service == null) return false;

            if (_onEvalByIndex.HasReceivers)
            {
                int i = MathUtil.WrapIndex(_wrapMode, (int)service.GetStatOrDefault(_category, _token), _onEvalByIndex.TargetCount);
                _onEvalByIndex.ActivateTriggerAt(i, this, null);
            }
            return true;
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onEvalByIndex };
        }

        #endregion

    }
}
