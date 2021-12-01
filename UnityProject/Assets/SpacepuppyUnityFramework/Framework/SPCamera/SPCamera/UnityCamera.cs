#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Cameras
{

    [RequireComponentInEntity(typeof(Camera))]
    [DisallowMultipleComponent()]
    public class UnityCamera : SPComponent, ICamera
    {

        #region Fields

        [SerializeField]
        private CameraCategory _type;

        [SerializeField]
        [DefaultFromSelf(Relativity = EntityRelativity.Entity)]
        private Camera _camera;

        #endregion

        #region CONSTRUCTOR

        public UnityCamera()
        {
            _nameCache = new NameCache.UnityObjectNameCache(this);
        }

        protected override void Awake()
        {
            base.Awake();

            if (_camera != null)
            {
                CameraPool.Register(this);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            CameraPool.Unregister(this);
        }

        #endregion

        #region ICamera Interface

        public CameraCategory Category
        {
            get { return _type; }
            set { _type = value; }
        }

        public new Camera camera
        {
            get { return _camera; }
        }
        Camera ICamera.camera
        {
            get { return _camera; }
        }

        public bool IsAlive { get { return _camera != null; } }

        public bool Contains(Camera cam)
        {
            return object.ReferenceEquals(_camera, cam);
        }

        #endregion

        #region INameable Interface

        private NameCache.UnityObjectNameCache _nameCache;
        public new string name
        {
            get { return _nameCache.Name; }
            set { _nameCache.Name = value; }
        }
        string INameable.Name
        {
            get { return _nameCache.Name; }
            set { _nameCache.Name = value; }
        }
        public bool CompareName(string nm)
        {
            return _nameCache.CompareName(nm);
        }
        void INameable.SetDirty()
        {
            _nameCache.SetDirty();
        }

        #endregion

    }

}
