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

        [SerializeField]
        private int _order;

        [SerializeField()]
        private ActivateEvent _activateOn = ActivateEvent.OnStartOrEnable;

        [SerializeField]
        private DataProviderRef _dataProvider = new DataProviderRef();

        [SerializeField]
        private int _maxVisible = 100;

        [SerializeField]
        [DefaultFromSelf]
        private Transform _container;

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
                this.Bind();
            }
        }

        protected override void Start()
        {
            base.Start();

            if ((_activateOn & ActivateEvent.OnStart) != 0 || (_activateOn & ActivateEvent.OnEnable) != 0)
            {
                this.Bind();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!this.started) return;

            if ((_activateOn & ActivateEvent.OnEnable) != 0)
            {
                this.Bind();
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
            switch(source)
            {
                case System.Collections.IEnumerable e:
                    this.Bind(e);
                    break;
                default:
                    this.Bind(new object[] { source });
                    break;
            }
        }

        public void Bind()
        {
            this.Bind(_dataProvider.Value);
        }

        public void Bind(System.Collections.IEnumerable dataprovider)
        {
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
                    GameObject inst = _stampSource.InstantiateStamp(_container);
                    DataBindingContext.BroadcastBindMessage(inst, item, index, true, true);
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
                GameObject inst = await source.InstantiateStampAsync(container);
                if (inst == null) continue;

                DataBindingContext.BroadcastBindMessage(inst, item, index, true, true);
                index++;
            }
        }

        #endregion

        #region IDataProvider Interface

        object IDataProvider.FirstElement => DataBindingContext.GetFirstElementOfDataProvider(this.DataSource);

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this.DataSource as System.Collections.IEnumerable)?.GetEnumerator() ?? Enumerable.Empty<object>().GetEnumerator();
        }

        #endregion

    }
}
