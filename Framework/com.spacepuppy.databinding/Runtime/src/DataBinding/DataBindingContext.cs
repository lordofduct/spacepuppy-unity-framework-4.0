using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Collections;
using com.spacepuppy.Events;
using com.spacepuppy.Utils;
using com.spacepuppy.Project;

namespace com.spacepuppy.DataBinding
{

    public interface IDataBindingContext
    {
        void Bind(object source, int index);
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

        public object CurrentSource { get; private set; }

        #endregion

        #region IDataBindingContext Interface

        public virtual void Bind(object source, int index)
        {
            this.CurrentSource = source;
            var protocol = _bindingProtocol ?? StandardBindingProtocol.Default;

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

        object IDataProvider.FirstElement => this.CurrentSource;

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            yield return this.CurrentSource;
        }

        #endregion

    }

    [System.Serializable]
    public class DataBindingContextRef : SerializableInterfaceRef<IDataBindingContext> { }

}
