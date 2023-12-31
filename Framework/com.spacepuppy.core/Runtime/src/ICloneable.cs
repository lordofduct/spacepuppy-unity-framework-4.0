using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.spacepuppy
{
    public interface ICloneable<T> : System.ICloneable where T : ICloneable<T>
    {

        object System.ICloneable.Clone() => ((ICloneable<T>)this).Clone();
        new T Clone();

    }
}
