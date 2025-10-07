using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Motor;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Motor
{

    /// <summary>
    /// Represents a MovementStyle that operates regardless of if a MovementStyleController is present or not, or even a IMotor. 
    /// Most MovementStyles rely on the MovementStyleController state machine to manage when it is enabled or not, and an IMotor. 
    /// to handle appropriately calculate translation on the call to 'Move'.
    /// 
    /// Some MovementStyles though may only need
    /// </summary>
    public abstract class DumbMovementStyle : SPEntityComponent, IMovementStyle, IMStartOrEnableReceiver
    {

        public enum UpdateMode
        {
            Inactive = 0,
            MovementController = 1,
            MotorOnly = 2,
            CharacterController = 3,
            DumbRigidbody = 4,
            DumbTransformOnly = 5
        }

        protected virtual void OnHitSomething(Collider c)
        {
        }
        protected virtual void OnHitSomething(Collision c)
        {
        }

        #region Fields

        [SerializeField]
        private bool _activateOnStart;

        [System.NonSerialized()]
        private UpdateMode _mode;

        [System.NonSerialized()]
        private IMotor _motor;
        [System.NonSerialized()]
        private MovementStyleController _controller;
        [System.NonSerialized()]
        private CharacterController _charController;
        [System.NonSerialized()]
        private Rigidbody _rigidbody;

        [System.NonSerialized()]
        private RadicalCoroutine _routine;

        [System.NonSerialized()]
        private Vector3 _lastPos;
        [System.NonSerialized()]
        private Vector3 _dumbVel;


        [System.NonSerialized()]
        private bool _activeStatus;
        [System.NonSerialized()]
        private bool _paused;
        [System.NonSerialized]
        private bool _rigidbodyKinematicCache;

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();

            this.DetermineUpdateMode();
        }

        protected virtual void OnStartOrEnable()
        {
            if (_activateOnStart && _mode > UpdateMode.MovementController)
            {
                this.MakeActiveStyle();
            }
        }
        void IMStartOrEnableReceiver.OnStartOrEnable() { this.OnStartOrEnable(); }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (GameLoop.ApplicationClosing) return;

            if (_activeStatus)
            {
                this.DeactivateStyle();
            }
        }

        #endregion

        #region Properties

        public bool ActivateOnStart
        {
            get { return _activateOnStart; }
            set { _activateOnStart = value; }
        }

        public IMotor Motor { get { return _motor; } }

        public MovementStyleController Controller { get { return _controller; } }

        public CharacterController CharacterController { get { return _charController; } }

        public Rigidbody Rigidbody { get { return _rigidbody; } }

        public UpdateMode Mode { get { return _mode; } }

        public bool PrefersFixedUpdate
        {
            get
            {
                switch (_mode)
                {
                    case UpdateMode.MovementController:
                    case UpdateMode.MotorOnly:
                        return _motor.PrefersFixedUpdate;
                    case UpdateMode.CharacterController:
                        return false;
                    case UpdateMode.DumbRigidbody:
                        return true;
                    case UpdateMode.DumbTransformOnly:
                    default:
                        return false;
                }
            }
        }

        public Vector3 Position
        {
            get
            {
                switch (_mode)
                {
                    case UpdateMode.MovementController:
                    case UpdateMode.MotorOnly:
                        return _controller.transform.position;
                    case UpdateMode.CharacterController:
                        return _charController.transform.position;
                    case UpdateMode.DumbRigidbody:
                        return _rigidbody.position;
                    case UpdateMode.DumbTransformOnly:
                    default:
                        return this.entityRoot.transform.position;
                }
            }
            set
            {
                switch (_mode)
                {
                    case UpdateMode.MovementController:
                    case UpdateMode.MotorOnly:
                        _controller.transform.position = value;
                        break;
                    case UpdateMode.CharacterController:
                        _charController.transform.position = value;
                        break;
                    case UpdateMode.DumbRigidbody:
                        _rigidbody.position = value;
                        break;
                    case UpdateMode.DumbTransformOnly:
                    default:
                        this.entityRoot.transform.position = value;
                        break;
                }
            }
        }

        public Vector3 Velocity
        {
            get
            {
                switch (_mode)
                {
                    case UpdateMode.MovementController:
                    case UpdateMode.MotorOnly:
                        return _motor.Velocity;
                    case UpdateMode.CharacterController:
                        return _charController.velocity;
                    case UpdateMode.DumbRigidbody:
                        if (_rigidbody.isKinematic)
                        {
                            return _dumbVel;
                        }
                        else
                        {
#if UNITY_2023_3_OR_NEWER
                            return _rigidbody.linearVelocity;
#else
                            return _rigidbody.velocity;
#endif
                        }
                    case UpdateMode.DumbTransformOnly:
                        return _dumbVel;
                    default:
                        return Vector3.zero;
                }
            }
            set
            {
                switch (_mode)
                {
                    case UpdateMode.MovementController:
                    case UpdateMode.MotorOnly:
                        _motor.Velocity = value;
                        break;
                    case UpdateMode.CharacterController:
                        //do nothing
                        break;
                    case UpdateMode.DumbRigidbody:
                        if (_rigidbody.isKinematic)
                        {
                            _dumbVel = value;
                        }
                        else
                        {
#if UNITY_2023_3_OR_NEWER
                            _rigidbody.linearVelocity = value;
#else
                            _rigidbody.velocity = value;
#endif
                        }
                        break;
                    case UpdateMode.DumbTransformOnly:
                        _dumbVel = value;
                        break;
                }
            }
        }

        public bool Paused
        {
            get { return _paused; }
        }

        #endregion

        #region Methods

        public void MakeActiveStyle(bool stackState = false)
        {
            if (!this.isActiveAndEnabled) return;

            if (_mode == UpdateMode.MovementController)
            {
                if (stackState)
                    _controller.StackState(this);
                else
                    _controller.ChangeState(this);
            }
            else
            {
                _activeStatus = true;
                if (_routine == null || _routine.Finished)
                    _routine = this.StartRadicalCoroutine(this.SelfUpdateRoutine(), RadicalCoroutineDisableMode.Pauses);
                else if (_routine.OperatingState == RadicalCoroutineOperatingState.Inactive)
                    _routine.Start(this, RadicalCoroutineDisableMode.Pauses);
            }
        }

        public void DeactivateStyle(MovementStyleController.ReleaseMode mode = MovementStyleController.ReleaseMode.PopAllOrDefault, float precedence = 0f)
        {
            if (!_activeStatus) return;

            if (_mode == UpdateMode.MovementController)
            {
                _controller.ReleaseCurrentState(mode, precedence);
            }
            else
            {
                if (_routine != null)
                {
                    _routine.Stop();
                }
                if (_paused) this.Pause(false);
                this.OnDeactivate(null, ActivationReason.Standard);
                _activeStatus = false;
            }
            _mode = UpdateMode.Inactive;
        }

        /// <summary>
        /// Puts the movement style into a state that it no longer moves the entity, if there is a non-kinematic rigidbody to the component, it is set to kinematic. 
        /// This will only work if this style is currently active, and will automatically be unpaused on deactivate.
        /// </summary>
        public void Pause(bool pause)
        {
            if (pause)
            {
                if (!_paused)
                {
                    if (!this.IsActiveStyle) return;

                    switch (_mode)
                    {
                        case UpdateMode.Inactive:
                            //do nothing
                            break;
                        case UpdateMode.MovementController:
                        case UpdateMode.MotorOnly:
                            _paused = true;
                            //_controller.Pause(true);
                            break;
                        case UpdateMode.CharacterController:
                            //do nothing
                            break;
                        case UpdateMode.DumbRigidbody:
                            if (!_rigidbody.isKinematic)
                            {
                                _paused = true;
                                _rigidbodyKinematicCache = _rigidbody.isKinematic;
                                _rigidbody.isKinematic = true;
                            }
                            break;
                        case UpdateMode.DumbTransformOnly:
                            //do nothing
                            break;
                    }
                }
            }
            else
            {
                if (_paused)
                {
                    _paused = false;
                    switch (_mode)
                    {
                        case UpdateMode.Inactive:
                            //do nothing
                            break;
                        case UpdateMode.MovementController:
                        case UpdateMode.MotorOnly:
                            //_controller.Pause(false);
                            break;
                        case UpdateMode.CharacterController:
                            //do nothing
                            break;
                        case UpdateMode.DumbRigidbody:
                            _rigidbody.isKinematic = _rigidbodyKinematicCache;
                            break;
                        case UpdateMode.DumbTransformOnly:
                            //do nothing
                            break;
                    }
                }
            }
        }

        protected UpdateMode DetermineUpdateMode()
        {
            _controller = this.GetComponent<MovementStyleController>(); //movementstylecontroller must be on the same gameobject
            _motor = this.entityRoot.GetComponent<IMotor>();
            _charController = this.entityRoot.GetComponent<CharacterController>();
            _rigidbody = this.entityRoot.GetComponent<Rigidbody>();

            if (_controller != null && _motor != null)
            {
                _mode = UpdateMode.MovementController;
            }
            else if (_motor != null)
            {
                _mode = UpdateMode.MotorOnly;
            }
            else if (_charController != null)
            {
                _mode = UpdateMode.CharacterController;
            }
            else if (_rigidbody != null)
            {
                _mode = UpdateMode.DumbRigidbody;
            }
            else
            {
                _mode = UpdateMode.DumbTransformOnly;
            }

            return _mode;
        }

        private System.Collections.IEnumerator SelfUpdateRoutine()
        {
            var fixedYieldInstruct = new WaitForFixedUpdate();
            this.OnActivate(null, ActivationReason.Standard);

        Loop:
            switch (_mode)
            {
                case UpdateMode.MotorOnly:
                    try
                    {
                        this.UpdateMovement();
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(ex, this);
                    }
                    finally
                    {
                    }
                    yield return (_motor.PrefersFixedUpdate) ? fixedYieldInstruct : null;
                    break;

                case UpdateMode.DumbRigidbody:
                    if (_rigidbody.isKinematic)
                    {
                        yield return fixedYieldInstruct;
                        _lastPos = _rigidbody.position;
                        this.UpdateMovement();
                        _dumbVel = (_rigidbody.position - _lastPos) / Time.deltaTime;
                    }
                    else
                    {
                        yield return fixedYieldInstruct;
                        this.UpdateMovement();
                    }
                    break;

                case UpdateMode.DumbTransformOnly:
                    _lastPos = this.entityRoot.transform.position;
                    this.UpdateMovement();
                    _dumbVel = (this.entityRoot.transform.position - _lastPos) / Time.deltaTime;
                    yield return null;
                    break;
            }

            goto Loop;
        }

        protected abstract void UpdateMovement();

        protected virtual void OnActivate(IMovementStyle lastStyle, ActivationReason reason)
        {

        }

        protected virtual void OnDeactivate(IMovementStyle nextStyle, ActivationReason reason)
        {

        }

        protected virtual void OnPurgedFromStack()
        {

        }

        #endregion

        #region IMovementStyle Interface

        void IMovementStyle.OnActivate(IMovementStyle lastStyle, ActivationReason reason)
        {
            if (_mode == UpdateMode.Inactive) this.DetermineUpdateMode();
            _activeStatus = true;
            this.OnActivate(lastStyle, reason);
        }

        void IMovementStyle.OnDeactivate(IMovementStyle nextStyle, ActivationReason reason)
        {
            if (_paused) this.Pause(false);
            this.OnDeactivate(nextStyle, reason);
            _activeStatus = false;
        }

        void IMovementStyle.OnPurgedFromStack()
        {
            this.OnPurgedFromStack();
        }

        void IMovementStyle.UpdateMovement()
        {
            this.UpdateMovement();
        }

        #endregion

        #region Movement Interface

        public bool IsActiveStyle { get { return _activeStatus; } }

        public void Move(Vector3 mv)
        {
            if (_paused) return;

            switch (this.Mode)
            {
                case UpdateMode.MovementController:
                    _motor.Move(mv);
                    break;

                case UpdateMode.MotorOnly:
                    _motor.Move(mv);
                    break;
                case UpdateMode.CharacterController:
                    if (_charController.Move(mv) != CollisionFlags.None)
                    {
                        this.OnHitSomething((Collider)null);
                    }
                    break;
                case UpdateMode.DumbRigidbody:
                    if (_rigidbody.isKinematic)
                    {
                        _rigidbody.MovePosition(_rigidbody.position + mv);
                    }
                    else
                    {
#if UNITY_2023_3_OR_NEWER
                        _rigidbody.linearVelocity = mv / Time.deltaTime;
#else
                        _rigidbody.velocity = mv / Time.deltaTime;
#endif
                    }
                    break;
                case UpdateMode.DumbTransformOnly:
                    this.entityRoot.transform.Translate(mv);

                    break;
            }
        }

        public void MovePosition(Vector3 pos)
        {
            if (_paused) return;

            switch (this.Mode)
            {
                case UpdateMode.MovementController:
                    _motor.Move(pos - _motor.transform.position);
                    break;

                case UpdateMode.MotorOnly:
                    _motor.Move(pos - _controller.transform.position);
                    break;
                case UpdateMode.CharacterController:
                    if (_charController.Move(pos - _charController.transform.position) != CollisionFlags.None)
                    {
                        this.OnHitSomething((Collider)null);
                    }
                    break;
                case UpdateMode.DumbRigidbody:
                    _rigidbody.MovePosition(pos);
                    break;
                case UpdateMode.DumbTransformOnly:
                    this.entityRoot.transform.position = pos;

                    break;
            }
        }

        #endregion

        /*

        #region IIgnorableCollision Interface

        public void IgnoreCollision(Collider coll, bool ignore)
        {
            switch (_mode)
            {
                case UpdateMode.Motor:
                case UpdateMode.MovementControllerOnly:
                    _controller.IgnoreCollision(coll, ignore);
                    break;
                case UpdateMode.CharacterController:
                    Physics.IgnoreCollision(_charController, coll, ignore);
                    break;
                case UpdateMode.DumbRigidbody:
                    IgnorableRigidbody.GetIgnorableCollision(_rigidbody).IgnoreCollision(coll, ignore);
                    break;
            }
        }

        public void IgnoreCollision(IIgnorableCollision coll, bool ignore)
        {
            switch (_mode)
            {
                case UpdateMode.Motor:
                case UpdateMode.MovementControllerOnly:
                    _controller.IgnoreCollision(coll, ignore);
                    break;
                case UpdateMode.CharacterController:
                    coll.IgnoreCollision(_charController, ignore);
                    break;
                case UpdateMode.DumbRigidbody:
                    IgnorableRigidbody.GetIgnorableCollision(_rigidbody).IgnoreCollision(coll, ignore);
                    break;
            }
        }

        #endregion

        */

    }

}
