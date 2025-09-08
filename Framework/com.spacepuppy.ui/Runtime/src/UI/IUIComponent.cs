using UnityEngine;
using UnityEngine.EventSystems;
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

    public abstract class SPUIBehaviour : UIBehaviour, IUIComponent, ISPDisposable, INameable
    {

        #region Fields

        [System.NonSerialized]
        private bool _synced;
        [System.NonSerialized]
        private Canvas _canvas;

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();
            if (this is IMixin mx) MixinUtil.InitializeMixins(mx);
        }

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

        /// <summary>
        /// This should only be used if you're not using RadicalCoroutine. If you are, use StopAllRadicalCoroutines instead.
        /// </summary>
        public new void StopAllCoroutines()
        {
            //this is an attempt to capture this method, it's not guaranteed and honestly you should avoid calling StopAllCoroutines all together and instead call StopAllRadicalCoroutines.
            this.SendMessage("RadicalCoroutineManager_InternalHook_StopAllCoroutinesCalled", this, SendMessageOptions.DontRequireReceiver);
            base.StopAllCoroutines();
        }

        protected void SyncCanvas()
        {
            _canvas = IUIComponentExtensions.FindCanvas(this.gameObject);
            _synced = true;
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();

            _canvas = null;
            _synced = false;
        }

        #endregion

        #region IComponent Interface

        bool IComponent.enabled
        {
            get { return this.enabled; }
            set { this.enabled = value; }
        }

        Component IComponent.component
        {
            get { return this; }
        }

        //implemented implicitly
        /*
        GameObject IComponent.gameObject { get { return this.gameObject; } }
        Transform IComponent.transform { get { return this.transform; } }
        */

        #endregion

        #region ISPDisposable Interface

        bool ISPDisposable.IsDisposed
        {
            get
            {
                return !ObjUtil.IsObjectAlive(this);
            }
        }

        void System.IDisposable.Dispose()
        {
            ObjUtil.SmartDestroy(this);
        }

        #endregion

        #region INameable Interface

        public new string name
        {
            get => NameCache.GetCachedName(this.gameObject);
            set => NameCache.SetCachedName(this.gameObject, value);
        }
        string INameable.Name
        {
            get => NameCache.GetCachedName(this.gameObject);
            set => NameCache.SetCachedName(this.gameObject, value);
        }
        public bool CompareName(string nm) => this.gameObject.CompareName(nm);
        void INameable.SetDirty() => NameCache.SetDirty(this.gameObject);

        #endregion

#if UNITY_EDITOR && UNITY_2022_3_OR_NEWER
        [ContextMenu("Move To Top")]
        void ComponentEditor_MoveToTop()
        {
            int steps = this.gameObject.GetComponentIndex(this) - 1;
            for (int i = 0; i < steps; i++)
            {
                UnityEditorInternal.ComponentUtility.MoveComponentUp(this);
            }
        }

        [ContextMenu("Move To Bottom")]
        void ComponentEditor_MoveToBottom()
        {
            int lastindex = this.gameObject.GetComponentCount() - 1;
            int steps = lastindex - this.gameObject.GetComponentIndex(this);
            for (int i = 0; i < steps; i++)
            {
                UnityEditorInternal.ComponentUtility.MoveComponentDown(this);
            }
        }
#endif

    }

    [RequireComponent(typeof(RectTransform))]
    public abstract class SPUIComponent : SPUIBehaviour, IEventfulComponent
    {

        #region Events

        public event System.EventHandler OnEnabled;
        public event System.EventHandler OnStarted;
        public event System.EventHandler OnDisabled;
        public event System.EventHandler ComponentDestroyed;

        #endregion

        #region Fields

        #endregion

        #region CONSTRUCTOR

        protected override void Start()
        {
            base.Start();
            this.started = true;
            try
            {
                this.OnStarted?.Invoke(this, System.EventArgs.Empty);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            try
            {
                this.OnEnabled?.Invoke(this, System.EventArgs.Empty);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            try
            {
                this.OnDisabled?.Invoke(this, System.EventArgs.Empty);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            try
            {
                this.ComponentDestroyed?.Invoke(this, System.EventArgs.Empty);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Start has been called on this component.
        /// </summary>
        public bool started { get; private set; }

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
