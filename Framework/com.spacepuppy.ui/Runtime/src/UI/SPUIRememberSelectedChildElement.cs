using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using com.spacepuppy.Utils;

namespace com.spacepuppy.UI
{

    public sealed class SPUIRememberSelectedChildElement : SPComponent, IMStartOrEnableReceiver
    {

        #region Fields

        [SerializeField]
        [RespectsIProxy]
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

            var current = Services.Get<IEventSystem>()?.GetSelectedGameObject(this.gameObject);
            if (current && this.gameObject.IsParentOf(current))
            {
                _lastSelectedElement = current;
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
            Services.Get<IEventSystem>()?.SetSelectedGameObject(_lastSelectedElement ? _lastSelectedElement : GameObjectUtil.GetGameObjectFromSource(_defaultElement, true), this.gameObject);
        }

        #endregion

    }

}
