using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using com.spacepuppy.Collections;

namespace com.spacepuppy
{

    /// <summary>
    /// Represents a duration of time to wait for that can be paused, and can use various kinds of TimeSuppliers. 
    /// 
    /// The timer starts immediately upon creation. To restart the timer call Reset. This also facilitates reusing the token. 
    /// 
    /// Note that the static factory methods return pooled versions of the token. 
    /// These should never be reused as they are disposed immediately upon completion.
    /// </summary>
    public class WaitForDuration : IRadicalEnumerator, IPausibleYieldInstruction, IProgressingYieldInstruction, System.IDisposable
    {

        private enum States : sbyte
        {
            Pooled = -2,
            WaitingRelease = -1,
            Unknown = 0,
            Running = 1,
            Paused = 2,
            Complete = 3,
        }

        #region Fields

        private ITimeSupplier _supplier;
        private float _dur;
        private double _tally;
        private double _startTime;
        private States _state;

        #endregion

        #region CONSTRUCTOR

        public WaitForDuration(float dur, ITimeSupplier supplier)
        {
            this.Reset(dur, supplier);
        }

        internal WaitForDuration()
        {
            //allow overriding
        }

        #endregion

        #region Properties

        public ITimeSupplier Time => _supplier ?? SPTime.Normal;

        public float Duration { get { return _dur; } }

        public double CurrentTime { get { return _tally + this.GetCurrentRunningTime(); } }

        #endregion

        #region Methods

        public WaitForDuration Reset()
        {
            _tally = 0d;
            _startTime = this.Time.TotalPrecise;
            _state = States.Running;
            return this;
        }

        public WaitForDuration Reset(float dur, ITimeSupplier supplier)
        {
            _supplier = supplier ?? SPTime.Normal;
            _dur = dur;
            _tally = 0d;
            _startTime = this.Time.TotalPrecise;
            _state = States.Running;
            return this;
        }

        public void Cancel()
        {
            _tally = 0d;
            _startTime = 0d;
            _state = States.Unknown;
        }

        /// <summary>
        /// Calculates the time since the last 'start' time. This isn't necessarily the total time unless _tally = 0.
        /// </summary>
        /// <returns></returns>
        private double GetCurrentRunningTime()
        {
            return (_state == States.Running) ? (this.Time.TotalPrecise - _startTime) : 0d;
        }

        private bool TestIsComplete()
        {
            GameLoop.AssertMainThread();

            switch (_state)
            {
                case States.Running:
                    if(this.CurrentTime >= _dur)
                    {
                        _tally = this.CurrentTime;
                        _state = States.Complete;
                        _startTime = 0d;
                        if (this is IPooledYieldInstruction)
                        {
                            Release(this);
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case States.Paused:
                    return false;
                case States.Complete:
                default:
                    return true;
            }
        }

        #endregion

        #region IRadicalYieldInstruction Interface

        public bool IsComplete => this.TestIsComplete();

        float IProgressingYieldInstruction.Progress
        {
            get { return Mathf.Clamp01((float)(this.CurrentTime / _dur)); }
        }

        bool IRadicalYieldInstruction.Tick(out object yieldObject)
        {
            yieldObject = null;
            return !this.TestIsComplete();
        }

        void IPausibleYieldInstruction.OnPause()
        {
            if(_state == States.Running)
            {
                _tally += this.GetCurrentRunningTime();
                _state = States.Paused;
            }
        }

        void IPausibleYieldInstruction.OnResume()
        {
            if (_state == States.Paused)
            {
                _startTime = this.Time.TotalPrecise;
                _state = States.Running;
            }
        }

        #endregion

        #region IEnumerator Interface

        object IEnumerator.Current
        {
            get
            {
                return null;
            }
        }

        bool IEnumerator.MoveNext()
        {
            return !this.TestIsComplete();
        }

        void IEnumerator.Reset()
        {
            this.Reset();
        }

        #endregion

        #region IDisposable Interface

        public virtual void Dispose()
        {
            _supplier = null;
            _dur = 0f;
            _tally = 0d;
            _startTime = 0d;
            _state = States.Unknown;
        }

        #endregion

        #region Static Interface

        private static com.spacepuppy.Collections.ObjectCachePool<PooledWaitForDuration> _pool = new com.spacepuppy.Collections.ObjectCachePool<PooledWaitForDuration>(128, () => new PooledWaitForDuration(), (h) => h._state = States.Pooled, false);
        private readonly static FiniteDeque<WaitForDuration> _toRelease = new FiniteDeque<WaitForDuration>(128);
        private void Release(WaitForDuration h)
        {
            lock(_toRelease)
            {
                h._state = States.WaitingRelease;
                _toRelease.Push(h);
                if(_toRelease.Count == 1)
                {
                    GameLoop.UpdateHandle.BeginInvoke(_releaseNextFrameCallback);
                }
            }
        }
        private System.Action _releaseNextFrameCallback = () =>
        {
            using (var lst = TempCollection.GetList<WaitForDuration>())
            {
                lock (_toRelease)
                {
                    while (_toRelease.Count > 0)
                    {
                        var p = _toRelease.Pop();
                        if (p._state == States.WaitingRelease)
                        {
                            lst.Add(p);
                        }
                    }
                }

                foreach(var p in lst)
                {
                    p.Dispose();
                }
            }
        };

        private class PooledWaitForDuration : WaitForDuration, IPooledYieldInstruction
        {

            public override void Dispose()
            {
                base.Dispose();
                _pool.Release(this);
            }

        }

        /// <summary>
        /// Create a WaitForDuration in seconds as a pooled object.
        /// 
        /// NOTE - This retrieves a pooled WaitForDuration that should be used only once. It should be immediately yielded and not used again.
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="supplier"></param>
        /// <returns></returns>
        public static WaitForDuration Seconds(float seconds, ITimeSupplier supplier = null)
        {
            var w = _pool.GetInstance();
            w.Reset(seconds, supplier);
            return w;
        }

        /// <summary>
        /// Create a WaitForDuration from a WaitForSeconds object as a pooled object.
        /// 
        /// NOTE - This retrieves a pooled WaitForDuration that should be used only once. It should be immediately yielded and not used again.
        /// </summary>
        /// <param name="wait"></param>
        /// <param name="returnNullIfZero"></param>
        /// <returns></returns>
        public static WaitForDuration FromWaitForSeconds(WaitForSeconds wait, bool returnObjEvenIfZero = false)
        {
            float dur = (float)WaitForSeconds_SecondsField.GetValue(wait);
            if (dur <= 0f && !returnObjEvenIfZero)
            {
                return null;
            }
            else
            {
                var w = _pool.GetInstance();
                w.Reset(dur, SPTime.Normal);
                return w;
            }
        }
        private static System.Reflection.FieldInfo _waitForSeconds_SecondsField;
        private static System.Reflection.FieldInfo WaitForSeconds_SecondsField => _waitForSeconds_SecondsField ?? (_waitForSeconds_SecondsField = typeof(WaitForSeconds).GetField("m_Seconds", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));

        /// <summary>
        /// Create a WaitForDuration from a SPTimePeriod as a pooled object.
        /// 
        /// NOTE - This retrieves a pooled WaitForDuration that should be used only once. It should be immediately yielded and not used again.
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        public static WaitForDuration Period(SPTimePeriod period)
        {
            var w = _pool.GetInstance();
            w.Reset((float)period.Seconds, period.TimeSupplier);
            return w;
        }

        #endregion
        
    }

}
