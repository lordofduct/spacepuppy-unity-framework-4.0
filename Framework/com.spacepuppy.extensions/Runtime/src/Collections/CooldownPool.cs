using System;
using System.Collections;
using System.Collections.Generic;

namespace com.spacepuppy.Collections
{

    /// <summary>
    /// Used to store temporary references for a duration of time. Call Update to update the pool 
    /// releasing objects that are old enough. Always call Update on the unity main thread!
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CooldownPool<T> : IUpdateable, IEnumerable<CooldownPool<T>.CooldownInfo> where T : class
    {

        #region Fields

        private Dictionary<T, CooldownInfo> _table = new Dictionary<T, CooldownInfo>();
        private ITimeSupplier _time;
        private bool _autoUpdate;

        #endregion

        #region CONSTRUCTOR

        public CooldownPool()
        {
        }

        #endregion

        #region Properties

        public int Count { get { return _table.Count; } }

        public ITimeSupplier UpdateTimeSupplier
        {
            get => _time ?? SPTime.Normal;
            set => _time = value;
        }

        public bool AutoUpdate
        {
            get => _autoUpdate;
            set
            {
                if (_autoUpdate == value) return;
                _autoUpdate = value;
                if (_autoUpdate && _table.Count > 0)
                {
                    GameLoop.EarlyUpdatePump.Add(this);
                }
                else if (!_autoUpdate)
                {
                    GameLoop.EarlyUpdatePump.Remove(this);
                }
            }
        }

        #endregion

        #region Methods

        public void Add(T obj, float duration)
        {
            CooldownInfo info;
            if (_table.TryGetValue(obj, out info))
            {
                info.Duration += duration;
            }
            else
            {
                _table[obj] = new CooldownInfo(obj, this.UpdateTimeSupplier.Total, duration);
            }

            if (_autoUpdate)
            {
                GameLoop.EarlyUpdatePump.Add(this);
            }
        }

        public bool Contains(T obj)
        {
            return _table.ContainsKey(obj);
        }

        public void Update()
        {
            var t = this.UpdateTimeSupplier.Total;
            CooldownInfo info;

            using (var toRemove = TempCollection.GetList<T>())
            {
                var e1 = _table.GetEnumerator();
                while (e1.MoveNext())
                {
                    info = e1.Current.Value;
                    if (info.Object == null || t - info.StartTime > info.Duration)
                    {
                        toRemove.Add(info.Object);
                    }
                }

                if (toRemove.Count > 0)
                {
                    var e2 = toRemove.GetEnumerator();
                    while (e2.MoveNext())
                    {
                        _table.Remove(e2.Current);
                    }
                }
            }

            if (_autoUpdate && _table.Count == 0)
            {
                GameLoop.EarlyUpdatePump.Remove(this);
            }
        }

        public void Clear()
        {
            _table.Clear();
            if (_autoUpdate)
            {
                GameLoop.EarlyUpdatePump.Remove(this);
            }
        }

        #endregion

        #region IEnumerable Interface

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<CooldownPool<T>.CooldownInfo> IEnumerable<CooldownInfo>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region Special Types

        public struct CooldownInfo
        {
            private T _obj;
            private float _startTime;
            private float _dur;

            public CooldownInfo(T obj, float startTime, float dur)
            {
                _obj = obj;
                _startTime = startTime;
                _dur = dur;
            }

            public T Object { get { return _obj; } }
            public float StartTime
            {
                get { return _startTime; }
                internal set { _startTime = value; }
            }
            public float Duration
            {
                get { return _dur; }
                internal set { _dur = value; }
            }

        }

        public struct Enumerator : IEnumerator<CooldownInfo>
        {

            private Dictionary<T, CooldownInfo>.Enumerator _e;

            public Enumerator(CooldownPool<T> pool)
            {
                if (pool == null) throw new System.ArgumentNullException();
                _e = pool._table.GetEnumerator();
            }

            public CooldownInfo Current
            {
                get
                {
                    return _e.Current.Value;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return _e.Current.Value;
                }
            }

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
                throw new System.NotSupportedException();
            }
        }

        #endregion

    }
}
