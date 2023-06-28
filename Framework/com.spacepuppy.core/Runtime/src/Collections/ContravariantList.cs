using System;
using System.Collections.Generic;
using System.Linq;

namespace com.spacepuppy.Collections
{

    public class ContravariantList<TOuter, TInner> : IList<TOuter>
    {

        private IList<TInner> _coll;

        public ContravariantList(IList<TInner> coll)
        {
            _coll = coll;
        }

        public IList<TInner> InternalCollection
        {
            get => _coll;
            set => _coll = value;
        }


        public int Count => _coll?.Count ?? 0;

        public bool IsReadOnly => _coll == null;

        public TOuter this[int index]
        {
            get => _coll != null ? (_coll[index] is TOuter ot ? ot : default) : default;
            set => _coll[index] = value is TInner ot ? ot : default(TInner);
        }

        public void Add(TOuter item)
        {
            _coll?.Add(item is TInner ot ? ot : default);
        }

        public void Clear()
        {
            _coll?.Clear();
        }

        public bool Contains(TOuter item)
        {
            return item is TInner ot ? _coll?.Contains(ot) ?? false : false;
        }

        public void CopyTo(TOuter[] array, int arrayIndex)
        {
            if (_coll != null && array is TInner[] aot)
            {
                _coll.CopyTo(aot, arrayIndex);
            }
        }

        public bool Remove(TOuter item)
        {
            return item is TInner ot ? _coll?.Remove(ot) ?? false : false;
        }

        public int IndexOf(TOuter item)
        {
            if (_coll == null) return -1;
            return item is TInner ot ? _coll.IndexOf(ot) : -1;
        }

        public void Insert(int index, TOuter item)
        {
            _coll?.Insert(index, item is TInner ot ? ot : default);
        }

        public void RemoveAt(int index)
        {
            _coll?.RemoveAt(index);
        }

        public IEnumerator<TOuter> GetEnumerator()
        {
            return _coll.OfType<TOuter>().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

    }

}
