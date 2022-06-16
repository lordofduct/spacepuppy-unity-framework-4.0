using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    [System.Serializable]
    public sealed class SlimVariant
    {

        #region Fields

        [SerializeReference]
        private object _value;

        #endregion

        #region Properties

        public object Value
        {
            get
            {
                if (_value is IValue ival)
                    return ival.Value;
                else
                    return _value;
            }
            set
            {
                switch(VariantReference.GetVariantType(value))
                {
                    case VariantType.Object:
                    case VariantType.GameObject:
                    case VariantType.Component:
                        if(!object.ReferenceEquals(this.Value, value))
                        {
                            _value = new UObjectValue() { Value = value as UnityEngine.Object };
                        }
                        break;
                    case VariantType.Null:
                        _value = null;
                        break;
                    case VariantType.String:
                        if (!value.Equals(this.Value))
                        {
                            _value = new StringValue() { Value = value as string };
                        }
                        break;
                    case VariantType.Boolean:
                        if (!value.Equals(this.Value))
                        {
                            _value = new BoolValue() { Value = ConvertUtil.ToBool(value) };
                        }
                        break;
                    case VariantType.Integer:
                        int ival = ConvertUtil.ToInt(value);
                        if (!ival.Equals(this.Value))
                        {
                            _value = new IntValue() { Value = ival };
                        }
                        break;
                    case VariantType.Float:
                        float fval = ConvertUtil.ToSingle(value);
                        if (!fval.Equals(this.Value))
                        {
                            _value = new FloatValue() { Value = fval };
                        }
                        break;
                    case VariantType.Double:
                        double dval = ConvertUtil.ToDouble(value);
                        if (!dval.Equals(this.Value))
                        {
                            _value = new DoubleValue() { Value = dval };
                        }
                        break;
                    case VariantType.Vector2:
                    case VariantType.Vector3:
                    case VariantType.Vector4:
                    case VariantType.Quaternion:
                    case VariantType.Color:
                    case VariantType.DateTime:
                    case VariantType.LayerMask:
                    case VariantType.Rect:
                    case VariantType.Numeric:
                    case VariantType.Ref:
                        if (!value.Equals(this.Value))
                        {
                            _value = value;
                        }
                        break;
                }
            }
        }

        #endregion

        #region Special Types

        public interface IValue
        {
            object Value { get; }
        }

        [System.Serializable]
        private struct UObjectValue : IValue
        {
            public UnityEngine.Object Value;
            object IValue.Value => this.Value;
        }

        [System.Serializable]
        private struct StringValue : IValue
        {
            public string Value;
            object IValue.Value => this.Value;
        }

        [System.Serializable]
        private struct BoolValue : IValue
        {
            public bool Value;
            object IValue.Value => this.Value;
        }

        [System.Serializable]
        private struct IntValue : IValue
        {
            public int Value;
            object IValue.Value => this.Value;
        }

        [System.Serializable]
        private struct FloatValue : IValue
        {
            public float Value;
            object IValue.Value => this.Value;
        }

        [System.Serializable]
        private struct DoubleValue : IValue
        {
            public double Value;
            object IValue.Value => this.Value;
        }

        public class Config : System.Attribute
        {
            public VariantType RestrictToType { get; set; }

            public Config(VariantType restrictToType)
            {
                this.RestrictToType = restrictToType;
            }
        }

        #endregion

#if UNITY_EDITOR
        public static object CreateInnerValuewrapper(object value)
#else
        private static object CreateInnerValuewrapper(object value)
#endif
        {
            switch (VariantReference.GetVariantType(value))
            {
                case VariantType.Object:
                case VariantType.GameObject:
                case VariantType.Component:
                    return new UObjectValue() { Value = value as UnityEngine.Object };
                case VariantType.Null:
                    return null;
                case VariantType.String:
                    return new StringValue() { Value = value as string };
                case VariantType.Boolean:
                    return new BoolValue() { Value = ConvertUtil.ToBool(value) };
                case VariantType.Integer:
                    return new IntValue() { Value = ConvertUtil.ToInt(value) };
                case VariantType.Float:
                    return new FloatValue() { Value = ConvertUtil.ToSingle(value) };
                case VariantType.Double:
                    return new DoubleValue() { Value = ConvertUtil.ToDouble(value) };
                case VariantType.Vector2:
                case VariantType.Vector3:
                case VariantType.Vector4:
                case VariantType.Quaternion:
                case VariantType.Color:
                case VariantType.DateTime:
                case VariantType.LayerMask:
                case VariantType.Rect:
                case VariantType.Numeric:
                case VariantType.Ref:
                    return value;
                default:
                    return null;
            }
        }

    }

}
