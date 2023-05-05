using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Spawn.Events
{

    public class t_OnSpawned : SPComponent, IObservableTrigger, IOnSpawnHandler
    {

        #region Fields

        [SerializeField()]
        [UnityEngine.Serialization.FormerlySerializedAs("_trigger")]
        [SPEvent.Config("controller (SpawnedObjectController)")]
        private SPEvent _onSpawned = new SPEvent("OnSpawned");

        #endregion

        #region Properties

        public SPEvent OnSpawned => _onSpawned;

        #endregion

        #region IOnSpawnHandler Interface

        void IOnSpawnHandler.OnSpawn(SpawnedObjectController cntrl)
        {
            _onSpawned.ActivateTrigger(this, cntrl);
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onSpawned };
        }

        #endregion

    }

}
