using System;
using System.Collections;
using System.Collections.Generic;

namespace com.spacepuppy.Collections
{
    public class OverridableEqualityComparer<T> : IEqualityComparer<T>, IEqualityComparer
    {

        private IEqualityComparer<T> _comparer;

        public OverridableEqualityComparer(IEqualityComparer<T> comparer = default)
        {
            this.Comparer = comparer;
        }

        public IEqualityComparer<T> Comparer
        {
            get { return _comparer; }
            set { _comparer = value ?? EqualityComparer<T>.Default; }
        }


        public bool Equals(T x, T y)
        {
            return _comparer.Equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return _comparer.GetHashCode(obj);
        }

        bool IEqualityComparer.Equals(object x, object y)
        {
            return (_comparer as IEqualityComparer)?.Equals(x, y) ?? false;
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            return (_comparer as IEqualityComparer)?.GetHashCode(obj) ?? obj?.GetHashCode() ?? 0;
        }
    }
}
