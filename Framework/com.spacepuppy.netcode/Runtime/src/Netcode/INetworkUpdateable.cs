using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

using com.spacepuppy.Utils;
using com.spacepuppy.Collections;

namespace com.spacepuppy.Netcode
{

    public interface INetworkUpdateable : IComponent
    {
        void NetworkUpdate();
    }

    public interface INetworkFixedUpdateable : IComponent
    {
        void NetworkFixedUpdate();
    }

    public interface INetworkLateUpdateable : IComponent
    {
        void NetworkLateUpdate();
    }

    public static class NetworkUpdateableExtensions
    {

        /// <summary>
        /// Will return the local network status regardless of if this GameObject is in a NetworkObject or not.
        /// </summary>
        /// <param name="nobj"></param>
        /// <returns></returns>
        static NetworkStatus GetNetworkStatusUnbiased(GameObject go)
        {
            if (go.GetComponentInParent(out NetworkObject nobj))
            {
                return nobj.GetNetworkStatus();
            }
            else if (NetworkManager.Singleton)
            {
                var result = NetworkStatus.Unkown;
                if (NetworkManager.Singleton.IsServer)
                {
                    result |= NetworkStatus.Server;
                }
                if (NetworkManager.Singleton.IsClient)
                {
                    result |= NetworkStatus.Client;
                }
                return result;
            }
            else
            {
                return NetworkStatus.Offline;
            }
        }

        public static bool RegisterNetworkUpdate(this INetworkUpdateable obj, NetworkStatus status)
        {
            if (obj == null || obj.gameObject == null) throw new System.ArgumentNullException(nameof(obj));

            if (GetNetworkStatusUnbiased(obj.gameObject).Intersects(status))
            {
                var updater = obj.gameObject.AddOrGetComponent<SPNetworkUpdater>();
                updater.Register(obj);
                return true;
            }

            return false;
        }

        public static bool RegisterNetworkUpdate(this INetworkUpdateable obj)
        {
            if (obj == null || obj.gameObject == null) throw new System.ArgumentNullException(nameof(obj));

            var updater = obj.gameObject.AddOrGetComponent<SPNetworkUpdater>();
            updater.Register(obj);
            return true;
        }

        public static bool RegisterNetworkFixedUpdate(this INetworkFixedUpdateable obj, NetworkStatus status)
        {
            if (obj == null || obj.gameObject == null) throw new System.ArgumentNullException(nameof(obj));

            if (GetNetworkStatusUnbiased(obj.gameObject).Intersects(status))
            {
                var updater = obj.gameObject.AddOrGetComponent<SPNetworkFixedUpdater>();
                updater.Register(obj);
                return true;
            }

            return false;
        }

        public static bool RegisterNetworkFixedUpdate(this INetworkFixedUpdateable obj)
        {
            if (obj == null || obj.gameObject == null) throw new System.ArgumentNullException(nameof(obj));

            var updater = obj.gameObject.AddOrGetComponent<SPNetworkFixedUpdater>();
            updater.Register(obj);
            return true;
        }

        public static bool RegisterNetworkLateUpdate(this INetworkLateUpdateable obj, NetworkStatus status)
        {
            if (obj == null || obj.gameObject == null) throw new System.ArgumentNullException(nameof(obj));

            if (GetNetworkStatusUnbiased(obj.gameObject).Intersects(status))
            {
                var updater = obj.gameObject.AddOrGetComponent<SPNetworkLateUpdater>();
                updater.Register(obj);
                return true;
            }

            return false;
        }

        public static bool RegisterNetworkLateUpdate(this INetworkLateUpdateable obj)
        {
            if (obj == null || obj.gameObject == null) throw new System.ArgumentNullException(nameof(obj));

            var updater = obj.gameObject.AddOrGetComponent<SPNetworkLateUpdater>();
            updater.Register(obj);
            return true;
        }




        public static bool UnregisterNetworkUpdate(this INetworkUpdateable obj)
        {
            if (obj == null || obj.gameObject == null) throw new System.ArgumentNullException(nameof(obj));

            if (obj.gameObject.TryGetComponent(out SPNetworkUpdater updater))
            {
                return updater.Unregister(obj);
            }

            return false;
        }

        public static bool UnregisterNetworkFixedUpdate(this INetworkFixedUpdateable obj)
        {
            if (obj == null || obj.gameObject == null) throw new System.ArgumentNullException(nameof(obj));

            if (obj.gameObject.TryGetComponent(out SPNetworkFixedUpdater updater))
            {
                return updater.Unregister(obj);
            }

            return false;
        }

        public static bool UnregisterNetworkLateUpdate(this INetworkLateUpdateable obj)
        {
            if (obj == null || obj.gameObject == null) throw new System.ArgumentNullException(nameof(obj));

            if (obj.gameObject.TryGetComponent(out SPNetworkLateUpdater updater))
            {
                return updater.Unregister(obj);
            }

            return false;
        }

    }


    internal sealed class SPNetworkUpdater : MonoBehaviour
    {

        #region Fields

        [System.NonSerialized]
        private LockingList<INetworkUpdateable> _lst = new();

        #endregion

        #region CONSTRUCTOR

        #endregion

        #region Methods

        public void Register(INetworkUpdateable obj)
        {
            if (obj == null) return;

            if (_lst.Locked)
            {
                if (!_lst.Contains(obj)) _lst.Add(obj);
            }
            else
            {
                if (!_lst.Contains(obj)) _lst.Add(obj);
                this.enabled = true;
            }
        }

        public bool Unregister(INetworkUpdateable obj)
        {
            if (obj == null) return false;

            if (_lst.Locked)
            {
                return _lst.Remove(obj);
            }
            else
            {
                bool result = _lst.Remove(obj);
                if (_lst.Count == 0) this.enabled = false;
                return result;
            }
        }

        private void Update()
        {
            if (_lst.Count == 0)
            {
                this.enabled = false;
                return;
            }

            try
            {
                _lst.Lock();

                foreach (var t in _lst)
                {
                    if (t.IsDestroyed())
                    {
                        _lst.Remove(t);
                    }
                    else
                    {
                        try
                        {
                            t.NetworkUpdate();
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogException(ex, t.component);
                        }
                    }
                }
            }
            finally
            {
                _lst.Unlock();
                if (_lst.Count == 0) this.enabled = false;
            }
        }

        #endregion

    }

    internal sealed class SPNetworkFixedUpdater : MonoBehaviour
    {

        #region Fields

        [System.NonSerialized]
        private LockingList<INetworkFixedUpdateable> _lst = new();

        #endregion

        #region CONSTRUCTOR

        #endregion

        #region Methods

        public void Register(INetworkFixedUpdateable obj)
        {
            if (obj == null) return;

            if (_lst.Locked)
            {
                if (!_lst.Contains(obj)) _lst.Add(obj);
            }
            else
            {
                if (!_lst.Contains(obj)) _lst.Add(obj);
                this.enabled = true;
            }
        }

        public bool Unregister(INetworkFixedUpdateable obj)
        {
            if (obj == null) return false;

            if (_lst.Locked)
            {
                return _lst.Remove(obj);
            }
            else
            {
                bool result = _lst.Remove(obj);
                if (_lst.Count == 0) this.enabled = false;
                return result;
            }
        }

        private void FixedUpdate()
        {
            if (_lst.Count == 0)
            {
                this.enabled = false;
                return;
            }

            try
            {
                _lst.Lock();

                foreach (var t in _lst)
                {
                    if (t.IsDestroyed())
                    {
                        _lst.Remove(t);
                    }
                    else
                    {
                        try
                        {
                            t.NetworkFixedUpdate();
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogException(ex, t.component);
                        }
                    }
                }
            }
            finally
            {
                _lst.Unlock();
                if (_lst.Count == 0) this.enabled = false;
            }
        }

        #endregion

    }

    internal sealed class SPNetworkLateUpdater : MonoBehaviour
    {

        #region Fields

        [System.NonSerialized]
        private LockingList<INetworkLateUpdateable> _lst = new();

        #endregion

        #region CONSTRUCTOR

        #endregion

        #region Methods

        public void Register(INetworkLateUpdateable obj)
        {
            if (obj == null) return;

            if (_lst.Locked)
            {
                if (!_lst.Contains(obj)) _lst.Add(obj);
            }
            else
            {
                if (!_lst.Contains(obj)) _lst.Add(obj);
                this.enabled = true;
            }
        }

        public bool Unregister(INetworkLateUpdateable obj)
        {
            if (obj == null) return false;

            if (_lst.Locked)
            {
                return _lst.Remove(obj);
            }
            else
            {
                bool result = _lst.Remove(obj);
                if (_lst.Count == 0) this.enabled = false;
                return result;
            }
        }

        private void LateUpdate()
        {
            if (_lst.Count == 0)
            {
                this.enabled = false;
                return;
            }

            try
            {
                _lst.Lock();

                foreach (var t in _lst)
                {
                    if (t.IsDestroyed())
                    {
                        _lst.Remove(t);
                    }
                    else
                    {
                        try
                        {
                            t.NetworkLateUpdate();
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogException(ex, t.component);
                        }
                    }
                }
            }
            finally
            {
                _lst.Unlock();
                if (_lst.Count == 0) this.enabled = false;
            }
        }

        #endregion

    }

}
