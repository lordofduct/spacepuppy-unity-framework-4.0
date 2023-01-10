namespace com.spacepuppy
{

    /// <summary>
    /// A yield instruction used by RadicalCoroutine that can also have a callback manually attached to it through the 'IRadicalWaitHandle.OnComplete' method. 
    /// Note that this OnComplete being called should not rely on the 'Tick' method to call as Manual/Task/UniTask won't necessarily 'tick' the instruction. 
    /// So if your waithandle is "polled", you should allow both IsComplete/Tick to behave as the polling method. 
    /// </summary>
    public interface IRadicalWaitHandle : IRadicalYieldInstruction
    {

        /// <summary>
        /// The completion of the wait handle was the result of it being cancelled.
        /// </summary>
        bool Cancelled { get; }

        /// <summary>
        /// Called when the wait handle completed. This includes being cancelled. Check 'Cancelled' to know why it completed.
        /// </summary>
        /// <param name="callback"></param>
        void OnComplete(System.Action<IRadicalWaitHandle> callback);

    }

    /// <summary>
    /// Base implemenation of IRadicalWaitHandle.
    /// </summary>
    public class RadicalWaitHandle : IRadicalWaitHandle, IPooledYieldInstruction, IRadicalEnumerator
    {

        #region Fields

        private bool _complete;
        private System.Action<IRadicalWaitHandle> _callback;

        #endregion

        #region CONSTRUCTOR

        protected RadicalWaitHandle()
        {

        }

        #endregion

        #region Methods

        public void SignalCancelled()
        {
            if (_complete) return;

            _complete = true;
            this.Cancelled = true;

            var d = _callback;
            d?.Invoke(this);
            _callback = null;
        }

        public void SignalComplete()
        {
            if (_complete) return;

            _complete = true;

            var d = _callback;
            d?.Invoke(this);
            _callback = null;
        }

        public void Reset()
        {
            _complete = false;
            this.Cancelled = false;
            _callback = null;
        }

        protected virtual bool Tick(out object yieldObject)
        {
            yieldObject = null;
            return !_complete;
        }

        #endregion

        #region IRadicalWaitHandle Interface

        public bool Cancelled
        {
            get;
            private set;
        }

        public bool IsComplete
        {
            get { return _complete; }
        }

        public void OnComplete(System.Action<IRadicalWaitHandle> callback)
        {
            if (callback == null) throw new System.ArgumentNullException("callback");
            if (_complete) throw new System.InvalidOperationException("Can not wait for complete on an already completed IRadicalWaitHandle.");
            _callback += callback;
        }

        bool IRadicalYieldInstruction.Tick(out object yieldObject)
        {
            if (_complete)
            {
                yieldObject = null;
                return false;
            }

            return this.Tick(out yieldObject);
        }

        #endregion

        #region IPooledYieldInstruction Interface

        public void Dispose()
        {
            if (this.GetType() == typeof(RadicalWaitHandle)) //we only release if the handle is directly a RadicalWaitHandle rather than one inherited from RadicalWaitHandle
            {
                _complete = true;
                this.Cancelled = false;
                _callback = null;
                _pool.Release(this);
            }
            else
            {
                this.Reset();
            }
        }

        #endregion

        #region IEnumerator Interface

        object System.Collections.IEnumerator.Current => null;

        bool System.Collections.IEnumerator.MoveNext()
        {
            object inst;
            return this.Tick(out inst);
        }

        void System.Collections.IEnumerator.Reset()
        {
            //do nothing
        }

        #endregion

        #region Static Interface

        private static com.spacepuppy.Collections.ObjectCachePool<RadicalWaitHandle> _pool = new com.spacepuppy.Collections.ObjectCachePool<RadicalWaitHandle>(-1, () => new RadicalWaitHandle(), o => o.Reset(), true);

        public static IRadicalWaitHandle Null
        {
            get
            {
                return NullYieldInstruction.Null;
            }
        }

        public static RadicalWaitHandle Create()
        {
            return _pool.GetInstance();
        }

        #endregion

    }

    public class RadicalWaitHandle<T> : IRadicalWaitHandle, IRadicalEnumerator
    {

        #region Fields

        private bool _complete;
        private T _result;
        private System.Action<IRadicalWaitHandle> _callback;

        #endregion

        #region CONSTRUCTOR

        public RadicalWaitHandle()
        {

        }

        #endregion

        #region Properties

        public T Result => _result;

        #endregion

        #region Methods

        public void SignalCancelled()
        {
            if (_complete) return;

            _result = default(T);

            _complete = true;
            this.Cancelled = true;

            _callback?.Invoke(this);
            _callback = null;
        }

        public void SignalComplete(T result)
        {
            if (_complete) return;

            _result = result;
            _complete = true;

            _callback?.Invoke(this);
            _callback = null;
        }

        public void Reset()
        {
            _result = default(T);
            _complete = false;
            this.Cancelled = false;
            _callback = null;
        }

        protected virtual bool Tick(out object yieldObject)
        {
            yieldObject = null;
            return !_complete;
        }

        #endregion

        #region IRadicalWaitHandle Interface

        public bool Cancelled
        {
            get;
            private set;
        }

        public bool IsComplete
        {
            get { return _complete; }
        }

        public void OnComplete(System.Action<IRadicalWaitHandle> callback)
        {
            if (callback == null) throw new System.ArgumentNullException("callback");
            if (_complete) throw new System.InvalidOperationException("Can not wait for complete on an already completed IRadicalWaitHandle.");
            _callback += callback;
        }

        bool IRadicalYieldInstruction.Tick(out object yieldObject)
        {
            if (_complete)
            {
                yieldObject = null;
                return false;
            }

            return this.Tick(out yieldObject);
        }

        #endregion

        #region IEnumerator Interface

        object System.Collections.IEnumerator.Current => null;

        bool System.Collections.IEnumerator.MoveNext()
        {
            object inst;
            return this.Tick(out inst);
        }

        void System.Collections.IEnumerator.Reset()
        {
            //do nothing
        }

        #endregion

    }

}
