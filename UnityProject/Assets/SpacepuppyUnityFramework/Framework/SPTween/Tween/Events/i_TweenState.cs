#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;

using com.spacepuppy.Dynamic;
using com.spacepuppy.Events;
using com.spacepuppy.Tween;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Tween.Events
{

    /// <summary>
    /// Tweens all the properties found on the source token for the target. 
    /// This is useful if you've saved a StateToken of a something like a Transform (position, rotation, scale). 
    /// You could just tween To that token to tween back to that saved state. 
    /// </summary>
    public class i_TweenState : AutoTriggerable
    {

        public const string PROP_TARGET = nameof(_target);
        public const string PROP_SOURCE = nameof(_source);
        public const string PROP_SOURCEALT = nameof(_sourceAlt);
        public const string PROP_ANIMMODE = nameof(_mode);
        public const string PROP_EASE = nameof(_ease);
        public const string PROP_DURATION = nameof(_duration);
        public const string PROP_TWEENTOKEN = nameof(_tweenToken);

        #region Fields

        [SerializeField]
        [TriggerableTargetObject.Config(typeof(UnityEngine.Object))]
        private TriggerableTargetObject _target;

        [SerializeField]
        [TriggerableTargetObject.Config(typeof(UnityEngine.Object))]
        private TriggerableTargetObject _source;

        [SerializeField]
        [TriggerableTargetObject.Config(typeof(UnityEngine.Object))]
        private TriggerableTargetObject _sourceAlt;

        [SerializeField]
        private TweenHash.AnimMode _mode;
        [SerializeField()]
        private EaseStyle _ease;
        [SerializeField()]
        public SPTimePeriod _duration;

        [SerializeField()]
        [Tooltip("Leave blank for tweens to be unique to this component.")]
        private string _tweenToken;

        [SerializeField()]
        private SPEvent _onComplete;

        [SerializeField()]
        private SPEvent _onTick;

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();

            if (string.IsNullOrEmpty(_tweenToken)) _tweenToken = "i_Tween*" + this.GetInstanceID().ToString();
        }

        /*
         * TODO - if want to kill these tweens, need to store each tween that was started. Can't kill all on '_target' since we changed over to TriggerableTargetObject.
         * 
        protected override void OnDisable()
        {
            base.OnDisable();

            SPTween.KillAll(_target, _tweenToken);
        }
        */

        #endregion

        #region Properties

        public TriggerableTargetObject Target
        {
            get { return _target; }
        }

        public TriggerableTargetObject Source
        {
            get { return _source; }
        }

        public TriggerableTargetObject SourceAlt
        {
            get { return _sourceAlt; }
        }

        public TweenHash.AnimMode Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        public EaseStyle Ease
        {
            get { return _ease; }
            set { _ease = value; }
        }

        public SPTimePeriod Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        public string TweenToken
        {
            get { return _tweenToken; }
            set { _tweenToken = value; }
        }

        public SPEvent OnComplete
        {
            get { return _onComplete; }
        }

        public SPEvent OnTick
        {
            get { return _onTick; }
        }

        #endregion

        #region Triggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var targ = _target.GetTarget<object>(arg);
            var source = _source.GetTarget<object>(arg);

            var twn = SPTween.Tween(targ)
                             .TweenWithToken(_mode, EaseMethods.GetEase(_ease), _duration.Seconds, source, _sourceAlt)
                             .Use(_duration.TimeSupplier)
                             .SetId(targ);
            if (_onComplete?.HasReceivers ?? false)
                twn.OnFinish((t) => _onComplete.ActivateTrigger(this, null));

            if (_onTick?.HasReceivers ?? false)
                twn.OnStep((t) => _onTick.ActivateTrigger(this, null));

            twn.Play(true, _tweenToken);
            return true;
        }

        #endregion

    }

}
