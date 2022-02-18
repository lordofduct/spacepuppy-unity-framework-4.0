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
        [TypeRestriction(typeof(IDataProvider), AllowProxy = true)]
        private UnityEngine.Object _dataProvider;

        [SerializeField]
        [TypeRestriction(typeof(IDataBindingContext), AllowProxy = true)]
        private UnityEngine.Object _targetDataBindingContext;

        [SerializeField]
        [Tooltip("All DataBindingContexts and children of the TargetDataBindingContext will have their bind events called.")]
        private bool _broadcastBindingMessage = true;

        #endregion

        #region Properties

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

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var source = com.spacepuppy.DataBinding.DataBindingContext.GetFirstElementOfDataProvider(_dataProvider);

            GameObject go;
            IDataBindingContext context;
            if (_broadcastBindingMessage && (go = GameObjectUtil.GetGameObjectFromSource(_targetDataBindingContext, true)))
            {
                com.spacepuppy.DataBinding.DataBindingContext.BroadcastBindMessage(go, source, 0, true, true);
            }
            else if ((context = ObjUtil.GetAsFromSource<IDataBindingContext>(_targetDataBindingContext, true)) != null)
            {
                context.Bind(source, 0);
            }
            return true;
        }
        private static readonly System.Action<IDataBindingContext, System.ValueTuple<object, int>> _stampFunctor = (s, t) => s.Bind(t.Item1, t.Item2);

        #endregion

    }

}
