using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Events;

#if SP_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace com.spacepuppy.DataBinding
{

    public class LayoutGroupDataBindingContext : SPComponent, IDataBindingContext, IDataProvider
    {

        #region Fields

        [SerializeField()]
        private ActivateEvent _activateOn = ActivateEvent.None;

        [SerializeField]
        [SelectableObject(AllowProxy = true)]
        [Tooltip("A datasource to be binded to during the 'ActivateOn' event or if calling 'BindConfiguredDataSource'.")]
        private UnityEngine.Object _dataSource;

        [SerializeField]
        private bool _respectProxySources = false;

        [SerializeField]
        private int _maxVisible = 100;

        [SerializeField]
        [DefaultFromSelf]
        private Transform _container;

        [SerializeField]
        private Messaging.MessageSendCommand _bindMessageSettings = new Messaging.MessageSendCommand()
        {
            SendMethod = Messaging.MessageSendMethod.Broadcast,
            IncludeDisabledComponents = true,
            IncludeInactiveObjects = true,
        };

        [SerializeReference]
        [SerializeRefPicker(typeof(IStampSource), AllowNull = false, AlwaysExpanded = true, DisplayBox = true)]
        private IStampSource _stampSource = new GameObjectStampSource();

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            base.Awake();

            if ((_activateOn & ActivateEvent.Awake) != 0)
            {
                this.BindConfiguredDataSource();
            }
        }

        protected override void Start()
        {
            base.Start();

            if ((_activateOn & ActivateEvent.OnStart) != 0 || (_activateOn & ActivateEvent.OnEnable) != 0)
            {
                this.BindConfiguredDataSource();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!this.started) return;

            if ((_activateOn & ActivateEvent.OnEnable) != 0)
            {
                this.BindConfiguredDataSource();
            }
        }

        #endregion

        #region Properties

        public ActivateEvent ActivateOn
        {
            get => _activateOn;
            set => _activateOn = value;
        }

        public UnityEngine.Object ConfiguredDataSource
        {
            get => _dataSource;
            set => _dataSource = value;
        }

        public bool RespectProxySources
        {
            get => _respectProxySources;
            set => _respectProxySources = value;
        }

        public int MaxVisible
        {
            get => _maxVisible;
            set => _maxVisible = value;
        }

        public Transform Container
        {
            get => _container;
            set => _container = value;
        }

        public Messaging.MessageSendCommand BindMessageSettings
        {
            get => _bindMessageSettings;
            set => _bindMessageSettings = value;
        }

        public IStampSource StampSource
        {
            get => _stampSource;
            set => _stampSource = value;
        }

        #endregion

        #region IDataBindingContext Interface

        public object DataSource { get; private set; }

        public void Bind(object source, int index)
        {
            this.Bind(source);
        }

        public void BindConfiguredDataSource()
        {
            this.Bind(_dataSource);
        }

        public void Bind(object source)
        {
            var dataprovider = DataProviderUtils.GetAsDataProvider(source, _respectProxySources);
            this.DataSource = dataprovider;

            if (_container && _container.childCount > 0)
            {
                foreach (Transform t in _container)
                {
                    t.gameObject.Kill();
                }
            }

            if (dataprovider == null) return;

            if (_stampSource == null)
            {
                return;
            }
            else if (_stampSource.IsAsync)
            {
                _ = this.DoStampLayoutGroup(_container, dataprovider, _stampSource);
            }
            else
            {
                int index = 0;

                foreach (var item in dataprovider.Cast<object>().Take(_maxVisible))
                {
                    GameObject inst = _stampSource.InstantiateStamp(_container, item);
                    DataBindingContext.SendBindMessage(_bindMessageSettings, inst, item, index);
                    index++;
                }
            }
        }

#if SP_UNITASK
        private async UniTaskVoid DoStampLayoutGroup(Transform container, System.Collections.IEnumerable dataProvider, IStampSource source)
#else
        private async System.Threading.Tasks.Task DoStampLayoutGroup(Transform container, System.Collections.IEnumerable dataProvider, IStampSource source)
#endif
        {
            int index = 0;
            foreach (var item in dataProvider.Cast<object>().Take(_maxVisible))
            {
                GameObject inst = await source.InstantiateStampAsync(container, item);
                if (inst == null) continue;

                DataBindingContext.SendBindMessage(_bindMessageSettings, inst, item, index);
                index++;
            }
        }

        #endregion

        #region IDataProvider Interface

        object IDataProvider.FirstElement => DataProviderUtils.GetFirstElementOfDataProvider(this.DataSource);

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return DataProviderUtils.GetAsDataProvider(this.DataSource)?.GetEnumerator() ?? Enumerable.Empty<object>().GetEnumerator();
        }

        #endregion

    }
}
