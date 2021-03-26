using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;

public class DebugStartup : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (!GameLoop.Initialized) GameLoop.Init();
    }

}
