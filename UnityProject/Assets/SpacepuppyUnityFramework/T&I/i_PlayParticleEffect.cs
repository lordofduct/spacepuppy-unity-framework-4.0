#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using com.spacepuppy.Utils;
using com.spacepuppy.Events;

namespace com.spacepuppy.Events
{

    public class i_PlayParticleEffect : Triggerable, IObservableTrigger
    {

        public const string TRG_ONFINISH = "OnSpawned";

        #region Fields

        [SerializeField()]
        private ParticleSystem _effectPrefab;

        [SerializeField()]
        [TimeUnitsSelector()]
        [Tooltip("Delete particle effect after a duration. Leave 0 to use the 'duration' of the particle effect.")]
        private SPTimePeriod _duration;

        [SerializeField()]
        private bool _spawnAsChild = true;

        [SerializeField()]
        private SPEvent _onSpawnedObject = new SPEvent(TRG_ONFINISH);

        #endregion

        #region CONSTRUCTOR

        #endregion

        #region Properties

        public ParticleSystem EffectsPrefab
        {
            get { return _effectPrefab; }
            set { _effectPrefab = value; }
        }

        public SPTimePeriod Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        public bool SpawnAsChild
        {
            get { return _spawnAsChild; }
            set { _spawnAsChild = value; }
        }

        #endregion

        #region ISpawner Interface

        public GameObject Spawn()
        {
            if (!this.enabled) return null;
            if (_effectPrefab == null) return null;

            var go = Instantiate(_effectPrefab.gameObject, this.transform.position, this.transform.rotation, (_spawnAsChild) ? this.transform : null);
            if (go == null) return null;

            //TODO - InvokeGuaranteed
            //if(_duration.Seconds > 0.00001f)
            //{
            //    GameLoop.InvokeGuaranteed(() =>
            //    {
            //        Destroy(go);
            //    }, _duration.Seconds, _duration.TimeSupplier);
            //}
            //else
            //{
            //    var dur = _effectPrefab.main.duration;
            //    if (!float.IsNaN(dur) && !float.IsInfinity(dur))
            //    {
            //        GameLoop.InvokeGuaranteed(() =>
            //        {
            //            Destroy(go);
            //        }, dur, _effectPrefab.main.useUnscaledTime ? SPTime.Real : SPTime.Normal);
            //    }
            //}

            

            //if (_onSpawnedObject != null && _onSpawnedObject.HasReceivers)
            //    _onSpawnedObject.ActivateTrigger(this, go);

            return go;
        }

        #endregion

        #region ITriggerableMechanism Interface

        public override bool CanTrigger
        {
            get { return base.CanTrigger && _effectPrefab != null; }
        }

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            return this.Spawn() != null;
        }

        #endregion

        #region IObserverableTarget Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onSpawnedObject };
        }

        #endregion

    }

}