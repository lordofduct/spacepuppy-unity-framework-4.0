using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using com.spacepuppy;
using com.spacepuppy.Project;

public class zTest02 : MonoBehaviour
{

    public float Speed = 10f;
    public float Radius = 5f;
    public TextRef TextR;
    public VariantCollection Coll;

    // Update is called once per frame
    void Update()
    {
        this.transform.position = Quaternion.Euler(0f, Time.time * this.Speed, 0f) * Vector3.right * Radius;
    }
}
