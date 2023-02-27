using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.Spawn
{

    public interface ISpawnable
    {

        bool Spawn(out GameObject instance, ISpawnPool pool, Vector3 position, Quaternion rotation, Transform parent);

    }

}
