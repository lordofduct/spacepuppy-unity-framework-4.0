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
        private DataProviderRef _dataProvider = new DataProviderRef();

        [SerializeField]
        private DataBindingContextRef _targetDataBindingContext = new DataBindingContextRef();

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

        public IDataProvider DataProvider
        {
            get => _dataProvider.Value;
            set => _dataProvider.Value = value;
        }

        public IDataBindingContext DataBindingContext
        {
            get => _targetDataBindingContext.Value;
            set => _targetDataBindingContext.Value = value;
        }

        #endregion

        #region Methods

        public void Stamp()
        {
            var source = this.DataProvider?.FirstElement;

            var context = this.DataBindingContext;
            GameObject go;
            if (_broadcastBindingMessage && (go = GameObjectUtil.GetGameObjectFromSource(context)))
            {
                go.Broadcast((source, 0), _stampFunctor, true, true);
            }
            else
            {
                context.Bind(source, 0);
            }
        }
        private static readonly System.Action<IDataBindingContext, System.ValueTuple<object, int>> _stampFunctor = (s, t) => s.Bind(t.Item1, t.Item2);

        #endregion

    }

}
