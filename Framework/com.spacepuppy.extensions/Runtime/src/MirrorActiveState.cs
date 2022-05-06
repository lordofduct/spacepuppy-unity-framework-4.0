using UnityEngine;

namespace com.spacepuppy
{

    [Infobox("Sets target to the same active/enabled state as this during the enable/disable events.\r\n\r\nNote - this doesn't start syncing until the first time it has been enabled, the Sync method can be called to premptively sync it.")]
    public class MirrorActiveState : SPComponent, IMStartOrEnableReceiver
    {

        #region Fields

        [SerializeField]
        [ReorderableArray]
        private GameObject[] _targets;

        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            this.Sync();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            this.Sync();
        }

        #endregion

        #region Properties

        public GameObject[] Targets
        {
            get => _targets;
            set => _targets = value;
        }

        #endregion

        #region Methods

        public void Sync()
        {
            if (_targets == null) return;

            foreach (var targ in _targets)
            {
                if (targ) targ.SetActive(this.isActiveAndEnabled);
            }
        }

        #endregion

    }

}
