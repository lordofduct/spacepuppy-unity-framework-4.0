using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.Geom
{
    public interface IGeom
    {

        void Move(Vector3 mv);
        void RotateAroundPoint(Vector3 point, Quaternion rot);

        AxisInterval Project(Vector3 axis);
        Bounds GetBounds();
        Sphere GetBoundingSphere();
        IEnumerable<Vector3> GetAxes();

        bool Contains(Vector3 pos);

    }

    public interface IPhysicsObject
    {

        bool TestOverlap(int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal);
        int OverlapNonAlloc(Collider[] buffer, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal);
        int Overlap(ICollection<Collider> results, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
        {
            if (results is Collider[] arr)
            {
                return this.OverlapNonAlloc(arr, layerMask, query);
            }
            else
            {
                var buffer = PhysicsUtil.GetNonAllocColliderBuffer();
                try
                {
                    int cnt = this.OverlapNonAlloc(buffer, layerMask, query);
                    for (int i = 0; i < cnt; i++)
                    {
                        results.Add(buffer[i]);
                    }
                    return cnt;
                }
                finally
                {
                    PhysicsUtil.ReleaseNonAllocColliderBuffer(buffer);
                }
            }
        }
        bool Cast(Vector3 direction, out RaycastHit hitinfo, float distance, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal);
        int CastAll(Vector3 direction, ICollection<RaycastHit> results, float distance, int layerMask, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal);

    }

    public interface IPhysicsGeom : IGeom, IPhysicsObject
    {
        
    }

}
