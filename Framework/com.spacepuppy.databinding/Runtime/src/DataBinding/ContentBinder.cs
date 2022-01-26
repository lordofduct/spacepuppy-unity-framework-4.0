using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;

namespace com.spacepuppy.DataBinding
{

    [RequireComponent(typeof(DataBindingContext))]
    public abstract class ContentBinder : SPComponent
    {

#if UNITY_EDITOR
        public const string PROP_KEY = nameof(_key);
#endif


        public event System.EventHandler KeyChanged;

        #region Fields

        [SerializeField]
        private string _key;

        [System.NonSerialized]
        private DataBindingContext _context;

        #endregion

        #region CONSTRUCTOR

        protected override void Awake()
        {
            _context = this.AddOrGetComponent<DataBindingContext>();

            base.Awake();
        }

        #endregion

        #region Properties

        public DataBindingContext Context => _context;

        public string Key
        {
            get => _key;
            set
            {
                if (_key == value) return;
                _key = value;
                this.KeyChanged?.Invoke(this, System.EventArgs.Empty);
            }
        }

        #endregion

        #region Methods

        public abstract void SetValue(object value);

        #endregion

    }

}
