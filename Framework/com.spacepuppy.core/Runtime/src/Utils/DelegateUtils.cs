using System;
using System.Collections.Generic;
using System.Linq;

namespace com.spacepuppy.Utils
{
    public static class DelegateUtils
    {

        public static Predicate<T> ChainOr<T>(this Predicate<T> pred, Predicate<T> next)
        {
            if(pred == null)
            {
                return next;
            }
            else if(next == null)
            {
                return pred;
            }
            else
            {
                return (o) => pred(o) || next(o);
            }
        }

        public static Predicate<T> ChainAnd<T>(this Predicate<T> pred, Predicate<T> next)
        {
            if (pred == null)
            {
                return next;
            }
            else if (next == null)
            {
                return pred;
            }
            else
            {
                return (o) => pred(o) && next(o);
            }
        }

        public static Func<T, bool> ChainOr<T>(this Func<T, bool> pred, Func<T, bool> next)
        {
            if (pred == null)
            {
                return next;
            }
            else if (next == null)
            {
                return pred;
            }
            else
            {
                return (o) => pred(o) || next(o);
            }
        }

        public static Func<T, bool> ChainAnd<T>(this Func<T, bool> pred, Func<T, bool> next)
        {
            if (pred == null)
            {
                return next;
            }
            else if (next == null)
            {
                return pred;
            }
            else
            {
                return (o) => pred(o) && next(o);
            }
        }

    }
}
