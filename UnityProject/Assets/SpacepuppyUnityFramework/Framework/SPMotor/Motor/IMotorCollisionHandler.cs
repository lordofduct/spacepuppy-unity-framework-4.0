using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.Motor
{

    public interface IMotorCollisionHandler : IMSignalEnabledUpwards<IMotorCollisionHandler>
    {
        void OnCollision(MotorCollisionInfo info);
    }

    public static class MotorCollisionHandlerHelper
    {
        public static readonly System.Action<IMotorCollisionHandler, MotorCollisionInfo> OnCollisionFunctor = (c, d) => c.OnCollision(d);
    }

    public enum MotorCollisionType
    {
        Undefined = 0,
        CharacterController = 1,
        Rigidbody = 2,
    }

    public struct MotorCollisionInfo
    {

        public IMotor Motor { get; set; }
        public Collider Collider { get; set; }
        public MotorCollisionType MotorCollisionType { get; set; }
        public Collision Collision { get; set; }
        public ControllerColliderHit ControllerColliderHit { get; set; }

        public MotorCollisionInfo(IMotor motor, Collision data)
        {
            this.Motor = motor;
            this.Collider = data.collider;
            this.MotorCollisionType = MotorCollisionType.Rigidbody;
            this.Collision = data;
            this.ControllerColliderHit = null;
        }

        public MotorCollisionInfo(IMotor motor, ControllerColliderHit data)
        {
            this.Motor = motor;
            this.Collider = data.collider;
            this.MotorCollisionType = MotorCollisionType.Rigidbody;
            this.Collision = null;
            this.ControllerColliderHit = data;
        }

    }

}
