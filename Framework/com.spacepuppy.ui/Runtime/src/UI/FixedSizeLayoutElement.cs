using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace com.spacepuppy.UI
{

    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    public class FixedSizeLayoutElement : UIBehaviour, ILayoutElement, ILayoutIgnorer
    {

        #region Fields

        [SerializeField]
        private bool _ignoreLayout = false;

        [SerializeField]
        private float _width = -1f;

        [SerializeField]
        private float _height = -1f;

        [SerializeField]
        private int _layoutPriority = 1;

        #endregion

        #region Properties

        public new RectTransform transform
        {
            get => base.transform as RectTransform;
        }

        public float width
        {
            get => _width;
            set
            {
                if (_width != value)
                {
                    _width = value;
                    SetDirty();
                }
            }
        }

        public float height
        {
            get => _height;
            set
            {
                if (_height != value)
                {
                    _height = value;
                    SetDirty();
                }
            }
        }

        #endregion

        #region Methods

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnTransformParentChanged()
        {
            SetDirty();
        }

        protected override void OnDisable()
        {
            SetDirty();
            base.OnDisable();
        }

        protected override void OnDidApplyAnimationProperties()
        {
            SetDirty();
        }

        protected override void OnBeforeTransformParentChanged()
        {
            SetDirty();
        }

        protected void SetDirty()
        {
            if (IsActive())
            {
                LayoutRebuilder.MarkLayoutForRebuild(base.transform as RectTransform);
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirty();
        }
#endif

        #endregion

        #region ILayoutElement Interface

        public bool ignoreLayout
        {
            get => _ignoreLayout;
            set
            {
                if (_ignoreLayout != value)
                {
                    _ignoreLayout = value;
                    SetDirty();
                }
            }
        }

        public float minWidth => _width;

        public float minHeight => _height;

        public float preferredWidth => _width;

        public float preferredHeight => _height;

        public float flexibleWidth => _width;

        public float flexibleHeight => _height;

        public int layoutPriority
        {
            get => _layoutPriority;
            set
            {
                if (_layoutPriority != value)
                {
                    _layoutPriority = value;
                    SetDirty();
                }
            }
        }

        void ILayoutElement.CalculateLayoutInputHorizontal()
        {
        }

        void ILayoutElement.CalculateLayoutInputVertical()
        {
        }

        #endregion

    }
}
