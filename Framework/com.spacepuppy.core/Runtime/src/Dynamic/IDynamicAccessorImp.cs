using System;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using System.Linq;

using com.spacepuppy.Utils;

namespace com.spacepuppy.Dynamic
{

    /// <summary>
    /// Defines how DynamicUtil will access objects dynamically when the target is not a IDynamic itself (IDynamic implements its distinct way of accessing itself).
    /// 
    /// You can override the default behaviour of DynamicUtil by assigning a custom implementation of this interface to DynamicUtil.Accessor.
    /// </summary>
    public interface IDynamicAccessorImp
    {

        bool SetValue(object obj, string sMemberName, object value);
        bool SetValue(object obj, string sMemberName, object value, params object[] index);
        bool SetValue(object obj, MemberInfo member, object value);
        bool SetValue(object obj, MemberInfo member, object value, params object[] index);
        bool SetValue<T>(object obj, string sMemberName, T value);
        bool SetValue<T>(object obj, MemberInfo member, T value);

        bool TryGetValue(object obj, string sMemberName, out object result, params object[] args);
        bool TryGetValue(object obj, MemberInfo member, out object result, params object[] args);
        bool TryGetValue<T>(object obj, string sMemberName, out T result, params object[] args);
        bool TryGetValue<T>(object obj, MemberInfo member, out T result, params object[] args);

        object InvokeMethod(object obj, string name, params object[] args);

    }

    public sealed class StandardDynamicAccessorImp : IDynamicAccessorImp
    {

        public static readonly StandardDynamicAccessorImp Default = new StandardDynamicAccessorImp();

        public bool SetValue(object obj, string sMemberName, object value)
        {
            if (obj == null) return false;

            return DynamicUtil.SetValueDirect(obj, sMemberName, value, (object[])null);
        }

        public bool SetValue(object obj, string sMemberName, object value, params object[] index)
        {
            if (obj == null) return false;

            return DynamicUtil.SetValueDirect(obj, sMemberName, value, index);
        }

        public bool SetValue(object obj, MemberInfo member, object value)
        {
            if (obj == null) return false;

            return DynamicUtil.SetValueDirect(obj, member, value, (object[])null);
        }

        public bool SetValue(object obj, MemberInfo member, object value, params object[] index)
        {
            if (obj == null) return false;

            return DynamicUtil.SetValueDirect(obj, member, value, index);
        }

        public bool SetValue<T>(object obj, string sMemberName, T value)
        {
            if (obj == null) return false;

            return DynamicUtil.SetValueDirect(obj, sMemberName, value, (object[])null);
        }

        public bool SetValue<T>(object obj, MemberInfo member, T value)
        {
            if (obj == null) return false;

            return DynamicUtil.SetValueDirect(obj, member, value);
        }

        public bool TryGetValue(object obj, string sMemberName, out object result, params object[] args)
        {
            if (obj == null)
            {
                result = null;
                return false;
            }

            return DynamicUtil.TryGetValueDirect(obj, sMemberName, out result, args);
        }

        public bool TryGetValue(object obj, MemberInfo member, out object result, params object[] args)
        {
            result = null;
            if (obj == null) return false;

            try
            {
                result = DynamicUtil.GetValueDirect(obj, member, args);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TryGetValue<T>(object obj, string sMemberName, out T result, params object[] args)
        {
            object temp;
            if(TryGetValue(obj, sMemberName, out temp, args))
            {
                result = ConvertUtil.Coerce<T>(temp);
                return true;
            }
            else
            {
                result = default(T);
                return false;
            }
        }

        public bool TryGetValue<T>(object obj, MemberInfo member, out T result, params object[] args)
        {
            object temp;
            if (TryGetValue(obj, member, out temp, args))
            {
                result = ConvertUtil.Coerce<T>(temp);
                return true;
            }
            else
            {
                result = default(T);
                return false;
            }
        }

        public object InvokeMethod(object obj, string name, params object[] args)
        {
            if (obj == null) return false;

            return DynamicUtil.InvokeMethodDirect(obj, name, args);
        }

    }

}
