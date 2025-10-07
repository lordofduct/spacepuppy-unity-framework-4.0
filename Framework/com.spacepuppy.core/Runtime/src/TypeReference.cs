using UnityEngine;

using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    /// <summary>
    /// Allows for a serializable reference to a type in the namespace.
    /// 
    /// If the serialized type deserializes improperly, the Type returned with be the 'void' type. 
    /// A property 'IsVoid' exists to easily test this. This state implies that the serialized data 
    /// was loaded with out the required namespace loaded.
    /// </summary>
    [System.Serializable()]
    public struct TypeReference : System.Runtime.Serialization.ISerializable
    {

        #region Fields

        [SerializeField()]
        private string _typeHash;

        [System.NonSerialized()]
        private System.Type _type;
        
        #endregion

        #region CONSTRUCTOR

        public TypeReference(System.Type tp)
        {
            _type = tp;
            _typeHash = TypeUtil.HashType(_type);
        }

        #endregion

        #region Properties

        public System.Type Type
        {
            get
            {
                if (_type == null) _type = TypeUtil.UnHashType(_typeHash) ?? typeof(void); // set type to void if the type is unfruitful, this way we're not constantly retesting this
                return _type;
            }
            set
            {
                _type = value;
                _typeHash = TypeUtil.HashType(_type);
            }
        }

        public bool IsVoid
        {
            get
            {
                return this.Type == typeof(void);
            }
        }

        #endregion


        #region ISerializable Interface

        private TypeReference(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            _typeHash = info.GetString("hash");
            _type = null;
        }

        void System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            _typeHash = TypeUtil.HashType(_type);
            info.AddValue("hash", _typeHash);
        }

        #endregion

        #region Operators

        public static implicit operator System.Type(TypeReference a)
        {
            return a.Type;
        }

        #endregion


        #region Special Types

        [System.AttributeUsage(System.AttributeTargets.Field)]
        public class ConfigAttribute : System.Attribute
        {

            public System.Type[] inheritsFromTypes;
            public bool allowAbstractClasses = false;
            public bool allowInterfaces = false;
            public bool allowGeneric = false;
            public System.Type defaultType = null;
            public System.Type[] excludedTypes = null;
            public TypeDropDownListingStyle dropDownStyle = TypeDropDownListingStyle.Flat;
            public int MaxVisibleCount = int.MinValue; //int.MinValue means "default", all other negative/zero means all, positive means limited

            public ConfigAttribute(System.Type inheritsFromType)
            {
                this.inheritsFromTypes = new System.Type[] { inheritsFromType };
            }

            public ConfigAttribute(params System.Type[] inheritsFromTypes)
            {
                this.inheritsFromTypes = inheritsFromTypes;
            }

        }

        #endregion

    }

}