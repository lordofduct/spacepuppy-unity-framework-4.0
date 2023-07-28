using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

namespace com.spacepuppy
{

    public sealed class SPUISelectOnPointerEnter : MonoBehaviour, IPointerEnterHandler
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

    }
}
