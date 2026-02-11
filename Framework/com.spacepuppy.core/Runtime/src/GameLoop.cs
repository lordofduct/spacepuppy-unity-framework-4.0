using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;
using com.spacepuppy.Hooks;

namespace com.spacepuppy
{
    /// <summary>
    /// An entry point into the gameloop.
    /// 
    /// This is registered on start of the game automatically. If you'd prefer to manually initialize it (or not initialize it at all, not-advised if using SP Framework) 
    /// just add MANUALLY_REGISTER_SPGAMELOOP as a define symbol in the 'Player Settings' (Edit->Project Settings->Player) then call Init on the main thread.
    /// </summary>
    /// <remarks>
    /// Currently in 4.0 we're diverging from the classic 'GameLoop' of earlier SP versions. We may refactor this later.
    /// This includes hooking into the new "PlayerLoop" interface Unity released in 2019.3
    /// </remarks>
    [DefaultExecutionOrder(GameLoop.DEFAULT_EXECUTION_ORDER)]
    [AddComponentMenu("")] //do not show in the 'Add Component' list
    public sealed class GameLoop : ServiceComponent<GameLoop>, IService
    {
        public const int DEFAULT_EXECUTION_ORDER = -32000;

        #region Events

        public static event System.EventHandler BeforeApplicationQuit;
        public static event System.EventHandler ApplicatinQuit;

        public static event System.EventHandler EarlyUpdate;
        public static event System.EventHandler OnUpdate;
        public static event System.EventHandler TardyUpdate;
        public static event System.EventHandler EarlyFixedUpdate;
        public static event System.EventHandler OnFixedUpdate;
        public static event System.EventHandler TardyFixedUpdate;
        public static event System.EventHandler EarlyLateUpdate;
        public static event System.EventHandler OnLateUpdate;
        public static event System.EventHandler TardyLateUpdate;

        #endregion

        #region Singleton Interface

        private const string SPECIAL_NAME = "Spacepuppy.GameLoop";
        private static GameLoop _instance;

        #endregion

        #region Fields

        private static UpdateSequence _currentSequence;
        private static QuitState _quitState;
        private static System.Action<bool> _internalEarlyUpdate;

        private ulong _frameCount = 0;
        private ulong _fixedFrameCount = 0;
        private UpdateEventHooks _updateHook;
        private TardyExecutionUpdateEventHooks _tardyUpdateHook;

        [System.NonSerialized]
        private static UpdatePump _earlyUpdatePump = new UpdatePump();
        [System.NonSerialized]
        private static UpdatePump _earlyFixedUpdatePump = new UpdatePump();
        [System.NonSerialized]
        private static UpdatePump _updatePump = new UpdatePump();
        [System.NonSerialized]
        private static UpdatePump _fixedUpdatePump = new UpdatePump();
        [System.NonSerialized]
        private static UpdatePump _lateUpdatePump = new UpdatePump();
        [System.NonSerialized]
        private static UpdatePump _tardyUpdatePump = new UpdatePump();
        [System.NonSerialized]
        private static UpdatePump _tardyFixedUpdatePump = new UpdatePump();

        [System.NonSerialized]
        private static com.spacepuppy.Async.InvokePump _updateInvokeHandle;
        [System.NonSerialized]
        private static com.spacepuppy.Async.InvokePump _lateUpdateInvokeHandle;
        [System.NonSerialized]
        private static com.spacepuppy.Async.InvokePump _fixedUpdateInvokeHandle;

        private static int _currentFrame;
        private static int _currentLateFrame;
        private static int _currentEndOfFrame;

        private Coroutine _eofCoroutine;

        #endregion

        #region CONSTRUCTOR

        private GameLoop() : base(Services.AutoRegisterOption.Register, Services.MultipleServiceResolutionOption.UnregisterSelf, Services.UnregisterResolutionOption.DestroySelf) { }

#if !MANUALLY_REGISTER_SPGAMELOOP
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        public static void Init()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("Registered Spacepuppy GameLoop");
#endif
            if (!object.ReferenceEquals(_instance, null))
            {
                if (ObjUtil.IsDestroyed(_instance))
                {
                    _instance = null;
                }
                else
                {
                    return;
                }
            }

            _instance = Services.Create<GameLoop>(true, SPECIAL_NAME);
        }

        protected override void OnValidAwake()
        {
            _instance = this;

            _updateInvokeHandle = new Async.InvokePump();
            _lateUpdateInvokeHandle = new Async.InvokePump();
            _fixedUpdateInvokeHandle = new Async.InvokePump();

            _currentSequence = UpdateSequence.None;
            _updateHook = this.gameObject.AddComponent<UpdateEventHooks>();
            _tardyUpdateHook = this.gameObject.AddComponent<TardyExecutionUpdateEventHooks>();

            _updateHook.UpdateHook += _updateHook_Update;
            _tardyUpdateHook.UpdateHook += _tardyUpdateHook_Update;

            _updateHook.FixedUpdateHook += _updateHook_FixedUpdate;
            _tardyUpdateHook.FixedUpdateHook += _tardyUpdateHook_FixedUpdate;

            _updateHook.LateUpdateHook += _updateHook_LateUpdate;
            _tardyUpdateHook.LateUpdateHook += _tardyUpdateHook_LateUpdate;
        }

        protected override void Start()
        {
            base.Start();

            if (!object.ReferenceEquals(_instance, this))
            {
                Destroy(this);
            }
            else
            {
                this.StartEndOfFrameRoutine();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (this.started)
            {
                this.StartEndOfFrameRoutine();
            }
        }

        protected override void OnDisable()
        {
            this.StopEndOfFrameRoutine();
            base.OnDisable();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Has the GameLoop service been initialized.
        /// </summary>
        public static bool Initialized { get { return _instance != null; } }

        /// <summary>
        /// Hook into the GameLoop MonoBehaviour. Can be used for invoking coroutines directly on the GameLoop (not advised unless you know what you're doing).
        /// </summary>
        public static GameLoop Hook
        {
            get
            {
                return _instance;
            }
        }

        /// <summary>
        /// Returns true if the caller is on a thread other than the main thread. 
        /// This will return false if GameLoop is not initialized.
        /// </summary>
        public static bool InvokeRequired => _updateInvokeHandle?.InvokeRequired ?? false;

        /// <summary>
        /// Returns true if the caller is on the main thread. 
        /// This will return false if GameLoop is not initialized.
        /// </summary>
        public static bool IsMainThread => !(_updateInvokeHandle?.InvokeRequired ?? true);

        /// <summary>
        /// The number of frames since this GameLoop was started. This doesn't necessarily 
        /// match the Time.frameCount since GameLoop can be initialized late. 
        /// </summary>
        public static int FrameCount => _instance ? (int)(_instance._frameCount & 0x7FFFFFFF) : 0;
        public static ulong FrameCountLong => _instance ? _instance._frameCount : 0;

        /// <summary>
        /// The number of FixedUpdate call since the GameLoop was started. 
        /// </summary>
        public static int FixedFrameCount => _instance ? (int)(_instance._fixedFrameCount & 0x7FFFFFFF) : 0;
        public static ulong FixedFrameCountLong => _instance ? _instance._fixedFrameCount : 0;

        /// <summary>
        /// Returns which event sequence that code is currently operating as. 
        /// WARNING - during 'OnMouseXXX' messages this will report that we're in the FixedUpdate sequence. 
        /// This is because there's no end of FixedUpdate available to hook into, so it reports FixedUpdate 
        /// until Update starts, and 'OnMouseXXX' occurs in between those 2.
        /// </summary>
        public static UpdateSequence CurrentSequence { get { return _currentSequence; } }

        /// <summary>
        /// The current QuitState during BeforeApplicationQuit event.
        /// </summary>
        public static QuitState QuitState { get { return _quitState; } }

        /// <summary>
        /// Returns true if the OnApplicationQuit message has been received.
        /// </summary>
        public static bool ApplicationClosing { get { return _quitState == QuitState.Quit; } }

        /// <summary>
        /// The first Update call that frame.
        /// </summary>
        public static UpdatePump EarlyUpdatePump { get { return _earlyUpdatePump; } }

        /// <summary>
        /// A general Update call for that frame (timing unknown).
        /// </summary>
        public static UpdatePump UpdatePump { get { return _updatePump; } }

        /// <summary>
        /// The last Update call that frame.
        /// </summary>
        public static UpdatePump TardyUpdatePump { get { return _tardyUpdatePump; } }

        /// <summary>
        /// The first FixedUpdate call that fixed update frame.
        /// </summary>
        public static UpdatePump EarlyFixedUpdatePump { get { return _earlyFixedUpdatePump; } }

        /// <summary>
        /// A general FixedUpdate call that fixed update frame.
        /// </summary>
        public static UpdatePump FixedUpdatePump { get { return _fixedUpdatePump; } }

        /// <summary>
        /// The last FixedUpdate call that fixed update frame.
        /// </summary>
        public static UpdatePump TardyFixedUpdatePump { get { return _tardyFixedUpdatePump; } }

        /// <summary>
        /// A general LateUpdate call for that frame.
        /// </summary>
        public static UpdatePump LateUpdatePump { get { return _lateUpdatePump; } }

        /// <summary>
        /// Used to schedule an action on the next Update regardless of threading.
        /// </summary>
        public static com.spacepuppy.Async.InvokePump UpdateHandle { get { return _updateInvokeHandle; } }

        /// <summary>
        /// Used to schedule an action on the next LateUpdate regardless of threading.
        /// </summary>
        public static com.spacepuppy.Async.InvokePump LateUpdateHandle { get { return _lateUpdateInvokeHandle; } }

        /// <summary>
        /// Used to schedule an action on the next FixedUpdate regardless of threading.
        /// </summary>
        public static com.spacepuppy.Async.InvokePump FixedUpdateHandle { get { return _fixedUpdateInvokeHandle; } }

        /// <summary>
        /// Returns true if the UpdatePump and Update event were ran.
        /// </summary>
        public static bool UpdateWasCalled
        {
            get { return _currentFrame == UnityEngine.Time.frameCount; }
        }

        /// <summary>
        /// Returns true if the LateUpdatePump and LateUpdate event were ran.
        /// </summary>
        public static bool LateUpdateWasCalled
        {
            get { return _currentLateFrame == UnityEngine.Time.frameCount; }
        }

        public static bool EndOfFrameWasCalled
        {
            get { return _currentEndOfFrame == UnityEngine.Time.frameCount; }
        }

        #endregion

        #region Methods

        public static void AssertMainThread()
        {
            if (_updateInvokeHandle.InvokeRequired)
            {
                throw new System.InvalidOperationException("Attempted to access thread dependent code from a thread other than the main thread.");
            }
        }

        /// <summary>
        /// Preferred method of closing application.
        /// </summary>
        public static void QuitApplication()
        {
            if (_quitState == QuitState.None)
            {
                _quitState = QuitState.BeforeQuit;
                if (BeforeApplicationQuit != null) BeforeApplicationQuit(_instance, System.EventArgs.Empty);

                if (_quitState == QuitState.BeforeQuit)
                {
                    //wasn't cancelled, or force quit
#if UNITY_EDITOR 
                    if (UnityEngine.Application.isEditor)
                    {
                        try
                        {
                            var tp = com.spacepuppy.Utils.TypeUtil.FindType("UnityEditor.EditorApplication");
                            tp.GetProperty("isPlaying").SetValue(null, false, null);
                        }
                        catch
                        {
                            UnityEngine.Debug.Log("Failed to stop play in editor.");
                        }
                    }
                    else
                    {
                        UnityEngine.Application.Quit();
                    }
#else
                    UnityEngine.Application.Quit();
#endif

                }
            }
        }

        /// <summary>
        /// If you listen to the BeforeApplicationQuit event, you can call this from that event's callback to cancel the quit of the application.
        /// </summary>
        public static void CancelQuit()
        {
            if (_quitState == QuitState.BeforeQuit)
            {
                _quitState = QuitState.None;
            }
        }

        public static void RegisterNextUpdate(IUpdateable obj)
        {
            if (UpdateWasCalled) _updatePump.Add(obj);
            else _updatePump.DelayedAdd(obj);
        }

        public static void RegisterNextLateUpdate(IUpdateable obj)
        {
            if (LateUpdateWasCalled) _lateUpdatePump.Add(obj);
            else _lateUpdatePump.DelayedAdd(obj);
        }

        /// <summary>
        /// A special static, register once, earlyupdate event hook that preceeds ALL other events. 
        /// This is used internally by some special static classes (namely SPTime) that needs extra 
        /// high precedence early access.
        /// </summary>
        /// <param name="d"></param>
        internal static void RegisterInternalEarlyUpdate(System.Action<bool> d)
        {
            _internalEarlyUpdate -= d;
            _internalEarlyUpdate += d;
        }

        internal static void UnregisterInternalEarlyUpdate(System.Action<bool> d)
        {
            _internalEarlyUpdate -= d;
        }

        #endregion

        #region Event Handlers

        private void OnApplicationQuit()
        {
            _quitState = QuitState.Quit;
            ApplicatinQuit?.Invoke(this, System.EventArgs.Empty);
        }

        //Update

        private void Update()
        {
            //Track entry into update loop
            _currentSequence = UpdateSequence.Update;
            _frameCount++;

            _internalEarlyUpdate?.Invoke(false);

            _earlyUpdatePump.Update();
            _updateInvokeHandle.Update();

            EarlyUpdate?.Invoke(this, System.EventArgs.Empty);
        }

        private void _updateHook_Update(object sender, System.EventArgs e)
        {
            _updatePump.Update();
            OnUpdate?.Invoke(this, e);
            _currentFrame = UnityEngine.Time.frameCount;
        }

        private void _tardyUpdateHook_Update(object sender, System.EventArgs e)
        {
            _tardyUpdatePump.Update();
            TardyUpdate?.Invoke(this, e);
        }

        //Fixed Update

        private void FixedUpdate()
        {
            //Track entry into fixedupdate loop
            _currentSequence = UpdateSequence.FixedUpdate;
            _fixedFrameCount++;

            _internalEarlyUpdate?.Invoke(true);

            _earlyFixedUpdatePump.Update();
            _fixedUpdateInvokeHandle.Update();

            EarlyFixedUpdate?.Invoke(this, System.EventArgs.Empty);
        }

        private void _updateHook_FixedUpdate(object sender, System.EventArgs e)
        {
            _fixedUpdatePump.Update();
            if (OnFixedUpdate != null) OnFixedUpdate(this, e);
        }

        private void _tardyUpdateHook_FixedUpdate(object sender, System.EventArgs e)
        {
            _tardyFixedUpdatePump.Update();
            TardyFixedUpdate?.Invoke(this, e);

            ////Track exit of fixedupdate loop
            //_currentSequence = UpdateSequence.None;
        }

        //LateUpdate

        private void LateUpdate()
        {
            if (!object.ReferenceEquals(_instance, this)) return;

            _currentSequence = UpdateSequence.LateUpdate;
            EarlyLateUpdate?.Invoke(this, System.EventArgs.Empty);
        }

        private void _updateHook_LateUpdate(object sender, System.EventArgs e)
        {
            if (!object.ReferenceEquals(_instance, this)) return;

            _lateUpdatePump.Update();
            OnLateUpdate?.Invoke(this, e);
            _currentLateFrame = Time.frameCount;
        }

        private void _tardyUpdateHook_LateUpdate(object sender, System.EventArgs e)
        {
            if (!object.ReferenceEquals(_instance, this)) return;

            _lateUpdateInvokeHandle.Update();
            TardyLateUpdate?.Invoke(this, e);

            //Track exit of update loop
            _currentSequence = UpdateSequence.None;
        }

        //EndOfFrame

        void StartEndOfFrameRoutine()
        {
            if (_eofCoroutine != null)
            {
                var r = _eofCoroutine;
                _eofCoroutine = null;
                this.StopCoroutine(r);
            }
            _eofCoroutine = this.StartCoroutine(this.EndOfFrameRoutine_Callback());
        }

        void StopEndOfFrameRoutine()
        {
            if (_eofCoroutine != null)
            {
                var r = _eofCoroutine;
                _eofCoroutine = null;
                this.StopCoroutine(r);
            }
        }

        System.Collections.IEnumerator EndOfFrameRoutine_Callback()
        {
            var eof = new WaitForEndOfFrame();
            while (this.isActiveAndEnabled)
            {
                yield return eof;
                _currentEndOfFrame = UnityEngine.Time.frameCount;
            }
        }

        #endregion

    }

}
