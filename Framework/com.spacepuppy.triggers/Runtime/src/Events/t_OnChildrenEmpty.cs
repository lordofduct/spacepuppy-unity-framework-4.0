using UnityEngine;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    public sealed class t_OnChildrenEmpty : TriggerComponent, IMStartOrEnableReceiver
    {

        #region Fields

        [SerializeField()]
        [DefaultFromSelf()]
        [TypeRestriction(typeof(Transform), AllowProxy = true)]
        private UnityEngine.Object _target;

        [System.NonSerialized]
        private MonitorForEmpty _current;

        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            var targ = ObjUtil.GetAsFromSource<Transform>(_target, true);
            if (targ == null) return;

            _current = targ.AddOrGetComponent<MonitorForEmpty>();
            if (_current)
            {
                _current.SignalEmpty += _current_SignalEmpty;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (_current)
            {
                _current.SignalEmpty -= _current_SignalEmpty;
                _current = null;
            }
        }

        #endregion

        #region Methods

        private void _current_SignalEmpty(object sender, System.EventArgs e)
        {
            this.ActivateTrigger();
        }

        #endregion

        #region Special Type

        private class MonitorForEmpty : MonoBehaviour
        {

            public event System.EventHandler SignalEmpty;
            private bool _haschildren;

            private void OnEnable()
            {
                _haschildren = this.transform.childCount > 0;
            }

            private void OnTransformChildrenChanged()
            {
                if (_haschildren && this.transform.childCount == 0)
                {
                    this.SignalEmpty?.Invoke(this, System.EventArgs.Empty);
                }
            }
        }

        #endregion

    }
}
