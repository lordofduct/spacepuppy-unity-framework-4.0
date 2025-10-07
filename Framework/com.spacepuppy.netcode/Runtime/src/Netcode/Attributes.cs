using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.Netcode
{

    /// <summary>
    /// Currently only supports simple networkvariables with simple value (think: float, string, int, etc)
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class SerializedNetworkVariableAttribute : SPPropertyAttribute
    {

    }

}
