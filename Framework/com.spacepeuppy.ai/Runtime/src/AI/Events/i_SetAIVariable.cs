#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Events;

namespace com.spacepuppy.AI.Events
{

    public class i_SetAIVariable : Triggerable
    {


        #region Fields

        [SerializeField()]
        [TypeRestriction(typeof(IAIController))]
        private UnityEngine.Object _aiController;

        [SerializeField()]
        [AIVariableName()]
        private string _variable;
        [SerializeField()]
        private VariantReference _value = new VariantReference();

        #endregion

        #region Properties

        public IAIController AIController
        {
            get => _aiController as IAIController;
            set => _aiController = value as UnityEngine.Object;
        }

        public string Variable
        {
            get => _variable;
            set => _variable = value;
        }

        public VariantReference Value => _value;

        #endregion

        #region ITriggerableMechanism Interface

        public override bool CanTrigger
        {
            get
            {
                return base.CanTrigger && _aiController != null && !string.IsNullOrEmpty(_variable);
            }
        }

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            this.AIController.Variables[_variable] = _value.Value;
            return true;
        }

        #endregion

    }

}
