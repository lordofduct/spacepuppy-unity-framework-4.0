using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.netcode
{

    //NOTE - NetworkIdentifiableObject does not enable any sort of RPC logic, it just allows locating an object by an id over the network.
    [Infobox("Represents a static scene object that can be identified over the network by an ID.")]
    public sealed class NetworkIdentifiableObject : SPMonoBehaviour
    {

        private static readonly Dictionary<ulong, NetworkIdentifiableObject> _table = new();

        public static NetworkIdentifiableObject Find(ulong id)
        {
            if (_table.TryGetValue(id, out NetworkIdentifiableObject nobj))
            {
                return nobj;
            }
            return null;
        }

        public static T Find<T>(ulong id) where T : class
        {
            Debug.Log("SEARCHING FOR: " + id.ToString("X") + " : WITH " + _table.Count + " INITIALIZED");
            if (_table.TryGetValue(id, out NetworkIdentifiableObject nobj))
            {
                return nobj.GetComponent<T>();
            }
            return null;
        }

        public static ulong GetNID(Component c)
        {
            if (c is NetworkIdentifiableObject n)
            {
                return n.Id;
            }
            else if (c && c.TryGetComponent(out NetworkIdentifiableObject n2))
            {
                return n2.Id;
            }
            else
            {
                return 0;
            }
        }

        public static ulong GetNID(GameObject go)
        {
            if (go && go.TryGetComponent(out NetworkIdentifiableObject n))
            {
                return n.Id;
            }
            return 0;
        }

        public static ulong GetNID(object obj)
        {
            if (obj is NetworkIdentifiableObject n)
            {
                return n.Id;
            }
            else if (obj is Component c)
            {
                if (c && c.TryGetComponent(out NetworkIdentifiableObject n2))
                {
                    return n2.Id;
                }
            }
            else if (obj is GameObject go)
            {
                if (go && go.TryGetComponent(out NetworkIdentifiableObject n2))
                {
                    return n2.Id;
                }
            }

            return 0;
        }

        #region Fields

        [SerializeField, ShortUid.Config(LinkToGlobalId = true), DisableOnPlay]
        private ShortUid _id;

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();

            _table[(ulong)_id] = this;
            Debug.Log("REGISTERED NID: " + _id.ToString());
        }

        void OnDestroy()
        {
            _table.Remove((ulong)_id);
        }

        #endregion

        #region Properties

        public ulong Id => _id;

        #endregion

    }

}
