using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using com.spacepuppy;
using com.spacepuppy.Collections;
using com.spacepuppy.Utils;
using com.spacepuppy.Tween;

public class zTest01 : SPComponent
{

    private RadicalCoroutine _routine;

    protected override void Start()
    {
        base.Start();

        _routine = this.StartRadicalCoroutine(this.DoStuff());
    }

    protected override void OnDisable()
    {
        Debug.Log("DISABLE");
        base.OnDisable();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            this.StopAllCoroutines();
        }
        //else
        //{
        //    Debug.Log(_routine.OperatingState);
        //}
    }

    private IEnumerator DoStuff()
    {
        while(true)
        {
            Debug.Log(Time.frameCount);
            yield return null;
        }
    }

}
