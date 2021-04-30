using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Motor;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Motor.Events
{
    public class i_ChangeMovementStyle : AutoTriggerable
    {

        public enum ChangeMode
        {
            Change,
            Stack,
            Pop,
            PopAll,
            Purge
        }

        #region Fields

        [SerializeField]
        private MovementStyleController _controller;
        [SerializeField]
        [TypeRestriction(typeof(IMovementStyle))]
        private Component _movementStyle;
        [SerializeField]
        private ChangeMode _mode;
        [SerializeField]
        private float _precedence;

        #endregion

        #region Properties

        public MovementStyleController Controller
        {
            get { return _controller; }
            set { _controller = value; }
        }

        public IMovementStyle MovementStyle
        {
            get { return _movementStyle as IMovementStyle; }
            set { _movementStyle = value as Component; }
        }

        public ChangeMode Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        public float Precedence
        {
            get { return _precedence; }
            set { _precedence = value; }
        }

        #endregion


        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;
            if (_controller == null) return false;

            try
            {
                switch (_mode)
                {
                    case ChangeMode.Change:
                        {
                            var style = this.MovementStyle;
                            if (style == null) return false;

                            if (_controller.Contains(style))
                            {
                                _controller.ChangeState(style, _precedence);
                                return true;
                            }
                        }
                        break;
                    case ChangeMode.Stack:
                        {
                            var style = this.MovementStyle;
                            if (style == null) return false;

                            if (_controller.Contains(style))
                            {
                                _controller.StackState(style, _precedence);
                                return true;
                            }
                        }
                        break;
                    case ChangeMode.Pop:
                        {
                            _controller.PopState(_precedence);
                            return true;
                        }
                    case ChangeMode.PopAll:
                        {
                            _controller.PopAllStates(_precedence);
                            return true;
                        }
                    case ChangeMode.Purge:
                        {
                            var style = this.MovementStyle;
                            if (style == null) return false;

                            if (_controller.Contains(style))
                            {
                                _controller.PurgeStackedState(style);
                                return true;
                            }
                        }
                        break;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }

            return false;
        }
    }
}
