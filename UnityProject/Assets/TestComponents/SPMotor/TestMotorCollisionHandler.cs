using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Motor;

public class TestMotorCollisionHandler : SPComponent, IMotorCollisionMessageHandler
{

    protected override void Awake()
    {
        base.Awake();
    }

    void IMotorCollisionMessageHandler.OnCollision(MotorCollisionInfo info)
    {
        Debug.Log("Motor Was Hit");
    }
}
