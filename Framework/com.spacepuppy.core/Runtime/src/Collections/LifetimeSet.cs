using System;
using System.Collections.Generic;
using System.Linq;

namespace com.spacepuppy.Collections
{

    /// <summary>
    /// A set whose entries only last for a certain duration of time.
    /// </summary>
    public class LifetimeSet<T> : ICollection<T>
    {

        #region Fields

        private Dictionary<T, TimeStampInfo> _table;
        private T _nearestToDie;

        #endregion

        #region CONSTRUCTOR

        public LifetimeSet()
        {
            _table = new Dictionary<T, TimeStampInfo>();
        }

        public LifetimeSet(IEqualityComparer<T> comparer)
        {
            _table = new Dictionary<T, TimeStampInfo>(comparer ?? EqualityComparer<T>.Default);
        }

        #endregion

        #region Properties

        public IEqualityComparer<T> Comparer { get { return _table.Comparer; } }

        public TimeSpan DefaultLifeTime { get; set; } = TimeSpan.FromSeconds(60);

        #endregion

        #region Methods

        public void Add(T item, TimeSpan lifetime)
        {
            _table[item] = new TimeStampInfo()
            {
                TimeStamp = DateTime.UtcNow,
                LifeTime = lifetime
            };

            this.Sync(true);
        }

        public void Add(T item, object tag, TimeSpan lifetime)
        {
            _table[item] = new TimeStampInfo()
            {
                TimeStamp = DateTime.UtcNow,
                LifeTime = lifetime,
                Tag = tag
            };

            this.Sync(true);
        }

        public bool TryGetTag(T item, out object tag, bool donotTouchEntry = false)
        {
            if (!donotTouchEntry) this.Touch(item);
            this.Sync();

            TimeStampInfo info;
            if (_table.TryGetValue(item, out info))
            {
                tag = info.Tag;
                return true;
            }
            else
            {
                tag = null;
                return false;
            }
        }

        public void Touch(T item)
        {
            TimeStampInfo info;
            if (_table.TryGetValue(item, out info))
            {
                info.TimeStamp = DateTime.UtcNow;
                _table[item] = info;
            }
        }

        /// <summary>
        /// Purge any entries that are dead.
        /// </summary>
        /// <returns>Returns true if the collection changed.</returns>
        public bool Sync(bool forceSync = false)
        {
            if (_table.Count == 0) return false;

            TimeStampInfo info;
            var current = DateTime.UtcNow;
            if (!forceSync && _table.TryGetValue(_nearestToDie, out info) && (current - info.TimeStamp) < info.LifeTime) return false;

            bool result = false;
            _nearestToDie = default(T);
            TimeSpan durationToDeath = TimeSpan.FromTicks(long.MaxValue);
            using (var lst = TempCollection.GetList<T>(_table.Keys))
            {
                for (int i = 0; i < lst.Count; i++)
                {
                    info = _table[lst[i]];
                    var diff = current - info.TimeStamp;
                    if (diff >= info.LifeTime)
                    {
                        _table.Remove(lst[i]);
                        result = true;
                    }
                    else
                    {
                        diff = info.LifeTime - diff;
                        if (diff < durationToDeath)
                        {
                            _nearestToDie = lst[i];
                            durationToDeath = diff;
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        #region ICollection Interface

        public int Count { get { return _table.Count; } }

        bool ICollection<T>.IsReadOnly { get { return false; } }

        public void Add(T item)
        {
            this.Add(item, this.DefaultLifeTime);
        }

        public void Clear()
        {
            _table.Clear();
            _nearestToDie = default(T);
        }

        public bool Contains(T item)
        {
            this.Sync();
            return _table.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.Sync();
            _table.Keys.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            this.Sync();
            return _table.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            this.Sync();
            return _table.Keys.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region Special Types

        private struct TimeStampInfo
        {
            public DateTime TimeStamp;
            public TimeSpan LifeTime;
            public object Tag;
        }

        public struct Enumerator : IEnumerator<T>
        {

            private Dictionary<T, TimeStampInfo>.KeyCollection.Enumerator _e;

            public Enumerator(LifetimeSet<T> coll)
            {
                _e = coll._table.Keys.GetEnumerator();
            }

            public T Current { get { return _e.Current; } }

            object System.Collections.IEnumerator.Current { get { return _e.Current; } }

            public void Dispose()
            {
                _e.Dispose();
            }

            public bool MoveNext()
            {
                return _e.MoveNext();
            }

            void System.Collections.IEnumerator.Reset()
            {
                ((System.Collections.IEnumerator)_e).Reset();
            }
        }

        #endregion

    }

}
