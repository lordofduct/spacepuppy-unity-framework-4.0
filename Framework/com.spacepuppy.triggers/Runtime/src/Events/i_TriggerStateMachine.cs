using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;
using com.spacepuppy.Project;
using com.spacepuppy.Async;

namespace com.spacepuppy.Events
{

    [Infobox("Triggering this forwards the trigger down to the current state using that state's configuration.")]
    public class i_TriggerStateMachine : Triggerable, IObservableTrigger, IStateMachine, IReadOnlyStateMachine
    {

        public enum WrapMode
        {
            Clamp = 0,
            Loop = 1,
        }

        #region Fields

        [SerializeField]
        [Min(-1)]
        private int _initialState;

        [SerializeField]
        private bool _returnToInitialStateOnEnable;

        [SerializeField]
        [Tooltip("If this is triggered, don't signal the state that is active.")]
        private bool _ignoreTriggerForwarding;

        [SerializeField]
        private bool _triggerCurrentOnStateChange;

        [SerializeField]
        private StateCollection _states = new StateCollection();

        [SerializeField, SPEvent.Config("state (object)")]
        private SPEvent _onExitState = new SPEvent("OnExitState");

        [SerializeField, SPEvent.Config("state (object)")]
        [UnityEngine.Serialization.FormerlySerializedAs("_onStateChanged")]
        private SPEvent _onEnterState = new SPEvent("OnEnterState");

        [SerializeField, Tooltip("Defines how a state is activated. By default only the current state is enabled and all others are disabled.")]
        private InterfaceRefOrPicker<IStateActivator> _stateActivator = new();
        [SerializeField]
        private InterfaceRefOrPicker<IStateTransition> _stateTransition = new();

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            _states.GoToState(-1);
            _states.ExitingState += (s, e) => _onExitState.ActivateTrigger(this, _states.CurrentState?.Target);
            _states.EnteringState += (s, e) => _onEnterState.ActivateTrigger(this, _states.CurrentState?.Target);
            _states.Owner = this;
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();

            if (_states.CurrentState == null || _returnToInitialStateOnEnable)
            {
                _states.GoToState(_initialState);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (_returnToInitialStateOnEnable && this.started)
            {
                _states.GoToState(_initialState);
            }
        }

        #endregion

        #region Properties

        public int InitialState
        {
            get => _initialState;
            set => _initialState = value;
        }

        public bool ReturnToInitialStateOnEnable
        {
            get => _returnToInitialStateOnEnable;
            set => _returnToInitialStateOnEnable = value;
        }

        public bool IgnoreTriggerForwarding
        {
            get => _ignoreTriggerForwarding;
            set => _ignoreTriggerForwarding = value;
        }

        public bool TriggerCurrentOnStateChange
        {
            get => _triggerCurrentOnStateChange;
            set => _triggerCurrentOnStateChange = value;
        }

        public StateInfo? CurrentState => _states.CurrentState;

        public StateCollection States => _states;

        public SPEvent OnExitState => _onExitState;

        public SPEvent OnEnterState => _onEnterState;

        public IStateActivator StateActivator
        {
            get => _stateActivator.Value;
            set => _stateActivator.Value = value;
        }

        public IStateTransition StateTransition
        {
            get => _stateTransition.Value;
            set => _stateTransition.Value = value;
        }

        #endregion

        #region IStateMachine Interface

        public event System.EventHandler StateChanged
        {
            add => _states.StateChanged += value;
            remove => _states.StateChanged -= value;
        }

        public int StateCount => _states.Count;

        [ShowNonSerializedProperty("Current State Id")]
        public string CurrentStateId => _states.CurrentStateId;

        [ShowNonSerializedProperty("Current State Index")]
        public int? CurrentStateIndex => _states.CurrentStateIndex;

        public virtual void GoToState(int index)
        {
            _states.GoToState(index);
        }

        public void GoToStateById(string id)
        {
            _states.GoToStateById(id);
        }

        public void GoToNextState(WrapMode mode = WrapMode.Loop)
        {
            _states.GoToNextState(mode);
        }

        public void GoToPreviousState(WrapMode mode = WrapMode.Loop)
        {
            _states.GoToPreviousState(mode);
        }

        #endregion

        #region ITriggerable Interface

        public override bool CanTrigger => !_ignoreTriggerForwarding && base.CanTrigger;

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var stateindex = _states.CurrentStateIndex;
            if (stateindex != null && stateindex >= 0 && stateindex < _states.Count)
            {
                _states.ActivateTriggerAt(stateindex.Value, this, null);
            }
            return true;
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onExitState, _onEnterState };
        }

        #endregion

        #region Special Types

        [System.Serializable]
        public class StateCollection : IList<StateInfo>
        {

            public event System.EventHandler ExitingState;
            public event System.EventHandler StateChanged;
            public event System.EventHandler EnteringState;

            #region Fields

            [SerializeField]
            [UnityEngine.Serialization.FormerlySerializedAs("_targets")]
            private List<StateInfo> _states = new List<StateInfo>();

            private const int MIN_SIZE_TOSEARCH = 8;
            [System.NonSerialized]
            private Dictionary<string, int> _idToIndex;

            [System.NonSerialized]
            private int? _currentState = null;

            [System.NonSerialized]
            private int _transitionVersion;

            #endregion

            #region Properties

            public int Count => _states.Count;

            bool ICollection<StateInfo>.IsReadOnly => false;

            public StateInfo this[int index]
            {
                get => _states[index];
                set
                {
                    _states[index] = value;
                    _idToIndex = null;
                }
            }

            public string CurrentStateId => this.CurrentState?.Id ?? string.Empty;

            public int? CurrentStateIndex => _currentState;

            public StateInfo? CurrentState
            {
                get
                {
                    if (_currentState == null || _currentState < 0 || _currentState >= _states.Count) return null;
                    return _states[_currentState.Value];
                }
            }

            public i_TriggerStateMachine Owner { get; internal set; }

            #endregion

            #region Methods

            public void ActivateTriggerAt(int index, object sender, object arg)
            {
                if (index < 0 || index >= _states.Count) return;

                EventTriggerEvaluator.Current.TriggerAllOnTarget(_states[index].Target, arg, sender, arg);
            }

            public StateInfo? Find(string id)
            {
                if (id == null) return null;

                if (_states.Count < MIN_SIZE_TOSEARCH)
                {
                    for (int i = 0; i < _states.Count; i++)
                    {
                        if (string.Equals(_states[i].Id, id)) return _states[i];
                    }
                }
                else
                {
                    if (_idToIndex == null) this.SanitizeLookup();

                    int index;
                    if (!_idToIndex.TryGetValue(id, out index)) return null;

                    return _states[index];
                }
                return null;
            }

            public int FindIndex(string id)
            {
                if (id == null) return -1;

                if (_states.Count < MIN_SIZE_TOSEARCH)
                {
                    for (int i = 0; i < _states.Count; i++)
                    {
                        if (string.Equals(_states[i].Id, id)) return i;
                    }
                }
                else
                {
                    if (_idToIndex == null) this.SanitizeLookup();

                    int index;
                    if (!_idToIndex.TryGetValue(id, out index)) index = -1;
                    return index;
                }
                return -1;
            }

            private void SanitizeLookup()
            {
                var dict = new Dictionary<string, int>();
                for (int i = _states.Count - 1; i >= 0; i--)
                {
                    dict[_states[i].Id] = i;
                }
                _idToIndex = dict;
            }

            public virtual void GoToState(int index)
            {
                if (_currentState != index && _currentState >= 0)
                {
                    var trans = this.Owner?.StateTransition;
                    if (trans != null)
                    {
                        if (_transitionVersion > 0)
                        {
                            trans.CancelExit(this);
                        }

                        _transitionVersion++;
                        int v = _transitionVersion;
                        var state = this.CurrentState;
                        this.ExitingState?.Invoke(this, System.EventArgs.Empty);
                        trans.OnExit(this, state, index >= 0 && index < this.Count ? this[index] : null).OnComplete(o =>
                        {
                            if (v != _transitionVersion) return;
                            _transitionVersion = 0;
                            this.CompleteStateTransition(state, index);
                        });
                    }
                    else
                    {
                        this.ExitingState?.Invoke(this, System.EventArgs.Empty);
                        this.CompleteStateTransition(this.CurrentState, index);
                    }
                }
                else
                {
                    this.CompleteStateTransition(this.CurrentState, index);
                }
            }

            private void CompleteStateTransition(StateInfo? laststate, int index)
            {
                //first disable, then enable, this way you can use the OnDisable and OnEnable of the states to perform actions predictably
                bool signal = _currentState != index;
                _currentState = index;
                (this.Owner?.StateActivator ?? StandardStateActivator.Default).Activate(this);

                if (signal)
                {
                    this.StateChanged?.Invoke(this, System.EventArgs.Empty);

                    var trans = this.Owner?.StateTransition;
                    var currentstate = this.CurrentState;
                    this.EnteringState?.Invoke(this, System.EventArgs.Empty);
                    if (trans != null)
                    {
                        trans.OnEnter(this, laststate, currentstate);
                    }

                    if (this.Owner && this.Owner.TriggerCurrentOnStateChange)
                    {
                        this.Owner.Trigger(null, null);
                    }
                }
            }

            public void GoToStateById(string id)
            {
                this.GoToState(this.FindIndex(id));
            }

            public void GoToNextState(WrapMode mode = WrapMode.Loop)
            {
                switch (mode)
                {
                    case WrapMode.Loop:
                        this.GoToState(MathUtil.Wrap((_currentState ?? -1) + 1, _states.Count));
                        break;
                    case WrapMode.Clamp:
                    default:
                        this.GoToState(Mathf.Clamp((_currentState ?? -1) + 1, 0, _states.Count - 1));
                        break;
                }
            }

            public void GoToPreviousState(WrapMode mode = WrapMode.Loop)
            {
                switch (mode)
                {
                    case WrapMode.Loop:
                        this.GoToState(MathUtil.Wrap((_currentState ?? 1) - 1, _states.Count));
                        break;
                    case WrapMode.Clamp:
                    default:
                        this.GoToState(Mathf.Clamp((_currentState ?? _states.Count) - 1, 0, _states.Count - 1));
                        break;
                }
            }

            #endregion

            #region List Interface

            public void Add(StateInfo state)
            {
                _states.Add(state);
                _idToIndex = null;
            }

            public int IndexOf(StateInfo item)
            {
                return _states.IndexOf(item);
            }

            public void Insert(int index, StateInfo item)
            {
                _states.Insert(index, item);
                _idToIndex = null;
            }

            public void RemoveAt(int index)
            {
                _states.RemoveAt(index);
                _idToIndex = null;
            }

            public void Clear()
            {
                _states.Clear();
                _idToIndex = null;
            }

            public bool Contains(StateInfo item)
            {
                return _states.Contains(item);
            }

            public void CopyTo(StateInfo[] array, int arrayIndex)
            {
                _states.CopyTo(array, arrayIndex);
            }

            public bool Remove(StateInfo item)
            {
                if (_states.Remove(item))
                {
                    _idToIndex = null;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public List<StateInfo>.Enumerator GetEnumerator()
            {
                return _states.GetEnumerator();
            }

            IEnumerator<StateInfo> IEnumerable<StateInfo>.GetEnumerator()
            {
                return _states.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return _states.GetEnumerator();
            }

            #endregion

        }

        [System.Serializable]
        public struct StateInfo
        {

            #region Fields

            [SerializeField]
            private string _id;

            [SerializeField()]
            [UnityEngine.Serialization.FormerlySerializedAs("_triggerable")]
            private UnityEngine.Object _target;

            #endregion

            #region Properties

            public string Id
            {
                get => _id;
                set => _id = value;
            }

            public UnityEngine.Object Target
            {
                get => _target;
                set => _target = value;
            }

            #endregion

            #region Methods

            public override string ToString() => _id;

            #endregion

        }

        /// <summary>
        /// Allows delaying the exit transition so that you can perform cleanup over time. 
        /// Useful for things like a UI transition.
        /// </summary>
        public interface IStateTransition
        {
            AsyncWaitHandle OnExit(StateCollection collection, StateInfo? exitstate, StateInfo? enterstate);
            void CancelExit(StateCollection collection);
            void OnEnter(StateCollection collection, StateInfo? exitstate, StateInfo? enterstate);
        }

        [System.Serializable]
        public class DelayExitTransition : IStateTransition
        {
            public SPTimePeriod delay = 1f;

            AsyncWaitHandle IStateTransition.OnExit(StateCollection collection, StateInfo? exitstate, StateInfo? enterstate)
            {
                return WaitForDuration.Period(delay).AsAsyncWaitHandle();
            }

            void IStateTransition.CancelExit(StateCollection collection) { }
            void IStateTransition.OnEnter(StateCollection collection, StateInfo? exitstate, StateInfo? enterstate) { }
        }

        [System.Serializable]
        public class FadeCanvasGroupTransition : IStateTransition
        {

            public SPTime timeSupplier = new SPTime(DeltaTimeType.Real);
            public float fadeInTime = 0.5f;
            public float fadeOutTime = 0.5f;
            public float interactableDelay = 0.5f;

            void IStateTransition.OnEnter(StateCollection collection, StateInfo? exitstate, StateInfo? enterstate)
            {
                var grp = ObjUtil.GetAsFromSource<CanvasGroup>(enterstate?.Target);
                if (!grp) return;

                if (fadeInTime > 0f && collection.Owner)
                {
                    collection.Owner.StartRadicalCoroutine(this.FadeIn(grp, new SPTimePeriod(fadeInTime, timeSupplier.TimeSupplier ?? SPTime.Real)));
                }
                else
                {
                    grp.alpha = 1f;
                    grp.interactable = true;
                    grp.blocksRaycasts = true;
                }
            }
            System.Collections.IEnumerator FadeIn(CanvasGroup grp, SPTimePeriod period)
            {
                double startTime = period.TimeSupplier.TotalPrecise;
                float startAlpha = grp.alpha;
                while (grp && !period.Elapsed(startTime))
                {
                    float t = (float)(period.TimeSupplier.TotalPrecise - startTime);
                    grp.alpha = t / period.Seconds;
                    if (t > interactableDelay)
                    {
                        grp.interactable = true;
                        grp.blocksRaycasts = true;
                    }
                    yield return null;
                }

                if (grp)
                {
                    grp.alpha = 1f;
                    grp.interactable = true;
                    grp.blocksRaycasts = true;
                }
            }

            AsyncWaitHandle IStateTransition.OnExit(StateCollection collection, StateInfo? exitstate, StateInfo? enterstate)
            {
                var grp = ObjUtil.GetAsFromSource<CanvasGroup>(exitstate?.Target);
                if (!grp) return AsyncWaitHandle.CompletedHandle;

                grp.interactable = false;
                grp.blocksRaycasts = false;
                if (fadeOutTime > 0f && collection.Owner)
                {
                    float timeLeft = grp.alpha * fadeOutTime;
                    if (timeLeft > 0f)
                    {
                        var period = new SPTimePeriod(timeLeft, timeSupplier.TimeSupplier ?? SPTime.Real);
                        double startTime = period.TimeSupplier.TotalPrecise - (fadeOutTime - timeLeft);
                        return collection.Owner.StartRadicalCoroutine(this.FadeOut(grp, period, startTime)).AsAsyncWaitHandle();
                    }
                    else
                    {
                        grp.alpha = 0f;
                    }
                }
                else
                {
                    grp.alpha = 0f;
                }

                return AsyncWaitHandle.CompletedHandle;
            }
            System.Collections.IEnumerator FadeOut(CanvasGroup grp, SPTimePeriod period, double startTime)
            {
                float startAlpha = grp.alpha;
                while (grp && !period.Elapsed(startTime))
                {
                    float t = (float)(period.TimeSupplier.TotalPrecise - startTime) / period.Seconds;
                    grp.alpha = Mathf.Lerp(startAlpha, 0f, t);
                    yield return null;
                }

                if (grp)
                {
                    grp.alpha = 0f;
                }
            }

            void IStateTransition.CancelExit(StateCollection collection) { }

        }

        [System.Serializable]
        public class CrossFadeCanvasGroupTransition : IStateTransition
        {

            public SPTime timeSupplier = new SPTime(DeltaTimeType.Real);
            public float fadeInTime = 0.5f;
            public float fadeOutTime = 0.5f;
            public float interactableDelay = 0.5f;

            private int _version;

            void IStateTransition.OnEnter(StateCollection collection, StateInfo? exitstate, StateInfo? enterstate)
            {
                var grp = ObjUtil.GetAsFromSource<CanvasGroup>(enterstate?.Target);
                if (!grp) return;

                if (fadeInTime > 0f && collection.Owner)
                {
                    if (exitstate == null)
                    {
                        var period = new SPTimePeriod(fadeInTime, timeSupplier.TimeSupplier ?? SPTime.Real);
                        double startTime = period.TimeSupplier.TotalPrecise;
                        grp.alpha = 0f;
                        _version++;
                        collection.Owner.StartRadicalCoroutine(this.CrossFade(grp, null, period, fadeInTime, 0f, startTime, _version));
                    }
                    else if (grp.alpha < 1f)
                    {
                        float timeLeft = (1f - grp.alpha) * fadeInTime;
                        var period = new SPTimePeriod(timeLeft, timeSupplier.TimeSupplier ?? SPTime.Real);
                        double startTime = period.TimeSupplier.TotalPrecise - (fadeInTime - timeLeft);
                        _version++;
                        collection.Owner.StartRadicalCoroutine(this.CrossFade(grp, null, period, timeLeft, 0f, startTime, _version));
                    }
                    else
                    {
                        grp.alpha = 1f;
                        grp.interactable = true;
                        grp.blocksRaycasts = true;
                    }
                }
                else
                {
                    grp.alpha = 1f;
                    grp.interactable = true;
                    grp.blocksRaycasts = true;
                }
            }
            AsyncWaitHandle IStateTransition.OnExit(StateCollection collection, StateInfo? exitstate, StateInfo? enterstate)
            {
                var grp_enter = ObjUtil.GetAsFromSource<CanvasGroup>(enterstate?.Target);
                var grp_exit = ObjUtil.GetAsFromSource<CanvasGroup>(exitstate?.Target);
                if (!grp_exit && !grp_enter) return AsyncWaitHandle.CompletedHandle;

                if (!collection.Owner || (fadeOutTime <= 0f && fadeInTime <= 0f))
                {
                    if (grp_enter)
                    {
                        grp_enter.alpha = 1f;
                        grp_enter.interactable = true;
                        grp_enter.blocksRaycasts = true;
                    }
                    if (grp_exit)
                    {
                        grp_exit.alpha = 1f;
                        grp_exit.interactable = false;
                        grp_exit.blocksRaycasts = false;
                    }
                    return AsyncWaitHandle.CompletedHandle;
                }

                grp_enter.TrySetActive(true); //pre-emptively enable
                var period = new SPTimePeriod(Mathf.Max(fadeInTime, fadeOutTime), timeSupplier.TimeSupplier ?? SPTime.Real);
                _version++;
                return collection.Owner.StartRadicalCoroutine(this.CrossFade(grp_enter, grp_exit, period, fadeInTime, fadeOutTime, period.TimeSupplier.TotalPrecise, _version)).AsAsyncWaitHandle();
            }

            System.Collections.IEnumerator CrossFade(CanvasGroup grp_enter, CanvasGroup grp_exit, SPTimePeriod period, float time_enter, float time_exit, double startTime, int version)
            {
                float offsetA = Mathf.Max(period.Seconds - time_enter, 0f);
                float alphaA = grp_enter ? grp_enter.alpha : 0f;
                float alphaB = grp_exit ? grp_exit.alpha : 0f;
                while (_version == version && !period.Elapsed(startTime))
                {
                    double ct = period.TimeSupplier.TotalPrecise;
                    if (grp_enter)
                    {
                        float t = Mathf.Max((float)(ct - startTime - offsetA), 0f);
                        grp_enter.alpha = t / time_enter;
                        if (t > interactableDelay)
                        {
                            grp_enter.interactable = true;
                            grp_enter.blocksRaycasts = true;
                        }
                    }
                    if (grp_exit)
                    {
                        float t = (float)(ct - startTime) / time_exit;
                        grp_exit.alpha = Mathf.Lerp(alphaB, 0f, t);
                    }

                    yield return null;
                }

                if (_version == version)
                {
                    if (grp_enter)
                    {
                        grp_enter.alpha = 1f;
                        grp_enter.interactable = true;
                        grp_enter.blocksRaycasts = true;
                    }
                    if (grp_exit)
                    {
                        grp_exit.alpha = 1f;
                        grp_exit.interactable = false;
                        grp_exit.blocksRaycasts = false;
                    }
                }
            }


            void IStateTransition.CancelExit(StateCollection collection)
            {
                _version++;
            }

        }


        /// <summary>
        /// Defines how a state is activated (enabled/disabled).
        /// </summary>
        public interface IStateActivator
        {
            void Activate(StateCollection states);
        }

        class StandardStateActivator : IStateActivator
        {
            public static readonly StandardStateActivator Default = new();

            public void Activate(StateCollection states)
            {
                var index = states.CurrentStateIndex;
                var currentGo = index >= 0 && index < states.Count ? GameObjectUtil.GetGameObjectFromSource(states[index.Value].Target, true) : null;
                for (int i = 0; i < states.Count; i++)
                {
                    var go = GameObjectUtil.GetGameObjectFromSource(states[i].Target, true);
                    if (go && i != index && go != currentGo) go.SetActive(false);
                }
                if (currentGo) currentGo.SetActive(true);
            }
        }

        [System.Serializable]
        [SerializeRefLabel("Cascade")]
        public class CascadeStateActivator : IStateActivator
        {
            public void Activate(StateCollection states)
            {
                var index = states.CurrentStateIndex;
                for (int i = 0; i < states.Count; i++)
                {
                    var go = GameObjectUtil.GetGameObjectFromSource(states[i].Target, true);
                    if (go) go.SetActive(i <= index);
                }
            }
        }

        #endregion

    }

}
