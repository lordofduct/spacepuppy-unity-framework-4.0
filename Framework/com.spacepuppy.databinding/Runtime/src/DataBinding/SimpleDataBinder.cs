using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.DataBinding
{

    public class SimpleDataBinder : SPComponent
    {

        #region Fields

        [SerializeField]
        private int _order;

        [SerializeField()]
        private ActivateEvent _activateOn = ActivateEvent.OnStartOrEnable;

        [SerializeField]
        [TypeRestriction(typeof(IDataProvider), AllowProxy = true)]
        private UnityEngine.Object _dataProvider;

        [SerializeField]
        [TypeRestriction(typeof(IDataBindingContext), AllowProxy = true)]
        private UnityEngine.Object _targetDataBindingContext;

        [SerializeField]
        [Tooltip("All DataBindingContexts and children of the TargetDataBindingContext will have their bind events called.")]
        private bool _broadcastBindingMessage = true;

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();

            if ((_activateOn & ActivateEvent.Awake) != 0)
            {
                this.Stamp();
            }
        }

        protected override void Start()
        {
            base.Start();

            if ((_activateOn & ActivateEvent.OnStart) != 0 || (_activateOn & ActivateEvent.OnEnable) != 0)
            {
                this.Stamp();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!this.started) return;

            if ((_activateOn & ActivateEvent.OnEnable) != 0)
            {
                this.Stamp();
            }
        }

        #endregion

        #region Properties

        public int Order
        {
            get => _order;
            set => _order = value;
        }

        public ActivateEvent ActivateOn
        {
            get => _activateOn;
            set => _activateOn = value;
        }

        public UnityEngine.Object DataProvider
        {
            get => _dataProvider;
            set => _dataProvider = value;
        }

        public UnityEngine.Object DataBindingContext
        {
            get => _targetDataBindingContext;
            set => _targetDataBindingContext = value;
        }

        #endregion

        #region Methods

        public void Stamp()
        {
            var provider = ObjUtil.GetAsFromSource<IDataProvider>(_dataProvider, true);
            var source = provider?.FirstElement ?? _dataProvider;

            GameObject go;
            IDataBindingContext context;
            if (_broadcastBindingMessage && (go = GameObjectUtil.GetGameObjectFromSource(_targetDataBindingContext, true)))
            {
                com.spacepuppy.DataBinding.DataBindingContext.BroadcastBindMessage(go, source, 0, true, true);
            }
            else if((context = ObjUtil.GetAsFromSource<IDataBindingContext>(_targetDataBindingContext, true)) != null)
            {
                context.Bind(source, 0);
            }
        }
        private static readonly System.Action<IDataBindingContext, System.ValueTuple<object, int>> _stampFunctor = (s, t) => s.Bind(t.Item1, t.Item2);

        #endregion

    }

}
