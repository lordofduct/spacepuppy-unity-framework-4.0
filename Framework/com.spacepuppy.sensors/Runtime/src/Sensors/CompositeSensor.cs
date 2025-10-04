#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Collections;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Sensors
{

    public sealed class CompositeSensor : Sensor, IEnumerable<Sensor>, IMStartOrEnableReceiver
    {

        #region Fields

        [SerializeField]
        private bool _mustBeVisibleByAll;
        [SerializeField, Tooltip("Composite sensors can be a little slow for large groups. If you poll the sensor multiple times per frame you can set this true so that it limits that it caches that heavy lift for the first call to 'Sense' that frame.")]
        private bool _cacheSensedAspectsPerFrame;

        [System.NonSerialized()]
        private Sensor[] _sensors;

        [System.NonSerialized]
        private HashSet<IAspect> _sensedAllCache = new();
        [System.NonSerialized]
        private int _sensedAllCacheVersion;

        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            _sensedAllCacheVersion = Time.frameCount - 1;
            this.SyncChildSensors();
        }

        #endregion

        #region Properties

        public bool MustBeVisibeByAll
        {
            get => _mustBeVisibleByAll;
            set => _mustBeVisibleByAll = value;
        }

        #endregion

        #region Methods

        public void SyncChildSensors()
        {
            using (var lst = TempCollection.GetList<Sensor>())
            {
                this.GetComponentsInChildren<Sensor>(false, lst);
                for (int i = 0; i < lst.Count; i++)
                {
                    if (lst[i] == this || !lst[i].enabled)
                    {
                        lst.RemoveAt(i);
                        i--;
                    }
                }
                _sensors = lst.ToArray();
            }
        }

        public bool Contains(Sensor sensor)
        {
            if (_sensors == null) return false;

            return System.Array.IndexOf(_sensors, sensor) >= 0;
        }

        IEnumerable<IAspect> SenseAllInternal(System.Func<IAspect, bool> p = null)
        {
            if (_sensors == null) this.SyncChildSensors();

            if (!_cacheSensedAspectsPerFrame)
            {
                switch (_sensors.Length)
                {
                    case 0:
                        return Enumerable.Empty<IAspect>();
                    case 1:
                        return _sensors[0].SenseAll(p);
                    default:
                        {
                            _sensedAllCache.Clear();
                            if (_mustBeVisibleByAll && _sensors.Length > 1)
                            {
                                using (var set = TempCollection.GetSet<IAspect>())
                                {
                                    _sensors[0].SenseAll(set, p);
                                    var e = set.GetEnumerator();
                                    while (e.MoveNext())
                                    {
                                        int cnt = 1;
                                        for (int i = 1; i < _sensors.Length; i++)
                                        {
                                            if (!_sensors[i].Visible(e.Current)) cnt++;
                                        }
                                        if (cnt == _sensors.Length) _sensedAllCache.Add(e.Current);
                                    }
                                    return _sensedAllCache;
                                }
                            }
                            else
                            {
                                foreach (var s in _sensors)
                                {
                                    foreach (var a in s.SenseAll())
                                    {
                                        _sensedAllCache.Add(a);
                                    }
                                }
                                return _sensedAllCache;
                            }
                        }
                }
            }
            else if (_sensedAllCacheVersion == Time.frameCount)
            {
                return p == null ? _sensedAllCache : _sensedAllCache.Where(p);
            }
            else
            {
                _sensedAllCacheVersion = Time.frameCount;
                _sensedAllCache.Clear();

                if (_sensors.Length == 0)
                {
                    return p == null ? _sensedAllCache : _sensedAllCache.Where(p);
                }
                else if (_sensors.Length == 1)
                {
                    _sensedAllCache.AddRange(_sensors[0].SenseAll());
                    return p == null ? _sensedAllCache : _sensedAllCache.Where(p);
                }
                else
                {
                    if (_mustBeVisibleByAll && _sensors.Length > 1)
                    {
                        using (var set = TempCollection.GetSet<IAspect>())
                        {
                            _sensors[0].SenseAll(set);
                            var e = set.GetEnumerator();
                            while (e.MoveNext())
                            {
                                int cnt = 1;
                                for (int i = 1; i < _sensors.Length; i++)
                                {
                                    if (!_sensors[i].Visible(e.Current)) cnt++;
                                }
                                if (cnt == _sensors.Length) _sensedAllCache.Add(e.Current);
                            }
                        }
                    }
                    else
                    {
                        foreach (var s in _sensors)
                        {
                            foreach (var a in s.SenseAll())
                            {
                                _sensedAllCache.Add(a);
                            }
                        }
                    }
                    return p == null ? _sensedAllCache : _sensedAllCache.Where(p);
                }
            }
        }

        #endregion

        #region Sensor Interface

        public override bool ConcernedWith(UnityEngine.Object obj)
        {
            if (_sensors == null) this.SyncChildSensors();
            if (_sensors.Length == 0) return false;

            if (_mustBeVisibleByAll)
            {
                for (int i = 0; i < _sensors.Length; i++)
                {
                    if (!_sensors[i].ConcernedWith(obj)) return false;
                }
                return true;
            }
            else
            {
                for (int i = 0; i < _sensors.Length; i++)
                {
                    if (_sensors[i].ConcernedWith(obj)) return true;
                }
                return false;
            }
        }

        public override bool SenseAny(System.Func<IAspect, bool> p = null)
        {
            if (_sensors == null) this.SyncChildSensors();
            if (_sensors.Length == 0) return false;

            if (_cacheSensedAspectsPerFrame)
            {
                return this.SenseAllInternal(p).Any();
            }
            else if (_sensors.Length == 1)
            {
                return _sensors[0].SenseAny(p);
            }
            else if (_mustBeVisibleByAll)
            {
                foreach (var a in _sensors[0].SenseAll(p))
                {
                    int cnt = 1;
                    for (int i = 1; i < _sensors.Length; i++)
                    {
                        if (_sensors[i].Visible(a)) cnt++;
                    }
                    if (cnt == _sensors.Length) return true;
                }
            }
            else
            {
                for (int i = 0; i < _sensors.Length; i++)
                {
                    if (_sensors[i].SenseAny(p)) return true;
                }
            }

            return false;
        }

        public override IAspect Sense(System.Func<IAspect, bool> p = null)
        {
            if (_sensors == null) this.SyncChildSensors();
            if (_sensors.Length == 0) return null;

            if (_cacheSensedAspectsPerFrame)
            {
                return this.SenseAllInternal(p).FirstOrDefault();
            }
            else if (_sensors.Length == 1)
            {
                return _sensors[0].Sense(p);
            }
            else if (_mustBeVisibleByAll)
            {
                foreach (var a in _sensors[0].SenseAll(p))
                {
                    int cnt = 1;
                    for (int i = 1; i < _sensors.Length; i++)
                    {
                        if (_sensors[i].Visible(a)) cnt++;
                    }
                    if (cnt == _sensors.Length) return a;
                }
            }
            else
            {
                for (int i = 0; i < _sensors.Length; i++)
                {
                    var a = _sensors[i].Sense(p);
                    if (a != null) return a;
                }
            }

            return null;
        }

        public override IEnumerable<IAspect> SenseAll(System.Func<IAspect, bool> p = null) => this.SenseAllInternal(p);

        public override int SenseAll(ICollection<IAspect> lst, System.Func<IAspect, bool> p = null)
        {
            if (lst == null) throw new System.ArgumentNullException("lst");
            if (lst.IsReadOnly) throw new System.ArgumentException("List to fill can not be read-only.", "lst");

            int resultCnt = 0;
            foreach (var a in this.SenseAllInternal(p))
            {
                resultCnt++;
                lst.Add(a);
            }
            return resultCnt;
        }

        public override int SenseAll<T>(ICollection<T> lst, System.Func<T, bool> p = null)
        {
            if (lst == null) throw new System.ArgumentNullException("lst");
            if (lst.IsReadOnly) throw new System.ArgumentException("List to fill can not be read-only.", "lst");

            int resultCnt = 0;
            if (p == null)
            {
                foreach (var a in this.SenseAllInternal())
                {
                    if (a is T t)
                    {
                        resultCnt++;
                        lst.Add(t);
                    }
                }
            }
            else
            {
                foreach (var a in this.SenseAllInternal())
                {
                    if (a is T t && p(t))
                    {
                        resultCnt++;
                        lst.Add(t);
                    }
                }
            }
            return resultCnt;
        }

        public override bool Visible(IAspect aspect)
        {
            if (_sensors == null) this.SyncChildSensors();
            if (_sensors.Length == 0) return false;

            if (_mustBeVisibleByAll && _sensors.Length > 1)
            {
                for (int i = 0; i < _sensors.Length; i++)
                {
                    if (!_sensors[i].Visible(aspect)) return false;
                }
                return true;
            }
            else
            {
                for (int i = 0; i < _sensors.Length; i++)
                {
                    if (_sensors[i].Visible(aspect)) return true;
                }
            }

            return false;
        }

        #endregion

        #region IEnumerable Interface

        public IEnumerator<Sensor> GetEnumerator()
        {
            return (_sensors != null) ? (_sensors as IEnumerable<Sensor>).GetEnumerator() : System.Linq.Enumerable.Empty<Sensor>().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

#if UNITY_EDITOR
        void OnValidate()
        {
            if (Application.isPlaying)
            {
                _sensedAllCacheVersion = Time.frameCount - 1;
            }
        }
#endif

    }

}
