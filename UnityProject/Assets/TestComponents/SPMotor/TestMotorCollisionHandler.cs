using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Motor;

public class TestMotorCollisionHandler : SPComponent, IMotorCollisionHandler
{

    protected override void Awake()
    {
        base.Awake();
    }

    void IMotorCollisionHandler.OnCollision(MotorCollisionInfo info)
    {
        Debug.Log("Motor Was Hit");
    }
}
