using UnityEngine;

namespace com.spacepuppy.Events
{

    public class i_AutoSequenceSignal : AutoTriggerable, IAutoSequenceSignal
    {

        private RadicalWaitHandle _handle;


        public IRadicalWaitHandle Wait()
        {
            if (_handle == null) _handle = RadicalWaitHandle.Create();
            return _handle;
        }


        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            if (_handle != null)
            {
                var h = _handle;
                _handle = null;
                h.SignalComplete();
            }

            return true;
        }

    }

}
