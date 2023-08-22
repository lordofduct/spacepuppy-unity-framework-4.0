using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

namespace com.spacepuppy.UI
{

    [RequireComponent(typeof(RectTransform))]
    public sealed class SPUISelectOnPointerEnter : MonoBehaviour, IUIComponent, IPointerEnterHandler
    {

        #region Fields

        [SerializeField]
        [DefaultFromSelf]
        private Selectable _selectable;

        #endregion

        #region CONSTRUCTOR

        void OnEnable()
        {
            if (!_selectable) _selectable = GetComponent<Selectable>();
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

        #region IUIComponent Interface

        public new RectTransform transform => base.transform as RectTransform;

        RectTransform IUIComponent.transform => base.transform as RectTransform;

        Component IComponent.component => this;

        #endregion

    }
}
