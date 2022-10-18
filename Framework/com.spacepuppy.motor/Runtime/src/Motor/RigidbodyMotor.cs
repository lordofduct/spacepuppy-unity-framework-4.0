using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Collections;
using com.spacepuppy.Geom;
using com.spacepuppy.Hooks;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Motor
{

    /// <summary>
    /// IMotor interface for a Rigidbody that treats the Rigidbody with out forces.
    /// 
    /// Rigidbody.MovePosition is used to move the Rigidbody around.
    /// </summary>
    public class RigidbodyMotor : SPComponent, IMotor, IUpdateable, ISignalEnabledMessageHandler
    {

        #region Fields

        [SerializeField]
        [DefaultFromSelf(Relativity = EntityRelativity.Entity)]
        private Rigidbody _rigidbody;
        [SerializeField]
        [OneOrMany]
        [Tooltip("Colliders considered associated with this Motor, leave empty if this should be auto-associated at Awake.")]
        private Collider[] _colliders;

        [SerializeField()]
        private float _stepOffset;
        [SerializeField()]
        private float _skinWidth;
        [SerializeField]
        [Tooltip("The velocity of the Rigidbody is locked to (0,0,0) so that it can't be moved around. The motor's velocity still reflects its motion applied to it.")]
        private bool _constrainSimulatedRigidbodyVelocity = true;
        [SerializeField]
        private bool _paused;

        [System.NonSerialized()]
        private Vector3 _vel;
        [System.NonSerialized()]
        private Vector3 _lastPos;
        [System.NonSerialized()]
        private Vector3 _lastVel;

        [System.NonSerialized()]
        private Vector3 _talliedMove;


        [System.NonSerialized()]
        private bool _moveCalledLastFrame;
        [System.NonSerialized()]
        private Vector3 _fullTalliedMove;

        [System.NonSerialized]
        private Messaging.MessageToken<IMotorCollisionMessageHandler> _onCollisionMessage;
        [System.NonSerialized]
        private CollisionHooks _collisionHook;

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();

            if (!object.ReferenceEquals(_rigidbody, null) && _colliders == null || _colliders.Length == 0)
            {
                _colliders = _rigidbody.GetComponentsInChildren<Collider>();
            }
            _onCollisionMessage = Messaging.CreateBroadcastToken<IMotorCollisionMessageHandler>(this.gameObject);
        }

        protected override void OnEnable()
        {
            if (object.ReferenceEquals(_rigidbody, null)) throw new System.InvalidOperationException("RigidbodyMotor must be initialized with an appropriate Rigidbody.");

            base.OnEnable();

            _rigidbody.isKinematic = false;
            _vel = Vector3.zero;
            _moveCalledLastFrame = false;
            _talliedMove = Vector3.zero;
            _fullTalliedMove = Vector3.zero;

            GameLoop.TardyFixedUpdatePump.Add(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            GameLoop.TardyFixedUpdatePump.Remove(this);
        }

        #endregion

        #region Properties

        public Rigidbody Rigidbody
        {
            get { return _rigidbody; }
            set { _rigidbody = value; }
        }

        public Collider[] Colliders
        {
            get { return _colliders; }
            set { _colliders = value ?? ArrayUtil.Empty<Collider>(); }
        }

        #endregion

        #region IMotor Interface

        public bool PrefersFixedUpdate
        {
            get { return true; }
        }

        public float Mass
        {
            get
            {
                return !object.ReferenceEquals(_rigidbody, null) ? _rigidbody.mass : 0f;
            }
            set
            {
                if (!object.ReferenceEquals(_rigidbody, null)) _rigidbody.mass = value;
            }
        }

        public float StepOffset
        {
            get
            {
                return _stepOffset;
            }
            set
            {
                _stepOffset = Mathf.Max(value, 0f);
            }
        }

        public float SkinWidth
        {
            get
            {
                return _skinWidth;
            }
            set
            {
                _skinWidth = Mathf.Max(value, 0f);
            }
        }

        public bool ConstrainSimulatedRigidbodyVelocity
        {
            get => _constrainSimulatedRigidbodyVelocity;
            set => _constrainSimulatedRigidbodyVelocity = value;
        }

        public bool CollisionEnabled
        {
            get
            {
                for (int i = 0; i < _colliders.Length; i++)
                {
                    if (!_colliders[i].enabled) return false;
                }
                return true;
            }
            set
            {
                for (int i = 0; i < _colliders.Length; i++)
                {
                    _colliders[i].enabled = value;
                }
            }
        }

        public bool Paused { get { return _paused; } set { _paused = value; } }

        public Vector3 Velocity
        {
            get { return _vel; }
            set { _vel = value; }
        }

        public Vector3 Position
        {
            get
            {
                return !object.ReferenceEquals(_rigidbody, null) ? _rigidbody.position : Vector3.zero;
            }
            set
            {
                if (object.ReferenceEquals(_rigidbody, null)) throw new System.InvalidOperationException("RigidbodyMotor must be initialized with an appropriate Rigidbody.");

                _rigidbody.position = value;
            }
        }

        public Vector3 LastPosition
        {
            get { return _lastPos; }
        }

        public Vector3 LastVelocity
        {
            get { return _lastVel; }
        }



        public void Move(Vector3 mv)
        {
            if (object.ReferenceEquals(_rigidbody, null)) throw new System.InvalidOperationException("RigidbodyMotor must be initialized with an appropriate Rigidbody.");
            if (_paused) return;

            _fullTalliedMove += mv;
            _talliedMove += mv;
            _vel = _talliedMove / Time.deltaTime;
            _moveCalledLastFrame = true;
        }

        public void AtypicalMove(Vector3 mv)
        {
            if (object.ReferenceEquals(_rigidbody, null)) throw new System.InvalidOperationException("RigidbodyMotor must be initialized with an appropriate Rigidbody.");
            if (_paused) return;

            _fullTalliedMove += mv;
            _moveCalledLastFrame = true;
        }

        public void MovePosition(Vector3 pos, bool setVelocityByChangeInPosition = false)
        {
            if (object.ReferenceEquals(_rigidbody, null)) throw new System.InvalidOperationException("RigidbodyMotor must be initialized with an appropriate Rigidbody.");
            if (_paused) return;

            var mv = (pos - _rigidbody.position);
            _fullTalliedMove += mv;
            _talliedMove += mv;
            _vel = _talliedMove / Time.deltaTime;
            _moveCalledLastFrame = true;
        }

        #endregion

        #region IForceReceiver Interface

        public void AddForce(Vector3 f, ForceMode mode)
        {
            if (object.ReferenceEquals(_rigidbody, null)) throw new System.InvalidOperationException("RigidbodyMotor must be initialized with an appropriate Rigidbody.");
            if (_paused) return;

            switch (mode)
            {
                case ForceMode.Force:
                    //force = mass*distance/time^2
                    //distance = force * time^2 / mass
                    this.Move(f * Time.deltaTime * Time.deltaTime / this.Mass);
                    break;
                case ForceMode.Acceleration:
                    //acceleration = distance/time^2
                    //distance = acceleration * time^2
                    this.Move(f * (Time.deltaTime * Time.deltaTime));
                    break;
                case ForceMode.Impulse:
                    //impulse = mass*distance/time
                    //distance = impulse * time / mass
                    this.Move(f * Time.deltaTime / this.Mass);
                    break;
                case ForceMode.VelocityChange:
                    //velocity = distance/time
                    //distance = velocity * time
                    this.Move(f * Time.deltaTime);
                    break;
            }
        }

        public void AddForceAtPosition(Vector3 f, Vector3 pos, ForceMode mode)
        {
            this.AddForce(f, mode);
        }

        public void AddExplosionForce(float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModifier = 0f, ForceMode mode = ForceMode.Force)
        {
            if (_paused) return;

            var v = _rigidbody.centerOfMass - explosionPosition;
            var force = v.normalized * Mathf.Clamp01(v.magnitude / explosionRadius) * explosionForce;
            //TODO - apply upwards modifier

            this.AddForce(force, mode);
        }

        #endregion

        #region IPhysicsObject Interface

        public bool TestOverlap(int layerMask, QueryTriggerInteraction query)
        {
            foreach (var c in _colliders)
            {
                if (GeomUtil.GetGeom(c).TestOverlap(layerMask, query)) return true;
            }

            return false;
        }

        public int Overlap(ICollection<Collider> results, int layerMask, QueryTriggerInteraction query)
        {
            if (results == null) throw new System.ArgumentNullException("results");

            using (var set = TempCollection.GetSet<Collider>())
            {
                foreach (var c in _colliders)
                {
                    GeomUtil.GetGeom(c).Overlap(set, layerMask, query);
                }

                if (set.Count > 0)
                {
                    var e = set.GetEnumerator();
                    while (e.MoveNext())
                    {
                        results.Add(e.Current);
                    }
                    return set.Count;
                }
            }

            return 0;
        }

        public bool Cast(Vector3 direction, out RaycastHit hitinfo, float distance, int layerMask, QueryTriggerInteraction query)
        {
            foreach (var c in _colliders)
            {
                if (GeomUtil.GetGeom(c).Cast(direction, out hitinfo, distance, layerMask, query)) return true;
            }

            hitinfo = default(RaycastHit);
            return false;
        }

        public int CastAll(Vector3 direction, ICollection<RaycastHit> results, float distance, int layerMask, QueryTriggerInteraction query)
        {
            if (results == null) throw new System.ArgumentNullException("results");

            using (var set = TempCollection.GetSet<RaycastHit>())
            {
                foreach (var c in _colliders)
                {
                    GeomUtil.GetGeom(c).CastAll(direction, set, distance, layerMask, query);
                }

                if (set.Count > 0)
                {
                    var e = set.GetEnumerator();
                    while (e.MoveNext())
                    {
                        results.Add(e.Current);
                    }
                    return set.Count;
                }
            }

            return 0;
        }

        #endregion

        #region IUpdatable Interface

        void IUpdateable.Update()
        {
            _lastPos = _rigidbody.position;

            if (_moveCalledLastFrame)
            {
                //_rigidbody.velocity = Vector3.zero;
                _rigidbody.MovePosition(_rigidbody.position + _fullTalliedMove);
            }

            if (_constrainSimulatedRigidbodyVelocity)
            {
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
            }

            _vel = _talliedMove / Time.deltaTime;
            _lastVel = _vel;

            _moveCalledLastFrame = false;
            _fullTalliedMove = Vector3.zero;
            _talliedMove = Vector3.zero;
        }

        #endregion

        #region CollisionHandler Implementation

        private void ValidateCollisionHandler()
        {
            if (_collisionHook == null && _onCollisionMessage?.Count > 0)
            {
                _collisionHook = this.AddComponent<CollisionHooks>();
                _collisionHook.OnEnter += _collisionHook_ControllerColliderHit;
                _collisionHook.OnStay += _collisionHook_ControllerColliderHit;
                _collisionHook.OnExit += _collisionHook_ControllerColliderHit;
            }
        }

        private void _collisionHook_ControllerColliderHit(object sender, Collision hit)
        {
            if (_onCollisionMessage.Count > 0)
            {
                _onCollisionMessage.Invoke(new MotorCollisionInfo(this, hit), MotorCollisionHandlerHelper.OnCollisionFunctor);
            }
            else if (_collisionHook != null)
            {
                ObjUtil.SmartDestroy(_collisionHook);
                _collisionHook = null;
            }
        }

        void ISignalEnabledMessageHandler.OnComponentEnabled(IEventfulComponent component)
        {
            if (component is IMotorCollisionMessageHandler)
            {
                _onCollisionMessage?.SetDirty();
                this.ValidateCollisionHandler();
            }
        }

        void ISignalEnabledMessageHandler.OnComponentDisabled(IEventfulComponent component)
        {
            if (component is IMotorCollisionMessageHandler)
            {
                _onCollisionMessage?.SetDirty();
            }
        }

        #endregion

    }

}
