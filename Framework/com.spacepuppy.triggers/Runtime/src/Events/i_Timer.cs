using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.Events
{

    public class i_Timer : AutoTriggerable, IUpdateable, IObservableTrigger
    {

        public enum Action
        {
            Restart = 0,
            EndImmediately = 1,
            Pause = 2,
            Resume = 3,
            TogglePause = 4
        }

        #region Fields

        [SerializeField]
        private SPTimePeriod _duration;

        [SerializeField]
        private Action _onTriggeredAction = Action.Restart;

        [SerializeField]
        private SPEvent _onComplete;

        [System.NonSerialized]
        private float _tally = float.NaN;

        #endregion

        #region Properties

        public SPTimePeriod Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        public Action OnTriggeredAction
        {
            get { return _onTriggeredAction; }
            set { _onTriggeredAction = value; }
        }

        public SPEvent OnComplete
        {
            get { return _onComplete; }
        }

        public bool IsRunning
        {
            get
            {
                return this.isActiveAndEnabled && !float.IsNaN(_tally);
            }
        }

        public bool IsPaused
        {
            get
            {
                return this.isActiveAndEnabled && !float.IsNaN(_tally) && !GameLoop.UpdatePump.Contains(this);
            }
        }

        [ShowNonSerializedProperty("Current Time")]
        public float CurrentTime
        {
            get { return _tally; }
        }

        [ShowNonSerializedProperty("Remaining Time")]
        public float RemainingTime => Mathf.Max(0f, _duration.Seconds - _tally);

        #endregion

        #region Methods

        protected override void OnDisable()
        {
            base.OnDisable();

            this.Stop(false);
        }

        void IUpdateable.Update()
        {
            if (float.IsNaN(_tally))
            {
                this.Stop(false);
                return;
            }

            _tally += _duration.TimeSupplier.Delta;
            if (_tally >= _duration.Seconds)
            {
                this.Stop(true);
            }
        }

        public void Restart()
        {
            _tally = 0f;
            GameLoop.UpdatePump.Add(this);
        }

        public void Resume()
        {
            if (!double.IsNaN(_tally)) GameLoop.UpdatePump.Add(this);
        }

        public void Pause()
        {
            GameLoop.UpdatePump.Remove(this);
        }

        public void TogglePause()
        {
            if (this.IsPaused)
                this.Resume();
            else
                this.Pause();
        }

        public void Stop(bool fireEvent)
        {
            _tally = float.NaN;
            GameLoop.UpdatePump.Remove(this);
            if (fireEvent)
            {
                _onComplete.ActivateTrigger(this, null);
            }
        }

        #endregion

        #region ITriggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            switch (_onTriggeredAction)
            {
                case Action.Restart:
                    this.Restart();
                    return true;
                case Action.EndImmediately:
                    this.Stop(this.IsRunning);
                    return true;
                case Action.Pause:
                    this.Pause();
                    return true;
                case Action.Resume:
                    this.Resume();
                    return true;
                case Action.TogglePause:
                    this.TogglePause();
                    return true;
                default:
                    return false;
            }
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onComplete };
        }

        #endregion

    }

}
