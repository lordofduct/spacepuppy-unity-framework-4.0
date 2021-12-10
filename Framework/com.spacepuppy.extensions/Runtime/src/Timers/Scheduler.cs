using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Collections;

namespace com.spacepuppy.Timers
{

    public enum SchedulingStyle
    {
        FromZero = 0,
        FromNow = 1,
        FromRelative = 2
    }

    public class ScheduledEvent : System.EventArgs
    {

        #region Fields

        private Scheduler _owner;
        internal int _currentCount;
        internal double _nextScheduledTime;

        #endregion

        #region CONSTRUCTOR

        public ScheduledEvent(Scheduler owner, double time, double interval, int repeatCount, System.Action<ScheduledEvent> callback)
        {
            if (owner == null) throw new System.ArgumentNullException(nameof(owner));

            _owner = owner;
            this.Time = time;
            this.Interval = interval;
            this.RepeatCount = repeatCount;
            this.Callback = callback;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The time of the scheduled event.
        /// </summary>
        public double Time { get; protected set; }

        /// <summary>
        /// The interval after 'Time' that the event should repeat, if it repeats.
        /// </summary>
        public double Interval { get; protected set; }

        /// <summary>
        /// Number of times the interval should repeat before completing, values &lt 0 repeat forever.
        /// </summary>
        public int RepeatCount { get; protected set; }

        public System.Action<ScheduledEvent> Callback { get; set; }

        /// <summary>
        /// Has all the events finished.
        /// </summary>
        public bool Complete { get { return this.RepeatCount >= 0 && this.CurrentCount > this.RepeatCount; } }

        /// <summary>
        /// The Scheduler with which this event is registered.
        /// </summary>
        public Scheduler Scheduler { get { return _owner; } }

        /// <summary>
        /// Number of times the event has occurred.
        /// </summary>
        public int CurrentCount { get { return _currentCount; } }

        /// <summary>
        /// The next time this event is currently schedule to fire at (only if a member of the Scheduler, otherwise not set)
        /// </summary>
        public double NextScheduledTime { get { return _nextScheduledTime; } }

        #endregion

        #region Methods

        public void Cancel()
        {
            if (_owner != null)
            {
                _owner.Remove(this);
            }
        }

        public double GetNextScheduledTime()
        {
            return GetNextScheduledTime(_owner?.TimeSupplier?.TotalPrecise ?? 0d);
        }

        /// <summary>
        /// Returns the next time after 'time' that this event aught to be raised.
        /// </summary>
        /// <param name="time">The time after which to get the next scheduled time.</param>
        /// <returns></returns>
        public double GetNextScheduledTime(double time)
        {
            if (this.Complete) return double.NaN;

            if (time < this.Time) return this.Time;
            if (this.RepeatCount == 0) return double.NaN;

            var t = time - this.Time;
            int cnt = (int)System.Math.Floor(t / this.Interval);
            if (this.RepeatCount >= 0 && cnt > this.RepeatCount) return double.NaN;

            return this.Time + (this.Interval * (cnt + 1));
        }

        #endregion

        #region IDisposable Interface

        public void Dispose()
        {
            if (_owner != null)
                _owner.Remove(this);

            this.Interval = 0f;
            this.Time = 0f;
            this.RepeatCount = 0;
            this.Callback = null;
        }

        #endregion

    }

    public class Scheduler : ICollection<ScheduledEvent>
    {

        #region Fields

        private ITimeSupplier _time;
        private BinaryHeap<ScheduledEvent> _heap;

        #endregion

        #region CONSTRUCTOR

        public Scheduler(ITimeSupplier time)
        {
            if (time == null) throw new System.ArgumentNullException(nameof(time));
            _time = time;
            _heap = new BinaryHeap<ScheduledEvent>(ScheduledEventComparer.Default);
        }

        #endregion

        #region Properties

        public ITimeSupplier TimeSupplier
        {
            get { return _time; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Reschedule an event that was previously cancelled based on the time that schedule was initially started. 
        /// This is useful for saving and loading scheduled events. You can serialize the start time, and calculate 
        /// and restart the event based on that.
        /// </summary>
        /// <param name="style"></param>
        /// <param name="originalStartTime"></param>
        /// <param name="time"></param>
        /// <param name="repeatInterval"></param>
        /// <param name="repeatCount"></param>
        /// <param name="callback"></param>
        /// <returns>Returns the ScheduledEvent token. Will return null if no event will occur, for example, if the current time is pass the time the event would have occurred.</returns>
        public ScheduledEvent Reschedule(SchedulingStyle style, double originalStartTime, double time, double repeatInterval, int repeatCount, System.Action<ScheduledEvent> callback)
        {
            switch (style)
            {
                case SchedulingStyle.FromZero:
                    {
                        //calculate next time to schedule at
                        var currentTime = _time.TotalPrecise;
                        var next = System.Math.Ceiling((currentTime - time) / repeatInterval) * repeatInterval + time;


                        //determine how many repeats are left based on the _startTime, this is necessary incase this was loaded and started again
                        if (repeatCount < 0)
                        {
                            repeatCount = -1; //negative is infinite
                        }
                        else
                        {
                            repeatCount = repeatCount - (int)System.Math.Round((next - time) / repeatInterval);
                            if (repeatCount < 0) return null; //completed, so don't start
                        }

                        this.Schedule(next, repeatInterval, repeatCount, callback);
                    }
                    break;
                case SchedulingStyle.FromNow:
                    {
                        //calculate next time to schedule at
                        var currentTime = _time.TotalPrecise;
                        var firstTime = originalStartTime + time;
                        var next = System.Math.Ceiling((currentTime - firstTime) / repeatInterval) * repeatInterval + firstTime;


                        //determine how many repeats are left based on the _startTime, this is necessary incase this was loaded and started again
                        if (repeatCount < 0)
                        {
                            repeatCount = -1; //negative is infinite
                        }
                        else
                        {
                            repeatCount = repeatCount - (int)System.Math.Round((next - firstTime) / repeatInterval);
                            if (repeatCount < 0) return null; //completed, so don't start
                        }

                        this.Schedule(next, repeatInterval, repeatCount, callback);
                    }
                    break;
                case SchedulingStyle.FromRelative:
                    {
                        //calculate next time to schedule at
                        var currentTime = _time.TotalPrecise;
                        var next = System.Math.Ceiling((currentTime - time) / repeatInterval) * repeatInterval + time;


                        //determine how many repeats are left based on the _startTime, this is necessary incase this was loaded and started again
                        if (repeatCount < 0)
                        {
                            repeatCount = -1; //negative is infinite
                        }
                        else
                        {
                            var first = System.Math.Ceiling((originalStartTime - time) / repeatInterval) * repeatInterval + time;
                            repeatCount = repeatCount - (int)System.Math.Round((next - first) / repeatInterval);
                            if (repeatCount < 0) return null; //completed, so don't start
                        }

                        this.Schedule(next, repeatInterval, repeatCount, callback);
                    }
                    break;
            }

            return null;
        }

        /// <summary>
        /// Schedule an event to occur at some point in time.
        /// </summary>
        /// <param name="time">The time of the event</param>
        /// <param name="callback">Callback to respond to the event</param>
        /// <returns></returns>
        public ScheduledEvent Schedule(double time, System.Action<ScheduledEvent> callback)
        {
            var ev = new ScheduledEvent(this, time, 0d, 0, callback);
            this.Insert(ev, _time.TotalPrecise);
            return ev;
        }
        /// <summary>
        /// Create an event that occurs on some interval.
        /// </summary>
        /// <param name="time">The time at which the interval begins counting, good for repeating intervals</param>
        /// <param name="interval">The interval of the event</param>
        /// <param name="repeatCount">Number of times to repeat, negative values are treated as infinite</param>
        /// <param name="callback">Callback to respond to the event</param>
        /// <returns></returns>
        public ScheduledEvent Schedule(double time, double interval, int repeatCount, System.Action<ScheduledEvent> callback)
        {
            var ev = new ScheduledEvent(this, time, interval, repeatCount, callback);
            this.Insert(ev, _time.TotalPrecise);
            return ev;
        }

        /// <summary>
        /// Schedule an event that raises at some duration from the current time.
        /// </summary>
        /// <param name="duration">Amount of time from now the event occurs</param>
        /// <param name="callback">Callback to respond to the event</param>
        /// <returns></returns>
        public ScheduledEvent ScheduleFromNow(double duration, System.Action<ScheduledEvent> callback)
        {
            var ev = new ScheduledEvent(this, _time.TotalPrecise + duration, 0d, 0, callback);
            this.Insert(ev, _time.TotalPrecise);
            return ev;
        }

        /// <summary>
        /// Schedule an event that raises at some duration from the current time.
        /// </summary>
        /// <param name="duration">Amount of time from now the first event occurs</param>
        /// <param name="repeatFrequency">Frequency of the event</param>
        /// <param name="repeatCount">Number of times the event occurs</param>
        /// <param name="callback">Callback to respond to the event</param>
        /// <returns></returns>
        public ScheduledEvent ScheduleFromNow(double duration, double repeatFrequency, int repeatCount, System.Action<ScheduledEvent> callback)
        {
            var ev = new ScheduledEvent(this, _time.TotalPrecise + duration, repeatFrequency, repeatCount, callback);
            this.Insert(ev, _time.TotalPrecise);
            return ev;
        }

        /// <summary>
        /// Schedules an event relative to current time so that it would have been at the repeated time of 'interval' of a scheduled time at 'offset'. 
        /// 
        /// For example if you wanted to schedule an event for on the half hour past the current hour. You'd pass in 30 minutes for 'firstTime' and 'hour' 
        /// for interval. It'll calculate the next half hour on the hour from now, and schedule an event for it.
        /// </summary>
        /// <param name="firstTime"></param>
        /// <param name="interval"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ScheduledEvent ScheduleOnIntervalFromNow(double firstTime, double interval, System.Action<ScheduledEvent> callback)
        {
            var t = System.Math.Ceiling((_time.TotalPrecise - firstTime) / interval) * interval + firstTime;
            var ev = new ScheduledEvent(this, t, 0d, 0, callback);
            this.Insert(ev, _time.TotalPrecise);
            return ev;
        }

        /// <summary>
        /// Schedules an event relative to current time so that it would have been at the repeated time of 'interval' of a scheduled time at 'offset'. 
        /// And then repeat that interval 'repeatCount' times.
        /// 
        /// For example if you wanted to schedule an event for on the half hour past the current hour. You'd pass in 30 minutes for 'firstTime' and 'hour' 
        /// for interval. It'll calculate the next half hour on the hour from now, and schedule an event for it.
        /// </summary>
        /// <param name="firstTime"></param>
        /// <param name="interval"></param>
        /// <param name="repeatCount"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ScheduledEvent ScheduleOnIntervalFromNow(double firstTime, double interval, int repeatCount, System.Action<ScheduledEvent> callback)
        {
            var t = System.Math.Ceiling((_time.TotalPrecise - firstTime) / interval) * interval + firstTime;
            var ev = new ScheduledEvent(this, t, interval, repeatCount, callback);
            this.Insert(ev, _time.TotalPrecise);
            return ev;
        }

        public void ChangeTimeSupplier(ITimeSupplier time)
        {
            if (time == null) throw new System.ArgumentNullException("time");
            if (time == _time) return;

            _time = time;
            if (_heap.Count == 0) return;

            var total = _time.TotalPrecise;
            using (var lst = com.spacepuppy.Collections.TempCollection.GetList<ScheduledEvent>(_heap))
            {
                _heap.Clear();
                foreach(var node in lst)
                {
                    this.Insert(node, total);
                }
            }
        }

        public void Tick()
        {
            var total = _time.TotalPrecise;
            using (var lst = TempCollection.GetList<ScheduledEvent>())
            {
                while (_heap.Count > 0 && total > _heap.Peek()._nextScheduledTime)
                {
                    var node = _heap.Pop();
                    node._currentCount++;
                    var d = node.Callback;
                    d?.Invoke(node);

                    if (!node.Complete)
                    {
                        lst.Add(node);
                    }
                }

                foreach(var node in lst)
                {
                    this.Insert(node, total);
                }
            }
        }

        private void Insert(ScheduledEvent node, double time)
        {
            node._nextScheduledTime = node.GetNextScheduledTime(time);
            if (!double.IsNaN(node._nextScheduledTime) && !double.IsInfinity(node._nextScheduledTime))
            {
                _heap.Add(node);
            }
        }

        #endregion

        #region ICollection Interface

        void ICollection<ScheduledEvent>.Add(ScheduledEvent item)
        {
            if (item.Scheduler != this) throw new System.ArgumentException("Added ScheduledEvent to an un-affiliated Scheduler.", nameof(item));

            this.Insert(item, _time.TotalPrecise);
        }

        public void Clear()
        {
            _heap.Clear();
        }

        public bool Contains(ScheduledEvent item)
        {
            return _heap.Contains(item);
        }

        public void CopyTo(ScheduledEvent[] array, int arrayIndex)
        {
            _heap.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _heap.Count; }
        }

        bool ICollection<ScheduledEvent>.IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(ScheduledEvent item)
        {
            return _heap.Remove(item);
        }

        public BinaryHeap<ScheduledEvent>.Enumerator GetEnumerator()
        {
            return _heap.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _heap.GetEnumerator();
        }

        IEnumerator<ScheduledEvent> IEnumerable<ScheduledEvent>.GetEnumerator()
        {
            return _heap.GetEnumerator();
        }

        #endregion

        #region Special Types

        private class ScheduledEventComparer : IComparer<ScheduledEvent>
        {

            public static readonly ScheduledEventComparer Default = new ScheduledEventComparer();

            public int Compare(ScheduledEvent x, ScheduledEvent y)
            {
                return -(x?.NextScheduledTime ?? double.NegativeInfinity).CompareTo(y?.NextScheduledTime ?? double.NegativeInfinity);
            }
        }

        #endregion

    }

}
