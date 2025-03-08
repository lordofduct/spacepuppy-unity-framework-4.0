using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

using com.spacepuppy.Utils;

namespace com.spacepuppy.netcode
{

    [System.Serializable]
    public struct NetworkIdToken : INetworkSerializable
    {
        private byte mode;
        private ulong id;

        public NetworkIdToken(NetworkIdentifiableObject nidobj)
        {
            if (nidobj)
            {
                mode = 2;
                id = nidobj.Id;
            }
            else
            {
                mode = 0;
                id = 0;
            }
        }
        public NetworkIdToken(NetworkObject nobj)
        {
            if (nobj)
            {
                mode = 1;
                id = nobj.NetworkObjectId;
            }
            else
            {
                mode = 0;
                id = 0;
            }
        }

        public GameObject FindTarget(NetworkManager manager = null)
        {
            switch (mode)
            {
                case 1:
                    if (manager == null) manager = NetworkManager.Singleton;
                    return manager && manager.SpawnManager.SpawnedObjects.TryGetValue(id, out NetworkObject nobj) && nobj ? nobj.gameObject : null;
                case 2:
                    return NetworkIdentifiableObject.Find<GameObject>(id);
                default:
                    return null;
            }
        }
        public bool TryFindTarget(out GameObject go, NetworkManager manager = null)
        {
            switch (mode)
            {
                case 1:
                    if (manager == null) manager = NetworkManager.Singleton;
                    if (manager && manager.SpawnManager.SpawnedObjects.TryGetValue(id, out NetworkObject nobj) && nobj)
                    {
                        go = nobj.gameObject;
                        return true;
                    }
                    else
                    {
                        go = null;
                        return false;
                    }
                case 2:
                    go = NetworkIdentifiableObject.Find<GameObject>(id);
                    return go != null;
                default:
                    go = null;
                    return false;
            }
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref mode);
            serializer.SerializeValue(ref id);
        }
    }

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
            //Debug.Log("SEARCHING FOR: " + id.ToString("X") + " : WITH " + _table.Count + " INITIALIZED");
            if (_table.TryGetValue(id, out NetworkIdentifiableObject nobj))
            {
                return ObjUtil.GetAsFromSource<T>(nobj);
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
