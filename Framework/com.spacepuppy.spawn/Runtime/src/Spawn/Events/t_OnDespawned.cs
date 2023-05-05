using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Spawn.Events
{

    public class t_OnDespawned : SPComponent, IObservableTrigger, IOnDespawnHandler
    {

        #region Fields

        [SerializeField()]
        [UnityEngine.Serialization.FormerlySerializedAs("_trigger")]
        [SPEvent.Config("controller (SpawnedObjectController)")]
        private SPEvent _onDespawned = new SPEvent("OnDespawned");

        #endregion

        #region Properties

        public SPEvent OnDespawned => _onDespawned;

        #endregion

        #region IOnDespawnHandler Interface

        void IOnDespawnHandler.OnDespawn(SpawnedObjectController cntrl)
        {
            _onDespawned.ActivateTrigger(this, cntrl);
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onDespawned };
        }

        #endregion

    }

}
