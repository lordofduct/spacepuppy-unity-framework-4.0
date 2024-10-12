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
    public abstract class BaseSerializableInterfaceRef
    {

        public const string PROP_UOBJECT = "_obj";
        public const string PROP_REFOBJECT = "_ref";

    }

    /// <summary>
    /// Supports easily serializing a UnityEngine.Object as some interface type, while also allowing the assignment of non-UnityEngine.Object's as said interface type as well (but not serialized).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Serializable]
    public class InterfaceRef<T> : BaseSerializableInterfaceRef, ISerializationCallbackReceiver, IDynamicProperty where T : class
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

        #region IDynamicProperty Interface

        object IDynamicProperty.Get() => this.Value;

        void IDynamicProperty.Set(object value) => this.Value = value as T;

        System.Type IDynamicProperty.GetType() => typeof(T);

        #endregion

    }

    public abstract class BaseSerializableInterfaceCollection
    {

    }

    [System.Serializable]
    public class InterfaceRefCollection<T> : BaseSerializableInterfaceCollection, IEnumerable<T>, ISerializationCallbackReceiver where T : class
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


    [System.Serializable]
    public class InterfaceRefOrPicker<T> : BaseSerializableInterfaceRef, IDynamicProperty where T : class
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

        #region IDynamicProperty Interface

        object IDynamicProperty.Get() => this.Value;

        void IDynamicProperty.Set(object value) => this.Value = value as T;

        System.Type IDynamicProperty.GetType() => typeof(T);

        #endregion

    }

}
