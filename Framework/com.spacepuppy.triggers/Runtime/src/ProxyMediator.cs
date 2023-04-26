using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using com.spacepuppy.Events;
using com.spacepuppy.Project;

namespace com.spacepuppy
{

    /// <summary>
    /// Facilitates sending events between assets/prefabs. By relying on a ScriptableObject to mediate an event between 2 other assets 
    /// you can have communication between those assets setup at dev time. For example communicating an event between 2 prefabs, or a prefab 
    /// and a scene, or any other similar situation. 
    /// 
    /// To use:
    /// Create a ProxyMediator as an asset and name it something unique.
    /// Any SPEvent/Trigger can target this asset just by dragging it into the target field.
    /// Now any script can accept a ProxyMediator and listen for the 'OnTriggered' event to receive a signal that the mediator had been triggered elsewhere.
    /// You can also attach a T_OnProxyMediatorTriggered, and drag the ProxyMediator asset in question into the 'Mediator' field. This T_ will fire when the mediator is triggered elsewhere.
    /// 
    /// Note that ProxyMediators are considered equal if they have the same Guid. This includes the == operator. But just like UnityEngine.Object, if the ProxyMediator is typed as object the == operator is not used. Collections will still respect it though since they rely on the Equals method. 
    /// Basically think of this as like 'string' where even though 2 strings may not be the same reference object, they are treated as == if the value of the string is the same. String also fails == operator if you cast it to object.
    /// </summary>
    [CreateAssetMenu(fileName = "ProxyMediator", menuName = "Spacepuppy/Proxy/ProxyMediator", order = int.MinValue)]
    public class ProxyMediator : ScriptableObject, IAssetGuidIdentifiable, System.IEquatable<ProxyMediator>, ITriggerable, IProxy
    {

        #region Cross-Domain Lookup

        /// <summary>
        /// Addressables/AssetBundles break ProxyMediator's original implementation. Since these loaded assets will create new instances of the mediator. 
        /// This lookup table allows us to link all of these mediators together. A refcount is used to destory the hook when the last mediator for that guid 
        /// is destroyed.
        /// </summary>
        private static readonly Dictionary<System.Guid, CrossDomainHook> _crossDomainLookupTable = new Dictionary<System.Guid, CrossDomainHook>();

        private static bool FindHook(ProxyMediator proxy, out CrossDomainHook hook, bool ignorePropogateIfNotInitialized = false)
        {
            if (_crossDomainLookupTable.TryGetValue(proxy.AssetId, out hook))
            {
                return true;
            }
            else if (!proxy._initialized && !ignorePropogateIfNotInitialized)
            {
                //this state is if someone attempts to register before 'Awake' is called due to out of order initilization. Allow the hook to propogate, but Awake will tick the RefCount later.
                hook = new CrossDomainHook();
                _crossDomainLookupTable[proxy.AssetId] = hook;
                return true;
            }
            else
            {
                //we're destroyed... do nothing
                return false;
            }
        }

        public static IEnumerable<ProxyInfo> EnumerateActiveProxyMediators() => _crossDomainLookupTable.Select(o => new ProxyInfo()
        {
            Guid = o.Key,
            Target = o.Value.Target,
        });

        /// <summary>
        /// Set's the target of an initialized proxymediator with matching guid. 
        /// If not matching proxymediator is found, nothing happens.
        /// </summary>
        public static bool SetProxyTarget(System.Guid guid, object target)
        {
            CrossDomainHook hook;
            if (_crossDomainLookupTable.TryGetValue(guid, out hook))
            {
                hook.Target = target;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Finds a matching initialized proxymediator with matching guid and calls WaitForNextTrigger on it. 
        /// RadicalWaitHandle.Null is returned if no mediator is located.
        /// </summary>
        public static IRadicalWaitHandle WaitForNextTrigger(System.Guid guid)
        {
            CrossDomainHook hook;
            if (_crossDomainLookupTable.TryGetValue(guid, out hook))
            {
                if (hook.Handle == null) hook.Handle = RadicalWaitHandle.Create();
                return hook.Handle;
            }
            else
            {
                return RadicalWaitHandle.Null;
            }
        }

        #endregion


        public event System.EventHandler<TempEventArgs> OnTriggered
        {
            add
            {
                CrossDomainHook hook;
                if (FindHook(this, out hook))
                {
                    hook.OnTriggered += value;
                }
            }
            remove
            {
                CrossDomainHook hook;
                if (FindHook(this, out hook, true))
                {
                    hook.OnTriggered -= value;
                }
            }
        }

        #region Fields

        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("_guid")]
        [SerializableGuid.Config(LinkToAsset = true, AllowZero = false)]
        private SerializableGuid _assetId;

        [SerializeField]
        private bool _triggerSyncedTargetWhenTriggered = false;
        [SerializeField]
        private bool _passAlongTriggerArgWhenTrigger = false;

        [System.NonSerialized]
        private bool _initialized;

        #endregion

        #region CONSTRUCTOR

        protected virtual void Awake()
        {
            CrossDomainHook hook;
            if (FindHook(this, out hook))
            {
                hook.RefCount++;
            }
            _initialized = true;
        }

        protected virtual void OnDestroy()
        {
            CrossDomainHook hook;
            if (FindHook(this, out hook, true))
            {
                hook.RefCount--;
                if (hook.RefCount <= 0)
                {
                    _crossDomainLookupTable.Remove(this.AssetId);
                }
            }
        }

        #endregion

        #region Properties

        public System.Guid AssetId => _assetId.ToGuid();

        public bool HasTarget => this.GetTarget() != null;

        #endregion

        #region Methods

        public void SetProxyTarget(object target)
        {
            CrossDomainHook hook;
            if (FindHook(this, out hook))
            {
                hook.Target = target;
            }
        }

        public IRadicalWaitHandle WaitForNextTrigger()
        {
            CrossDomainHook hook;
            if (FindHook(this, out hook))
            {
                if (hook.Handle == null) hook.Handle = RadicalWaitHandle.Create();
                return hook.Handle;
            }
            else
            {
                return RadicalWaitHandle.Null;
            }
        }

        public virtual void Trigger(object sender, object arg)
        {
            CrossDomainHook hook;
            if (FindHook(this, out hook))
            {
                var h = hook.Handle;
                hook.Handle = null;

                var d = hook.OnTriggered;
                if (d != null)
                {
                    using (var ev = TempEventArgs.Create(arg))
                    {
                        d(this, ev);
                    }
                }
                if (_triggerSyncedTargetWhenTriggered && hook.Target != null)
                {
                    EventTriggerEvaluator.Current.TriggerAllOnTarget(hook.Target, arg, sender, _passAlongTriggerArgWhenTrigger ? arg : null);
                }

                if (h != null)
                {
                    h.SignalComplete();
                    (h as System.IDisposable).Dispose();
                }
            }
        }

        #endregion

        #region Equality Interface

        public override bool Equals(object other)
        {
            return DefaultComparer.Equals(this, other as ProxyMediator);
        }

        public virtual bool Equals(ProxyMediator other)
        {
            return DefaultComparer.Equals(this, other);
        }

        public override int GetHashCode()
        {
            return DefaultComparer.GetHashCode(this);
        }

        public static bool operator ==(ProxyMediator a, ProxyMediator b)
        {
            return DefaultComparer.Equals(a, b);
        }

        public static bool operator !=(ProxyMediator a, ProxyMediator b)
        {
            return !DefaultComparer.Equals(a, b);
        }

        #endregion

        #region ITriggerableMechanism Interface

        bool ITriggerable.CanTrigger
        {
            get
            {
                return true;
            }
        }

        int ITriggerable.Order
        {
            get
            {
                return 0;
            }
        }

        bool ITriggerable.Trigger(object sender, object arg)
        {
            this.Trigger(sender, arg);
            return true;
        }

        #endregion

        #region IProxy Interface

        ProxyParams IProxy.Params => ProxyParams.HandlesTriggerDirectly;

        public object GetTarget()
        {
            CrossDomainHook hook;
            if (FindHook(this, out hook))
            {
                return hook.Target;
            }
            return null;
        }

        object IProxy.GetTargetInternal(System.Type expectedType, object arg)
        {
            return this.GetTarget();
        }

        System.Type IProxy.GetTargetType()
        {
            return this.GetTarget()?.GetType() ?? typeof(object);
        }

        #endregion

        #region Special Types

        private class CrossDomainHook
        {
            public System.EventHandler<TempEventArgs> OnTriggered;
            public RadicalWaitHandle Handle;
            public object Target;
            public int RefCount;
        }

        public static IEqualityComparer<ProxyMediator> DefaultComparer => AssetGuidIdentifiableEqualityComparer<ProxyMediator>.Default;

        public struct ProxyInfo
        {
            public System.Guid Guid;
            public object Target;

            public IRadicalWaitHandle WaitForNextTrigger() => ProxyMediator.WaitForNextTrigger(this.Guid);

            public void SetProxyTarget(object target)
            {
                this.Target = target;
                ProxyMediator.SetProxyTarget(this.Guid, target);
            }
        }

        #endregion

    }

}
