using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Events;

namespace com.spacepuppy.DataBinding.Events
{

    public class i_BindDataContext : AutoTriggerable, ITriggerable
    {

        #region Fields

        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("_dataProvider")]
        [SelectableObject(AllowProxy = true)]
        private UnityEngine.Object _dataSource;

        [SerializeField]
        [TypeRestriction(typeof(IDataBindingContext), AllowProxy = true)]
        private UnityEngine.Object _targetDataBindingContext;

        [SerializeField]
        private bool _reduceDataSourceAsProxyBeforeBinding = true;

        [SerializeField]
        [Tooltip("If the DataSource is a collection, the entire collection will be sent, rather than the first element. Use this if the target is something like a LayoutGroupDataBindingContext which expects an entire collection.")]
        private bool _bindDataSourceAsDataProvider = false;

        [SerializeField]
        private Messaging.MessageSendCommand _bindMessageSettings = new Messaging.MessageSendCommand()
        {
            SendMethod = Messaging.MessageSendMethod.Broadcast,
            IncludeDisabledComponents = true,
            IncludeInactiveObjects = true,
        };

        #endregion

        #region Properties

        public UnityEngine.Object DataSource
        {
            get => _dataSource;
            set => _dataSource = value;
        }

        public UnityEngine.Object DataBindingContext
        {
            get => _targetDataBindingContext;
            set => _targetDataBindingContext = value;
        }

        public Messaging.MessageSendCommand BindMessageSettings
        {
            get => _bindMessageSettings;
            set => _bindMessageSettings = value;
        }

        #endregion

        #region Methods

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            object source = _dataSource;
            if (_bindDataSourceAsDataProvider) source = DataProviderUtils.GetAsDataProvider(source, _reduceDataSourceAsProxyBeforeBinding);
            else source = DataProviderUtils.GetFirstElementOfDataProvider(source, _reduceDataSourceAsProxyBeforeBinding);

            GameObject go;
            IDataBindingContext context;
            if (_bindMessageSettings.SendMethod != Messaging.MessageSendMethod.Signal && (go = GameObjectUtil.GetGameObjectFromSource(_targetDataBindingContext, true)))
            {
                com.spacepuppy.DataBinding.DataBindingContext.SendBindMessage(_bindMessageSettings, go, source, 0);
            }
            else if ((context = ObjUtil.GetAsFromSource<IDataBindingContext>(_targetDataBindingContext, true)) != null)
            {
                context.Bind(source, 0);
            }
            return true;
        }

        #endregion

    }

}
