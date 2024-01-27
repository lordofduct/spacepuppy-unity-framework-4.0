using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Collections;

namespace com.spacepuppy.UI
{

    public interface IUIComponent : IComponent
    {
        new RectTransform transform { get; }
        Canvas canvas { get; }
    }

    [RequireComponent(typeof(RectTransform))]
    public abstract class SPUIComponent : SPComponent, IUIComponent
    {

        #region Fields

        [System.NonSerialized]
        private bool _synced;
        [System.NonSerialized]
        private Canvas _canvas;

        #endregion

        #region CONSTRUCTOR

        #endregion

        #region Properties

        public new RectTransform transform => base.transform as RectTransform;

        public Canvas canvas
        {
            get
            {
                if (!_synced) this.SyncCanvas();
                return _canvas;
            }
        }

        #endregion

        #region Methods

        protected void SyncCanvas()
        {
            _canvas = IUIComponentExtensions.FindCanvas(this.gameObject);
            _synced = true;
        }

        protected virtual void OnTransformParentChanged()
        {
            _canvas = null;
            _synced = false;
        }

        #endregion

    }

    public static class IUIComponentExtensions
    {

        public static Canvas FindCanvas(GameObject go)
        {
            if (!go) return null;

            using (var lst = TempCollection.GetList<Canvas>())
            {
                go.GetComponentsInParent(false, lst);
                if (lst.Count > 0)
                {
                    // Find the first active and enabled canvas.
                    for (int i = 0; i < lst.Count; ++i)
                    {
                        if (lst[i].isActiveAndEnabled)
                        {
                            return lst[i];
                        }
                    }
                }
            }
            return null;
        }

        public static Camera GetCanvasRenderCamera(this IUIComponent c)
        {
            var canvas = c?.canvas;
            if (canvas == null) return null;

            var cam = canvas.worldCamera;
            return cam ? cam : Camera.main;
        }

    }
}
