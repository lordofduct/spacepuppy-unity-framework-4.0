using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Geom;
using com.spacepuppy.Utils;
using com.spacepuppy.Events;

namespace com.vivarium
{

    public class DraggableUIElement : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {

        #region Fields

        [SerializeField]
        private bool _duplicateSelfOnDrag;

        [SerializeField]
        private bool _maintainOriginalParent;

        [SerializeField]
        private bool _resetOnEnd;

        [SerializeField]
        [SPEvent.Config("this (DraggableUIElement)")]
        private SPEvent _onDragBegin = new SPEvent("OnDragBegin");

        [SerializeField]
        [SPEvent.Config("this (DraggableUIElement)")]
        private SPEvent _onDragEnd = new SPEvent("OnDragEnd");



        [System.NonSerialized]
        protected RectTransform _surface;
        [System.NonSerialized]
        protected DraggableUIElement _duplicateSource;

        #endregion

        #region Properties

        public bool DuplicateSelfOnDrag
        {
            get => _duplicateSelfOnDrag;
            set => _duplicateSelfOnDrag = value;
        }

        public bool MaintainOriginalParent
        {
            get => _maintainOriginalParent;
            set => _maintainOriginalParent = value;
        }

        public bool ResetOnEnd
        {
            get => _resetOnEnd;
            set => _resetOnEnd = value;
        }

        public SPEvent OnDragBegin => _onDragBegin;

        public SPEvent OnDragEnd => _onDragEnd;

        public TransformStateCache LastBeginGlobalTransformation
        {
            get;
            set;
        }

        public bool IsDuplicate => !object.ReferenceEquals(_duplicateSource, null);

        #endregion

        #region Methods

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            this.OnBeginDrag(eventData);
        }
        protected virtual void OnBeginDrag(PointerEventData eventData)
        {
            var surface = this.GetComponentInParent<Canvas>()?.transform as RectTransform;
            if (surface == null) return;

            if (_duplicateSelfOnDrag)
            {
                var clone = Instantiate(this, this.transform.position, this.transform.rotation, _maintainOriginalParent ? this.transform.parent : surface);
                clone._duplicateSource = this;
                var rts = this.transform as RectTransform;
                var rtc = clone.transform as RectTransform;
                if(rtc && rts)
                {
                    rtc.sizeDelta = rts.sizeDelta;
                }

                eventData.pointerDrag = clone.gameObject;
                clone.StartDrag(surface, eventData);
            }
            else
            {
                this.StartDrag(surface, eventData);
            }

            _onDragBegin.ActivateTrigger(this, this);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            this.OnDrag(eventData);
        }
        protected virtual void OnDrag(PointerEventData eventData)
        {
            if (_surface == null) return;

            SetPositionInParent(eventData);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            this.OnEndDrag(eventData);
        }
        protected void OnEndDrag(PointerEventData eventData)
        {
            if (_surface == null) return;

            if (this.IsDuplicate)
            {
                Destroy(this.gameObject);

                if (_duplicateSource) _duplicateSource._onDragEnd.ActivateTrigger(this, null);
            }
            else
            {
                if (_resetOnEnd)
                {
                    if (!_maintainOriginalParent)
                    {
                        this.transform.SetParent(this.LastBeginGlobalTransformation.Parent, true);
                        this.transform.SetSiblingIndex(this.LastBeginGlobalTransformation.SiblingIndex);
                    }
                    this.LastBeginGlobalTransformation.Trans.SetToGlobal(this.transform, false);
                }

                _onDragEnd.ActivateTrigger(this, this);
            }
        }

        protected void SetPositionInParent(PointerEventData data)
        {
            var rt = this.transform as RectTransform;
            Vector3 globalMousePos;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(_surface, data.position, data.pressEventCamera, out globalMousePos))
            {
                rt.position = globalMousePos;
                rt.rotation = _surface.rotation;
            }
        }

        protected void StartDrag(RectTransform surface, PointerEventData eventData)
        {
            if (surface == null) throw new System.ArgumentNullException(nameof(surface));
            _surface = surface;
            this.LastBeginGlobalTransformation = TransformStateCache.GetGlobal(this.transform);

            if (!_maintainOriginalParent)
            {
                this.transform.SetParent(_surface, true);
            }

            SetPositionInParent(eventData);
        }

        #endregion

        #region Special Types

        public struct TransformStateCache
        {
            public Trans Trans;
            public Transform Parent;
            public int SiblingIndex;

            public static TransformStateCache GetGlobal(Transform t)
            {
                return new TransformStateCache()
                {
                    Trans = Trans.GetGlobal(t),
                    Parent = t.parent,
                    SiblingIndex = t.GetSiblingIndex()
                };
            }
        }

        #endregion

    }
}
