using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Collections;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;
using com.spacepuppy.Project;

namespace com.spacepuppy.DataBinding
{

    public interface IDataBindingMessageHandler
    {

        void Bind(object source, int index);

    }

    public interface IDataBindingContext : IDataBindingMessageHandler
    {

        object DataSource { get; }

    }

    public class DataBindingContext : SPComponent, IDataBindingContext, IDataProvider
    {

        #region Fields

        [SerializeReference]
        [Tooltip("If left NULL than the StandardBindingProtocol will be used.")]
        [SerializeRefPicker(typeof(ISourceBindingProtocol), AllowNull = true, AlwaysExpanded = true, DisplayBox = true)]
        private ISourceBindingProtocol _bindingProtocol = null;

        [SerializeField]
        private bool _respectProxySources = true;

        [SerializeField]
        private SPEvent _onDataBound = new SPEvent("OnDataBound");

        #endregion

        #region Properties

        public ISourceBindingProtocol BindingProtocol
        {
            get => _bindingProtocol;
            set => _bindingProtocol = value;
        }

        public bool RespectProxySources
        {
            get => _respectProxySources;
            set => _respectProxySources = value;
        }

        public SPEvent OnDataBound => _onDataBound;

        #endregion

        #region IDataBindingContext Interface

        public object DataSource { get; private set; }

        public virtual void Bind(object source, int index)
        {
            this.DataSource = source;
            var protocol = _bindingProtocol ?? StandardBindingProtocol.Default;

            object reducedref;
            if (protocol.PreferredSourceType != null && (reducedref = ObjUtil.GetAsFromSource(protocol.PreferredSourceType, source, _respectProxySources)) != null)
            {
                source = reducedref;
            }

            using (var lst = TempCollection.GetList<ContentBinder>())
            {
                this.GetComponents<ContentBinder>(lst);
                for (int i = 0; i < lst.Count; i++)
                {
                    lst[i].SetValue(protocol.GetValue(source, lst[i].Key));
                }
            }
        }

        #endregion

        #region IDataProvider Interface

        object IDataProvider.FirstElement => this.DataSource;

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            yield return this.DataSource;
        }

        #endregion

        #region Static Utils

        public static object GetFirstElementOfDataProvider(object source)
        {
            switch(source)
            {
                case IDataProvider dp:
                    return dp.FirstElement;
                case System.Collections.IEnumerable e:
                    return e.Cast<object>().FirstOrDefault();
                default:
                    return source;
            }
        }

        public static void SignalBindMessage(GameObject go, object source, int index, bool includeDisabledComponents = false)
        {
            go.Signal((source, index), _stampFunctor, includeDisabledComponents);
        }
        public static void SignalUpwardsBindMessage(GameObject go, object source, int index, bool includeDisabledComponents = false)
        {
            go.SignalUpwards((source, index), _stampFunctor, includeDisabledComponents);
        }
        public static void BroadcastBindMessage(GameObject go, object source, int index, bool includeInactiveObject = false, bool includeDisabledComponents = false)
        {
            go.Broadcast((source, index), _stampFunctor, includeInactiveObject, includeDisabledComponents);
        }
        private static readonly System.Action<IDataBindingMessageHandler, System.ValueTuple<object, int>> _stampFunctor = (s, t) => s.Bind(t.Item1, t.Item2);

        #endregion

    }

}
