using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

namespace com.spacepuppy.UI
{

    [RequireComponent(typeof(RectTransform))]
    public sealed class SPUISelectOnPointerEnter : SPUIComponent, IUIComponent, IPointerEnterHandler
    {

        #region Fields

        [SerializeField]
        [DefaultFromSelf]
        private Selectable _selectable;

        #endregion

        #region CONSTRUCTOR

        protected override void OnEnable()
        {
            if (!_selectable) _selectable = GetComponent<Selectable>();
            base.OnEnable();
        }

        #endregion

        #region Properties

        public Selectable Selectable
        {
            get => _selectable;
            set => _selectable = value;
        }

        #endregion

        #region IPointerEnterHandler Interface

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (_selectable && _selectable.isActiveAndEnabled && _selectable.interactable) _selectable.Select();
        }

        #endregion

    }
}
