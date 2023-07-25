#pragma warning disable 0649 // variable declared but not used.
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ObjUtil = com.spacepuppy.Utils.ObjUtil;
using TypeUtil = com.spacepuppy.Utils.TypeUtil;

namespace com.spacepuppy
{

    [System.Flags]
    public enum ProxyParams
    {
        None = 0,
        QueriesTarget = 1, //the proxy performs a runtime search/query that can't be done at editor time
        HandlesTriggerDirectly = 2, //if the proxy is triggered it should not be reduced to its target
        PrioritizeAsTargetFirst = 4, //if reducing as some target type, attempt to reduce the proxy itself before reaching for its target. This is primarily used for proxy's that are themselves Components rather than the more commong ScriptableObject
    }

    /// <summary>
    /// Interface to define an object that allows pass through of another object.
    /// 
    /// IProxy's are good for retrieving references to objects that can't otherwise be referenced directly in the inspector.
    /// 
    /// ObjUtil.GetAsFromSource respects IProxy.
    /// 
    /// This is useful at editor time when you may need to reference something in a scene that doesn't yet exist at editor time (an uninstantiated prefab for instance). 
    /// An IProxy may let you reference said object by name, tag, layer, type, etc.
    /// 
    /// If an IProxy also implement ITriggerable, triggers will not reduce the proxy, and instead allow the IProxy to do the appropriate heavy lifting.
    /// 
    /// For examples see:
    /// ProxyTarget
    /// </summary>
    public interface IProxy
    {

        /// <summary>
        /// Various flags that modify the way certain contexts will treat an IProxy.
        /// </summary>
        ProxyParams Params { get; }

        /// <summary>
        /// Returns the type the result will be returned as when calling GetTargetInternal.
        /// </summary>
        /// <returns></returns>
        System.Type GetTargetType();

        /// <summary>
        /// Returns the target. The expected type is only a suggestion to be used to coerce the type if necessary. 
        /// It will still return an object even if it does no match the expectedType.
        /// 
        /// This method should generally only be called internally by IProxyExnteions, use the GetTarget/GetTargetAs to interact with the IProxy.
        /// </summary>
        /// <param name="expectedType">A type to coerce to if the IProxy deems to do so, the result does not necessarily match the expectedType</param>
        /// <param name="arg">An optional argument that may be used by the proxy to lookup/query the target. It's use is specific to the IProxy implementation.</param>
        /// <returns></returns>
        object GetTargetInternal(System.Type expectedType, object arg);

    }

    public static class IProxyExtensions
    {

        public static bool QueriesTarget(this IProxy proxy)
        {
            return (proxy.Params & ProxyParams.QueriesTarget) != 0;
        }

        public static bool PrioritizesSelfAsTarget(this IProxy proxy)
        {
            return (proxy.Params & ProxyParams.PrioritizeAsTargetFirst) != 0;
        }

        public static object GetTarget(this IProxy proxy)
        {
            return proxy.GetTargetInternal(typeof(object), null);
        }

        public static object GetTarget(this IProxy proxy, object arg)
        {
            return proxy.GetTargetInternal(typeof(object), arg);
        }

        public static object GetTargetAs(this IProxy proxy, System.Type tp, object arg = null)
        {
            return ObjUtil.GetAsFromSource(tp, proxy.GetTargetInternal(tp, arg));
        }

        public static T GetTargetAs<T>(this IProxy proxy, object arg = null) where T : class
        {
            return ObjUtil.GetAsFromSource<T>(proxy.GetTargetInternal(typeof(T), arg));
        }

        public static object GetTarget_ParamsRespecting(this IProxy proxy, object arg = null)
        {
            if ((proxy.Params & ProxyParams.PrioritizeAsTargetFirst) != 0)
            {
                return proxy;
            }
            return proxy.GetTargetInternal(typeof(object), arg);
        }

        public static object GetTargetAs_ParamsRespecting(this IProxy proxy, System.Type tp, object arg = null)
        {
            if ((proxy.Params & ProxyParams.PrioritizeAsTargetFirst) != 0)
            {
                var result = ObjUtil.GetAsFromSource(tp, proxy);
                if (result != null) return result;
            }
            return ObjUtil.GetAsFromSource(tp, proxy.GetTargetInternal(tp, arg));
        }

        public static object GetTargetAs_ParamsRespecting(this IProxy proxy, System.Type[] types, object arg = null)
        {
            switch (types.Length)
            {
                case 0:
                    return GetTargetAs_ParamsRespecting(proxy, typeof(object), arg);
                case 1:
                    return GetTargetAs_ParamsRespecting(proxy, types[0] ?? typeof(object), arg);
                default:
                    {
                        if ((proxy.Params & ProxyParams.PrioritizeAsTargetFirst) != 0)
                        {
                            var result = ObjUtil.GetAsFromSource(types, proxy);
                            if (result != null) return result;
                        }

                        return ObjUtil.GetAsFromSource(types, proxy.GetTargetInternal(types[0] ?? typeof(object), arg));
                    }
            }
        }

        public static T GetTargetAs_ParamsRespecting<T>(this IProxy proxy, object arg) where T : class
        {
            if ((proxy.Params & ProxyParams.PrioritizeAsTargetFirst) != 0)
            {
                var result = ObjUtil.GetAsFromSource<T>(proxy);
                if (result != null) return result;
            }

            return ObjUtil.GetAsFromSource<T>(proxy.GetTargetInternal(typeof(T), arg));
        }

        public static object ReduceIfProxy(this object obj)
        {
            if (obj is IProxy p) return p.GetTarget_ParamsRespecting();

            return ObjUtil.SanitizeRef(obj);
        }

        public static object ReduceIfProxy(this object obj, object arg)
        {
            if (obj is IProxy p) return p.GetTarget_ParamsRespecting(arg);

            return ObjUtil.SanitizeRef(obj);
        }

        public static object ReduceIfProxyAs(this object obj, System.Type tp)
        {
            if (obj is IProxy p) return p.GetTargetAs_ParamsRespecting(tp);

            return ObjUtil.SanitizeRef(obj);
        }

        public static object ReduceIfProxyAs(this object obj, object arg, System.Type tp)
        {
            if (obj is IProxy p) return p.GetTargetAs_ParamsRespecting(tp, arg);

            return ObjUtil.SanitizeRef(obj);
        }

        /// <summary>
        /// Returns the object as the type T or as IProxy, prioritizing type T. 
        /// Used when setting fields to ensure that the incoming object is either T or IProxy for T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="assertProxyTargetTypeMatches"></param>
        /// <returns></returns>
        public static object FilterAsProxyOrType<T>(object obj, bool assertProxyTargetTypeMatches = false)
        {
            if (obj is T) return obj;

            var p = obj as IProxy;
            if (p == null) return null;

            if (assertProxyTargetTypeMatches && !TypeUtil.IsType(p.GetTargetType(), typeof(T))) return null;

            return p;
        }

        /// <summary>
        /// Returns true if object is an IProxy.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsProxy(this object obj)
        {
            return obj is IProxy;
        }

        /// <summary>
        /// Returns true if object is an IProxy and its Params does not PrioritizeAsTargetFirst.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsProxy_ParamsRespecting(this object obj)
        {
            return obj is IProxy p && (p.Params & ProxyParams.PrioritizeAsTargetFirst) == 0;
        }

        public static System.Type GetType(this object obj, bool respectProxy)
        {
            if (respectProxy && obj is IProxy p && (p.Params & ProxyParams.PrioritizeAsTargetFirst) == 0)
            {
                return p.GetTargetType() ?? typeof(object);
            }
            return obj?.GetType();
        }

    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class RespectsIProxyAttribute : System.Attribute
    {

    }

    [System.Serializable]
    public class ProxyRef : com.spacepuppy.Project.SerializableInterfaceRef<IProxy>
    {

    }

}
