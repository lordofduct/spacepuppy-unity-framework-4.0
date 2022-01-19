using System;

namespace com.spacepuppy
{

    /// <summary>
    /// 64-bit fixed point value in base-10 (the radix is a power of 10 rather than 2). 
    /// This is very similar to System.Decimal, but doesn't have a floating radix. As a 
    /// result it can easily be converted to and from decimal.
    /// </summary>
    [System.Serializable()]
    public struct FixedCurrency : IConvertible, IEquatable<FixedCurrency>, IFormattable
    {
        public const decimal MAX_VALUE = 9223372036854M;
        public const decimal MIN_VALUE = -MAX_VALUE;
        private const int PRECISION_DIGITCOUNT = 6;
        public const int PRECISION = 1000000;
        private const decimal PRECISION_M = (decimal)PRECISION;

        private const long MIN_INTVALUE = (long)MIN_VALUE * PRECISION;
        private const long MAX_INTVALUE = (long)MAX_VALUE * PRECISION;

        #region Fields

        [UnityEngine.SerializeField()]
        private long _value;

        #endregion

        #region CONSTRUCTOR

        public FixedCurrency(float value)
        {
            if (value < (float)MIN_VALUE) _value = MIN_INTVALUE;
            else if (value > (float)MAX_VALUE) _value = MAX_INTVALUE;
            else _value = (long)(value * (float)PRECISION);
        }

        public FixedCurrency(double value)
        {
            if (value < (double)MIN_VALUE) _value = MIN_INTVALUE;
            else if (value > (double)MAX_VALUE) _value = MAX_INTVALUE;
            else _value = (long)(value * (double)PRECISION);
        }

        public FixedCurrency(decimal value)
        {
            if (value < MIN_VALUE) _value = MIN_INTVALUE;
            else if (value > MAX_VALUE) _value = MAX_INTVALUE;
            else _value = (long)(value * PRECISION_M);
        }

        public FixedCurrency(long value)
        {
            if (value < MIN_VALUE) _value = MIN_INTVALUE;
            else if (value > MAX_VALUE) _value = MAX_INTVALUE;
            else _value = (long)(value * PRECISION_M);
        }

        #endregion


        #region Property

        public long RawValue
        {
            get { return _value; }
            set { _value = value; }
        }

        public decimal Value
        {
            get { return (decimal)_value / PRECISION_M; }
            set
            {
                if (value < MIN_VALUE) _value = MIN_INTVALUE;
                else if (value > MAX_VALUE) _value = MAX_INTVALUE;
                else _value = (long)(value * PRECISION_M);
            }
        }

        #endregion

        #region Methods

        public override bool Equals(object obj)
        {
            if (obj is FixedCurrency)
                return _value == ((FixedCurrency)obj)._value;

            var d = com.spacepuppy.Utils.ConvertUtil.ToDecimal(obj);
            return d == this.Value;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public bool Equals(FixedCurrency other)
        {
            return _value == other._value;
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }

        #endregion

        #region Static Utils

        public static FixedCurrency Clamp01(FixedCurrency p)
        {
            if (p._value < 0) p._value = 0;
            else if (p._value > PRECISION) p._value = PRECISION;
            return p;
        }

        public static FixedCurrency Clamp01(decimal value)
        {
            var result = new FixedCurrency();
            if (value < 0M)
            {
                result._value = 0;
            }
            else if (value > 1M)
            {
                result._value = PRECISION;
            }
            else
            {
                result._value = (long)(value * PRECISION_M);
            }
            return result;
        }

        public static FixedCurrency FromRawValue(long value)
        {
            return new FixedCurrency()
            {
                _value = value
            };
        }

        /// <summary>
        /// Attempts to parse a string into a FixedCurrency value. Values out of range will clamp unless flagged to fail.
        /// </summary>
        /// <param name="sval"></param>
        /// <param name="failOnOutOfRange"></param>
        /// <returns></returns>
        public static FixedCurrency Parse(string sval, bool failOnOutOfRange = false)
        {
            FixedCurrency result;
            if (!TryParse(sval, out result, false)) throw new System.FormatException("String is not a format supported by FixedCurrency.Parse.");
            return result;
        }

        /// <summary>
        /// Attempts to parse a string into a FixedCurrency value. Values out of range will clamp unless flagged to fail.
        /// </summary>
        /// <param name="sval"></param>
        /// <param name="failOnOutOfRange"></param>
        /// <returns></returns>
        public static bool TryParse(string sval, out FixedCurrency value, bool failOnOutOfRange = false)
        {
            value = new FixedCurrency()
            {
                _value = 0
            };
            if (string.IsNullOrEmpty(sval)) return false;

            long l = 0;
            long sign = 1;
            int fractionaldigitcount = -1;
            for (int i = 0; i < sval.Length; i++)
            {
                char c = sval[i];
                switch (c)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        if (fractionaldigitcount < 0)
                        {
                            if (l > MAX_VALUE)
                            {
                                if (failOnOutOfRange)
                                {
                                    return false;
                                }
                                else
                                {
                                    l = (long)MAX_VALUE;
                                    goto Done;
                                }
                            }
                        }
                        else if (fractionaldigitcount < PRECISION_DIGITCOUNT)
                        {
                            fractionaldigitcount++;
                        }
                        else if (fractionaldigitcount == PRECISION_DIGITCOUNT)
                        {
                            goto Done;
                        }
                        l = (l * 10) + ((long)c - (long)'0');
                        break;
                    case '.':
                        if (l == MAX_VALUE)
                            goto Done;
                        else if (fractionaldigitcount < 0)
                            fractionaldigitcount = 0;
                        else
                            return false;
                        break;
                    case '-':
                        if (i == 0)
                            sign = -1;
                        else
                            return false;
                        break;
                    default:
                        return false;
                }
            }

        Done:
            if (fractionaldigitcount <= 0)
            {
                l *= PRECISION;
            }
            else
            {
                for (int i = fractionaldigitcount; i < PRECISION_DIGITCOUNT; i++)
                {
                    l *= 10;
                }
            }
            value._value = Math.Min(l, MAX_INTVALUE) * sign;
            return true;
        }

        #endregion


        #region Conversion

        public static implicit operator FixedCurrency(float f)
        {
            return new FixedCurrency(f);
        }

        public static implicit operator FixedCurrency(double f)
        {
            return new FixedCurrency(f);
        }

        public static implicit operator FixedCurrency(decimal d)
        {
            return new FixedCurrency(d);
        }

        public static implicit operator FixedCurrency(int i)
        {
            return new FixedCurrency(i);
        }

        public static explicit operator float(FixedCurrency p)
        {
            return Convert.ToSingle(p.Value);
        }

        public static explicit operator double(FixedCurrency p)
        {
            return Convert.ToDouble(p.Value);
        }

        public static implicit operator decimal(FixedCurrency p)
        {
            return p.Value;
        }

        public static explicit operator int(FixedCurrency p)
        {
            return (int)p.Value;
        }

        public static explicit operator long(FixedCurrency p)
        {
            return (long)p.Value;
        }

        #endregion

        #region Operators

        public static FixedCurrency operator +(FixedCurrency a)
        {
            return a;
        }

        public static FixedCurrency operator ++(FixedCurrency a)
        {
            return new FixedCurrency(a.Value + 1M);
        }

        public static FixedCurrency operator +(FixedCurrency a, FixedCurrency b)
        {
            return new FixedCurrency(a.Value + b.Value);
        }

        public static FixedCurrency operator -(FixedCurrency a)
        {
            a._value = -a._value;
            return a;
        }

        public static FixedCurrency operator --(FixedCurrency a)
        {
            return new FixedCurrency(a.Value - 1M);
        }

        public static FixedCurrency operator -(FixedCurrency a, FixedCurrency b)
        {
            return new FixedCurrency(a.Value - b.Value);
        }

        public static FixedCurrency operator *(FixedCurrency a, FixedCurrency b)
        {
            return new FixedCurrency(a.Value * b.Value);
        }

        public static FixedCurrency operator /(FixedCurrency a, FixedCurrency b)
        {
            return new FixedCurrency(a.Value / b.Value);
        }

        public static bool operator >(FixedCurrency a, FixedCurrency b)
        {
            return a._value > b._value;
        }

        public static bool operator <(FixedCurrency a, FixedCurrency b)
        {
            return a._value < b._value;
        }

        public static bool operator >=(FixedCurrency a, FixedCurrency b)
        {
            return a._value >= b._value;
        }

        public static bool operator <=(FixedCurrency a, FixedCurrency b)
        {
            return a._value <= b._value;
        }

        public static bool operator ==(FixedCurrency a, FixedCurrency b)
        {
            return a._value == b._value;
        }

        public static bool operator !=(FixedCurrency a, FixedCurrency b)
        {
            return a._value != b._value;
        }

        #endregion

        #region IConvertible Interface

        public TypeCode GetTypeCode()
        {
            return TypeCode.Decimal;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return _value != 0;
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(this.Value);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(this.Value);
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return Convert.ToDateTime(this.Value);
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return this.Value;
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(this.Value);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(this.Value);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(this.Value);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(this.Value);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(this.Value);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(this.Value);
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            return Convert.ToString(this.Value);
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            return null;
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(this.Value);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(this.Value);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(this.Value);
        }

        #endregion

        #region IFormattable Interface

        public string ToString(string format)
        {
            return this.Value.ToString(format);
        }

        public string ToString(string format, IFormatProvider provider)
        {
            return this.Value.ToString(format, provider);
        }

        #endregion

        #region Special Types

        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
        public class ConfigAttribute : Attribute
        {

            public bool displayAsRange;
            public decimal min = FixedCurrency.MIN_VALUE;
            public decimal max = FixedCurrency.MAX_VALUE;

            public ConfigAttribute(double min, double max)
            {
                this.displayAsRange = true;
                this.min = (decimal)min;
                this.max = (decimal)max;
            }

        }

        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
        public class MinAttribute : ConfigAttribute
        {
            public MinAttribute(double min) : base(min, (double)FixedCurrency.MAX_VALUE)
            {
                this.displayAsRange = false;
            }
        }

        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
        public class MaxAttribute : ConfigAttribute
        {
            public MaxAttribute(double max) : base((double)FixedCurrency.MIN_VALUE, max)
            {
                this.displayAsRange = false;
            }
        }

        #endregion

    }

}
