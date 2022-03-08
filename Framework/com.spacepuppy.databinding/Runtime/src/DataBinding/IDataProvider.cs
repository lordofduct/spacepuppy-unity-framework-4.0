using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Project;

namespace com.spacepuppy.DataBinding
{

    public interface IDataProvider : System.Collections.IEnumerable
    {

        /// <summary>
        /// A binding context's might only care about binding the first element of the list, this is a fast accessor for that element.
        /// </summary>
        object FirstElement { get; }

    }

    public static class DataProviderUtils
    {

        public static System.Collections.IEnumerable GetAsDataProvider(object source)
        {
            return GetAsDataProviderOrNull(source) ?? new object[] { source };
        }

        public static System.Collections.IEnumerable GetAsDataProvider(object source, bool respectIfProxy)
        {
            if (respectIfProxy && source is IProxy p)
            {
                if (p.PrioritizesSelfAsTarget())
                {
                    var ddp = GetAsDataProviderOrNull(source);
                    if (ddp != null) return ddp;
                }

                source = p.GetTargetInternal(typeof(IDataProvider), null);
                return GetAsDataProviderOrNull(source) ?? new object[] { source };
            }
            else
            {
                return GetAsDataProviderOrNull(source) ?? new object[] { source };
            }
        }

        public static System.Collections.IEnumerable GetAsDataProviderOrNull(object source)
        {
            switch (source)
            {
                case IDataProvider dp:
                    return dp;
                case System.Collections.IEnumerable e:
                    return e;
                default:
                    return ObjUtil.GetAsFromSource<IDataProvider>(source);
            }
        }


        public static object GetFirstElementOfDataProvider(object source, bool respectIfProxy = false, bool ignoreReducingGenericEnumerable = false)
        {
            var dp = ObjUtil.GetAsFromSource<IDataProvider>(source, respectIfProxy);
            if (dp != null) return dp.FirstElement;

            if (respectIfProxy) source = source.ReduceIfProxy();

            if (!ignoreReducingGenericEnumerable && source is System.Collections.IEnumerable e) return e.Cast<object>().FirstOrDefault();

            return source;
        }

    }

}
