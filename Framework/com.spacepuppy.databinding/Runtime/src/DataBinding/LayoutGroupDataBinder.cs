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

    public class LayoutGroupDataBinder : SPComponent
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
                this.StampLayoutGroup();
            }
        }

        protected override void Start()
        {
            base.Start();

            if ((_activateOn & ActivateEvent.OnStart) != 0 || (_activateOn & ActivateEvent.OnEnable) != 0)
            {
                this.StampLayoutGroup();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!this.started) return;

            if ((_activateOn & ActivateEvent.OnEnable) != 0)
            {
                this.StampLayoutGroup();
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

        #region Methods

        public void StampLayoutGroup()
        {
            if (_container && _container.childCount > 0)
            {
                foreach (Transform t in _container)
                {
                    t.gameObject.Kill();
                }
            }

            if (this.DataProvider == null) return;

            if (_stampSource == null)
            {
                return;
            }
            else if(_stampSource.IsAsync)
            {
                _ = this.DoStampLayoutGroup(_container, this.DataProvider, _stampSource);
            }
            else
            {
                int index = 0;

                foreach (var item in this.DataProvider.Cast<object>().Take(_maxVisible))
                {
                    GameObject inst = _stampSource.InstantiateStamp(_container);
                    inst.Broadcast((item, index), _stampFunctor, true, true);
                    index++;
                }
            }
        }

#if SP_UNITASK
        private async UniTaskVoid DoStampLayoutGroup(Transform container, IDataProvider dataProvider, IStampSource source)
#else
        private async System.Threading.Tasks.Task DoAddStampers(Transform container, IEnumerable<object> dataProvider, IStampSource source)
#endif
        {
            int index = 0;
            foreach (var item in dataProvider.Cast<object>().Take(_maxVisible))
            {
                GameObject inst = await source.InstantiateStampAsync(container);
                if (inst == null) continue;

                inst.Broadcast((item, index), _stampFunctor, true, true);
                index++;
            }
        }
        private static readonly System.Action<IDataBindingContext, System.ValueTuple<object, int>> _stampFunctor = (s, t) => s.Bind(t.Item1, t.Item2);

        #endregion

    }
}
