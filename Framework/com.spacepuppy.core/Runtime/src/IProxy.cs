#pragma warning disable 0649 // variable declared but not used.
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using com.spacepuppy.Project;

namespace com.spacepuppy
{

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
    /// For examples see:
    /// ProxyTarget
    /// </summary>
    public interface IProxy
    {
        /// <summary>
        /// Returns true if the underlying proxy performs a search/query of the scene.
        /// Best judgement on if this should be true is if the target returned should be treated as an arbitrary entity of any type.
        /// </summary>
        bool QueriesTarget { get; }

        System.Type GetTargetType();

        object GetTarget();
        object GetTarget(object arg);
        /// <summary>
        /// Attempts to get the target similar to using ObjUtil.GetAsFromSource.
        /// </summary>
        /// <param name="tp"></param>
        /// <returns></returns>
        object GetTargetAs(System.Type tp);
        /// <summary>
        /// Attempts to get the target similar to using ObjUtil.GetAsFromSource.
        /// </summary>
        /// <param name="tp"></param>
        /// <returns></returns>
        object GetTargetAs(System.Type tp, object arg);

    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class RespectsIProxyAttribute : System.Attribute
    {

    }

    [System.Serializable]
    public class ProxyRef : SerializableInterfaceRef<IProxy>
    {

    }

}
