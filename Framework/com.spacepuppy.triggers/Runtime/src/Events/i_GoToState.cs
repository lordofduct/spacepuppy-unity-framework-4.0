using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Events;
using com.spacepuppy.Project;

namespace com.spacepuppy
{

    public class i_GoToState : AutoTriggerable
    {

        #region Fields

        [SerializeField]
        private InterfaceRef<IStateMachine> _stateMachine = new();

        [SerializeField, RefPickerConfig(AllowNull = false, AlwaysExpanded = true, DisplayBox = false)]
        private InterfacePicker<IMode> _mode = new(new ById());

        #endregion

        #region Properties

        public IStateMachine StateMachine
        {
            get => _stateMachine.Value;
            set => _stateMachine.Value = value;
        }

        public IMode Mode
        {
            get => _mode.Value;
            set => _mode.Value = value;
        }

        #endregion

        #region Triggerable Interface

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            this.Mode?.Trigger(_stateMachine.Value);
            return true;
        }

        #endregion

        #region Special Types

        public interface IMode
        {
            void Trigger(IStateMachine machine);
        }

        [System.Serializable]
        public class ById : IMode
        {

            public string id;

            public void Trigger(IStateMachine machine)
            {
                machine?.GoToStateById(id);
            }
        }

        [System.Serializable]
        public class ByIndex : IMode
        {

            public int index;

            public void Trigger(IStateMachine machine)
            {
                machine?.GoToState(index);
            }
        }

        #endregion

    }

}
