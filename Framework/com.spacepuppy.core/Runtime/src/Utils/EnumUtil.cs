using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;

namespace com.spacepuppy.Utils
{

    /// <summary>
    /// Readable shorthand methods for enum operations. The underlying bitwise operators are substantially faster. 
    /// These methods are intended for making code more readable, rather than more optimized. Use sparingly.
    /// </summary>
    public static class EnumUtil
    {

        #region unsafe conversion
        static uint ToUint<T>(T e) where T : struct, Enum
        {
            unsafe
            {
                void* ptr = UnsafeUtility.AddressOf(ref e);
                return *((uint*)ptr);
            }
        }
        static ulong ToUlong<T>(T e) where T : struct, Enum
        {
            unsafe
            {
                void* ptr = UnsafeUtility.AddressOf(ref e);
                return *((ulong*)ptr);
            }
        }
        static ushort ToUShort<T>(T e) where T : struct, Enum
        {
            unsafe
            {
                void* ptr = UnsafeUtility.AddressOf(ref e);
                return *((ushort*)ptr);
            }
        }
        static byte ToByte<T>(T e) where T : struct, Enum
        {
            unsafe
            {
                void* ptr = UnsafeUtility.AddressOf(ref e);
                return *((byte*)ptr);
            }
        }
        static ulong ToNumeric<T>(T e) where T : struct, Enum
        {
            int size = UnsafeUtility.SizeOf<T>();
            unsafe
            {
                void* ptr = UnsafeUtility.AddressOf(ref e);
                if (size == s_UIntSize)
                {
                    return (ulong)(*((uint*)ptr));
                }
                else if (size == s_ULongSize)
                {
                    return *((ulong*)ptr);
                }
                else if (size == s_UShortSize)
                {
                    return (ulong)(*((ushort*)ptr));
                }
                else if (size == s_ByteSize)
                {
                    return (ulong)(*((byte*)ptr));
                }
            }
            throw new Exception("No matching conversion function found for an Enum of size: " + size);
        }
        static int s_UIntSize = UnsafeUtility.SizeOf<uint>();
        static int s_ULongSize = UnsafeUtility.SizeOf<ulong>();
        static int s_UShortSize = UnsafeUtility.SizeOf<ushort>();
        static int s_ByteSize = UnsafeUtility.SizeOf<byte>();
        #endregion



        public static object ToEnumsNumericType(System.Enum e)
        {
            if (e == null) return null;

            switch (e.GetTypeCode())
            {
                case System.TypeCode.SByte:
                    return System.Convert.ToSByte(e);
                case System.TypeCode.Byte:
                    return System.Convert.ToByte(e);
                case System.TypeCode.Int16:
                    return System.Convert.ToInt16(e);
                case System.TypeCode.UInt16:
                    return System.Convert.ToUInt16(e);
                case System.TypeCode.Int32:
                    return System.Convert.ToInt32(e);
                case System.TypeCode.UInt32:
                    return System.Convert.ToUInt32(e);
                case System.TypeCode.Int64:
                    return System.Convert.ToInt64(e);
                case System.TypeCode.UInt64:
                    return System.Convert.ToUInt64(e);
                default:
                    return null;
            }
        }

        private static object ToEnumsNumericType(ulong v, System.TypeCode code)
        {
            switch (code)
            {
                case System.TypeCode.Byte:
                    return (byte)v;
                case System.TypeCode.SByte:
                    return (sbyte)v;
                case System.TypeCode.Int16:
                    return (short)v;
                case System.TypeCode.UInt16:
                    return (ushort)v;
                case System.TypeCode.Int32:
                    return (int)v;
                case System.TypeCode.UInt32:
                    return (uint)v;
                case System.TypeCode.Int64:
                    return (long)v;
                case System.TypeCode.UInt64:
                    return v;
                default:
                    return null;
            }
        }

        public static bool EnumValueIsDefined(object value, System.Type enumType)
        {
            if (enumType == null) throw new System.ArgumentNullException("enumType");
            if (!enumType.IsEnum) throw new System.ArgumentException("Must be enum type.", "enumType");
            if (value == null) return false;

            try
            {
                if (value is string)
                    return System.Enum.IsDefined(enumType, value);
                else if (ConvertUtil.IsNumeric(value))
                {
                    value = ConvertUtil.ToPrim(value, System.Type.GetTypeCode(enumType));
                    return System.Enum.IsDefined(enumType, value);
                }
            }
            catch
            {
            }

            return false;
        }

        public static bool EnumValueIsDefined<T>(object value)
        {
            return EnumValueIsDefined(value, typeof(T));
        }

        public static T AddFlag<T>(this T e, T value) where T : struct, System.Enum
        {
            return (T)System.Enum.ToObject(typeof(T), System.Convert.ToInt64(e) | System.Convert.ToInt64(value));
        }

        public static T RedactFlag<T>(this T e, T value) where T : struct, System.Enum
        {
            var x = System.Convert.ToInt64(e);
            var y = System.Convert.ToInt64(value);
            return (T)System.Enum.ToObject(typeof(T), x & ~(x & y));
        }

        public static T SetFlag<T>(this T e, T flag, bool status) where T : struct, System.Enum
        {
            var x = System.Convert.ToInt64(e);
            var y = System.Convert.ToInt64(flag);
            if (status)
                x |= y;
            else
                x &= ~(x & y);
            return (T)System.Enum.ToObject(typeof(T), x);
        }

        /*
         * UNNECESSARY - latest versions of .net has Enum.HasFlag built in
         * 
        public static bool HasFlag(this System.Enum e, System.Enum value)
        {
            ulong v = System.Convert.ToUInt64(value);
            return (System.Convert.ToUInt64(e) & v) == v;
        }
         */

        public static bool HasFlagT<T>(this T e, T value) where T : struct, System.Enum
        {
            ulong v = ToNumeric(value);
            return (ToNumeric(e) & v) == v;
            //ulong v = System.Convert.ToUInt64(value);
            //return (System.Convert.ToUInt64(e) & v) == v;
        }

        public static bool HasFlag(this System.Enum e, ulong value)
        {
            return (System.Convert.ToUInt64(e) & value) == value;
        }

        public static bool HasFlagT<T>(this T e, ulong value) where T : struct, System.Enum
        {
            return (ToNumeric(e) & value) == value;
            //return (System.Convert.ToUInt64(e) & value) == value;
        }

        public static bool HasFlag(this System.Enum e, long value)
        {
            return (System.Convert.ToInt64(e) & value) == value;
        }

        public static bool HasFlagT<T>(this T e, long value) where T : struct, System.Enum
        {
            return (ToNumeric(e) & (ulong)value) == (ulong)value;
            //return (System.Convert.ToInt64(e) & value) == value;
        }

        public static bool HasAnyFlagT<T>(this T e, T value) where T : struct, System.Enum
        {
            return (ToNumeric(e) & ToNumeric(value)) != 0UL;
            //return (System.Convert.ToUInt64(e) & System.Convert.ToUInt64(value)) != 0UL;
        }

        public static bool HasAnyFlag(this System.Enum e, System.Enum value)
        {
            return (System.Convert.ToUInt64(e) & System.Convert.ToUInt64(value)) != 0UL;
        }

        public static bool HasAnyFlagT<T>(this T e, ulong value) where T : struct, System.Enum
        {
            return (ToNumeric(e) & value) != 0UL;
            //return (System.Convert.ToUInt64(e) & value) != 0;
        }

        public static bool HasAnyFlag(this System.Enum e, ulong value)
        {
            return (System.Convert.ToUInt64(e) & value) != 0UL;
        }

        public static bool HasAnyFlagT<T>(this T e, long value) where T : struct, System.Enum
        {
            return (ToNumeric(e) & (ulong)value) != 0UL;
            //return (System.Convert.ToInt64(e) & value) != 0;
        }

        public static bool HasAnyFlag(this System.Enum e, long value)
        {
            return (System.Convert.ToInt64(e) & value) != 0L;
        }

        public static IEnumerable<System.Enum> EnumerateFlags(System.Enum e)
        {
            if (e == null) throw new System.ArgumentNullException("e");

            var tp = e.GetType();
            ulong max = 0;
            foreach (var en in System.Enum.GetValues(tp))
            {
                ulong v = System.Convert.ToUInt64(en);
                if (v > max) max = v;
            }
            int loops = (int)System.Math.Log(max, 2) + 1;


            ulong ie = System.Convert.ToUInt64(e);
            for (int i = 0; i < loops; i++)
            {
                ulong j = (ulong)System.Math.Pow(2, i);
                if ((ie & j) != 0)
                {
                    var js = ToEnumsNumericType(j, e.GetTypeCode());
                    if (System.Enum.IsDefined(tp, js)) yield return (System.Enum)System.Enum.Parse(tp, js.ToString());
                }
            }
        }

        public static IEnumerable<T> EnumerateFlags<T>(T e) where T : struct, System.Enum
        {
            var tp = e.GetType();
            if (!tp.IsEnum) throw new System.ArgumentException("Type must be an enum.", "T");

            ulong max = 0;
            foreach (var en in System.Enum.GetValues(tp))
            {
                ulong v = System.Convert.ToUInt64(en);
                if (v > max) max = v;
            }
            int loops = (int)System.Math.Log(max, 2) + 1;


            ulong ie = System.Convert.ToUInt64(e);
            for (int i = 0; i < loops; i++)
            {
                ulong j = (ulong)System.Math.Pow(2, i);
                if ((ie & j) != 0)
                {
                    var js = ToEnumsNumericType(j, e.GetTypeCode());
                    if (System.Enum.IsDefined(tp, js))
                    {
                        yield return (T)js;
                    }
                }
            }
        }

        public static IEnumerable<System.Enum> GetUniqueEnumFlags(System.Type enumType)
        {
            if (enumType == null) throw new System.ArgumentNullException("enumType");
            if (!enumType.IsEnum) throw new System.ArgumentException("Type must be an enum.", "enumType");

            foreach (System.Enum e in System.Enum.GetValues(enumType))
            {
                //var d = System.Convert.ToDecimal(e);
                //if (d > 0 && MathUtil.IsPowerOfTwo(System.Convert.ToUInt64(d))) yield return e as System.Enum;

                switch (e.GetTypeCode())
                {
                    case System.TypeCode.Byte:
                    case System.TypeCode.UInt16:
                    case System.TypeCode.UInt32:
                    case System.TypeCode.UInt64:
                        if (MathUtil.IsPowerOfTwo(System.Convert.ToUInt64(e))) yield return e;
                        break;
                    case System.TypeCode.SByte:
                        {
                            sbyte i = System.Convert.ToSByte(e);
                            if (i == -128 || (i > 0 && MathUtil.IsPowerOfTwo((ulong)i))) yield return e;
                        }
                        break;
                    case System.TypeCode.Int16:
                        {
                            short i = System.Convert.ToInt16(e);
                            if (i == -32768 || (i > 0 && MathUtil.IsPowerOfTwo((ulong)i))) yield return e;
                        }
                        break;
                    case System.TypeCode.Int32:
                        {
                            int i = System.Convert.ToInt32(e);
                            if (i == -2147483648 || (i > 0 && MathUtil.IsPowerOfTwo((ulong)i))) yield return e;
                        }
                        break;
                    case System.TypeCode.Int64:
                        {
                            long i = System.Convert.ToInt64(e);
                            if (i == -9223372036854775808 || (i > 0 && MathUtil.IsPowerOfTwo((ulong)i))) yield return e;
                        }
                        break;
                }
            }
        }

        public static IEnumerable<string> GetFriendlyNames(System.Type enumType)
        {
            if (!enumType.IsEnum) throw new System.ArgumentException("Type must be an enum.", "enumType");

            return enumType.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).OrderBy(fi => fi.GetValue(null)).Select(fi =>
            {
                var desc = fi.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false).FirstOrDefault() as System.ComponentModel.DescriptionAttribute;
                return desc != null ? desc.Description : StringUtil.NicifyVariableName(fi.Name);
            });
        }

        public static IEnumerable<KeyValuePair<System.Enum, string>> GetFriendlyNamePairs(System.Type enumType)
        {
            if (!enumType.IsEnum) throw new System.ArgumentException("Type must be an enum.", "enumType");

            return enumType.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).OrderBy(fi => fi.GetValue(null)).Select(fi =>
            {
                var desc = fi.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false).FirstOrDefault() as System.ComponentModel.DescriptionAttribute;
                return new KeyValuePair<System.Enum, string>((System.Enum)fi.GetValue(null), desc != null ? desc.Description : StringUtil.NicifyVariableName(fi.Name));
            });
        }

        public static IEnumerable<KeyValuePair<T, string>> GetFriendlyNamePairs<T>() where T : struct, System.Enum
        {
            return typeof(T).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).OrderBy(fi => fi.GetValue(null)).Select(fi =>
            {
                var desc = fi.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false).FirstOrDefault() as System.ComponentModel.DescriptionAttribute;
                return new KeyValuePair<T, string>((T)fi.GetValue(null), desc != null ? desc.Description : StringUtil.NicifyVariableName(fi.Name));
            });
        }

        public static string GetFriendlyName(System.Enum value)
        {
            if (value == null) return string.Empty;

            var tp = value.GetType();
            var nm = System.Enum.GetName(tp, value);
            var fi = tp.GetField(nm);
            if (fi == null) return nm;

            var desc = fi.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false).FirstOrDefault() as System.ComponentModel.DescriptionAttribute;
            return desc != null ? desc.Description : StringUtil.NicifyVariableName(nm);
        }

    }
}
