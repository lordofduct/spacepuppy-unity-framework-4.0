using System;
using System.Collections.Generic;

namespace com.spacepuppy.Collections
{

    public class TempDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ITempCollection<KeyValuePair<TKey, TValue>>
    {

        //private const int MAX_SIZE_INBYTES = 1024;
        private const int MAX_SIZE = 256;

        #region Fields

        private static ObjectCachePool<TempDictionary<TKey, TValue>> _pool = new ObjectCachePool<TempDictionary<TKey, TValue>>(-1, () => new TempDictionary<TKey, TValue>(), (c) => c.Comparer = null);

        private int _maxCapacityOnRelease;
        //private int _version;

        #endregion

        #region CONSTRUCTOR

        public TempDictionary()
            : base(new OverridableEqualityComparer<TKey>())
        {
            //var tp = typeof(TKey);
            //int sz = Math.Max((tp.IsValueType && !tp.IsEnum) ? System.Runtime.InteropServices.Marshal.SizeOf(tp) : 4, 4);
            //_maxCapacityOnRelease = MAX_SIZE_INBYTES / sz;
            _maxCapacityOnRelease = MAX_SIZE;
            //_version = 1;
        }

        public TempDictionary(IDictionary<TKey, TValue> dict)
            : base(new OverridableEqualityComparer<TKey>())
        {
            //var tp = typeof(TKey);
            //int sz = Math.Max((tp.IsValueType && !tp.IsEnum) ? System.Runtime.InteropServices.Marshal.SizeOf(tp) : 4, 4);
            //_maxCapacityOnRelease = MAX_SIZE_INBYTES / sz;
            _maxCapacityOnRelease = MAX_SIZE;
            //_version = 1;
        }

        #endregion

        #region Properties

        public new IEqualityComparer<TKey> Comparer
        {
            get { return base.Comparer; }
            set
            {
                (base.Comparer as OverridableEqualityComparer<TKey>).Comparer = value;
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

        public static TempDictionary<TKey, TValue> GetDict()
        {
            return _pool.GetInstance();
        }

        public static TempDictionary<TKey, TValue> GetDict(IEqualityComparer<TKey> comparer)
        {
            var result = _pool.GetInstance();
            result.Comparer = comparer;
            return result;
        }

        public static TempDictionary<TKey, TValue> GetDict(IDictionary<TKey, TValue> dict)
        {
            TempDictionary<TKey, TValue> result;
            if (_pool.TryGetInstance(out result))
            {
                var le = LightEnumerator.Create(dict);
                while (le.MoveNext())
                {
                    result.Add(le.Current.Key, le.Current.Value);
                }
            }
            else
            {
                result = new TempDictionary<TKey, TValue>(dict);
            }
            return result;
        }

        public static TempDictionary<TKey, TValue> GetDict(IDictionary<TKey, TValue> dict, IEqualityComparer<TKey> comparer)
        {
            TempDictionary<TKey, TValue> result;
            if (_pool.TryGetInstance(out result))
            {
                result.Comparer = comparer;
                var le = LightEnumerator.Create(dict);
                while (le.MoveNext())
                {
                    result.Add(le.Current.Key, le.Current.Value);
                }
            }
            else
            {
                result = new TempDictionary<TKey, TValue>(dict);
                result.Comparer = comparer;
            }
            return result;
        }

        #endregion

    }

}
