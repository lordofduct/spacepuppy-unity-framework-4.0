using System;
using System.Collections.Generic;

namespace com.spacepuppy.Collections
{

    public class TempList<T> : List<T>, ITempCollection<T>
    {

        private const int MAX_SIZE = 2048;
        private const int MIN_SIZE = 8;

        #region Fields

        private static ObjectCachePool<TempList<T>> _pool = new ObjectCachePool<TempList<T>>(8, () => new TempList<T>(MIN_SIZE));

        private int _maxCapacityOnRelease;
        private int _version;

        #endregion

        #region CONSTRUCTOR

        public TempList()
            : base()
        {
            _maxCapacityOnRelease = MAX_SIZE;
            _version = 1;
        }

        public TempList(IEnumerable<T> e)
            : base(e)
        {
            _maxCapacityOnRelease = MAX_SIZE;
            _version = 1;
        }

        public TempList(int count)
            : base(count)
        {
            _maxCapacityOnRelease = MAX_SIZE;
            _version = 1;
        }

        #endregion

        #region IDisposable Interface

        public virtual void Dispose()
        {
            this.Clear();
            if (_pool.Release(this))
            {
                //we allow cached lists to grow, but if they get too out of hand we shrink them back down
                if (this.Capacity > _maxCapacityOnRelease)
                {
                    this.Capacity = _maxCapacityOnRelease;
                    _version = 1;
                }
                else
                {
                    _version++;
                }
            }
        }

        #endregion

        #region Static Methods

        public static TempList<T> GetList()
        {
            return _pool.GetInstance();
        }

        public static TempList<T> GetList(IEnumerable<T> e)
        {
            int cnt = MIN_SIZE;
            if (e is ICollection<T> ic)
                cnt = ic.Count;
            else if (e is IReadOnlyCollection<T> rc)
                cnt = rc.Count;

            TempList<T> result;
            if (_pool.TryGetInstance(cnt, out result, (o, a) => o.Capacity >= a))
            {
                //result.AddRange(e);
                var e2 = new LightEnumerator<T>(e);
                while (e2.MoveNext())
                {
                    result.Add(e2.Current);
                }
            }
            else if (_pool.TryGetInstance(out result))
            {
                result.Capacity = cnt;
                var e2 = new LightEnumerator<T>(e);
                while (e2.MoveNext())
                {
                    result.Add(e2.Current);
                }
            }
            else
            {
                result = new TempList<T>(e);
            }
            return result;
        }

        public static TempList<T> GetList(int count)
        {
            TempList<T> result;
            if (_pool.TryGetInstance(count, out result, (o, a) => o.Capacity >= a))
            {
                if (result.Capacity < count) result.Capacity = count;
                return result;
            }
            else
            {
                result = new TempList<T>(count);
            }
            return result;
        }

        #endregion

    }

}
