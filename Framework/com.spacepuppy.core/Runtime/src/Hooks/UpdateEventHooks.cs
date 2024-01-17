using UnityEngine;

namespace com.spacepuppy.Hooks
{

    [AddComponentMenu("SpacePuppy/Hooks/UpdateEventHooks")]
    public class UpdateEventHooks : com.spacepuppy.SPComponent
    {

        public event System.EventHandler UpdateHook;
        public event System.EventHandler FixedUpdateHook;
        public event System.EventHandler LateUpdateHook;

        // Update is called once per frame
        void Update()
        {
            this.UpdateHook?.Invoke(this, System.EventArgs.Empty);
        }

        void FixedUpdate()
        {
            this.FixedUpdateHook?.Invoke(this, System.EventArgs.Empty);
        }

        void LateUpdate()
        {
            this.LateUpdateHook?.Invoke(this, System.EventArgs.Empty);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            UpdateHook = null;
            FixedUpdateHook = null;
            LateUpdateHook = null;
        }

    }

}