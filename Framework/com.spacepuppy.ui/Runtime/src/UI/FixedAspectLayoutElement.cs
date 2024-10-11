using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace com.spacepuppy.UI
{

    [RequireComponent(typeof(RectTransform))]
    public sealed class FixedAspectLayoutElement : UIBehaviour, ILayoutElement
    {

        #region Fields

        [SerializeField]
        private bool _ignoreLayout = false;

        [SerializeField]
        private float _aspectRatio = 1f;

        [SerializeField]
        private float _minWidth = -1f;

        [SerializeField]
        private float _preferredWidth = -1f;

        [SerializeField]
        private float _flexibleWidth = -1f;

        [SerializeField]
        private int _layoutPriority = 1;

        #endregion

        #region Properties

        public new RectTransform transform
        {
            get => base.transform as RectTransform;
        }

        public float aspectRatio
        {
            get => _aspectRatio;
            set
            {
                if (_aspectRatio != value)
                {
                    _aspectRatio = value;
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

        public float minWidth
        {
            get => _minWidth;
            set
            {
                if (_minWidth != value)
                {
                    _minWidth = value;
                    SetDirty();
                }
            }
        }

        public float minHeight => this.transform.sizeDelta.x * _aspectRatio;

        public float preferredWidth
        {
            get => _preferredWidth;
            set
            {
                if (_preferredWidth != value)
                {
                    _preferredWidth = value;
                    SetDirty();
                }
            }
        }

        public float preferredHeight => _preferredWidth * _aspectRatio;

        public float flexibleWidth
        {
            get => _flexibleWidth;
            set
            {
                if (_flexibleWidth != value)
                {
                    _flexibleWidth = value;
                    SetDirty();
                }
            }
        }

        public float flexibleHeight => _flexibleWidth * _aspectRatio;

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
