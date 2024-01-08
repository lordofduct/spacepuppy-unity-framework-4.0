using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.spacepuppy.Collections
{
    public class TempHashSet<T> : HashSet<T>, ITempCollection<T>
    {

        private const int MAX_SIZE_INBYTES = 1024;

        #region Fields

        private static ObjectCachePool<TempHashSet<T>> _pool = new ObjectCachePool<TempHashSet<T>>(8, () => new TempHashSet<T>(), (c) => c.Comparer = null);

        #endregion

        #region CONSTRUCTOR

        public TempHashSet()
            : base(new OverridableEqualityComparer<T>())
        {
        }

        public TempHashSet(IEnumerable<T> e)
            : base(e, new OverridableEqualityComparer<T>())
        {
        }

        #endregion

        #region Properties

        public new IEqualityComparer<T> Comparer
        {
            get { return base.Comparer; }
            set
            {
                (base.Comparer as OverridableEqualityComparer<T>).Comparer = value;
            }
        }

        #endregion


        #region IDisposable Interface

        public void Dispose()
        {
            this.Clear();
            _pool.Release(this);
        }

        #endregion

        #region Static Methods

        public static TempHashSet<T> GetSet()
        {
            return _pool.GetInstance();
        }

        public static TempHashSet<T> GetSet(IEqualityComparer<T> comparer)
        {
            var result = _pool.GetInstance();
            result.Comparer = comparer;
            return result;
        }

        public static TempHashSet<T> GetSet(IEnumerable<T> e)
        {
            TempHashSet<T> result;
            if (_pool.TryGetInstance(out result))
            {
                var le = LightEnumerator.Create<T>(e);
                while(le.MoveNext())
                {
                    result.Add(le.Current);
                }
            }
            else
            {
                result = new TempHashSet<T>(e);
            }
            return result;
        }

        public static TempHashSet<T> GetSet(IEnumerable<T> e, IEqualityComparer<T> comparer)
        {
            TempHashSet<T> result;
            if (_pool.TryGetInstance(out result))
            {
                result.Comparer = comparer;
                var le = LightEnumerator.Create<T>(e);
                while (le.MoveNext())
                {
                    result.Add(le.Current);
                }
            }
            else
            {
                result = new TempHashSet<T>(e);
                result.Comparer = comparer;
            }
            return result;
        }

        #endregion

    }
}
