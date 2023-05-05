using UnityEngine;

using com.spacepuppy.Events;
using com.spacepuppy.Geom;

namespace com.spacepuppy.SPInput.Events
{

    [Infobox("WARNING - This script is intended for prototyping only. It hooks into the old Unity 'OnMouseDown' and 'OnMouseUpAsButton' events. These are very limited in functionality and should only be used for prototyping. Prefer a more robust input system for release quality functionality.", MessageType = InfoBoxMessageType.Warning)]
    public sealed class pt_OnMouseClick : SPComponent, IObservableTrigger
    {

        #region Fields

        [SerializeField()]
        [Tooltip("A duration of time that the click must be held down to register as a click.")]
        private Interval _buttonLapse = Interval.MinMax(float.NegativeInfinity, float.PositiveInfinity);

        [SerializeField()]
        [UnityEngine.Serialization.FormerlySerializedAs("_trigger")]
        private SPEvent _onMouseClick = new SPEvent("OnMouseClick");

        [System.NonSerialized()]
        private float _downT = float.NaN;

        #endregion

        #region Properties

        public SPEvent OnMouseClick => _onMouseClick;

        #endregion

        #region Methods

        void OnMouseDown()
        {
            _downT = Time.unscaledTime;
        }

        void OnMouseUpAsButton()
        {
            if (float.IsNaN(_downT)) return;

            if (_buttonLapse.Intersects(Time.unscaledTime - _downT))
            {
                _onMouseClick.ActivateTrigger(this, null);
            }
            _downT = float.NaN;
        }

        #endregion

        #region IObservableTrigger Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onMouseClick };
        }

        #endregion

    }

}
