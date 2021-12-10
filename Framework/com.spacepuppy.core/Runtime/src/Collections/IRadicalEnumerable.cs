using System;
using System.Collections.Generic;

namespace com.spacepuppy.Collections
{

    public interface IRadicalEnumerable<T> : IEnumerable<T>
    {

        int Enumerate(ICollection<T> coll);
        int Enumerate(Action<T> callback);

    }

    //public static class RadicalEnumerableExtensions
    //{

    //}

}
