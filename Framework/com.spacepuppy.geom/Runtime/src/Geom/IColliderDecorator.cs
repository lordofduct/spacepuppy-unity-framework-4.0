using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Collections;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Geom
{

    public interface IColliderDecorator : IComponent, IPhysicsObject
    {
        Collider Collider { get; }

        IPhysicsGeom GetGeom();
    }

    public static class ColliderDecorator
    {

        #region Static Methods

        public static IColliderDecorator AsColliderDecorator(this Collider c)
        {
            return GetDecorator(c);
        }

        public static IColliderDecorator GetDecorator(Collider c)
        {
            if (c == null) return null;

            using (var lst = TempCollection.GetList<IColliderDecorator>())
            {
                c.GetComponents<IColliderDecorator>(lst);
                if (lst.Count > 0)
                {
                    for (int i = 0; i < lst.Count; i++)
                    {
                        if (lst[i].Collider == c) return lst[i];
                    }
                }
            }

            if (c is CharacterController)
            {
                return CharacterControllerDecorator.Configure(c as CharacterController);
            }
            if (c is CapsuleCollider)
            {
                return CapsuleColliderDecorator.Configure(c as CapsuleCollider);
            }
            else if (c is BoxCollider)
            {
                return BoxColliderDecorator.Configure(c as BoxCollider);
            }
            else if (c is SphereCollider)
            {
                return SphereColliderDecorator.Configure(c as SphereCollider);
            }
            else if (c is MeshCollider)
            {
                return MeshColliderDecorator.Configure(c as MeshCollider);
            }
            else
            {
                return BoundsBasedColliderDecorator.Configure(c);
            }
        }

        public static bool ContainsPoint(this Collider c, Vector3 point)
        {
            if (c == null) return false;

            return VectorUtil.FuzzyEquals(c.ClosestPoint(point), point);
        }

        #endregion


        private sealed class CharacterControllerDecorator : MonoBehaviour, IColliderDecorator
        {

            #region Fields

            [SerializeField]
            private CharacterController _collider;

            #endregion

            #region Properties

            public Collider Collider
            {
                get => _collider;
            }

            Component IComponent.component => this;

            #endregion

            #region Methods

            public IPhysicsGeom GetGeom()
            {
                return Capsule.FromCollider(_collider);
            }

            public bool Cast(Vector3 direction, out RaycastHit hitinfo, float distance, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                return Capsule.FromCollider(_collider).Cast(direction, out hitinfo, distance, layerMask, query);
            }

            public int CastAll(Vector3 direction, ICollection<RaycastHit> results, float distance, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                return Capsule.FromCollider(_collider).CastAll(direction, results, distance, layerMask, query);
            }

            public int OverlapNonAlloc(Collider[] buffer, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                return Capsule.FromCollider(_collider).OverlapNonAlloc(buffer, layerMask, query);
            }

            public int Overlap(ICollection<Collider> results, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                return Capsule.FromCollider(_collider).Overlap(results, layerMask, query);
            }

            public bool TestOverlap(int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                return Capsule.FromCollider(_collider).TestOverlap(layerMask, query);
            }

            public bool ContainsPoint(Vector3 point)
            {
                return _collider.ContainsPoint(point);
            }

            #endregion

            internal static IColliderDecorator Configure(CharacterController c)
            {
                var d = c.AddComponent<CharacterControllerDecorator>();
                d._collider = c;
                return d;
            }

        }

        private sealed class BoxColliderDecorator : MonoBehaviour, IColliderDecorator
        {

            #region Fields

            [SerializeField]
            private BoxCollider _collider;

            #endregion

            #region Properties

            public Collider Collider
            {
                get => _collider;
            }

            Component IComponent.component => this;

            #endregion

            #region Methods

            public IPhysicsGeom GetGeom()
            {
                return Box.FromCollider(_collider);
            }

            public bool Cast(Vector3 direction, out RaycastHit hitinfo, float distance, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                return Box.FromCollider(_collider).Cast(direction, out hitinfo, distance, layerMask, query);
            }

            public int CastAll(Vector3 direction, ICollection<RaycastHit> results, float distance, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                return Box.FromCollider(_collider).CastAll(direction, results, distance, layerMask, query);
            }

            public int OverlapNonAlloc(Collider[] buffer, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                return Box.FromCollider(_collider).OverlapNonAlloc(buffer, layerMask, query);
            }

            public int Overlap(ICollection<Collider> results, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                return Box.FromCollider(_collider).Overlap(results, layerMask, query);
            }

            public bool TestOverlap(int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                return Box.FromCollider(_collider).TestOverlap(layerMask, query);
            }

            public bool ContainsPoint(Vector3 point)
            {
                return _collider.ContainsPoint(point);
            }

            #endregion

            internal static IColliderDecorator Configure(BoxCollider c)
            {
                var d = c.AddComponent<BoxColliderDecorator>();
                d._collider = c;
                return d;
            }

        }

        private sealed class SphereColliderDecorator : MonoBehaviour, IColliderDecorator
        {

            #region Fields

            [SerializeField]
            private SphereCollider _collider;

            #endregion

            #region Properties

            public Collider Collider
            {
                get => _collider;
            }

            Component IComponent.component => this;

            #endregion

            #region Methods

            public IPhysicsGeom GetGeom()
            {
                return Sphere.FromCollider(_collider);
            }

            public bool Cast(Vector3 direction, out RaycastHit hitinfo, float distance, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                return Sphere.FromCollider(_collider).Cast(direction, out hitinfo, distance, layerMask, query);
            }

            public int CastAll(Vector3 direction, ICollection<RaycastHit> results, float distance, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                return Sphere.FromCollider(_collider).CastAll(direction, results, distance, layerMask, query);
            }

            public int OverlapNonAlloc(Collider[] buffer, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                return Sphere.FromCollider(_collider).OverlapNonAlloc(buffer, layerMask, query);
            }

            public int Overlap(ICollection<Collider> results, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                return Sphere.FromCollider(_collider).Overlap(results, layerMask, query);
            }

            public bool TestOverlap(int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                return Sphere.FromCollider(_collider).TestOverlap(layerMask, query);
            }

            public bool ContainsPoint(Vector3 point)
            {
                return _collider.ContainsPoint(point);
            }

            #endregion

            internal static IColliderDecorator Configure(SphereCollider c)
            {
                var d = c.AddComponent<SphereColliderDecorator>();
                d._collider = c;
                return d;
            }

        }

        private sealed class CapsuleColliderDecorator : MonoBehaviour, IColliderDecorator
        {

            #region Fields

            [SerializeField]
            private CapsuleCollider _collider;

            #endregion

            #region Properties

            public Collider Collider
            {
                get => _collider;
            }

            Component IComponent.component => this;

            #endregion

            #region Methods

            public IPhysicsGeom GetGeom()
            {
                return Capsule.FromCollider(_collider);
            }

            public bool Cast(Vector3 direction, out RaycastHit hitinfo, float distance, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                return Capsule.FromCollider(_collider).Cast(direction, out hitinfo, distance, layerMask, query);
            }

            public int CastAll(Vector3 direction, ICollection<RaycastHit> results, float distance, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                return Capsule.FromCollider(_collider).CastAll(direction, results, distance, layerMask, query);
            }

            public int OverlapNonAlloc(Collider[] buffer, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                return Capsule.FromCollider(_collider).OverlapNonAlloc(buffer, layerMask, query);
            }

            public int Overlap(ICollection<Collider> results, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                return Capsule.FromCollider(_collider).Overlap(results, layerMask, query);
            }

            public bool TestOverlap(int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                return Capsule.FromCollider(_collider).TestOverlap(layerMask, query);
            }

            public bool ContainsPoint(Vector3 point)
            {
                return _collider.ContainsPoint(point);
            }

            #endregion

            internal static IColliderDecorator Configure(CapsuleCollider c)
            {
                var d = c.AddComponent<CapsuleColliderDecorator>();
                d._collider = c;
                return d;
            }

        }

        private sealed class MeshColliderDecorator : MonoBehaviour, IColliderDecorator
        {

            #region Fields

            [SerializeField]
            private MeshCollider _collider;

            #endregion

            #region Properties

            public Collider Collider
            {
                get => _collider;
            }

            Component IComponent.component => this;

            #endregion

            #region Methods

            public IPhysicsGeom GetGeom()
            {
                if (GeomUtil.DefaultBoundingSphereAlgorithm == BoundingSphereAlgorithm.FromBounds)
                {
                    return AABBox.FromCollider(_collider);
                }
                else
                {
                    return Sphere.FromMesh(_collider.sharedMesh, GeomUtil.DefaultBoundingSphereAlgorithm, Trans.GetGlobal(this.transform));
                }
            }

            public bool Cast(Vector3 direction, out RaycastHit hitinfo, float distance, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                if(GeomUtil.DefaultBoundingSphereAlgorithm == BoundingSphereAlgorithm.FromBounds)
                {
                    return AABBox.FromCollider(_collider).Cast(direction, out hitinfo, distance, layerMask, query);
                }
                else
                {
                    return Sphere.FromMesh(_collider.sharedMesh, GeomUtil.DefaultBoundingSphereAlgorithm, Trans.GetGlobal(this.transform)).Cast(direction, out hitinfo, distance, layerMask, query);
                }
            }

            public int CastAll(Vector3 direction, ICollection<RaycastHit> results, float distance, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                if (GeomUtil.DefaultBoundingSphereAlgorithm == BoundingSphereAlgorithm.FromBounds)
                {
                    return AABBox.FromCollider(_collider).CastAll(direction, results, distance, layerMask, query);
                }
                else
                {
                    return Sphere.FromMesh(_collider.sharedMesh, GeomUtil.DefaultBoundingSphereAlgorithm, Trans.GetGlobal(this.transform)).CastAll(direction, results, distance, layerMask, query);
                }
            }

            public int OverlapNonAlloc(Collider[] buffer, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                if (GeomUtil.DefaultBoundingSphereAlgorithm == BoundingSphereAlgorithm.FromBounds)
                {
                    return AABBox.FromCollider(_collider).OverlapNonAlloc(buffer, layerMask, query);
                }
                else
                {
                    return Sphere.FromMesh(_collider.sharedMesh, GeomUtil.DefaultBoundingSphereAlgorithm, Trans.GetGlobal(this.transform)).OverlapNonAlloc(buffer, layerMask, query);
                }
            }

            public int Overlap(ICollection<Collider> results, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                if (GeomUtil.DefaultBoundingSphereAlgorithm == BoundingSphereAlgorithm.FromBounds)
                {
                    return AABBox.FromCollider(_collider).Overlap(results, layerMask, query);
                }
                else
                {
                    return Sphere.FromMesh(_collider.sharedMesh, GeomUtil.DefaultBoundingSphereAlgorithm, Trans.GetGlobal(this.transform)).Overlap(results, layerMask, query);
                }
            }

            public bool TestOverlap(int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                if (GeomUtil.DefaultBoundingSphereAlgorithm == BoundingSphereAlgorithm.FromBounds)
                {
                    return AABBox.FromCollider(_collider).TestOverlap(layerMask, query);
                }
                else
                {
                    return Sphere.FromMesh(_collider.sharedMesh, GeomUtil.DefaultBoundingSphereAlgorithm, Trans.GetGlobal(this.transform)).TestOverlap(layerMask, query);
                }
            }

            public bool ContainsPoint(Vector3 point)
            {
                return _collider.ContainsPoint(point);
            }

            #endregion

            internal static IColliderDecorator Configure(MeshCollider c)
            {
                var d = c.AddComponent<MeshColliderDecorator>();
                d._collider = c;
                return d;
            }

        }

        private sealed class BoundsBasedColliderDecorator : MonoBehaviour, IColliderDecorator
        {

            #region Fields

            [SerializeField]
            private Collider _collider;

            #endregion

            #region Properties

            public Collider Collider
            {
                get => _collider;
            }

            Component IComponent.component => this;

            #endregion

            #region Methods

            public IPhysicsGeom GetGeom()
            {
                return AABBox.FromCollider(_collider);
            }

            public bool Cast(Vector3 direction, out RaycastHit hitinfo, float distance, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                return AABBox.FromCollider(_collider).Cast(direction, out hitinfo, distance, layerMask, query);
            }

            public int CastAll(Vector3 direction, ICollection<RaycastHit> results, float distance, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                return AABBox.FromCollider(_collider).CastAll(direction, results, distance, layerMask, query);
            }

            public int OverlapNonAlloc(Collider[] buffer, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                return AABBox.FromCollider(_collider).OverlapNonAlloc(buffer, layerMask, query);
            }

            public int Overlap(ICollection<Collider> results, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                return AABBox.FromCollider(_collider).Overlap(results, layerMask, query);
            }

            public bool TestOverlap(int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
            {
                return AABBox.FromCollider(_collider).TestOverlap(layerMask, query);
            }

            public bool ContainsPoint(Vector3 point)
            {
                return AABBox.FromCollider(_collider).Contains(point);
            }

            #endregion

            internal static IColliderDecorator Configure(Collider c)
            {
                var d = c.AddComponent<BoundsBasedColliderDecorator>();
                d._collider = c;
                return d;
            }

        }

    }

}
