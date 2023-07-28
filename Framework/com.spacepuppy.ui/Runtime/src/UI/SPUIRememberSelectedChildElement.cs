using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    public sealed class SPUIRememberSelectedChildElement : SPComponent, IMStartOrEnableReceiver
    {

        #region Fields

        [SerializeField]
        [TypeRestriction(typeof(GameObject), AllowProxy = true)]
        private UnityEngine.Object _defaultElement;

        [System.NonSerialized]
        private GameObject _lastSelectedElement;

        #endregion

        #region CONSTRUCTOR

        void IMStartOrEnableReceiver.OnStartOrEnable()
        {
            this.SelectTargetElement();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            var evs = EventSystem.current;
            if (evs && evs.currentSelectedGameObject && this.gameObject.IsParentOf(evs.currentSelectedGameObject))
            {
                _lastSelectedElement = evs.currentSelectedGameObject;
            }
            else
            {
                _lastSelectedElement = null;
            }
        }

        #endregion

        #region Methods

        public void Clear()
        {
            _lastSelectedElement = null;
        }

        public void ClearAndSelectDefault()
        {
            _lastSelectedElement = null;
            this.SelectTargetElement();
        }

        public void SelectTargetElement()
        {
            UIEventUtil.SetSelectedGameObject(_lastSelectedElement ? _lastSelectedElement : GameObjectUtil.GetGameObjectFromSource(_defaultElement, true));
        }

        #endregion

    }

}
