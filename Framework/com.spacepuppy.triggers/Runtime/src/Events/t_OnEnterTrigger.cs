#pragma warning disable 0649 // variable declared but not used.
using UnityEngine;

using com.spacepuppy.Geom;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    public sealed class t_OnEnterTrigger : SPComponent, IObservableTrigger, ICompoundTriggerEnterHandler
    {

        #region Fields

        [SerializeField]
        private EventActivatorMaskRef _mask = new EventActivatorMaskRef();
        [SerializeField]
        private float _cooldownInterval = 0f;

        [SerializeField()]
        [UnityEngine.Serialization.FormerlySerializedAs("_trigger")]
        [SPEvent.Config("othercollider (Collider)")]
        private SPEvent _onEnterTrigger = new SPEvent("OnEnterTrigger");

        [System.NonSerialized()]
        private bool _coolingDown;
        [System.NonSerialized]
        private bool _usesCompoundTrigger;

        #endregion

        #region CONSTRUCTOR

        protected override void Start()
        {
            base.Start();

            this.ResolveCompoundTrigger();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            this.ResolveCompoundTrigger();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _coolingDown = false;
        }

        #endregion

        #region Properties

        public IEventActivatorMask Mask
        {
            get { return _mask.Value; }
            set { _mask.Value = value; }
        }

        public float CooldownInterval
        {
            get { return _cooldownInterval; }
            set { _cooldownInterval = value; }
        }

        [ShowNonSerializedProperty("Uses Compound Trigger", ShowAtEditorTime = true, ShowOutsideRuntimeValuesFoldout = true)]
        public bool UsesCompoundTrigger
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) return this.HasComponent<ICompoundTrigger>() || !(this.HasComponent<Collider>() || this.HasComponent<Rigidbody>());
#endif
                return _usesCompoundTrigger;
            }
        }

        public SPEvent OnEnterTrigger => _onEnterTrigger;

        #endregion

        #region Methods

        public void ResolveCompoundTrigger()
        {
            //we assume CompoundTrigger if we have one OR if we don't have anything that can signal OnTriggerEnter attached to us.
            _usesCompoundTrigger = this.HasComponent<ICompoundTrigger>() || !(this.HasComponent<Collider>() || this.HasComponent<Rigidbody>());
        }

        private void DoTestTriggerEnter(Collider other)
        {
            if (_mask.Value == null || _mask.Value.Intersects(other))
            {
                _onEnterTrigger.ActivateTrigger(this, other);

                if (_cooldownInterval > 0f)
                {
                    _coolingDown = true;
                    this.InvokeGuaranteed(() =>
                    {
                        _coolingDown = false;
                    }, _cooldownInterval);
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!this.isActiveAndEnabled || _coolingDown || _usesCompoundTrigger) return;

            this.DoTestTriggerEnter(other);
        }

        void ICompoundTriggerEnterHandler.OnCompoundTriggerEnter(ICompoundTrigger trigger, Collider other)
        {
            if (!this.isActiveAndEnabled || _coolingDown) return;

            this.DoTestTriggerEnter(other);
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onEnterTrigger };
        }

        #endregion

    }

}
