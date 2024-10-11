using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.SPInput
{

    public struct CursorRaycastHit
    {

        private GameObject _gameObject;
        private Component _collider;
        public Vector3 point;
        public Vector3 normal;
        public float distance;

        public CursorRaycastHit(GameObject go, Vector3 point, Vector3 normal, float distance)
        {
            _gameObject = go;
            _collider = null;
            this.point = point;
            this.normal = normal;
            this.distance = distance;
        }

        public CursorRaycastHit(Collider c, Vector3 point, Vector3 normal, float distance)
        {
            _gameObject = c ? c.gameObject : null;
            _collider = c;
            this.point = point;
            this.normal = normal;
            this.distance = distance;
        }

        public CursorRaycastHit(Collider2D c, Vector2 point, Vector2 normal, float distance)
        {
            _gameObject = c ? c.gameObject : null;
            _collider = c;
            this.point = point;
            this.normal = normal;
            this.distance = distance;
        }

        public GameObject gameObject => _gameObject;
        public Collider collider => _collider as Collider;
        public Collider2D collider2D => _collider as Collider2D;
        public Transform transform => _gameObject?.transform;

        public static implicit operator bool(CursorRaycastHit hit)
        {
            return (bool)hit._gameObject;
        }

        public static explicit operator CursorRaycastHit(RaycastHit hit)
        {
            return new CursorRaycastHit()
            {
                _gameObject = hit.collider ? hit.collider.gameObject : null,
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
                _gameObject = hit.collider ? hit.collider.gameObject : null,
                _collider = hit.collider,
                point = hit.point,
                normal = hit.normal,
                distance = hit.distance
            };
        }

    }

}
