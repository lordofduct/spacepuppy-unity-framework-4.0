using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Dynamic;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Project
{

    /// <summary>
    /// Exists to make SerializableInterfaceRef drawable. Do not inherit from this, instead inherit from SerializableInterfaceRef.
    /// </summary>
    public abstract class BaseSerializableInterfaceRef : IDynamicProperty
    {

        public const string PROP_UOBJECT = "_obj";
        public const string PROP_REFOBJECT = "_ref";

        #region Methods

        public abstract void SetValue(object value);
        public abstract object GetValue();
        public abstract System.Type GetValueType();

        #endregion

        #region IDynamicProperty Interface

        object IDynamicProperty.Get() => this.GetValue();

        void IDynamicProperty.Set(object value) => this.SetValue(value);

        System.Type IDynamicProperty.GetType() => this.GetValueType();

        #endregion

    }
    public abstract class BaseSerializableInterfaceRef<T> : BaseSerializableInterfaceRef where T : class
    {
        public override System.Type GetValueType() => typeof(T);
    }

    /// <summary>
    /// Supports easily serializing a UnityEngine.Object as some interface type, while also allowing the assignment of non-UnityEngine.Object's as said interface type as well (but not serialized).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Serializable]
    public class InterfaceRef<T> : BaseSerializableInterfaceRef<T>, ISerializationCallbackReceiver where T : class
    {

        #region Fields

        [SerializeField]
        private UnityEngine.Object _obj;
        [System.NonSerialized]
        private T _value;

        #endregion

        #region CONSTRUCTOR

        public InterfaceRef()
        {

        }

        public InterfaceRef(T value)
        {
            this.Value = value;
        }

        #endregion

        #region Properties

        public UnityEngine.Object RawValue
        {
            get => _obj;
            set => _obj = value;
        }

        public T Value
        {
            get
            {
                return !object.ReferenceEquals(_value, null) ? _value : ObjUtil.GetAsFromSource<T>(_obj, true);
            }
            set
            {
                _obj = value as UnityEngine.Object;
                _value = value;
            }
        }

        #endregion

        #region Methods

        public override object GetValue() => this.Value;
        public override void SetValue(object value) => this.Value = value as T;

        #endregion

        #region ISerializationCallbackReceiver Interface

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _value = _obj as T;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            //do nothing
        }

        #endregion

    }

    /// <summary>
    /// Serializes an object via 'SerializeReference' and includes a handy picker tool for the editor.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Serializable]
    public class InterfacePicker<T> : BaseSerializableInterfaceRef<T> where T : class
    {

        #region Fields

        [SerializeReference]
        private T _ref;

        #endregion

        #region CONSTRUCTOR

        public InterfacePicker()
        {

        }

        public InterfacePicker(T value)
        {
            this.Value = value;
        }

        #endregion

        #region Properties

        public T Value
        {
            get
            {
                return _ref;
            }
            set
            {
                _ref = value;
            }
        }

        #endregion

        #region Methods

        public override object GetValue() => this.Value;
        public override void SetValue(object value) => this.Value = value as T;

        #endregion

    }

    /// <summary>
    /// Joins InterfaceRef and InterfacePicker together in a dual mode supporting both simultaneously.
    /// 
    /// The way this is designed internally supports easily converting InterfaceRef or InterfacePicker to this without breaking existing serialized data. 
    /// For this reason any of these 3 are favored over the older 'SerializeRefPicker' attribute.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Serializable]
    public class InterfaceRefOrPicker<T> : BaseSerializableInterfaceRef<T> where T : class
    {

        #region Fields

        [SerializeField]
        private UnityEngine.Object _obj;
        [SerializeReference]
        private T _ref;

        #endregion

        #region CONSTRUCTOR

        public InterfaceRefOrPicker()
        {

        }

        public InterfaceRefOrPicker(T value)
        {
            this.Value = value;
        }

        #endregion

        #region Properties

        public T Value
        {
            get
            {
                return _obj is T uot ? uot : _ref;
            }
            set
            {
                if (value is UnityEngine.Object uot)
                {
                    _obj = uot;
                }
                else
                {
                    _ref = value;
                }
            }
        }

        #endregion

        #region Methods

        public override object GetValue() => this.Value;
        public override void SetValue(object value) => this.Value = value as T;

        #endregion

    }


    public abstract class BaseSerializableInterfaceList
    {
        public const string PROP_DATA = "_data";

        #region Methods
        public abstract System.Type GetValueType();
        #endregion

    }
    public abstract class BaseSerializableInterfaceList<T> : BaseSerializableInterfaceList, IList<T> where T : class
    {

        #region Fields

        [System.NonSerialized]
        private List<T> _values;

        #endregion

        #region CONSTRUCTOR

        public BaseSerializableInterfaceList()
        {
            _values = new List<T>();
        }

        public BaseSerializableInterfaceList(IEnumerable<T> values)
        {
            _values = new List<T>(values);
        }

        #endregion

        #region Properties

        public List<T> Values => _values;

        #endregion

        #region Methods

        public override System.Type GetValueType() => typeof(T);

        #endregion

        #region IList Interface

        public virtual int Count => _values.Count;
        public virtual T this[int index]
        {
            get => _values[index];
            set => _values[index] = value;
        }

        public virtual int IndexOf(T item) => _values.IndexOf(item);

        public virtual void Insert(int index, T item) => _values.Insert(index, item);

        public virtual void RemoveAt(int index) => _values.RemoveAt(index);

        public virtual void Add(T item) => _values.Add(item);

        public virtual void Clear() => _values.Clear();

        public virtual bool Contains(T item) => _values.Contains(item);

        public virtual void CopyTo(T[] array, int arrayIndex) => _values.CopyTo(array, arrayIndex);

        public virtual bool Remove(T item) => _values.Remove(item);

        public List<T>.Enumerator GetEnumerator() => _values.GetEnumerator();

        bool ICollection<T>.IsReadOnly => false;
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _values.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

        #endregion

    }

    [System.Serializable]
    public class InterfaceRefList<T> : BaseSerializableInterfaceList<T>, ISerializationCallbackReceiver where T : class
    {

        #region Fields

        [SerializeField]
        private InterfaceRef<T>[] _data;

        #endregion

        #region CONSTRUCTOR

        public InterfaceRefList() : base() { }
        public InterfaceRefList(IEnumerable<T> values) : base(values) { }

        #endregion

        #region ISerializationCallbackReceiver Interface

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            this.Values.Clear();
            if (_data?.Length > 0)
            {
                this.Values.AddRange(_data.Select(o => o as T));
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            _data = this.Values.Select(o => new InterfaceRef<T>(o is UnityEngine.Object ? o : default(T))).ToArray();
        }

        #endregion

    }

    [System.Serializable]
    public class InterfacePickerList<T> : BaseSerializableInterfaceList<T>, ISerializationCallbackReceiver where T : class
    {

        #region Fields

        [SerializeField]
        private InterfacePicker<T>[] _data;

        #endregion

        #region CONSTRUCTOR

        public InterfacePickerList() : base() { }
        public InterfacePickerList(IEnumerable<T> values) : base(values) { }

        #endregion

        #region ISerializationCallbackReceiver Interface

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            this.Values.Clear();
            if (_data?.Length > 0)
            {
                this.Values.AddRange(_data.Select(o => o.Value));
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            _data = this.Values.Select(o => new InterfacePicker<T>(!(o is UnityEngine.Object) ? o : default(T))).ToArray();
        }

        #endregion

    }

    //TODO - make InterfaceRefOrPickerList




    [System.Obsolete("Use InterfaceRefList")]
    public abstract class BaseObsoleteInterfaceRefCollection
    {
        public const string PROP_ARR_OBSOLETE = "_arr";
    }
    [System.Serializable, System.Obsolete("Use InterfaceRefList")]
    public class InterfaceRefCollection<T> : BaseObsoleteInterfaceRefCollection, IEnumerable<T>, ISerializationCallbackReceiver where T : class
    {

        #region Fields

        [SerializeField]
        private UnityEngine.Object[] _arr;
        [System.NonSerialized]
        private List<T> _values = new List<T>();

        #endregion

        #region CONSTRUCTOR

        public InterfaceRefCollection()
        {
            _values = new List<T>();
        }

        public InterfaceRefCollection(IEnumerable<T> values)
        {
            _values = new List<T>(values);
        }

        #endregion

        #region Properties

        public int Count => _values.Count;

        public List<T> Values => _values;

        #endregion

        #region ISerializationCallbackReceiver Interface

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _values.Clear();
            if (_arr?.Length > 0)
            {
                _values.AddRange(_arr.Select(o => o as T));
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            _arr = _values.Select(o => o as UnityEngine.Object).ToArray();
        }

        #endregion

        #region IEnumerable Interface

        public List<T>.Enumerator GetEnumerator() => _values.GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _values.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _values.GetEnumerator();

        #endregion

    }

}
