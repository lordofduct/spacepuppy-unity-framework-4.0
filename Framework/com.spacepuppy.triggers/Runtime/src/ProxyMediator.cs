using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

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
    public class ProxyMediator : ScriptableObject, ITriggerable, System.IEquatable<ProxyMediator>
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
            if (_crossDomainLookupTable.TryGetValue(proxy.Guid, out hook))
            {
                return true;
            }
            else if (!proxy._initialized && !ignorePropogateIfNotInitialized)
            {
                //this state is if someone attempts to register before 'Awake' is called due to out of order initilization. Allow the hook to propogate, but Awake will tick the RefCount later.
                hook = new CrossDomainHook();
                _crossDomainLookupTable[proxy.Guid] = hook;
                return true;
            }
            else
            {
                //we're destroyed... do nothing
                return false;
            }
        }

        #endregion


        public event System.EventHandler OnTriggered
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
        [SerializableGuid.Config(LinkToAsset = true, AllowZero = false)]
        private SerializableGuid _guid;

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
                    _crossDomainLookupTable.Remove(this.Guid);
                }
            }
        }

        #endregion

        #region Properties

        public System.Guid Guid => _guid.ToGuid();

        #endregion

        #region Methods

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

        public virtual void Trigger()
        {
            CrossDomainHook hook;
            if (FindHook(this, out hook))
            {
                var h = hook.Handle;
                hook.Handle = null;

                hook.OnTriggered?.Invoke(this, System.EventArgs.Empty);

                if (h != null)
                {
                    h.SignalComplete();
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
            this.Trigger();
            return true;
        }

        #endregion

        #region Special Types

        private class CrossDomainHook
        {
            public System.EventHandler OnTriggered;
            public RadicalWaitHandle Handle;
            public int RefCount;
        }

        public static readonly Comparer DefaultComparer = new Comparer();

        public class Comparer : IEqualityComparer<ProxyMediator>
        {
            public bool Equals(ProxyMediator x, ProxyMediator y)
            {
                bool xnull = object.ReferenceEquals(x, null);
                bool ynull = object.ReferenceEquals(y, null);
                if (xnull && ynull) return true;
                if (xnull) return !ObjUtil.IsObjectAlive(y);
                if (ynull) return !ObjUtil.IsObjectAlive(x);

                return x.Guid == y.Guid;
            }

            public int GetHashCode(ProxyMediator obj)
            {
                return obj?.Guid.GetHashCode() ?? 0;
            }
        }

        #endregion

    }

}
