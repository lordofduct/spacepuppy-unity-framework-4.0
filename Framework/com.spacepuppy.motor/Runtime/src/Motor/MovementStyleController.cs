using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Collections;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Motor
{

    /*
     * Set SP_MVSTYLECNTRL_RESPECTEXECORDER in your custom defines to make MovementStyleController respect execution order and use the 'Update'/'FixedUpdate' of the component. 
     * This performs slower for large numbers of entities, but gives you the freedom of directly controlling the execution order if you so please.
     */
#if SP_MVSTYLECNTRL_RESPECTEXECORDER
    public class MovementStyleController : SPComponent, IMStartOrEnableReceiver
    {
#else
    public class MovementStyleController : SPComponent, IUpdateable, IMStartOrEnableReceiver
    {

        public enum UpdateOrder
        {
            Standard = 0,
            Early = 1,
            Tardy = 2,
        }

#endif

        #region Events

        public event System.EventHandler BeforeUpdateMovement;
        public event System.EventHandler UpdateMovementComplete;
        public event StyleChangedEventHandler StyleChanged;

        #endregion

        #region Fields

#if !SP_MVSTYLECNTRL_RESPECTEXECORDER
        [SerializeField()]
        private UpdateOrder _updateOrder;
        [System.NonSerialized]
        private UpdatePump _currentPump;
#endif

        [SerializeField()]
        [TypeRestriction(typeof(IMovementStyle))]
        private Component _defaultMovementStyle;

        [System.NonSerialized]
        private bool _inUpdateSequence;

        [System.NonSerialized]
        private IMovementStyle _current;
        [System.NonSerialized]
        private Deque<IMovementStyle> _styleStack;
        [System.NonSerialized]
        private bool _stackingState = false;

        [System.NonSerialized]
        private System.Action _changeStyleDelayed;
        [System.NonSerialized]
        private float _changeStyleDelayedPrecedence;
        [System.NonSerialized]
        private OrderedDelegate<System.Action> _stackStyleDelayed = new OrderedDelegate<System.Action>();

        [System.NonSerialized]
        private Messaging.MessageToken<IMovementStyleModifier> _movementStyleModifierMessageToken;
        [System.NonSerialized]
        private System.Action<IMovementStyleModifier> __onBeforeUpdateFunctor;
        private System.Action<IMovementStyleModifier> _onBeforeUpdateFunctor => __onBeforeUpdateFunctor ?? (__onBeforeUpdateFunctor = (o) => o.OnBeforeUpdateMovement(this));
        [System.NonSerialized]
        private System.Action<IMovementStyleModifier> __onUpdateCompleteFunctor;
        private System.Action<IMovementStyleModifier> _onUpdateCompleteFunctor => __onUpdateCompleteFunctor ?? (__onUpdateCompleteFunctor = (o) => o.OnUpdateMovementComplete(this));

        [System.NonSerialized]
        private bool _safeEnabled;

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();

            _styleStack = new Deque<IMovementStyle>();
        }

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            this.OnStartOrEnable();
        }
        protected virtual void OnStartOrEnable()
        {
            _safeEnabled = true;
            _movementStyleModifierMessageToken = Messaging.CreateSignalToken<IMovementStyleModifier>(this.gameObject);

            if (_defaultMovementStyle is IMovementStyle && _current == null && this.Contains(_defaultMovementStyle as IMovementStyle))
            {
                this.ChangeState(_defaultMovementStyle as IMovementStyle);
            }
            else if (_current != null)
            {
                this.ResolveUpdateLoopSequencing();
                _current.OnActivate(null, ActivationReason.MotorPaused);
            }
        }

        protected override void OnDisable()
        {
            _safeEnabled = false;
            base.OnDisable();

#if !SP_MVSTYLECNTRL_RESPECTEXECORDER
            _currentPump?.Remove(this);
            _currentPump = null;
#endif

            if (_current != null)
            {
                _current.OnDeactivate(null, ActivationReason.MotorPaused);
            }
        }

        protected override void OnDestroy()
        {
            _safeEnabled = false;
            base.OnDestroy();

#if !SP_MVSTYLECNTRL_RESPECTEXECORDER
            _currentPump?.Remove(this);
            _currentPump = null;
#endif
        }

        #endregion

        #region Properties

        public IMovementStyle DefaultMovementStyle
        {
            get { return _defaultMovementStyle as IMovementStyle; }
            set
            {
                _defaultMovementStyle = ObjUtil.GetAsFromSource<Component>(value);
            }
        }

        public bool InUpdateSequence
        {
            get { return _inUpdateSequence; }
        }

        public IMovementStyle Current
        {
            get { return _current; }
        }

        /// <summary>
        /// The current state ignoring any stacked states that may be on top of it. This is the state that would be returned to if you popped all the states.
        /// </summary>
        public IMovementStyle CurrentUnstacked
        {
            get
            {
                if (_styleStack.Count > 0)
                    return _styleStack[0];
                else
                    return _current;
            }
        }

        public IEnumerable<IMovementStyle> CurrentStack { get { return _styleStack; } }

        #endregion

        #region Update Routine

#if SP_MVSTYLECNTRL_RESPECTEXECORDER
        protected virtual void Update()
        {
            if (_current == null || _current.PrefersFixedUpdate) return;

            try
            {
                _inUpdateSequence = true;
                this.BeforeUpdateMovement?.Invoke(this, System.EventArgs.Empty);
                if (_movementStyleModifierMessageToken.Count > 0) _movementStyleModifierMessageToken.Invoke(_onBeforeUpdateFunctor);

                _current.UpdateMovement();

                this.UpdateMovementComplete?.Invoke(this, System.EventArgs.Empty);
                if (_movementStyleModifierMessageToken.Count > 0) _movementStyleModifierMessageToken.Invoke(_onUpdateCompleteFunctor);
                _inUpdateSequence = false;
                this.DoDelayedStyleChange();
            }
            finally
            {
                _inUpdateSequence = false;
            }
        }

        protected virtual void FixedUpdate()
        {
            if (_current == null || !_current.PrefersFixedUpdate) return;

            try
            {
                _inUpdateSequence = true;
                this.BeforeUpdateMovement?.Invoke(this, System.EventArgs.Empty);
                if (_movementStyleModifierMessageToken.Count > 0) _movementStyleModifierMessageToken.Invoke(_onBeforeUpdateFunctor);

                _current.UpdateMovement();

                this.UpdateMovementComplete?.Invoke(this, System.EventArgs.Empty);
                if (_movementStyleModifierMessageToken.Count > 0) _movementStyleModifierMessageToken.Invoke(_onUpdateCompleteFunctor);
                _inUpdateSequence = false;
                this.DoDelayedStyleChange();
            }
            finally
            {
                _inUpdateSequence = false;
            }
        }

        public void ResolveUpdateLoopSequencing() { }
#else
        void IUpdateable.Update()
        {
            try
            {
                _inUpdateSequence = true;
                this.BeforeUpdateMovement?.Invoke(this, System.EventArgs.Empty);
                if (_movementStyleModifierMessageToken.Count > 0) _movementStyleModifierMessageToken.Invoke(_onBeforeUpdateFunctor);

                _current.UpdateMovement();

                this.UpdateMovementComplete?.Invoke(this, System.EventArgs.Empty);
                if (_movementStyleModifierMessageToken.Count > 0) _movementStyleModifierMessageToken.Invoke(_onUpdateCompleteFunctor);
                _inUpdateSequence = false;
                this.DoDelayedStyleChange();
            }
            finally
            {
                _inUpdateSequence = false;
            }
        }

        public void ResolveUpdateLoopSequencing()
        {
            _currentPump?.Remove(this);
            _currentPump = null;
            if (!_safeEnabled) return;

            if (_current != null)
            {
                switch (_updateOrder)
                {
                    case UpdateOrder.Standard:
                        _currentPump = _current.PrefersFixedUpdate ? GameLoop.FixedUpdatePump : GameLoop.UpdatePump;
                        break;
                    case UpdateOrder.Early:
                        _currentPump = _current.PrefersFixedUpdate ? GameLoop.EarlyFixedUpdatePump : GameLoop.EarlyUpdatePump;
                        break;
                    case UpdateOrder.Tardy:
                        _currentPump = _current.PrefersFixedUpdate ? GameLoop.TardyFixedUpdatePump : GameLoop.TardyUpdatePump;
                        break;
                    default:
                        _currentPump = _current.PrefersFixedUpdate ? GameLoop.FixedUpdatePump : GameLoop.UpdatePump;
                        break;
                }
                _currentPump.Add(this);
            }
        }
#endif

        #endregion

        #region Methods

        private void ChangeState_Imp(IMovementStyle style, float precedence, bool dumpStack)
        {
            if (_inUpdateSequence)
            {
                //test if we should replace the last ChangeStyle call... test if null so that one can ChangeStyle with a NegativeInfinity precendance
                if (precedence >= _changeStyleDelayedPrecedence || _changeStyleDelayed == null)
                {
                    _changeStyleDelayedPrecedence = precedence;
                    _changeStyleDelayed = delegate ()
                    {
                        if (dumpStack)
                        {
                            var e = _styleStack.GetEnumerator();
                            while (e.MoveNext())
                            {
                                if (e.Current != null & e.Current != style) e.Current.OnPurgedFromStack();
                            }
                            _styleStack.Clear();
                        }

                        this.SwapStateOut(style);
                    };
                }
            }
            else
            {
                if (dumpStack)
                {
                    var e = _styleStack.GetEnumerator();
                    while (e.MoveNext())
                    {
                        if (e.Current != null && e.Current != style) e.Current.OnPurgedFromStack();
                    }
                    _styleStack.Clear();
                }

                this.SwapStateOut(style);
            }
        }

        private void SwapStateOut(IMovementStyle style)
        {
            if (object.Equals(style, _current)) return;

            var oldState = _current;
            _current = style;
            this.ResolveUpdateLoopSequencing();

            if (oldState != null) oldState.OnDeactivate(style, _stackingState ? ActivationReason.Stacking : ActivationReason.Standard);
            if (style != null) style.OnActivate(oldState, _stackingState ? ActivationReason.Stacking : ActivationReason.Standard);

            //if (this.StateChanged != null) this.StateChanged(this, new StateChangedEventArgs<IMovementStyle>(oldState, style));
            if (this.StyleChanged != null) this.StyleChanged(this, new StyleChangedEventArgs(oldState, style, _stackingState));
        }

        private void DoDelayedStyleChange()
        {
            if (_changeStyleDelayed != null)
            {
                _changeStyleDelayed();
                _changeStyleDelayed = null;
                _changeStyleDelayedPrecedence = float.NegativeInfinity;
            }

            if (_stackStyleDelayed.HasEntries)
            {
                _stackStyleDelayed.Invoke();
                _stackStyleDelayed.Clear();
            }
        }

        #endregion

        #region State Machine Interface

        public bool Contains(IMovementStyle state)
        {
            return state.gameObject == this.gameObject;
        }

        public IMovementStyle ChangeState(IMovementStyle style, float precedence = 0)
        {
            if (!object.ReferenceEquals(style, null) && !this.Contains(style)) throw new System.ArgumentException("MovementStyle '" + style.GetType().Name + "' is not a member of the state machine.", "style");

            this.ChangeState_Imp(style, precedence, true);
            return style;
        }




        public IMovementStyle ReleaseCurrentState(ReleaseMode mode, float precedence = 0f)
        {
            switch (mode)
            {
                case ReleaseMode.Pop:
                    if (_styleStack.Count > 0)
                        return this.PopState(precedence);
                    else
                        return _current;
                case ReleaseMode.PopAll:
                    if (_styleStack.Count > 0)
                        return this.PopAllStates(precedence);
                    else
                        return _current;
                case ReleaseMode.Default:
                    return this.ChangeState(_defaultMovementStyle as IMovementStyle, precedence);
                case ReleaseMode.Null:
                    this.ChangeStateToNull(precedence);
                    return null;
                case ReleaseMode.PopOrDefault:
                    if (_styleStack.Count > 0)
                        return this.PopState(precedence);
                    else
                        return this.ChangeState(_defaultMovementStyle as IMovementStyle, precedence);
                case ReleaseMode.PopAllOrDefault:
                    if (_styleStack.Count > 0)
                        return this.PopAllStates(precedence);
                    else
                        return this.ChangeState(_defaultMovementStyle as IMovementStyle, precedence);
                case ReleaseMode.PopOrNull:
                    if (_styleStack.Count > 0)
                        return this.PopState(precedence);
                    else
                    {
                        this.ChangeStateToNull(precedence);
                        return null;
                    }
                case ReleaseMode.PopAllOrNull:
                    if (_styleStack.Count > 0)
                        return this.PopAllStates(precedence);
                    else
                    {
                        this.ChangeStateToNull(precedence);
                        return null;
                    }

            }

            throw new System.ArgumentException("ReleaseMode was of an indeterminate configuration.");
        }

        public void ChangeStateToNull(float precedence = 0f)
        {
            this.ChangeState((IMovementStyle)null, precedence);
        }

        #endregion

        #region Stacked State Interface

        public IMovementStyle StackState(IMovementStyle style, float precedence = 0)
        {
            if (!object.ReferenceEquals(style, null) && !this.Contains(style)) throw new System.ArgumentException("MovementStyle '" + style.GetType().Name + "' is not a member of the state machine.", "style");

            if (_inUpdateSequence)
            {
                //test if we should replace the last ChangeStyle call... test if null so that one can ChangeStyle with a NegativeInfinity precendance
                if (precedence >= _changeStyleDelayedPrecedence || _changeStyleDelayed == null)
                {
                    _stackStyleDelayed.Add(() =>
                    {
                        if (this.Current == style) return;
                        _styleStack.Push(this.Current);
                        _stackingState = true;
                        this.ChangeState_Imp(style, precedence, false);
                        _stackingState = false;
                    }, precedence);
                }
            }
            else
            {
                if (this.Current == style) return style;
                _styleStack.Push(this.Current);
                _stackingState = true;
                this.ChangeState_Imp(style, precedence, false);
                _stackingState = false;
            }

            return style;
        }

        public IMovementStyle PopState(float precedence = 0f)
        {
            if (_styleStack.Count > 0)
            {
                var style = _styleStack.Pop();
                while (_styleStack.Count > 0 && !ReferenceEquals(style, null) && (style.IsDestroyed() || !this.Contains(style)))
                {
                    style = _styleStack.Pop();
                }
                if (!ReferenceEquals(style, null) && (style.IsDestroyed() || !this.Contains(style)))
                {
                    style = null;
                }
                this.ChangeState_Imp(style, precedence, false);
                return style;
            }
            else
            {
                return null;
            }
        }

        public IMovementStyle PopAllStates(float precedence = 0f)
        {
            if (_styleStack.Count > 0)
            {
                var style = _styleStack.Unshift(); //get the 0 entry, removing it from the deque
                if (!ReferenceEquals(style, null) && (style.IsDestroyed() || !this.Contains(style)))
                {
                    style = null;
                }
                this.ChangeState_Imp(style, precedence, true);
                return style;
            }
            else
            {
                return null;
            }
        }



        public void PurgeStackedState(IMovementStyle style)
        {
            if (object.ReferenceEquals(style, null)) throw new System.ArgumentNullException("style");

            if (_styleStack.Count > 0)
            {
                if (object.ReferenceEquals(_current, style))
                {
                    this.PopState();
                }
                else
                {
                    int index = _styleStack.IndexOf(style);
                    if (index > 0)
                    {
                        _styleStack.RemoveAt(index);
                        style.OnPurgedFromStack();
                    }
                }
            }
            else if (object.ReferenceEquals(_current, style))
            {
                this.ChangeStateToNull(float.NegativeInfinity);
            }
        }

        public void ChangeCurrentUnstackedState(IMovementStyle style)
        {
            if (_styleStack.Count > 0)
            {
                var oldStyle = _styleStack[0];
                if (oldStyle == style) return;

                _styleStack[0] = style;
                if (oldStyle != null) oldStyle.OnPurgedFromStack();
            }
            else
            {
                this.ChangeState(style);
            }
        }

        #endregion

        #region Special Types

        public enum ReleaseMode
        {
            Pop = 1,
            PopAll = 2,
            Default = 3,
            Null = 1 << 31,
            PopOrDefault = 4,
            PopAllOrDefault = 5,
            PopOrNull = Pop | Null,
            PopAllOrNull = PopAll | Null
        }

        #endregion

    }

}
