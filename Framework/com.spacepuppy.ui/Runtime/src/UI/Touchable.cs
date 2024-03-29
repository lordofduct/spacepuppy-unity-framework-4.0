using UnityEngine;
using UnityEngine.UI;

namespace com.spacepuppy.UI
{

    [RequireComponent(typeof(RectTransform))]
    public class Touchable : Text, IUIComponent
    {
        protected override void Awake() { base.Awake(); }

        #region IUIComponent Interface

        public new RectTransform transform => base.transform as RectTransform;

        RectTransform IUIComponent.transform => base.transform as RectTransform;

        Component IComponent.component => this;

        #endregion

    }

}
