using UnityEngine;

using com.spacepuppy.Events;
using com.spacepuppy.Tween;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Tween.Events
{

    public class i_Tween : AutoTriggerable, IObservableTrigger
    {

        #region Fields

        [SerializeField()]
        private SPTime _timeSupplier;

        [SerializeField()]
        private TriggerableTargetObject _target = new TriggerableTargetObject();

        [SerializeField]
        private TweenWrapMode _wrapMode;
        [SerializeField]
        [Tooltip("How many time should the tween wrap. If WrapMode is 'Once', this is ignored.")]
        [DiscreteFloat.Positive()]
        private DiscreteFloat _wrapCount = DiscreteFloat.PositiveInfinity;

        [SerializeField]
        private float _delay;

        [SerializeReference()]
        private ITweenData[] _data;

        [SerializeField()]
        private SPEvent _onComplete = new SPEvent("OnComplete");

        [SerializeField()]
        private SPEvent _onTick = new SPEvent("OnTick");

        [SerializeField]
        private SPEvent _onWrap = new SPEvent("OnWrap");

        [SerializeField()]
        [UnityEngine.Serialization.FormerlySerializedAs("_tweenToken")]
        [Tooltip("Leave blank for tweens to be unique to this component.")]
        private string _killToken;

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();

            if (string.IsNullOrEmpty(_killToken)) _killToken = "i_Tween*" + this.GetInstanceID().ToString();
        }

        /*
         * TODO - if want to kill these tweens, need to store each tween that was started. Can't kill all on '_target' since we changed over to TriggerableTargetObject.
         * 
        protected override void OnDisable()
        {
            base.OnDisable();

            //SPTween.KillAll(_target, _tweenToken);
        }
        */

        #endregion

        #region Properties

        public SPTime TimeSupplier
        {
            get => _timeSupplier;
            set => _timeSupplier = value;
        }

        public TriggerableTargetObject Target => _target;

        public TweenWrapMode WrapMode
        {
            get => _wrapMode;
            set => _wrapMode = value;
        }

        /// <summary>
        /// If WrapMode is not Once, this is the number of times it will loop. 
        /// 0 or negative values mean infinity.
        /// </summary>
        public DiscreteFloat WrapCount
        {
            get => _wrapCount;
            set => _wrapCount = value;
        }

        public ITweenData[] Data
        {
            get => _data;
            set => _data = value;
        }

        public SPEvent OnComplete => _onComplete;

        public SPEvent OnTick => _onTick;

        public SPEvent OnWrap => _onWrap;

        public string KillToken
        {
            get => _killToken;
            set => _killToken = value;
        }

        #endregion

        #region Methods

        #endregion

        #region ITriggerable Interface

        public override bool CanTrigger
        {
            get
            {
                return base.CanTrigger && _data?.Length > 0;
            }
        }

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var target = _target.GetTarget<UnityEngine.Object>(arg);
            if (target == null) return false;

            var twn = SPTween.Tween(target);
            for (int i = 0; i < _data.Length; i++)
            {
                if (_data[i] != null) _data[i].Apply(twn);
            }

            twn.Delay(_delay);
            twn.Use(_timeSupplier.TimeSupplier);
            twn.SetId(target);
            if (_wrapMode != TweenWrapMode.Once)
            {
                twn.Wrap(_wrapMode, _wrapCount.ToStandardMetricInt());
            }

            if (_onComplete?.HasReceivers ?? false)
                twn.OnFinish((t) => _onComplete.ActivateTrigger(this, null));

            if (_onTick?.HasReceivers ?? false)
                twn.OnStep((t) => _onTick.ActivateTrigger(this, null));

            if (_onWrap?.HasReceivers ?? false)
                twn.OnWrap((t) => _onWrap.ActivateTrigger(this, null));

            twn.Play(true, _killToken);
            return true;
        }

        #endregion

        #region Special Types

        public interface ITweenData
        {
            void Apply(TweenHash hash);
        }

        [System.Serializable()]
        public class GenericTweenData : ITweenData
        {
            [SerializeField()]
            public TweenHash.AnimMode Mode;
            [SerializeField()]
            public string MemberName;
            [SerializeReference()]
            public EaseSelector Ease;
            [SerializeField()]
            public VariantReference ValueS;
            [SerializeField()]
            public VariantReference ValueE;
            [SerializeField()]
            [TimeUnitsSelector()]
            public float Duration;
            [SerializeField]
            public int Option;

            public void Apply(TweenHash hash)
            {
                hash.ByAnimMode(this.Mode, this.MemberName, this.Ease?.GetEase(), this.Duration, this.ValueS.Value, this.ValueE.Value, this.Option);
            }

        }

        #endregion

        #region IObservable Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onTick, _onComplete, _onWrap };
        }

        #endregion

    }

}
