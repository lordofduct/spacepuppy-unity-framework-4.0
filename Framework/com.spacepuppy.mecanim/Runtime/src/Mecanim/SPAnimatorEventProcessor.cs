using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Collections;
using com.spacepuppy.Utils;
using com.spacepuppy.Events;
using System.Linq;

namespace com.spacepuppy.Mecanim
{

    /// <summary>
    /// Similar to AnimationEvent but only exists as part of the IAnimationEventHandler message flow since the unity class is sealed and readonly. 
    /// Only Animator related properties exist since this message flow relies exclusively on Mecanim. 
    /// Note that this class is recycled for memory purposes and should only be trusted for the life of the message flow. Do not store references to it.
    /// </summary>
    public class AnimationEventMessage
    {

        internal AnimationEventMessage(Animator animator, AnimationEvent ev)
        {
            this.functionName = ev.functionName;
            this.stringParameter = ev.stringParameter;
            this.floatParameter = ev.floatParameter;
            this.intParameter = ev.intParameter;
            this.objectReferenceParameter = ev.objectReferenceParameter;
            this.time = ev.time;
            this.animator = animator;
        }

        public string functionName { get; private set; }
        public string stringParameter { get; private set; }
        public float floatParameter { get; private set; }
        public int intParameter { get; private set; }
        public UnityEngine.Object objectReferenceParameter { get; private set; }

        public float time { get; private set; }

        public Animator animator { get; private set; }

        public AnimatorStateInfo animatorStateInfo { get; set; }
        public AnimatorClipInfo animatorClipInfo { get; set; }

        /// <summary>
        /// Signals behaviours on the current layer of this event if this event was fired on the current layer/s.
        /// </summary>
        public void SignalStateMachineBehavioursOfAnimationEvent()
        {
            if (animator == null) return;

            int hash = animatorStateInfo.fullPathHash;
            int layerIndex = -1;

            //naively search for the layerIndex since Unity doesn't offer a way to get the layerIndex of a specific state. This also validates that the target state is the current state.
            for (int i = 0; i < animator.layerCount; i++)
            {
                if (animator.GetCurrentAnimatorStateInfo(i).fullPathHash == hash || (animator.IsInTransition(i) && animator.GetNextAnimatorStateInfo(i).fullPathHash == hash))
                {
                    layerIndex = i;
                    break;
                }
            }
            if (layerIndex < 0) return;

            var behaviours = animator.GetBehaviours(animatorStateInfo.fullPathHash, layerIndex);
            foreach (var b in behaviours)
            {
                try
                {
                    if (b is IAnimationEventHandler h) h.OnAnimationEvent(this);
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

    }

    /// <summary>
    /// This attempts to track all animation events that may occur on an Animator. 
    /// It only "attempts" because an animatoroverride can throw it off. Either always perform 
    /// overrides via SPAnimatorOverrideLayer OR manually call 'Sync' on this any time you override. 
    /// 
    /// Listening for the IAnimatorOverrideLayerHandler message one can then forward animation events to 
    /// StateMachineBehaviours via the 'AnimationEventMessage.SignalStateMachineBehavioursOfAnimationEvent' method.
    /// </summary>
    [Infobox("This processes Animation Events fired by an Animator and forwards them on to a IAnimatorOverrideLayerHandler message handler agnostically rather than via SendMessage.")]
    [RequireComponent(typeof(Animator))]
    public sealed class SPAnimatorEventProcessor : SPComponent, IAnimatorOverrideLayerHandler
    {

        #region Fields

        [SerializeField]
        [Tooltip("Forces all events attached to the animations to not require a receiver.")]
        private bool _forceReceiverNotRequired = true;

        [SerializeField]
        private bool _automaticallySignalStateMachineBehaviours = true;

        [SerializeField]
        private EventCallback[] _eventCallbacks;

        [System.NonSerialized]
        private Animator _animator;
        [System.NonSerialized]
        private Dictionary<EventToken, AnimationEventMessage> _activeEventTokens = new Dictionary<EventToken, AnimationEventMessage>(EventTokenEqualityComparer.Default);

        [System.NonSerialized]
        private Messaging.MessageToken<IAnimationEventHandler> _onAnimationEventMessageToken;

        [System.NonSerialized]
        private Dictionary<string, EventCallback> _eventCallbacksTable = new Dictionary<string, EventCallback>();

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();

            _animator = this.GetComponent<Animator>();

            _eventCallbacksTable.Clear();
            foreach (var o in _eventCallbacks)
            {
                _eventCallbacksTable.TryAdd(o.Name ?? string.Empty, o);
            }
        }

        protected override void Start()
        {
            base.Start();

            _onAnimationEventMessageToken = Messaging.CreateSignalToken<IAnimationEventHandler>(this.gameObject);
            this.Sync();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (this.started)
            {
                _onAnimationEventMessageToken?.SetDirty();
                this.Sync();
            }
        }

        #endregion

        #region Properties

        public Animator Animator => _animator;

        public bool ForceReceiverNotRequired
        {
            get => _forceReceiverNotRequired;
            set => _forceReceiverNotRequired = value;
        }

        public bool AutomaticallySignalStateMachineBehaviours
        {
            get => _automaticallySignalStateMachineBehaviours;
            set => _automaticallySignalStateMachineBehaviours = value;
        }

        #endregion

        #region Methods

        public void Sync()
        {
            _activeEventTokens.Clear();
            if (!_animator || !_animator.runtimeAnimatorController) return;

            foreach (var anim in _animator.runtimeAnimatorController.animationClips)
            {
                using (var existingHooks = TempCollection.GetDict<EventToken, AnimationEvent>())
                using (var lst = TempCollection.GetList<AnimationEvent>())
                {
                    //gather up the events that exist splitting them between our hooks and actual events
                    foreach (var ev in anim.events)
                    {
                        if (ev.functionName == nameof(SignalEventHook_198334))
                        {
                            existingHooks[new EventToken()
                            {
                                animName = anim.name,
                                functionName = ev.stringParameter,
                                time = ev.intParameter,
                            }] = ev;
                        }
                        else
                        {
                            if (_forceReceiverNotRequired) ev.messageOptions = SendMessageOptions.DontRequireReceiver;
                            lst.Add(ev);
                        }
                    }

                    //add hooks for any events that don't already have hooks
                    int cnt = lst.Count;
                    for (int i = 0; i < cnt; i++)
                    {
                        var token = new EventToken()
                        {
                            animName = anim.name,
                            functionName = lst[i].functionName,
                            time = (int)(lst[i].time * 1000),
                        };
                        _activeEventTokens[token] = new AnimationEventMessage(_animator, lst[i]);
                        AnimationEvent hookev;
                        if (existingHooks.TryGetValue(token, out hookev))
                        {
                            lst.Add(hookev);
                        }
                        else
                        {
                            hookev = new AnimationEvent()
                            {
                                functionName = nameof(SignalEventHook_198334),
                                stringParameter = token.functionName,
                                intParameter = token.time,
                                time = lst[i].time,
                                messageOptions = SendMessageOptions.DontRequireReceiver,
                            };
                            lst.Add(hookev);
                        }
                    }

                    anim.events = lst.ToArray();
                }
            }
        }

        public void Clear()
        {
            _activeEventTokens.Clear();
        }

        private void SignalEventHook_198334(AnimationEvent ev)
        {
            var token = new EventToken()
            {
                animName = ev.animatorClipInfo.clip.name,
                functionName = ev.stringParameter,
                time = ev.intParameter,
            };
            AnimationEventMessage msg;
            if (_activeEventTokens.TryGetValue(token, out msg))
            {
                msg.animatorStateInfo = ev.animatorStateInfo;
                msg.animatorClipInfo = ev.animatorClipInfo;
                if (_automaticallySignalStateMachineBehaviours) msg.SignalStateMachineBehavioursOfAnimationEvent();
                if (_onAnimationEventMessageToken.Count > 0) _onAnimationEventMessageToken.Invoke(msg, (o, e) => o.OnAnimationEvent(e));
                if (_eventCallbacksTable.TryGetValue(token.functionName, out EventCallback callback)) callback.Event.ActivateTrigger(this, null);
            }
        }

        #endregion

        #region IAnimatorOverrideLayerHandler Interface

        void IAnimatorOverrideLayerHandler.OnOverrideApplied(com.spacepuppy.Mecanim.SPAnimatorOverrideLayers layers)
        {
            this.Sync();
        }

        #endregion

        #region Static Utils

        private struct EventToken
        {
            public string animName;
            public string functionName;
            public int time;

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        private class EventTokenEqualityComparer : IEqualityComparer<EventToken>
        {
            public static readonly EventTokenEqualityComparer Default = new EventTokenEqualityComparer();

            public bool Equals(EventToken x, EventToken y)
            {
                return x.time == y.time && x.functionName == y.functionName && x.animName == y.animName;
            }

            public int GetHashCode(EventToken obj)
            {
                return ((obj.animName.GetHashCode() + obj.functionName.GetHashCode()) << 6) + obj.time;
            }
        }

        #endregion

        #region Special Types

        [System.Serializable]
        private class EventCallback
        {
            [SerializeField] private string _name;
            [SerializeField] private SPEvent _event = new SPEvent("Event");

            public string Name
            {
                get => _name;
                set => _name = value ?? string.Empty;
            }

            public SPEvent Event => _event;

        }

        #endregion

    }

}
