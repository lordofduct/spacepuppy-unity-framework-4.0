using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using com.spacepuppy.Utils;

namespace com.spacepuppy.UI
{

    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    public sealed class FixedAspectLayoutElement : UIBehaviour, ILayoutElement, ILayoutIgnorer
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

        private float _calculatedMinWidth;
        private float _calculatedMinHeight;
        private float _calculatedPreferredWidth;
        private float _calculatedPreferredHeight;
        private float _calculatedFlexibleWidth;
        private float _calculatedFlexibleHeight;

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
            get
            {
                return _minWidth;
            }
            set
            {
                if (_minWidth != value)
                {
                    _minWidth = value;
                    SetDirty();
                }
            }
        }

        public float preferredWidth
        {
            get
            {
                return _preferredWidth;
            }
            set
            {
                if (_preferredWidth != value)
                {
                    _preferredWidth = value;
                    SetDirty();
                }
            }
        }

        public float flexibleWidth
        {
            get
            {
                return _flexibleWidth;
            }
            set
            {
                if (_flexibleWidth != value)
                {
                    _flexibleWidth = value;
                    SetDirty();
                }
            }
        }

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

        #endregion

        #region Methods

        protected override void OnEnable()
        {
            SetDirty();
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            SetDirty();
            base.OnDisable();
        }

        protected override void OnDidApplyAnimationProperties()
        {
            SetDirty();
            base.OnDidApplyAnimationProperties();
        }

        protected override void OnBeforeTransformParentChanged()
        {
            SetDirty();
            base.OnBeforeTransformParentChanged();
        }

        protected override void OnTransformParentChanged()
        {
            SetDirty();
            base.OnTransformParentChanged();
        }

        protected override void Start()
        {
            SetDirty();
            base.Start();
        }

        void SetDirty()
        {
            (this as ILayoutElement).CalculateLayoutInputHorizontal();
            (this as ILayoutElement).CalculateLayoutInputVertical();
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

        float ILayoutElement.minWidth => _calculatedMinWidth;
        float ILayoutElement.minHeight => _calculatedMinHeight;
        float ILayoutElement.preferredWidth => _calculatedMinWidth;
        float ILayoutElement.preferredHeight => _calculatedMinHeight;
        float ILayoutElement.flexibleWidth => _calculatedMinWidth;
        float ILayoutElement.flexibleHeight => _calculatedMinHeight;

        void ILayoutElement.CalculateLayoutInputHorizontal()
        {
            Vector2 sz = this.transform.sizeDelta;
            bool isVertical = this.transform.parent && this.transform.parent.HasComponent<VerticalLayoutGroup>();
            if (isVertical)
            {
                _calculatedMinWidth = _minWidth;
            }
            else
            {
                sz.x = sz.y / _aspectRatio;
                _calculatedMinWidth = _minWidth >= 0f ? sz.x : -1f;
            }
            _calculatedPreferredWidth = _preferredWidth;
            _calculatedFlexibleWidth = _flexibleWidth;
        }

        void ILayoutElement.CalculateLayoutInputVertical()
        {
            Vector2 sz = this.transform.sizeDelta;
            bool isVertical = this.transform.parent && this.transform.parent.HasComponent<VerticalLayoutGroup>();
            if (isVertical)
            {
                sz.y = sz.x * _aspectRatio;
                _calculatedMinHeight = _minWidth >= 0f ? sz.y : 0f;
            }
            else
            {
                _calculatedMinHeight = _minWidth * _aspectRatio;
            }
            _calculatedPreferredHeight = _preferredWidth * _aspectRatio;
            _calculatedFlexibleHeight = _flexibleWidth * _aspectRatio;
        }

        void Update()
        {
            this.SetDirty();
        }

        #endregion

    }

}
