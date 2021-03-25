using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using com.spacepuppy;
using com.spacepuppy.Collections;
using com.spacepuppy.Utils;
using com.spacepuppy.Tween;

public class zTest01 : SPComponent
{


    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Space)) return;

        Debug.Log("TWEENING zTest01");
        SPTween.Tween(this)
               .FromTo("*Move", EaseMethods.ExpoEaseIn, 1f, this.transform.position, this.transform.position + Vector3.right * 5f)
               .Play();
    }

    public decimal PropX
    {
        get { return (decimal)this.transform.position.x; }
        set { this.transform.position = this.transform.position.SetX((float)value); }
    }

}
