using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.spacepuppy.Spawn
{

    /// <summary>
    /// This only occurs if the object was pooled and is being returned to the pool rather than destroyed. Use OnDestroyed in conjunction to handle destroyed.
    /// </summary>
    public interface IOnDespawnHandler
    {

        void OnDespawn(SpawnedObjectController cntrl);

    }

}
