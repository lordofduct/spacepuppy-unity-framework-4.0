using UnityEngine;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Render
{

    [System.Serializable()]
    public class MaterialPropertyReference
    {

        #region Fields

        [SerializeField()]
        [DisableOnPlay()]
        [TypeRestriction(typeof(MaterialSource), typeof(IMaterialSource), typeof(Renderer), typeof(UnityEngine.UI.Graphic), HideTypeDropDownIfSingle = true)]
        private UnityEngine.Object _material;
        [SerializeField()]
        private MaterialPropertyValueType _valueType;
        [SerializeField()]
        private string _propertyName;
        [SerializeField]
        private MaterialPropertyValueTypeMember _member;

        #endregion

        #region CONSTRUCTOR

        public MaterialPropertyReference()
        {

        }

        public MaterialPropertyReference(Material mat, string propName, MaterialPropertyValueType valueType, MaterialPropertyValueTypeMember member = MaterialPropertyValueTypeMember.None)
        {
            _material = mat;
            _propertyName = propName;
            _valueType = valueType;
            _member = member;
        }

        public MaterialPropertyReference(Renderer renderer, string propName, MaterialPropertyValueType valueType, MaterialPropertyValueTypeMember member = MaterialPropertyValueTypeMember.None)
        {
            _material = renderer;
            _propertyName = propName;
            _valueType = valueType;
            _member = member;
        }

        #endregion

        #region Properties

        public Material Material
        {
            get { return MaterialUtil.GetMaterialFromSource(_material); }
            set { _material = value; }
        }

        public MaterialPropertyValueType PropertyValueType
        {
            get { return _valueType; }
            set { _valueType = value; }
        }

        /// <summary>
        /// If the PropertyValueType is Color/Vector, this allows referencing one of the specific members of the Color/Vector.
        /// </summary>
        public MaterialPropertyValueTypeMember PropertyValueTypeMember
        {
            get { return _member; }
            set { _member = value; }
        }

        public string PropertyName
        {
            get { return _propertyName; }
            set { _propertyName = value; }
        }

        public object Value
        {
            get
            {
                return this.GetValue();
            }
            set
            {
                this.SetValue(value);
            }
        }

        #endregion

        #region Methods

        public void SetValue(object value, bool useMaterialPropertyBlockIfAvailable = false)
        {
            using (var setter = MaterialUtil.GetMaterialPropertySetter(_material, useMaterialPropertyBlockIfAvailable))
            {
                setter.SetProperty(_propertyName, _valueType, _member, value);
            }
        }

        public void SetValue(float value, bool useMaterialPropertyBlockIfAvailable = false)
        {
            if (_valueType != MaterialPropertyValueType.Float) return;
            if (!_material) return;

            using (var setter = MaterialUtil.GetMaterialPropertySetter(_material, useMaterialPropertyBlockIfAvailable))
            {
                switch (_valueType)
                {
                    case MaterialPropertyValueType.Float:
                        setter.SetFloat(_propertyName, value);
                        break;
                    case MaterialPropertyValueType.Color:
                        {
                            switch (_member)
                            {
                                case MaterialPropertyValueTypeMember.None:
                                    //do nothing
                                    break;
                                default:
                                    setter.SetProperty(_propertyName, _valueType, _member, value); //REFACTOR? - this causes minor GC in a function intended to be GC free, but it's not used a lot so will refactor later
                                    break;
                            }
                        }
                        break;
                    case MaterialPropertyValueType.Vector:
                        {
                            switch (_member)
                            {
                                case MaterialPropertyValueTypeMember.None:
                                    setter.SetVector(_propertyName, ConvertUtil.ToVector4(value));
                                    break;
                                default:
                                    setter.SetProperty(_propertyName, _valueType, _member, value); //REFACTOR? - this causes minor GC in a function intended to be GC free, but it's not used a lot so will refactor later
                                    break;
                            }
                        }
                        break;
                    case MaterialPropertyValueType.Texture:
                        //do nothing
                        break;
                }
            }
        }

        public void SetValue(Color value, bool useMaterialPropertyBlockIfAvailable = false)
        {
            if (_valueType != MaterialPropertyValueType.Color) return;
            if (!_material) return;

            using (var setter = MaterialUtil.GetMaterialPropertySetter(_material, useMaterialPropertyBlockIfAvailable))
            {
                switch (_valueType)
                {
                    case MaterialPropertyValueType.Float:
                        //do nothing
                        break;
                    case MaterialPropertyValueType.Color:
                        {
                            switch (_member)
                            {
                                case MaterialPropertyValueTypeMember.None:
                                    setter.SetColor(_propertyName, value);
                                    break;
                                default:
                                    setter.SetProperty(_propertyName, _valueType, _member, value); //REFACTOR? - this causes minor GC in a function intended to be GC free, but it's not used a lot so will refactor later
                                    break;
                            }
                        }
                        break;
                    case MaterialPropertyValueType.Vector:
                        {
                            switch (_member)
                            {
                                case MaterialPropertyValueTypeMember.None:
                                    setter.SetVector(_propertyName, ConvertUtil.ToVector4(value));
                                    break;
                                default:
                                    setter.SetProperty(_propertyName, _valueType, _member, value); //REFACTOR? - this causes minor GC in a function intended to be GC free, but it's not used a lot so will refactor later
                                    break;
                            }
                        }
                        break;
                    case MaterialPropertyValueType.Texture:
                        //do nothing
                        break;
                }
            }
        }

        public void SetValue(Vector4 value, bool useMaterialPropertyBlockIfAvailable = false)
        {
            if (_valueType != MaterialPropertyValueType.Vector) return;
            if (!_material) return;

            using (var setter = MaterialUtil.GetMaterialPropertySetter(_material, useMaterialPropertyBlockIfAvailable))
            {
                switch (_valueType)
                {
                    case MaterialPropertyValueType.Float:
                        //do nothing
                        break;
                    case MaterialPropertyValueType.Color:
                        {
                            switch (_member)
                            {
                                case MaterialPropertyValueTypeMember.None:
                                    setter.SetColor(_propertyName, ConvertUtil.ToColor(value));
                                    break;
                                default:
                                    setter.SetProperty(_propertyName, _valueType, _member, value); //REFACTOR? - this causes minor GC in a function intended to be GC free, but it's not used a lot so will refactor later
                                    break;
                            }
                        }
                        break;
                    case MaterialPropertyValueType.Vector:
                        {
                            switch (_member)
                            {
                                case MaterialPropertyValueTypeMember.None:
                                    setter.SetVector(_propertyName, value);
                                    break;
                                default:
                                    setter.SetProperty(_propertyName, _valueType, _member, value); //REFACTOR? - this causes minor GC in a function intended to be GC free, but it's not used a lot so will refactor later
                                    break;
                            }
                        }
                        break;
                    case MaterialPropertyValueType.Texture:
                        //do nothing
                        break;
                }
            }
        }

        public object GetValue(bool useMaterialPropertyBlockIfAvailable = false)
        {
            var mat = this.Material;
            if (mat == null) return null;

            try
            {
                using (var getter = MaterialUtil.GetMaterialPropertyGetter(_material, useMaterialPropertyBlockIfAvailable))
                {
                    return getter.GetProperty(_propertyName, _valueType, _member);
                }
            }
            catch { }

            return null;
        }

        #endregion

    }

}
