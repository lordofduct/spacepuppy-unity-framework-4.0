using UnityEngine;
using System.Collections.Generic;
using System;

namespace com.spacepuppy.Events
{

    [AutoInitMixin]
    public interface IMActivateOnReceiver : IMixin, IEventfulComponent
    {

        sealed void OnInitMixin()
        {
            OnActivateReceiverMixinLogic.Current.Initialize(this);
        }

        ActivateEvent ActivateOn { get; }
        void Activate();

    }

    public class OnActivateReceiverMixinLogic : IMixin
    {

        public static readonly OnActivateReceiverMixinLogic Default = new OnActivateReceiverMixinLogic();
        private static OnActivateReceiverMixinLogic _current;
        public static OnActivateReceiverMixinLogic Current
        {
            get => _current ?? Default;
            set => _current = value;
        }

        public virtual void Initialize(IMActivateOnReceiver receiver)
        {
            receiver.OnEnabled += Target_OnEnabled;
            receiver.OnStarted += Target_OnStarted;
            Target_OnAwake(receiver, EventArgs.Empty);
        }

        protected virtual void Target_OnAwake(object sender, EventArgs e)
        {
            var targ = sender as IMActivateOnReceiver;
            if (targ != null && (targ.ActivateOn & ActivateEvent.Awake) != 0)
            {
                targ.Activate();
            }
        }

        protected virtual void Target_OnEnabled(object sender, EventArgs e)
        {
            var targ = sender as IMActivateOnReceiver;
            if (targ == null || !targ.started) return;

            if ((targ.ActivateOn & ActivateEvent.OnEnable) != 0)
            {
                if (GameLoop.LateUpdateWasCalled)
                {
                    targ.Activate();
                }
                else
                {
                    GameLoop.LateUpdateHandle.BeginInvoke(targ.Activate);
                }
            }
        }

        protected virtual void Target_OnStarted(object sender, EventArgs e)
        {
            var targ = sender as IMActivateOnReceiver;
            if (targ == null) return;

            var aoe = targ.ActivateOn;
            if ((aoe & ActivateEvent.OnLateStart) != 0 && !GameLoop.LateUpdateWasCalled)
            {
                GameLoop.LateUpdateHandle.BeginInvoke(() => targ.Activate());
            }
            else if ((aoe & ActivateEvent.OnStart) != 0 || (aoe & ActivateEvent.OnEnable) != 0)
            {
                targ.Activate();
            }
        }

    }

}
