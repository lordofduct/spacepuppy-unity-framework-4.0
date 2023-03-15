using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

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

    public static class NetcodeExtensions
    {

        static System.Reflection.FieldInfo FIELD_NETWORKOBJ_GLOBALOBJID = typeof(NetworkObject).GetField("GlobalObjectIdHash", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);

        public static uint GetGlobalObjectIdHash(this NetworkObject no) => (uint)FIELD_NETWORKOBJ_GLOBALOBJID.GetValue(no);


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


        /// <summary>
        /// Returns true if not the server/client. This is the same as if GetNetworkRelationship returns 'Offline'.
        /// This does not adequately represent if the client is connected, check 'IsClientConnected' for that.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsOffline(this NetworkManager obj) => !(obj.IsServer || !obj.IsClient);

        public static bool IsOffline(this NetworkObject obj) => !((obj.NetworkManager?.IsServer ?? false) || (obj.NetworkManager?.IsClient ?? false));

        public static bool IsOffline(this NetworkBehaviour obj) => !((obj.NetworkManager?.IsServer ?? false) || (obj.NetworkManager?.IsClient ?? false));


        public static bool IsServer(this NetworkObject obj) => obj.NetworkManager?.IsServer ?? false;

        public static bool IsServer(this NetworkBehaviour behaviour) => behaviour.NetworkManager?.IsServer ?? false;


        public static bool IsServerOrOffline(this NetworkManager obj) => obj.IsServer || !obj.IsConnectedClient;

        public static bool IsServerOrOffline(this NetworkObject obj) => object.ReferenceEquals(obj.NetworkManager, null) || obj.NetworkManager.IsServer || !obj.NetworkManager.IsConnectedClient;

        public static bool IsServerOrOffline(this NetworkBehaviour obj) => object.ReferenceEquals(obj.NetworkManager, null) || obj.NetworkManager.IsServer || !obj.NetworkManager.IsConnectedClient;



        public static bool IsOwnerOrOffline(this NetworkObject obj) => obj.IsOwner || object.ReferenceEquals(obj.NetworkManager, null) || !(obj.NetworkManager.IsServer || obj.NetworkManager.IsConnectedClient);

        public static bool IsOwnerOrOffline(this NetworkBehaviour obj) => obj.IsOwner || object.ReferenceEquals(obj.NetworkManager, null) || !(obj.NetworkManager.IsServer || obj.NetworkManager.IsConnectedClient);

    }
}
