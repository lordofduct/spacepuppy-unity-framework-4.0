using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace com.spacepuppy.UI
{

    /// <remarks>
    /// Based on logic found in this thread: https://discussions.unity.com/t/rect-transform-size-limiter/730374/2
    /// Assumed distinct from Spacepuppy MIT license.
    /// </remarks>
    [ExecuteInEditMode]
    public sealed class RectSizeLimiter : SPUIBehaviour, ILayoutSelfController
    {

        #region Fields

        [SerializeField]
        private Vector2 _maxSize = Vector2.zero;

        [SerializeField]
        private Vector2 _minSize = Vector2.zero;

        [System.NonSerialized]
        private DrivenRectTransformTracker _tracker;

        #endregion

        #region CONSTRUCTOR

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnDisable()
        {
            _tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(this.transform);
            base.OnDisable();
        }

        #endregion

        #region Properties

        public Vector2 MaxSize
        {
            get { return _maxSize; }
            set
            {
                if (_maxSize != value)
                {
                    _maxSize = value;
                    SetDirty();
                }
            }
        }

        public Vector2 MinSize
        {
            get { return _minSize; }
            set
            {
                if (_minSize != value)
                {
                    _minSize = value;
                    SetDirty();
                }
            }
        }

        #endregion

        #region Methods

        void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(this.transform);
        }

        public void SetLayoutHorizontal()
        {
            if (_maxSize.x > 0f && this.transform.rect.width > _maxSize.x)
            {
                this.transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _maxSize.x);
                _tracker.Add(this, this.transform, DrivenTransformProperties.SizeDeltaX);
            }

            if (_minSize.x > 0f && this.transform.rect.width < _minSize.x)
            {
                this.transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _minSize.x);
                _tracker.Add(this, this.transform, DrivenTransformProperties.SizeDeltaX);
            }

        }

        public void SetLayoutVertical()
        {
            if (_maxSize.y > 0f && this.transform.rect.height > _maxSize.y)
            {
                this.transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _maxSize.y);
                _tracker.Add(this, this.transform, DrivenTransformProperties.SizeDeltaY);
            }

            if (_minSize.y > 0f && this.transform.rect.height < _minSize.y)
            {
                this.transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _minSize.y);
                _tracker.Add(this, this.transform, DrivenTransformProperties.SizeDeltaY);
            }

        }

        #endregion

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetDirty();
        }
#endif

    }

}