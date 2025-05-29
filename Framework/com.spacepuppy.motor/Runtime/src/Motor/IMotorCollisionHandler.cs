using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Motor
{

    [AutoInitMixin]
    public interface IMotorCollisionMessageHandler : IMixin, IEventfulComponent
    {
        sealed void OnInitMixin()
        {
            this.OnEnabled += (s,e) =>
            {
                this.gameObject.FindRoot().Broadcast<IMotor>(o => o.SetCollisionMessageDirty(true));
            };
            this.OnDisabled += (s, e) =>
            {
                this.gameObject.FindRoot().Broadcast<IMotor>(o => o.SetCollisionMessageDirty(true));
            };
        }

        void OnCollision(MotorCollisionInfo info);
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
