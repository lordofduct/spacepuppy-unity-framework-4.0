using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
#if SP_UNITASK
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace com.spacepuppy
{

    /// <remarks>
    /// While NetworkManager returns true for IsHost AND IsClient at the same time. 
    /// This enum makes an explicit distinction between the two. 
    /// To test if a client of any type just check if >= Host. (equivalent of NetworkManager.IsClient)
    /// To test if a server/host/offline, check if <= Host. (equivalent of NetworkManager.IsServer || NetworkManager.
    /// </remarks>
    public enum NetworkRelationship
    {
        Offline = 0,
        Server = 1,
        Host = 2,
        Client = 3,
        ConnectedClient = 4,
    }

    [System.Flags]
    public enum NetworkOwner
    {
        Unknown = 0,
        Server = 1,
        Local = 2,
        Remote = 4,
    }

    [System.Flags]
    public enum NetworkStatus
    {
        Unkown = 0,
        Offline = 1, //if NetworkBehaviour.IsOffline() would return true
        Server = 2, //if NetworkBehaviour.NetworkManager.IsServer would return true
        Client = 4, //if NetworkBehaviour.NetworkManager.IsClient would return true
        Owner = 8, //if NetworkBehaviour.IsOwner would return true

        ServerOrOffline = Offline | Server,
        OwnerOrOffline = Offline | Owner,
    }

    public static class NetcodeExtensions
    {

        static readonly System.Reflection.FieldInfo FIELD_NETWORKOBJ_GLOBALOBJID = typeof(NetworkObject).GetField("GlobalObjectIdHash", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        public static uint GetGlobalObjectIdHash(this NetworkObject no) => FIELD_NETWORKOBJ_GLOBALOBJID != null ? (uint)FIELD_NETWORKOBJ_GLOBALOBJID.GetValue(no) : 0;


        public static NetworkRelationship GetNetworkRelationship(this NetworkManager manager)
        {
            if (manager.IsServer)
                return manager.IsHost ? NetworkRelationship.Host : NetworkRelationship.Server;
            else if (manager.IsClient)
                return manager.IsConnectedClient ? NetworkRelationship.ConnectedClient : NetworkRelationship.Client;
            else
                return NetworkRelationship.Offline;
        }

        public static NetworkRelationship GetNetworkRelationship(this NetworkObject obj)
        {
            var manager = obj.NetworkManager ?? NetworkManager.Singleton;
            if (manager == null) return NetworkRelationship.Offline;

            if (manager.IsServer)
                return manager.IsHost ? NetworkRelationship.Host : NetworkRelationship.Server;
            else if (manager.IsClient)
                return manager.IsConnectedClient ? NetworkRelationship.ConnectedClient : NetworkRelationship.Client;
            else
                return NetworkRelationship.Offline;
        }

        public static NetworkRelationship GetNetworkRelationship(this NetworkBehaviour behaviour)
        {
            var manager = behaviour.NetworkManager ?? NetworkManager.Singleton;
            if (manager == null) return NetworkRelationship.Offline;

            if (manager.IsServer)
                return manager.IsHost ? NetworkRelationship.Host : NetworkRelationship.Server;
            else if (manager.IsClient)
                return manager.IsConnectedClient ? NetworkRelationship.ConnectedClient : NetworkRelationship.Client;
            else
                return NetworkRelationship.Offline;
        }

        public static NetworkOwner GetNetworkOwner(this NetworkObject nobj)
        {
            NetworkOwner owner = NetworkOwner.Unknown;
            if (nobj)
            {
                if (nobj.IsOwnedByServer)
                {
                    owner |= NetworkOwner.Server;
                }
                if (nobj.OwnerClientId == nobj.NetworkManager.LocalClientId)
                {
                    owner |= NetworkOwner.Local;
                }
                else
                {
                    owner |= NetworkOwner.Remote;
                }
            }
            return owner;
        }

        public static NetworkOwner GetNetworkOwner(this NetworkBehaviour behaviour)
        {
            var nobj = behaviour ? behaviour.NetworkObject : null;
            NetworkOwner owner = NetworkOwner.Unknown;
            if (nobj)
            {
                if (nobj.IsOwnedByServer)
                {
                    owner |= NetworkOwner.Server;
                }
                if (nobj.OwnerClientId == nobj.NetworkManager.LocalClientId)
                {
                    owner |= NetworkOwner.Local;
                }
                else
                {
                    owner |= NetworkOwner.Remote;
                }
            }
            return owner;
        }

        public static bool Intersects(this NetworkOwner e, NetworkOwner mask) => ((int)e & (int)mask) != 0;

        public static NetworkStatus GetNetworkStatus(this NetworkObject nobj)
        {
            var result = NetworkStatus.Unkown;
            if (nobj)
            {
                if (nobj.NetworkManager?.IsServer ?? false)
                {
                    result |= NetworkStatus.Server;
                }
                if (nobj.NetworkManager?.IsClient ?? false)
                {
                    result |= NetworkStatus.Client;
                }
                if (result == NetworkStatus.Unkown)
                {
                    result |= NetworkStatus.Offline;
                }
                if (nobj.IsOwner)
                {
                    result |= NetworkStatus.Owner;
                }
            }
            return result;
        }

        public static NetworkStatus GetNetworkStatus(this NetworkBehaviour behaviour)
        {
            var nobj = behaviour ? behaviour.NetworkObject : null;

            var result = NetworkStatus.Unkown;
            if (nobj)
            {
                if (nobj.NetworkManager?.IsServer ?? false)
                {
                    result |= NetworkStatus.Server;
                }
                if (nobj.NetworkManager?.IsClient ?? false)
                {
                    result |= NetworkStatus.Client;
                }
                if (result == NetworkStatus.Unkown)
                {
                    result |= NetworkStatus.Offline;
                }
                if (nobj.IsOwner)
                {
                    result |= NetworkStatus.Owner;
                }
            }
            return result;
        }

        public static bool Intersects(this NetworkStatus e, NetworkStatus mask) => ((int)e & (int)mask) != 0;

        #region NetworkManager Async Extensions

#if SP_UNITASK
        public async static UniTask ShutdownAsync(this NetworkManager manager, bool discardMessageQueue = false)
        {
            manager.Shutdown(discardMessageQueue);
            while (manager.ShutdownInProgress)
            {
                await UniTask.Yield();
            }
        }
#else
        public async static Task ShutdownAsync_Task(this NetworkManager manager, bool discardMessageQueue = false)
        {
            manager.Shutdown(discardMessageQueue);
            while (manager.ShutdownInProgress)
            {
                await Task.Yield();
            }
        }
#endif

        #endregion


        /// <summary>
        /// Returns true if not the server/client. This is the same as if GetNetworkRelationship returns 'Offline'.
        /// This does not adequately represent if the client is connected, check 'IsClientConnected' for that.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsOffline(this NetworkManager obj) => !(obj.IsServer || obj.IsClient);

        public static bool IsOffline(this NetworkObject obj) => !((obj.NetworkManager?.IsServer ?? false) || (obj.NetworkManager?.IsClient ?? false));

        public static bool IsOffline(this NetworkBehaviour obj) => !((obj.NetworkManager?.IsServer ?? false) || (obj.NetworkManager?.IsClient ?? false));


        public static bool IsServer(this NetworkObject obj) => obj.NetworkManager?.IsServer ?? false;

        public static bool IsServer(this NetworkBehaviour behaviour) => behaviour.NetworkManager?.IsServer ?? false;


        public static bool IsServerOrOffline(this NetworkManager obj) => obj.IsServer || !obj.IsClient;

        public static bool IsServerOrOffline(this NetworkObject obj) => object.ReferenceEquals(obj.NetworkManager, null) || obj.NetworkManager.IsServer || !obj.NetworkManager.IsClient;

        public static bool IsServerOrOffline(this NetworkBehaviour obj) => object.ReferenceEquals(obj.NetworkManager, null) || obj.NetworkManager.IsServer || !obj.NetworkManager.IsClient;



        public static bool IsOwnerOrOffline(this NetworkObject obj) => obj.IsOwner || object.ReferenceEquals(obj.NetworkManager, null) || !(obj.NetworkManager.IsServer || obj.NetworkManager.IsClient);

        public static bool IsOwnerOrOffline(this NetworkBehaviour obj) => obj.IsOwner || object.ReferenceEquals(obj.NetworkManager, null) || !(obj.NetworkManager.IsServer || obj.NetworkManager.IsClient);


        #region NetworkList

        public static IEnumerable<T> AsEnumerable<T>(this NetworkList<T> nlst) where T : unmanaged, System.IEquatable<T>
        {
            var e = nlst.GetEnumerator();
            while (e.MoveNext())
            {
                yield return e.Current;
            }
        }

        public static void AddRange<T>(this NetworkList<T> nlst, IEnumerable<T> e) where T : unmanaged, System.IEquatable<T>
        {
            foreach (var v in e)
            {
                nlst.Add(v);
            }
        }

        public static void Reset<T>(this NetworkList<T> nlst, IEnumerable<T> e) where T : unmanaged, System.IEquatable<T>
        {
            nlst.Clear();
            foreach (var v in e)
            {
                nlst.Add(v);
            }
        }

        #endregion

        #region NetworkPrefabHandler Extension Methods

        static System.Func<NetworkPrefabHandler, uint, bool> __METHOD_NETWORKPREFABHANDLER_CONTAINSHANDLER_UINT;
        static System.Func<NetworkPrefabHandler, uint, bool> METHOD_NETWORKPREFABHANDLER_CONTAINSHANDLER_UINT
        {
            get
            {
                if (__METHOD_NETWORKPREFABHANDLER_CONTAINSHANDLER_UINT == null)
                {
                    __METHOD_NETWORKPREFABHANDLER_CONTAINSHANDLER_UINT = com.spacepuppy.Dynamic.DynamicUtil.CreateUnboundFunction<NetworkPrefabHandler, uint, bool>("ContainsHandler", true);
                    var methinfo = typeof(NetworkPrefabHandler).GetMethod("ContainsHandler", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public, null, new[] { typeof(uint) }, null);
                    if (__METHOD_NETWORKPREFABHANDLER_CONTAINSHANDLER_UINT == null)
                    {
                        __METHOD_NETWORKPREFABHANDLER_CONTAINSHANDLER_UINT = (a, b) => false;
                        UnityEngine.Debug.LogWarning("This version of Spacepuppy Framework does not support the version of Unity it's being used with. (ObjUtil)");
                    }
                }
                return __METHOD_NETWORKPREFABHANDLER_CONTAINSHANDLER_UINT;
            }
        }
        public static bool ContainsHandler(this NetworkPrefabHandler prefabhandler, uint networkobjid) => METHOD_NETWORKPREFABHANDLER_CONTAINSHANDLER_UINT.Invoke(prefabhandler, networkobjid);

        static System.Func<NetworkPrefabHandler, NetworkObject, bool> __METHOD_NETWORKPREFABHANDLER_CONTAINSHANDLER_NOBJ;
        static System.Func<NetworkPrefabHandler, NetworkObject, bool> METHOD_NETWORKPREFABHANDLER_CONTAINSHANDLER_NOBJ
        {
            get
            {
                if (__METHOD_NETWORKPREFABHANDLER_CONTAINSHANDLER_NOBJ == null)
                {
                    __METHOD_NETWORKPREFABHANDLER_CONTAINSHANDLER_NOBJ = com.spacepuppy.Dynamic.DynamicUtil.CreateUnboundFunction<NetworkPrefabHandler, NetworkObject, bool>("ContainsHandler", true);
                    var methinfo = typeof(NetworkPrefabHandler).GetMethod("ContainsHandler", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public, null, new[] { typeof(uint) }, null);
                    if (__METHOD_NETWORKPREFABHANDLER_CONTAINSHANDLER_NOBJ == null)
                    {
                        __METHOD_NETWORKPREFABHANDLER_CONTAINSHANDLER_NOBJ = (a, b) => false;
                        UnityEngine.Debug.LogWarning("This version of Spacepuppy Framework does not support the version of Unity it's being used with. (ObjUtil)");
                    }
                }
                return __METHOD_NETWORKPREFABHANDLER_CONTAINSHANDLER_NOBJ;
            }
        }
        public static bool ContainsHandler(this NetworkPrefabHandler prefabhandler, NetworkObject nobj) => METHOD_NETWORKPREFABHANDLER_CONTAINSHANDLER_NOBJ.Invoke(prefabhandler, nobj);
        public static bool ContainsHandler(this NetworkPrefabHandler prefabhandler, GameObject go) => METHOD_NETWORKPREFABHANDLER_CONTAINSHANDLER_NOBJ.Invoke(prefabhandler, go.GetComponent<NetworkObject>());

#if NETCODE_1_4_ORNEWER
        public static bool TryAddNetworkPrefab(this NetworkManager manager, GameObject go)
        {
            if (go && manager != null && !manager.NetworkConfig.Prefabs.Contains(go))
            {
                manager.PrefabHandler.AddNetworkPrefab(go);
                return true;
            }
            else
            {
                return false;
            }
        }
#endif

        #endregion

    }
}
