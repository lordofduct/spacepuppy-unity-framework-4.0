using com.spacepuppy.Collections;
using UnityEngine;

namespace com.spacepuppy.Motor
{

    public interface IForceReceiver : IGameObjectSource
    {

        void Move(Vector3 mv);
        void AddForce(Vector3 force, ForceMode mode);
        void AddForceAtPosition(Vector3 force, Vector3 position, ForceMode mode);
        void AddExplosionForce(float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModifier = 0f, ForceMode mode = ForceMode.Force);

    }

    public class RigidbodyForceReceiver : MonoBehaviour, IForceReceiver
    {

        private Rigidbody _body;

        public void Move(Vector3 mv)
        {
            _body.MovePosition(_body.position + mv);
        }

        public void AddForce(Vector3 force, ForceMode mode)
        {
            _body.AddForce(force, mode);
        }

        public void AddForceAtPosition(Vector3 force, Vector3 position, ForceMode mode)
        {
            _body.AddForceAtPosition(force, position, mode);
        }

        public void AddExplosionForce(float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModifier, ForceMode mode)
        {
            _body.AddExplosionForce(explosionForce, explosionPosition, explosionRadius, upwardsModifier, mode);
        }


        public static RigidbodyForceReceiver Get(Rigidbody rb)
        {
            if (rb == null) throw new System.ArgumentNullException(nameof(rb));

            using (var lst = TempCollection.GetList<RigidbodyForceReceiver>())
            {
                rb.GetComponents<RigidbodyForceReceiver>(lst);
                foreach(var rf in lst)
                {
                    if (rf && rf._body == rb) return rf;
                }
            }

            var fr = rb.gameObject.AddComponent<RigidbodyForceReceiver>();
            fr._body = rb;
            return fr;
        }

    }

}
