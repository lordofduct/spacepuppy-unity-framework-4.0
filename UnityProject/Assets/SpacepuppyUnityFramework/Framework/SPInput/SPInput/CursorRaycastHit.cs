using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.SPInput
{

    public struct CursorRaycastHit
    {

        private Component _collider;
        public Vector3 point;
        public Vector3 normal;
        public float distance;

        public CursorRaycastHit(Collider c, Vector3 point, Vector3 normal, float distance)
        {
            _collider = c;
            this.point = point;
            this.normal = normal;
            this.distance = distance;
        }

        public CursorRaycastHit(Collider2D c, Vector2 point, Vector2 normal, float distance)
        {
            _collider = c;
            this.point = point;
            this.normal = normal;
            this.distance = distance;
        }

        public GameObject gameObject => _collider?.gameObject;
        public Collider collider => _collider as Collider;
        public Collider2D collider2D => _collider as Collider2D;
        public Transform transform => _collider?.transform;

        public static implicit operator bool(CursorRaycastHit hit)
        {
            return (bool)hit._collider;
        }

        public static explicit operator CursorRaycastHit(RaycastHit hit)
        {
            return new CursorRaycastHit()
            {
                _collider = hit.collider,
                point = hit.point,
                normal = hit.normal,
                distance = hit.distance
            };
        }

        public static explicit operator CursorRaycastHit(RaycastHit2D hit)
        {
            return new CursorRaycastHit()
            {
                _collider = hit.collider,
                point = hit.point,
                normal = hit.normal,
                distance = hit.distance
            };
        }

    }

}
