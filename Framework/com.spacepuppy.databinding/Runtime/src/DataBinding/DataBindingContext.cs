using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Collections;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;
using com.spacepuppy.Project;

#if SP_UNITASK
using Cysharp.Threading.Tasks.Triggers;
#endif


namespace com.spacepuppy.DataBinding
{

    public interface IDataBindingMessageHandler
    {

        int BindOrder { get; }

        void Bind(object source, int index);

    }

    public interface IDataBindingContext : IDataBindingMessageHandler
    {

        object DataSource { get; }

    }

    [DisallowMultipleComponent()]
    public class DataBindingContext : SPComponent, IDataBindingContext, IDataProvider, IMActivateOnReceiver
    {

        #region Fields

        [SerializeField]
        private int _bindOrder;

        [SerializeField()]
        private ActivateEvent _activateOn = ActivateEvent.None;

        [SerializeReference]
        [Tooltip("If left NULL than the StandardBindingProtocol will be used.")]
        [SerializeRefPicker(typeof(ISourceBindingProtocol), AllowNull = true, AlwaysExpanded = true, DisplayBox = true)]
        private ISourceBindingProtocol _bindingProtocol = null;

        [SerializeField]
        [SelectableObject(AllowProxy = true)]
        [Tooltip("A datasource to be binded to during the 'ActivateOn' event or if calling 'BindConfiguredDataSource'.")]
        private UnityEngine.Object _dataSource;

        [SerializeField]
        private bool _respectProxySources = false;

        [SerializeField]
        [Tooltip("By default, if an IDataProvider is received as a data source it is reduced to its first element. By setting this true the DataBindingContext will treat the dataprovider as the source directly.")]
        private bool _respectDataProviderAsSource = false;

        [SerializeField]
        [Tooltip("If the 'source' is a INotifyPropertyChanged the context will listen for the PropertyChanged event and rebind.")]
        private bool _bindToPropertyChangedEvent = false;

        [SerializeField]
        [SPEvent.Config("source (object)")]
        private SPEvent _onDataBound = new SPEvent("OnDataBound");

        [System.NonSerialized]
        private bool _waitingToBind;

        #endregion

        #region Properties

        public ActivateEvent ActivateOn
        {
            get { return _activateOn; }
            set { _activateOn = value; }
        }

        public int BindOrder
        {
            get => _bindOrder;
            set => _bindOrder = value;
        }

        public ISourceBindingProtocol BindingProtocol
        {
            get => _bindingProtocol ?? StandardBindingProtocol.Default;
            set => _bindingProtocol = value;
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

        public bool RespectDataProviderAsSource
        {
            get => _respectDataProviderAsSource;
            set => _respectDataProviderAsSource = value;
        }

        public SPEvent OnDataBound => _onDataBound;

        #endregion

        #region Methods

        public object GetBoundValue(object source, string key)
        {
            return this.BindingProtocol?.GetValue(this, source, key);
        }

        public T GetBoundValue<T>(object source, string key)
        {
            return ConvertUtil.Coerce<T>(this.BindingProtocol?.GetValue(this, source, key));
        }

        #endregion

        #region IDataBindingContext Interface

        int IDataBindingMessageHandler.BindOrder => _bindOrder;

        public object DataSource { get; private set; }

        public virtual void Bind(object source, int index)
        {
            this.UnbindPropertyChangedEvent();

            var protocol = this.BindingProtocol;
            if (_respectProxySources) source = IProxyExtensions.ReduceIfProxy(source);

            if (!_respectDataProviderAsSource
                && source is IDataProvider idp
                && (protocol.PreferredSourceType == null || !protocol.PreferredSourceType.IsInstanceOfType(source)))
            {
                source = idp.FirstElement;
            }

            object reducedref;
            if (protocol.PreferredSourceType != null && (reducedref = ObjUtil.GetAsFromSource(protocol.PreferredSourceType, source)) != null)
            {
                source = reducedref;
            }
            this.DataSource = source;

            this.DoBind();

            if (_bindToPropertyChangedEvent && source is System.ComponentModel.INotifyPropertyChanged npc)
            {
                npc.PropertyChanged += DataSource_PropertyChanged;
            }

            _onDataBound.ActivateTrigger(this, this.DataSource);
        }

        public void BindConfiguredDataSource()
        {
            this.Bind(_dataSource, 0);
        }

        public void UnbindPropertyChangedEvent()
        {
            if (this.DataSource is System.ComponentModel.INotifyPropertyChanged npc)
            {
                npc.PropertyChanged -= DataSource_PropertyChanged;
            }
        }


        private void DataSource_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!_waitingToBind)
            {
                _waitingToBind = true;
                GameLoop.LateUpdateHandle.BeginInvoke(this.DoBind);
            }
        }

        private void DoBind()
        {
            _waitingToBind = false;
            using (var lst = TempCollection.GetList<IContentBinder>())
            {
                this.GetComponents<IContentBinder>(lst);
                for (int i = 0; i < lst.Count; i++)
                {
                    try
                    {
                        if (lst[i] is Behaviour mb && !mb.enabled) continue;
                        lst[i].Bind(this, this.DataSource);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }

        #endregion

        #region IDataProvider Interface

        System.Type IDataProvider.ElementType => typeof(object);

        object IDataProvider.FirstElement => this.DataSource;

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            yield return this.DataSource;
        }

        #endregion

        #region IMActivateOnReceiver Interface

        void IMActivateOnReceiver.Activate()
        {
            if (_dataSource) this.BindConfiguredDataSource();
        }

        #endregion

        #region Static Utils

        public static void SendBindMessage(Messaging.MessageSendCommand settings, GameObject go, object source, int index)
        {
            settings.Send(go, (source, index), _bindFunctor, _bindSortOrder);
        }

        public static void SignalBindMessage(GameObject go, object source, int index, bool includeDisabledComponents = false)
        {
            go.Signal((source, index), _bindFunctor, includeDisabledComponents, _bindSortOrder);
        }
        public static void SignalUpwardsBindMessage(GameObject go, object source, int index, bool includeDisabledComponents = false)
        {
            go.SignalUpwards((source, index), _bindFunctor, includeDisabledComponents, _bindSortOrder);
        }
        public static void BroadcastBindMessage(GameObject go, object source, int index, bool includeInactiveObject = false, bool includeDisabledComponents = false)
        {
            go.Broadcast((source, index), _bindFunctor, includeInactiveObject, includeDisabledComponents, _bindSortOrder);
        }

        private static readonly System.Action<IDataBindingMessageHandler, System.ValueTuple<object, int>> _bindFunctor = (s, t) => s.Bind(t.Item1, t.Item2);
        private static readonly System.Comparison<IDataBindingMessageHandler> _bindSortOrder = (a, b) => a.BindOrder.CompareTo(b.BindOrder);

        #endregion

    }

}
