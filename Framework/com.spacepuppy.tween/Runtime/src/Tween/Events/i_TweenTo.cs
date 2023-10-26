#pragma warning disable 0649 // variable declared but not used.
using UnityEngine;

using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Tween.Events
{

    public class i_TweenTo : AutoTriggerable, IObservableTrigger
    {

        public enum TransformScope
        {
            Global = 0,
            Local = 1
        }

        #region Fields

        [SerializeField()]
        [TriggerableTargetObject.Config(typeof(Transform))]
        private TriggerableTargetObject _target = new TriggerableTargetObject();

        [SerializeField()]
        [TriggerableTargetObject.Config(typeof(Transform))]
        private TriggerableTargetObject _location = new TriggerableTargetObject();


        [SerializeField()]
        private EaseStyle _ease;
        [SerializeField()]
        private SPTimePeriod _duration;

        [SerializeField]
        private TransformScope _scope;

        [SerializeField]
        private bool _orientWithLocationRotation;

        [SerializeField()]
        private bool _tweenEntireEntity;

        [SerializeField()]
        private SPEvent _onComplete = new SPEvent("OnComplete");

        [SerializeField()]
        private SPEvent _onTick = new SPEvent("OnTick");

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

        protected override void OnDisable()
        {
            base.OnDisable();

            SPTween.KillAll(_target, _killToken);
        }

        #endregion

        #region Properties

        public TriggerableTargetObject Target
        {
            get { return _target; }
        }

        public TriggerableTargetObject Location
        {
            get { return _location; }
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

        public TransformScope Scope
        {
            get => _scope;
            set => _scope = value;
        }

        public bool OrientWithLocationRotation
        {
            get { return _orientWithLocationRotation; }
            set { _orientWithLocationRotation = value; }
        }

        public bool TweenEntireEntity
        {
            get { return _tweenEntireEntity; }
            set { _tweenEntireEntity = value; }
        }

        public SPEvent OnComplete
        {
            get { return _onComplete; }
        }

        public SPEvent OnTick
        {
            get { return _onTick; }
        }

        public string KillToken
        {
            get => _killToken;
            set => _killToken = value;
        }

        #endregion

        #region TriggerableMechanism Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var targ = this._target.GetTarget<Transform>(arg);
            if (targ == null) return false;
            if (_tweenEntireEntity) targ = GameObjectUtil.FindRoot(targ).transform;

            var loc = _location.GetTarget<Transform>(arg);
            if (targ == null || loc == null) return false;

            TweenHash twn = null;
            switch (_scope)
            {
                case TransformScope.Global:
                    {
                        twn = SPTween.Tween(targ)
                             .Prop(targ.position_ref()).To(EaseMethods.GetEase(_ease), _duration.Seconds, loc.position)
                             .Use(_duration.TimeSupplier)
                             .SetId(_target);
                        if (_orientWithLocationRotation) twn.Prop(targ.rotation_ref()).To(EaseMethods.GetEase(_ease), _duration.Seconds, loc.rotation);
                    }
                    break;
                case TransformScope.Local:
                    {
                        twn = SPTween.Tween(targ)
                             .Prop(targ.localPosition_ref()).To(EaseMethods.GetEase(_ease), _duration.Seconds, targ.ParentInverseTransformPoint(loc.position))
                             .Use(_duration.TimeSupplier)
                             .SetId(_target);
                        if (_orientWithLocationRotation) twn.Prop(targ.localRotation_ref()).To(EaseMethods.GetEase(_ease), _duration.Seconds, targ.ParentInverseTransformRotation(loc.rotation));
                    }
                    break;
                default:
                    return false;
            }

            if (_onComplete?.HasReceivers ?? false)
                twn.OnFinish((t) => _onComplete.ActivateTrigger(this, null));

            if (_onTick?.HasReceivers ?? false)
                twn.OnStep((t) => _onTick.ActivateTrigger(this, null));

            twn.Play(true, _killToken);

            return true;
        }

        #endregion

        #region IObservable Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onTick, _onComplete };
        }

        #endregion

    }

}
