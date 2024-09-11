using UnityEngine;

namespace com.spacepuppy
{

    [Infobox("Sets target to the same active/enabled state as this during the enable/disable events.\r\n\r\nNote - this doesn't start syncing until the first time it has been enabled, the Sync method can be called to premptively sync it.")]
    public class MirrorActiveState : SPComponent
    {

        #region Fields

        [SerializeField]
        private bool _inverted;

        [SerializeField]
        [ReorderableArray]
        private GameObject[] _targets;

        #endregion

        #region CONSTRUCTOR

        protected override void OnEnable()
        {
            this.Sync();
            base.OnEnable();
        }

        protected override void Start()
        {
            this.Sync();
            base.Start();
        }

        protected override void OnDisable()
        {
            this.Sync();
            base.OnDisable();
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
                if (targ) targ.SetActive(_inverted ? !this.isActiveAndEnabled : this.isActiveAndEnabled);
            }
        }

        #endregion

    }

}
