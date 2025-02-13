using UnityEngine;
using System.Collections.Generic;
using com.spacepuppy.Utils;
using com.spacepuppy.Events;

namespace com.spacepuppy
{

    public interface IStateMachine
    {

        event System.EventHandler StateChanged;
        public EventHandlerRef StateChanged_ref() => EventHandlerRef.Create(this, (o, l) => o.StateChanged += l, (o, l) => o.StateChanged -= l);

        int StateCount { get; }
        string CurrentStateId { get; }
        int? CurrentStateIndex { get; }


        void GoToStateById(string id);
        void GoToState(int index);
        void GoToNextState(WrapMode mode = WrapMode.Loop)
        {
            switch (mode)
            {
                case WrapMode.Loop:
                    this.GoToState(MathUtil.Wrap((this.CurrentStateIndex ?? -1) + 1, this.StateCount));
                    break;
                case WrapMode.Clamp:
                default:
                    this.GoToState(Mathf.Clamp((this.CurrentStateIndex ?? -1) + 1, 0, this.StateCount - 1));
                    break;
            }
        }

        void GoToPreviousState(WrapMode mode = WrapMode.Loop)
        {

            switch (mode)
            {
                case WrapMode.Loop:
                    this.GoToState(MathUtil.Wrap((this.CurrentStateIndex ?? 1) - 1, this.StateCount));
                    break;
                case WrapMode.Clamp:
                default:
                    this.GoToState(Mathf.Clamp((this.CurrentStateIndex ?? this.StateCount) - 1, 0, this.StateCount - 1));
                    break;
            }
        }

    }

}
